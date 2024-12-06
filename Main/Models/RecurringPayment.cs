using Main.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class RecurringPayment
    {
        public int RecurringPaymentID { get; set; }
        public string RecurringPaymentName { get; set; }
        public int UserID { get; set; }
        public int? StoreID { get; set; }
        public int? CategoryID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int FrequencyID { get; set; }
        public bool IsActive { get; set; } 
        public int CreatedByUserID { get; set; }


        public string FormattedAmount => Amount.ToString("C");
        public List<RecurringPaymentHistory> History => Service.GetHistoryByRecurringPaymentID(RecurringPaymentID);
        public string FrequencyName => Service.GetFrequencyNameByFrequencyID(FrequencyID);
        public string CreatedByUserName => Service.GetUserNameByUserID(CreatedByUserID);
        public string UserName => Service.GetUserNameByUserID(UserID);
        public decimal TotalAmount
        {
            get
            {
                return History
                    .Where(h => h.ActionTypeID != 3)
                    .Sum(h => h.Amount);
            }
        }
        public string FormattedTotalAmount => TotalAmount.ToString("C");
    }
}
