using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class LimitsModel
    {
        public int LimitId {  get; set; }
        public int FamilyId {  get; set; }
        public int UserId {  get; set; }
        public int CategoryId {  get; set; }
        public double LimitAmount {  get; set; }
        public int FrequencyId {  get; set; }
        public int IsFamilyWide {  get; set; }

        public LimitsModel() 
        { 
        }
    }
}
