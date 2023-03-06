using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db):base(db)
        {
            _db = db;

        }
        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Product obj)
        {
            //_db.Products.Update(obj);
            //嚴謹的更新方式
            var objFromDb = _db.Products.FirstOrDefault(u => u.id == obj.id);
            if (objFromDb != null)
            {
                objFromDb.Title = obj.Title;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.Price = obj.Price;
                objFromDb.Price50 = obj.Price50;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price100 = obj.Price100;
                objFromDb.Description = obj.Description;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.Author = obj.Author;
                objFromDb.CoverTypeId = obj.CoverTypeId;
                //讓他一定有圖片
                if(obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl=obj.ImageUrl;
                }
            }
        }
    }
}
