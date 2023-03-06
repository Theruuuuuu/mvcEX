using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

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
        public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender)
        {
            _unitOfWork=unitOfWork;
            _emailSender=emailSender;   
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==claims.Value,includeProperties:"Product"),
                OrderHeader = new()
            };
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.Count,cart.Product.Price,cart.Product.Price50,cart.Product.Price100);
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
                OrderHeader=new()
            };
            //把資料往前一層移動，就能不會動到原始註冊資料，又能更改本次購物的資料
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x=>x.Id==claims.Value);
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(
                u=>u.ApplicationUserId == claims.Value , includeProperties:"Product");
            
            
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            //公司與個人客戶的邏輯不一樣
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
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



            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //Stript Settings
                var domain = "https://localhost:44332/";
                var onlineServer = "https://dotnetmvc0305.azurewebsites.net/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = onlineServer + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = onlineServer + $"customer/cart/index",
                };
                foreach (var item in ShoppingCartVM.ListCart)
                {

                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),//20.00->2000
                            Currency = "twd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title,
                            },
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                //ShoppingCartVM.OrderHeader.SessionId = session.Id;
                //ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.OrderHeader.UpdateStriptPaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
            }
            //付款成功才刪除購物車內容
            //_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            //_unitOfWork.Save();
            //return RedirectToAction("Index","Home");
        }
        public IActionResult OrderConfirmation(int id)
        {            
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id == id,includeProperties:"ApplicationUser");
            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //檢查stripe狀態
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    //PaymentIntentId再付款後才會有值
                    _unitOfWork.OrderHeader.UpdateStriptPaymentID(id, session.Id, session.PaymentIntentId);
                    //
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "新訂單", "<p>產品正在處理中</p>");
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==
            orderHeader.ApplicationUserId).ToList();
            HttpContext.Session.Clear();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var  cart=_unitOfWork.ShoppingCart.GetFirstOrDefault(u=>u.Id==cartId);
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
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count-1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }            
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

        private double GetPriceBaseOnQuantity(double quantity,double price,double price50,double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if(quantity <=100)
                { 
                    return price50;                
                }
                return price100;
            }
        }
    }
}
