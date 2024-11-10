﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    public class PendingUser
    {
        private readonly int userid;
        public string Name {  get; set; }   
        public string Role {  get; set; }   

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