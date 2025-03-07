using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCards_Granig.BusinessLayer.Models;


namespace MonsterTradingCards_Granig.BusinessLayer.Controller
{
    public class UserManager
    {
        private static List<User> users = new List<User>(); // Speicher für User

        public string RegisterUser(string username, string password)
        {
            if (users.Any(u => u.Username == username))
                return "User already exists";

            users.Add(new User(username, password));
            return "User registered successfully";
        }

        public string LoginUser(string username, string password)
        {
            var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
                return "Login failed";

            // Token nur beim ersten Login erstellen
            if (string.IsNullOrEmpty(user.Token))
            {
                user.Token = GenerateToken(username);
            }

            return $"Login successful. Token: {user.Token}";
        }

        private string GenerateToken(string username)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(username + DateTime.Now.Ticks));
                return "mtcgToken-" + BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
