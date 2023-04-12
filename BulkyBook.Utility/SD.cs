using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public static class SD
    {
        public const string Role_User_Indi = "Individual";
        public const string Role_User_Comp = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        public const string StatusPending = "待辦";
        public const string StatusApproved = "已確認";
        public const string StatusInProcess = "處理中";
        public const string StatusShipped = "已送達";
        public const string StatusCancelled = "已取消";
        public const string StatusRefunded = "已退款";

        //公司帳戶可以貨到付款
        public const string PaymentStatusPending = "待辦";
        public const string PaymentStatusApproved = "已確認";
        public const string PaymentStatusDelayedPayment = "已確認延遲付款";
        public const string PaymentStatusRejected = "已拒絕";

        public const string SessionCart = "SessionShoppingCart";
    }
}
