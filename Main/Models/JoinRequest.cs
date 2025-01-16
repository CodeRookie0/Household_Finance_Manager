using System;

namespace Main.Logic
{
    public class JoinRequest
    {
        public int JoinRequestID { get; set; }
        public int UserID { get; set; }
        public string FamilyName { get; set; }
        public DateTime JoinRequestDate { get; set; }
        public string RequestStatus { get; set; }
        public string UserName => Service.GetUserNameByUserID(UserID) ;
    }
}
