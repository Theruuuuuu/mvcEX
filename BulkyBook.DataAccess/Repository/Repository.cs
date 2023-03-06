using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }
            
            if (includeProperties != null)
            {
                foreach(var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.ToList();
        }
        //依Lambda敘述式 找指定的資料
        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }


            query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            //因為是泛型所以不能夠用指定型別的方式
            //_db.Categories.Remove(entity);
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
