using Main.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class Store
    {
        private readonly int storeId;
        private readonly int userId;
        private readonly bool isFavorit;
        public int StoreId { get { return storeId; } }
        public string CategoryName {  get; set; }  
        public string StoreName {  get; set; }
        public int UserId { get { return userId; } }
        public bool IsFavorite { get { return isFavorit; } }
        public string CreatedBy => Service.GetUserNameByUserID(UserId);

        public Store(int storeId,int userId,bool isFavorite)
        {
            this.storeId = storeId;
            this.userId = userId;
            this.isFavorit = isFavorite;
        }
    }
}
