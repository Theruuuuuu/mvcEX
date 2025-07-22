using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe.Checkout;
using System.Drawing;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WebApplication2.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailSender _emailSender;
		[BindProperty] //讓Post方法也能直接用
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public int OrderTotal { get; set; }
		public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;
		}
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			//轉換成顯示類別
			ShoppingCartVM = new ShoppingCartVM()
			{
				ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includeProperties: "Product"),
				OrderHeader = new()
			};
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				//根據數量計算價格
				cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVM);
		}
		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartVM()
			{
				ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includeProperties: "Product"),
				OrderHeader = new()
			};
			//把資料往前一層移動，就能不會動到原始註冊資料，又能更改本次購物的資料
			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claims.Value);

			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.state = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPOST(ShoppingCartVM ShoppingCartVM)
		{
			//取得UserId
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(
				u => u.ApplicationUserId == claims.Value, includeProperties: "Product");


			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);
			//個人戶必須要先付款
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			//公司戶可延遲付款
			else
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}

			//按下summary按鈕成功進入付款頁面時，OrderHeader資料就已經儲存
			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.Productid,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count,
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}


			//付款處理
			//回傳付款網址
			Task<string> res = PayHandler(ShoppingCartVM);

			var jsondoc = JsonDocument.Parse(res.Result);
			string? webUrl = jsondoc
							.RootElement
							.GetProperty("info")
							.GetProperty("paymentUrl")
							.GetProperty("web")
							.GetString();
			string? transcationId = jsondoc
							.RootElement
							.GetProperty("info")
							.GetProperty("transactionId")
							.GetRawText();
			//訂單成立 給予此訂單Linepay交易ID
			_unitOfWork.OrderHeader.UpdateStriptPaymentID(ShoppingCartVM.OrderHeader.Id, transcationId);
			_unitOfWork.Save();
			//導向付款網址
			return Redirect(webUrl);
		}

		public async Task<string> PayHandler(ShoppingCartVM shoppingCartVM)
		{
			var domain = "https://localhost:7097/";
			var helper = new LinePayHelper();
			//data格式固定
			var response = await helper.RequestOnlineAPIAsync(
				HttpMethod.Post,
				"/v3/payments/request",
				data: new
				{
					amount = (int)shoppingCartVM.ListCart.Sum(u => u.Price * u.Count),
					currency = "TWD",
					orderId = shoppingCartVM.OrderHeader.Id,
					packages = new[]
					{
						new {
							id = shoppingCartVM.OrderHeader.Id,
							amount = (int)shoppingCartVM.ListCart.Sum(u => u.Price * u.Count),
							name = "測試包",
							products = shoppingCartVM.ListCart.Select(u => new LinepayDTO
							{
								id = u.Id.ToString(),
								name = u.Product.Title,
								imageUrl = u.Product.ImageUrl,
								quantity = u.Count,
								price = (int)u.Price,
							})

						}
					},
					redirectUrls = new
					{
						confirmUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}&transactionId=",
						cancelUrl = domain + $"customer/cart/index",
					}
				}
			);
			return response;
		}

		//付款成功頁面
		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");
			//如果是個人戶
			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				//付款完成修改訂單狀態為已完成
				_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
				_unitOfWork.Save();
			}

			//預計刪除公司戶設定
			//else if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			//{
			//	_unitOfWork.OrderHeader.UpdateStriptPaymentID(id, "尚未付款待確認");
			//	_unitOfWork.Save();
			//}
			//_emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "新訂單", "<p>產品正在處理中</p>");
			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId ==
			orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
			_unitOfWork.Save();
			//清除購物車數量
			HttpContext.Session.Clear();
			return View(id);
		}
		public IActionResult Plus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			_unitOfWork.ShoppingCart.IncrementCount(cart, 1);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			if (cart.Count <= 1)
			{
				_unitOfWork.ShoppingCart.Remove(cart);
				var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
				HttpContext.Session.SetInt32(SD.SessionCart, count);
			}
			else
			{
				_unitOfWork.ShoppingCart.DecrementCount(cart, 1);
			}
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		[HttpPost]
		public IActionResult UpdateCount(int cartId, int count)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			cart.Count = count;
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cart);
			_unitOfWork.Save();
			var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
			HttpContext.Session.SetInt32(SD.SessionCart, count);
			return RedirectToAction(nameof(Index));
		}

		private double GetPriceBaseOnQuantity(double quantity, double price, double price50, double price100)
		{
			if (quantity <= 50)
			{
				return price;
			}
			else
			{
				if (quantity <= 100)
				{
					return price50;
				}
				return price100;
			}
		}
	}
}
