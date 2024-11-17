using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class Subcategory
    {
        public int SubcategoryID { get; set; }
        public int CategoryID { get; set; }
        public string SubcategoryName { get; set; }
        public int UserID { get; set; }
    }
}
