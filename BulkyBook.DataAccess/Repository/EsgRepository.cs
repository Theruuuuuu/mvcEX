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
    public class EsgRepository : Repository<Esg>, IEsgRepository
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<Esg> dbSet;
        public EsgRepository(ApplicationDbContext db):base(db) //base 呼叫基底類別的建構子
                                                                    //也就是取回Repository的db內容
        {
            _db = db;
            this.dbSet = _db.Set<Esg>();
        }


        public void Update(Esg obj)
        {
            dbSet.Update(obj);
        }
    }
}
