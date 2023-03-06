using Microsoft.AspNetCore.Mvc;
using BulkyBook.Models;
using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork _db;
        public CompanyController(IUnitOfWork db)
        {
            _db = db;
            
        }
        public IActionResult Index()
        {
            
            return View();
        }

        //Get
        //資料
        public IActionResult Create()
        {           
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Company obj)
        {           
            if (ModelState.IsValid) { 
                _db.Company.Add(obj);
                _db.Save();
                TempData["success"] = "Conpany Create成功";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //編輯
        //Get
        public IActionResult Upsert(int? id)
        {
            //用ViewModel才能夠有效率的從不同Model中傳送資料給View
            //不須使用ViewBag,ViewData,TempData等
            Company company = new();
                            
            if (id == null || id == 0)
            {
                //如果沒有就Create一個新的
                //ViewBag.名字任意 把資料傳到View
                //ViewBag.CategoryList = CategoryList;
                return View(company);
            }
            else
            {
                company = _db.Company.GetFirstOrDefault(n=>n.Id==id);
                return View(company);
                //如果有就更新
            }

            
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {                
                if(obj.Id == 0)
                {
                    _db.Company.Add(obj);
                    TempData["success"] = "Conpany Create 成功";
                }
                else
                {
                    _db.Company.Update(obj);
                    TempData["success"] = "Conpany Update 成功";
                }
                
                _db.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }

        
        #region API CALLS
        public IActionResult GetAll()
        {
            var companyList = _db.Company.GetAll();
            //把資料回傳成Json格式
            return Json(new {data= companyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Company.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _db.Company.Remove(obj);
            _db.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }
        #endregion
    }

}
