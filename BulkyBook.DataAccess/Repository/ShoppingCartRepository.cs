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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<ShoppingCart> dbSet;
        public ShoppingCartRepository(ApplicationDbContext db):base(db) //base 呼叫基底類別的建構子
                                                                    //也就是取回Repository的db內容
        {
            _db = db;
            this.dbSet = _db.Set<ShoppingCart>();
        }

        public int DecrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count -= count;
            return shoppingCart.Count;
        }

        public int IncrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count += count;
            return shoppingCart.Count;
        }
    }
}
