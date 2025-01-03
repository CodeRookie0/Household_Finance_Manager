﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
