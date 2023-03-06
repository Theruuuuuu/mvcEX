using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id,string orderStatus,string? paymentStatus=null);//paymentStatus不一定每次都要更新
        void UpdateStriptPaymentID(int id, string sessionId, string paymentItentId);
    }
}
