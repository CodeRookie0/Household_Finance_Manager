using Main.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int UserID { get; set; }
        public int TransactionTypeID { get; set; }
        public DateTime Date { get; set; }
        public int CategoryID { get; set; }
        public int SubcategoryID { get; set; }
        public int StoreID { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }


        public string UserName => Service.GetUserNameByUserID(UserID);
        public string CategoryName => Service.GetCategoryNameByCategoryID(CategoryID);
        public string SubcategoryName => Service.GetSubcategoryNameBySubcategoryID(SubcategoryID);
        public string StoreName => Service.GetStoreNameByStoreID(StoreID);

        public string DateFormatted => Date.ToString("yyyy-MM-dd");
        public string Time => Date.ToString("HH:mm");

        public string FormattedAmount => Amount.ToString("C");
    }
}
