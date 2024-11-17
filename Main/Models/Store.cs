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
        public int StoreId { get { return storeId; } }
        public string CategoryName {  get; set; }  
        public string StoryName {  get; set; }
        public int UserId { get { return userId; } }

        public Store(int storeId,int userId)
        {
            this.storeId = storeId;
            this.userId = userId;
        }
        
    }
}
