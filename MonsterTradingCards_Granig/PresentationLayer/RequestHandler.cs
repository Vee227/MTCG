using MonsterTradingCards_Granig;
using System.Text.Json;
using MonsterTradingCards_Granig.BusinessLayer.Models;
using MonsterTradingCards_Granig.DataLayer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MonsterTradingCards_Granig.PresentationLayer
{
    public class RequestHandler
    {
        private readonly List<User> users = new();
        private readonly CardRepository cardRepository = new CardRepository();

        public async Task<string> HandleRequest(string httpMethod, string request, string? requestBody, Dictionary<string, string> headers)
        {
            Console.WriteLine("DEBUG: Eingehende Anfrage");
            Console.WriteLine($"Methode: {httpMethod}");
            Console.WriteLine($"Request: {request}");
            Console.WriteLine($"Raw Body: {requestBody}");

            if (httpMethod == "POST" && request == "/login")
            {
                return HandleLogin(requestBody);
            }
            if (httpMethod == "POST" && request == "/register")
            {
                return HandleRegister(requestBody);
            }
            else if (httpMethod == "GET" && request == "/cards")
            {
                return await GetUserCards(headers);
            }
            else if (httpMethod == "POST" && request == "/cards")
            {
                return await AddCard(requestBody, headers);
            }

            return "HTTP/1.1 404 Not Found\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Route not found\"}";
        }

        private string HandleRegister(string? jsonBody)
        {
            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"No data provided\"}";
            }

            var newUser = JsonSerializer.Deserialize<User>(jsonBody);
            if (newUser == null || string.IsNullOrWhiteSpace(newUser.Username) || string.IsNullOrWhiteSpace(newUser.Password))
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid user data\"}";
            }

            if (users.Any(u => u.Username == newUser.Username))
            {
                return "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Username already exists\"}";
            }

            users.Add(newUser);
            return "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User registered successfully\"}";
        }

        private string HandleLogin(string? jsonBody)
        {
            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"No data provided\"}";
            }

            var loginUser = JsonSerializer.Deserialize<User>(jsonBody);
            var user = users.FirstOrDefault(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

            if (user == null)
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid username or password\"}";
            }

            user.Token = $"{user.Username}-mtcgToken";
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{{\"token\": \"{user.Token}\"}}";
        }

        private async Task<string> GetUserCards(Dictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Missing Authorization Header\"}";
            }

            string token = headers["Authorization"];
            string username = ExtractUsernameFromToken(token);

            if (string.IsNullOrEmpty(username))
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid token\"}";
            }

            var cards = await cardRepository.GetCardsByUser(username);
            string jsonResponse = JsonSerializer.Serialize(cards);

            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }

        private async Task<string> AddCard(string? body, Dictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Missing Authorization Header\"}";
            }

            string token = headers["Authorization"];
            string username = ExtractUsernameFromToken(token);

            if (string.IsNullOrEmpty(username))
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid token\"}";
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Request body is empty\"}";
            }

            var cardData = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
            if (!cardData.ContainsKey("Name") || !cardData.ContainsKey("Damage") || !cardData.ContainsKey("ElementType") || !cardData.ContainsKey("CardType"))
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid request format\"}";
            }

            bool success = await cardRepository.AddCard(
                cardData["Name"].ToString(),
                Convert.ToInt32(cardData["Damage"]),
                Convert.ToInt32(cardData["ElementType"]),
                cardData["CardType"].ToString(),
                username
            );

            return success
                ? "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Card added successfully\"}"
                : "HTTP/1.1 500 Internal Server Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Failed to add card\"}";
        }

        private string ExtractUsernameFromToken(string token)
        {
            return token.EndsWith("-mtcgToken") ? token.Replace("-mtcgToken", "") : null;
        }
    }
}
