﻿

namespace Main.Models
{
    public class PendingUser
    {
        private readonly int userid;
        public string Name {  get; set; }   
        public string Role {  get; set; }
        public string RoleName { get; set; }

        public int Userid {
            get 
            { 
               return userid;
            } 
        }

        public PendingUser(int argUserId)
        {
            userid = argUserId;
        }
        
    }
}
