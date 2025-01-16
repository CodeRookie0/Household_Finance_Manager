using System;

namespace Main.Models
{
    public class User
    {
        public int UserID { get; set; } 
        public string UserName { get; set; }
        public string Email { get; set; } 
        public string PasswordHash { get; set; } 
        public string Salt { get; set; } 
        public int RoleID { get; set; } 
        public int? FamilyID { get; set; } 
        public DateTime CreatedAt { get; set; } 
        public string ProfileSettings { get; set; }
    }
}
