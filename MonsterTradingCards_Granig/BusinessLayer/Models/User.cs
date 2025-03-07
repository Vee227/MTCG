using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.BusinessLayer.Models
{
        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; } // Wird in-memory gespeichert (MD5 falls gewünscht)
            public string? Token { get; set; } // Erstellt sich beim ersten erfolgreichen Login

            public User(string username, string password)
            {
                Username = username;
                Password = password;
                Token = null; // Wird erst beim Login gesetzt
            }
        }
    
}
