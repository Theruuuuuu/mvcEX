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
    public class OrderDatailRepository : Repository<OrderDetail>, IOrderDatailRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderDatailRepository(ApplicationDbContext db):base(db) //base 呼叫基底類別的建構子
                                                                    //也就是取回Repository的db內容
        {
            _db = db;
        }


        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
