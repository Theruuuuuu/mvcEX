using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class EsgController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public EsgController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {

            return View();
        }

        #region API CALLS
        public IActionResult GetAll()
        {
            var dataList = _UnitOfWork.Esg.GetAll();
            //把資料回傳成Json格式
            return Json(new { data = dataList });
        }
        #endregion
    }
}
