using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<Category> dbSet;
        public CategoryRepository(ApplicationDbContext db):base(db) //base 呼叫基底類別的建構子
                                                                    //也就是取回Repository的db內容
        {
            _db = db;
            this.dbSet = _db.Set<Category>();
        }


        public void Update(Category obj)
        {
            dbSet.Update(obj);
        }
    }
}
