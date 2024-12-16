using Main.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class RecurringPaymentHistory
    {
        public int RecurringPaymentHistoryID { get; set; }
        public int RecurringPaymentID { get; set; }
        public int? TransactionID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int ActionTypeID { get; set; }
        public DateTime ActionDate { get; set; }
        public string FormattedAmount => Amount.ToString("C");
        public string ActionTypeName => Service.GetActionNameByActionID(ActionTypeID);
        public int UserID => Service.GetUserIdByRecurringPaymentId(RecurringPaymentID);
        public string UserName => Service.GetUserNameByUserID(UserID);
    }
}
