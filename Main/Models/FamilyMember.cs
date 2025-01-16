using Main.Logic; 

namespace Main.Models
{
    public class FamilyMember
    {
        public int UserID { get; set; }
        public int FamilyID { get; set; }
        public int RoleID { get; set; }

        public string UserName => Service.GetUserNameByUserID(UserID);
    }
}
