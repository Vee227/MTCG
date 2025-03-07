using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCards_Granig.BusinessLayer.Controller;
using MonsterTradingCards_Granig.DataLayer;

namespace MonsterTradingCards_Granig.RoutingLayer
{
    public class Router
    {
        private UserRepository userRepository = new UserRepository();

        public async Task<string> HandleRequest(string method, string path, string body)
        {
            path = path.Trim().Replace("\n", "").Replace("\r", "");
            path = path.Replace("%0A", "");


            Console.WriteLine("DEBUG: Eingehende Anfrage");
            Console.WriteLine($"Methode: {method}");
            Console.WriteLine($"Request: {path}");
            Console.WriteLine($"Raw Body: {body}");
            if (method == "POST" && path == "/users")
            {
                var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (!userData.ContainsKey("Username") || !userData.ContainsKey("Password"))
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nInvalid request format";

                bool success = await userRepository.RegisterUser(userData["Username"], userData["Password"]);
                return success
                    ? "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User registered successfully\"}"
                    : "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Username already exists\"}";
            }
            else if (method == "POST" && path == "/sessions")
            {
                var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (!userData.ContainsKey("Username") || !userData.ContainsKey("Password"))
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nInvalid request format";

                string? token = await userRepository.LoginUser(userData["Username"], userData["Password"]);
                return token != null
                    ? $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{{\"token\": \"{token}\"}}"
                    : "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid username or password";
            }

            return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nRoute not found";
        }
    }
}
