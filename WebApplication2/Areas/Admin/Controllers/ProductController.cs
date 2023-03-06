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
    public class ProductController : Controller
    {

        private readonly IUnitOfWork _db;
        //取的本機的資訊
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db,IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
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
        public IActionResult Create(Product obj)
        {           
            if (ModelState.IsValid) { 
                _db.Product.Add(obj);
                _db.Save();
                TempData["success"] = "Product Create成功";
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
            ProductViewModel productVM = new()
            {
                Product = new(),
                CategoryList= _db.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList=_db.CoverType.GetAll().Select(i => new SelectListItem
                { 
                    Text = i.Name,
                    Value =i.Id.ToString()
                })
            };                       
            if (id == null || id == 0)
            {
                //如果沒有就Create一個新的
                //ViewBag.名字任意 把資料傳到View
                //ViewBag.CategoryList = CategoryList;
                return View(productVM);
            }
            else
            {
                productVM.Product=_db.Product.GetFirstOrDefault(n=>n.id==id);
                return View(productVM);
                //如果有就更新
            }

            
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                //取得wwwroot的路徑
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    //產生隨機數來當作檔名 避免上傳同名檔案時 會被覆蓋
                    string fileName = Guid.NewGuid().ToString();
                    //合併路徑 讓路徑能到達products資料夾 會加"@"是因為"\"字元式特殊字元
                    var uploads = Path.Combine(wwwRootPath, @"img\products");
                    //GetExtension() 取得file副檔名
                    var extension = Path.GetExtension(file.FileName);

                    //如果圖片存在
                    if (obj.Product.ImageUrl != null)
                    {
                        var previousImg = Path.Combine(wwwRootPath,obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(previousImg))
                        {
                            System.IO.File.Delete(previousImg);
                        }
                    }

                    //合併最終路徑 並建立圖片
                    using (var filestreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        //將傳入的圖片建立到資料夾
                        file.CopyTo(filestreams);
                    }
                    obj.Product.ImageUrl = @"img\products\" + fileName + extension;
                }
                if(obj.Product.id == 0)
                {
                    _db.Product.Add(obj.Product);
                }
                else
                {
                    _db.Product.Update(obj.Product);
                }
                
                _db.Save();
                TempData["success"] = "Product Edit 成功";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        
        #region API CALLS
        public IActionResult GetAll()
        {
            var productList = _db.Product.GetAll(includeProperties:"Category,CoverType");
            //把資料回傳成Json格式
            return Json(new {data=productList});
        }
        [HttpDelete]     
        public IActionResult Delete(int? id)
        {
            var obj = _db.Product.GetFirstOrDefault(u => u.id == id);
            if (obj == null)
            {
                return Json(new { success=false,message="Error while deleting"});
            }

            var previousImg = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(previousImg))
            {
                System.IO.File.Delete(previousImg);
            }
            _db.Product.Remove(obj);
            _db.Save();
            return Json(new { success = true, message = "Delete Success" });
        }
        #endregion
    }

}
