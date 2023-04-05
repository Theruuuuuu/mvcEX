using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace WebApplication2.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        [BindProperty]
        public List<WeatherVM> list { get; set; }
        private readonly IUnitOfWork _unitOfWork;
        
        string url = "https://opendata.cwb.gov.tw/api/v1/rest/datastore/F-C0032-001?Authorization=CWB-C35A226E-1E25-494A-AB54-DC778A21822E";
        HttpClient client = new HttpClient();
        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string? option)
        {
            if(option == null)
            {
                IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
                return View(products);
            }
            IEnumerable<Product> productss = _unitOfWork.Product.GetAll(u=>u.Category.Name==option,includeProperties: "Category,CoverType");
            return View(productss);
        }


        public IActionResult Details(int productid)
        {
            ShoppingCart Cartobj = new()
            {
                Productid = productid,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.id == productid, includeProperties: "Category,CoverType"),
                Count = 1 //預設1 
            };
            return View(Cartobj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]//有驗證的使用者才能進入此method
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //取得目前登入帳號的ID
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claims.Value;

            //找尋如果購物車裡已經有同樣商品
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                x=>x.ApplicationUserId == claims.Value && x.Productid==shoppingCart.Productid);
            //沒有就新增
            if (cartFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,_unitOfWork.ShoppingCart.GetAll(
                    u=>u.ApplicationUserId == claims.Value).ToList().Count);
            }
            //有就改Count
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                _unitOfWork.Save();
            }
            
            return RedirectToAction(nameof(Index));
        }
             
        public async Task<IActionResult> PrivacyAsync(string? location)
        {
            var httpre = await client.GetAsync(url);
            string jsonres = await httpre.Content.ReadAsStringAsync();
            var myweather = JsonConvert.DeserializeObject<Root>(jsonres);

            list = new List<WeatherVM>();
            for (int i = 0; i < myweather.Records.Location.Count; i++)
            {
                list.Add(new WeatherVM()
                {
                    location = myweather.Records.Location[i].LocationName,
                    Description = myweather.Records.Location[i].WeatherElement[0].Time[0].Parameter.ParameterName,
                    Rain = myweather.Records.Location[i].WeatherElement[1].Time[0].Parameter.ParameterName,
                    oC = int.Parse(myweather.Records.Location[i].WeatherElement[2].Time[0].Parameter.ParameterName)                    
                });
            }
            
            var selectList = list.Select(x => new SelectListItem()
            {
                Text=x.location,
                Value=x.location
            });

            ViewBag.SelectList = selectList;

            ViewBag.Loca = list.Select(u=>u.location);
            if(location != null)
            {
                var querylist = list.Where(u => u.location == location);
                return View(querylist);
            }
            else
            {
                var querylist = list;
                return View(querylist);
            }
            
        }

        public async Task<IActionResult> WeatherAsync(string option)
        {
            var httpre = await client.GetAsync(url);
            string jsonres = await httpre.Content.ReadAsStringAsync();
            var myweather = JsonConvert.DeserializeObject<Root>(jsonres);

            list = new List<WeatherVM>();
            for (int i = 0; i < myweather.Records.Location.Count; i++)
            {
                list.Add(new WeatherVM()
                {
                    location = myweather.Records.Location[i].LocationName,
                    Description = myweather.Records.Location[i].WeatherElement[0].Time[0].Parameter.ParameterName,
                    Rain = myweather.Records.Location[i].WeatherElement[1].Time[0].Parameter.ParameterName,
                    oC = int.Parse(myweather.Records.Location[i].WeatherElement[2].Time[0].Parameter.ParameterName)
                });
            }

            foreach (var i in list)
            {
                if(i.location == option)
                {
                    var weatherVM = i;
                    return PartialView("Weather", weatherVM);
                }
            }
            
            return RedirectToAction("PrivacyAsync");
            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}