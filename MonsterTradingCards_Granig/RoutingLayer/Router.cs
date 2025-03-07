using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCards_Granig.BusinessLayer.Models;
using MonsterTradingCards_Granig.DataLayer;

namespace MonsterTradingCards_Granig.RoutingLayer
{
    public class Router
    {
        private UserRepository userRepository = new UserRepository();

        public async Task<string> HandleRequest(string method, string path, string body, Dictionary<string, string> headers)
        {
            Console.WriteLine($"DEBUG: Methode={method}, Pfad={path}, Body={body}");

            path = path.Trim().Replace("\n", "").Replace("\r", "").Replace("%0A", "");

            Console.WriteLine("DEBUG: Eingehende Anfrage");
            Console.WriteLine($"Methode: {method}");
            Console.WriteLine($"Request: {path}");
            Console.WriteLine($"Raw Body: {body}");

            if (method == "POST" && path == "/users")
            {
                return await RegisterUser(body);
            }
            else if (method == "POST" && path == "/sessions")
            {
                return await LoginUser(body);
            }
            else if (method == "GET" && path == "/cards")
            {
                return await GetUserCards(headers);
            }
            else if (method == "POST" && path == "/cards")
            {
                return await AddCard(body, headers);
            }

            return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nRoute not found";
        }

        
        private async Task<string> RegisterUser(string body)
        {
            try
            {
                var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (userData == null || !userData.ContainsKey("Username") || !userData.ContainsKey("Password"))
                {
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid request format\"}";
                }

                bool success = await userRepository.RegisterUser(userData["Username"], userData["Password"]);
                return success
                    ? "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User registered successfully\"}"
                    : "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Username already exists\"}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return "HTTP/1.1 500 Internal Server Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"An error occurred\"}";
            }
        }

        private async Task<string> LoginUser(string body)
        {
            try
            {
                var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (userData == null || !userData.ContainsKey("Username") || !userData.ContainsKey("Password"))
                {
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid request format\"}";
                }

                string? token = await userRepository.LoginUser(userData["Username"], userData["Password"]);
                return token != null
                    ? $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{{\"token\": \"{token}\"}}"
                    : "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid username or password";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return "HTTP/1.1 500 Internal Server Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"An error occurred\"}";
            }
        }
        
        private async Task<string> GetUserCards(Dictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Missing Authorization Header\"}";
            }

            string token = headers["Authorization"];
            string username = ExtractUsernameFromToken(token);

            if (username == null)
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid token\"}";
            }

            var cards = await Card.GetCardsByUser(username);
            string jsonResponse = JsonSerializer.Serialize(cards);

            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }

        
        private string ExtractUsernameFromToken(string token)
        {
            if (!token.EndsWith("-mtcgToken"))
                return null;

            return token.Replace("-mtcgToken", "");
        }
        private async Task<string> AddCard(string body, Dictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Missing Authorization Header\"}";
            }

            string token = headers["Authorization"];
            string username = ExtractUsernameFromToken(token);

            if (username == null)
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid token\"}";
            }

            var cardData = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
            if (!cardData.ContainsKey("Name") || !cardData.ContainsKey("Damage") || !cardData.ContainsKey("ElementType") || !cardData.ContainsKey("CardType"))
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid request format\"}";
            }

            bool success = await Card.AddCard(
     ((JsonElement)cardData["Name"]).GetString(),
     ((JsonElement)cardData["Damage"]).GetInt32(),
     ((JsonElement)cardData["ElementType"]).GetString(),
     ((JsonElement)cardData["CardType"]).GetString(),
     username
 );




            return success
                ? "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Card added successfully\"}"
                : "HTTP/1.1 500 Internal Server Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Failed to add card\"}";
        }


    }
}
