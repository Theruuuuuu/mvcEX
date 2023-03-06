using Microsoft.AspNetCore.Mvc;
using BulkyBook.Models;
using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Utility;

namespace WebApplication2.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CategoryController(IUnitOfWork db)
        {
            _unitofwork = db;
        }
        public IActionResult Index()
        {
            //使資料能夠被列舉
            IEnumerable<Category> objCategoryList = _unitofwork.Category.GetAll().OrderBy(x=>x.DisplayOrder);
            return View(objCategoryList);
        }

        //Get
        //資料
        public IActionResult Create()
        {           
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if(obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Name不能跟DisplayOrder相同");
            }
            if (ModelState.IsValid) { 
                _unitofwork.Category.Add(obj);
                _unitofwork.Save();
                TempData["success"] = "Create成功";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //編輯
        //接收資料
        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            //var categoryFromDb = _db.Categories.Find(id);//找尋想要修改的資料
            var categoryFromDbFirst = _unitofwork.Category.GetFirstOrDefault(u=>u.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);
            if(categoryFromDbFirst == null)
            {
                return NotFound();
            }
            //傳回目標資料給Edit的View，以利修改
            return View(categoryFromDbFirst);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Name不能跟DisplayOrder相同");
            }
            //驗證
            if (ModelState.IsValid)
            {
                //通過驗證就進行Update Save 導回Index
                _unitofwork.Category.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Edit成功";
                return RedirectToAction("Index");
            }
            //沒通過驗證就停在頁面
            return View(obj);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var categoryFromDb = _db.Categories.Find(id);
            var categoryFromDbFirst = _unitofwork.Category.GetFirstOrDefault(u=>u.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }           
            return View(categoryFromDbFirst);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitofwork.Category.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitofwork.Category.Remove(obj);
            _unitofwork.Save();
            TempData["success"] = "Delete成功";
            return RedirectToAction("Index");
        }
    }
}
