using Main.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class FamilyMember
    {
        public int UserID;
        public int FamilyID { get; set; }
        public int RoleID { get; set; }

        public string UserName => Service.GetUserNameByUserID(UserID);
    }
}
