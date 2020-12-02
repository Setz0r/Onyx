using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Game
{
    public class Account
    {
        public string username;
        public string password;
        public string email;
        public bool ValidateLogin(string username, string password)
        {
            return false;
        }
    }
}
