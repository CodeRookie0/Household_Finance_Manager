using Main.GUI;
using Main.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public int? TransactionTypeID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int FrequencyID { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserID { get; set; }

        private bool _canEditAndDeactivate = true; // Domyślnie ustawione na `true`
        public bool CanEditAndDeactivate
        {
            get => _canEditAndDeactivate;
            set
            {
                if (_canEditAndDeactivate != value)
                {
                    _canEditAndDeactivate = value;
                    OnPropertyChanged(nameof(CanEditAndDeactivate));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
