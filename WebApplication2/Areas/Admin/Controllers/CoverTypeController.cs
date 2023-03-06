using Microsoft.AspNetCore.Mvc;
using BulkyBook.Models;
using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _db;
        public CoverTypeController(IUnitOfWork db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> objCoverTypeList = _db.CoverType.GetAll();
            return View(objCoverTypeList);
        }

        //Get
        //資料
        public IActionResult Create()
        {           
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {           
            if (ModelState.IsValid) { 
                _db.CoverType.Add(obj);
                _db.Save();
                TempData["success"] = "CoverType Create成功";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //編輯
        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            //var categoryFromDb = _db.Categories.Find(id);//找尋想要修改的資料
            var coverTypeFromDbFirst = _db.CoverType.GetFirstOrDefault(u=>u.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);
            if(coverTypeFromDbFirst == null)
            {
                return NotFound();
            }
            //傳回目標資料給View，以利修改
            return View(coverTypeFromDbFirst);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _db.CoverType.Update(obj);
                _db.Save();
                TempData["success"] = "CoverType Edit成功";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var categoryFromDb = _db.Categories.Find(id);
            var coverTypeFromDbFirst = _db.CoverType.GetFirstOrDefault(u=>u.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);
            if (coverTypeFromDbFirst == null)
            {
                return NotFound();
            }
            return View(coverTypeFromDbFirst);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.CoverType.Remove(obj);
            _db.Save();
            TempData["success"] = "CoverType Delete成功";
            return RedirectToAction("Index");
        }
    }
}
