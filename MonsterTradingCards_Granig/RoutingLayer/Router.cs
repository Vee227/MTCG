using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCards_Granig.BusinessLayer.Models;
using MonsterTradingCards_Granig.DataLayer;
using Npgsql;

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
            else if (method == "GET" && path == "/users")
            {
                return await GetUsers();
            }
            else if (method == "PUT" && path.StartsWith("/users/"))
            {
                return await UpdateUserProfile(path, headers, body);
            }
            else if (method == "DELETE" && path.StartsWith("/users/"))
            {
                return await DeleteUser(path, headers);
            }
            else if (method == "GET" && path.StartsWith("/users/"))
            {
                return await GetUserProfile(path, headers);
            }
            else if (method == "POST" && path == "/sessions")
            {
                return await LoginUser(body);
            }
            else if (method == "POST" && path == "/packages")
            {
                return await CreatePackage(body, headers);
            }
            else if (method == "POST" && path == "/transactions/packages")
            {
                return await BuyPackage(headers);
            }
            else if (method == "GET" && path == "/deck")
            {
                return await GetDeck(headers);
            }
            else if (method == "PUT" && path == "/deck")
            {
                return await SetDeck(headers, body);
            }
            else if (method == "GET" && path == "/stack")
            {
                return await GetUserStack(headers);
            }
            



            return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nRoute not found";
        }

    //--------------------------------------------ALLES ZU USERS---------------------------------------------
    
        //****************************Register User**********************************
        private async Task<string> RegisterUser(string body)
        {
            try
            {
                var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (userData == null || !userData.ContainsKey("Username") || !userData.ContainsKey("Password"))
                {
                    return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid request format\"}";
                }

                Console.WriteLine($"DEBUG: Geparste Werte - Username={userData["Username"]}, Name={userData.GetValueOrDefault("Name", "")}, Bio={userData.GetValueOrDefault("Bio", "")}, Image={userData.GetValueOrDefault("Image", "")}");

                bool success = await userRepository.RegisterUser(
                    userData["Username"],
                    userData["Password"],
                    userData.GetValueOrDefault("Name", ""),
                    userData.GetValueOrDefault("Bio", ""),
                    userData.GetValueOrDefault("Image", "")
                );

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


        //****************************Get ALL Users**********************************
        private async Task<string> GetUsers()
        {
            try
            {
                var users = await userRepository.GetAllUsers();
                string jsonResponse = JsonSerializer.Serialize(users);

                return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return "HTTP/1.1 500 Internal Server Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Internal Server Error\"}";
            }
        }

        //****************************Update User**********************************
        private async Task<string> UpdateUserProfile(string path, Dictionary<string, string> headers, string body)
        {
            string usernameToUpdate = path.Substring(7);

            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Authorization header missing\"}";
            }

            string token = headers["Authorization"].Replace("Bearer ", "").Trim();

            var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            if (userData == null)
            {
                return "HTTP/1.1 400 Bad Request\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid JSON format\"}";
            }

            bool success = await userRepository.UpdateUserProfile(
                usernameToUpdate, token,
                userData.GetValueOrDefault("Name", ""),
                userData.GetValueOrDefault("Bio", ""),
                userData.GetValueOrDefault("Image", ""),
                userData.GetValueOrDefault("Password", "")
            );

            if (!success)
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid token or update failed\"}";
            }

            return "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User updated successfully\"}";
        }


        //****************************Delete User (nur als Admin)**********************************
        private async Task<string> DeleteUser(string path, Dictionary<string, string> headers)
        {
            string usernameToDelete = path.Substring(7);

            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Authorization header missing\"}";
            }

            string token = headers["Authorization"].Replace("Bearer ", "").Trim();

            if (token != "admin-mtcgToken")
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Only admin can delete users\"}";
            }

            bool success = await userRepository.DeleteUser(usernameToDelete);
            if (!success)
            {
                return "HTTP/1.1 404 Not Found\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User not found\"}";
            }

            return "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User deleted successfully\"}";
        }


        //****************************Eigene Profildaten abrufen**********************************
        private async Task<string> GetUserProfile(string path, Dictionary<string, string> headers)
        {
            string usernameToFetch = path.Substring(7);

            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Authorization header missing\"}";
            }

            string token = headers["Authorization"].Replace("Bearer ", "").Trim();

            if (token != $"{usernameToFetch}-mtcgToken")
            {
                return "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"You can only access your own profile\"}";
            }

            var user = await userRepository.GetUser(usernameToFetch);
            if (user == null)
            {
                return "HTTP/1.1 404 Not Found\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User not found\"}";
            }

            string jsonResponse = JsonSerializer.Serialize(user);
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
        }


        //****************************Login User**********************************
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


        //--------------------------------------------ALLES ZU CARDS UND PACKAGES----------------------------------------------


        //****************************Create a Package**********************************
        
        public async Task<string> CreatePackage(string body, Dictionary<string, string> headers)
        {
            try
            {
                if (!headers.ContainsKey("Authorization") || headers["Authorization"] != "Bearer admin-mtcgToken")
                {
                    return "HTTP/1.1 403 Forbidden\r\n\r\n{\"message\": \"Only admins can create packages\"}";
                }

                var cards = JsonSerializer.Deserialize<List<Card>>(body);

                if (cards == null || cards.Count != 5)
                {
                    return "HTTP/1.1 400 Bad Request\r\n\r\n{\"message\": \"A package must contain exactly 5 cards\"}";
                }

                var cardRepo = new CardRepository();
                bool success = await cardRepo.CreatePackage(cards);


                return success
                    ? "HTTP/1.1 201 Created\r\n\r\n{\"message\": \"Package created successfully\"}"
                    : "HTTP/1.1 500 Internal Server Error\r\n\r\n{\"message\": \"Failed to create package\"}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FEHLER: {ex.Message}");
                return "HTTP/1.1 500 Internal Server Error\r\n\r\n{\"message\": \"An error occurred\"}";
            }
        }


        //****************************User Buys Package**********************************
        private async Task<string> BuyPackage(Dictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\n\r\n{\"message\": \"Authorization header missing\"}";
            }

            string token = headers["Authorization"].Replace("Bearer ", "").Trim();
            string username = token.Replace("-mtcgToken", "");

            var cardRepo = new CardRepository();
            bool success = await cardRepo.BuyPackage(username);

            return success
                ? "HTTP/1.1 201 Created\r\n\r\n{\"message\": \"Package successfully purchased\"}"
                : "HTTP/1.1 400 Bad Request\r\n\r\n{\"message\": \"Package purchase failed\"}";
        }


        //****************************Show the deck of a user**********************************
        private async Task<string> GetDeck(Dictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authorization"))
                return "HTTP/1.1 401 Unauthorized\r\n\r\n{\"message\": \"Authorization header missing\"}";

            string token = headers["Authorization"].Replace("Bearer ", "").Trim();
            string username = token.Replace("-mtcgToken", "");

            var deckRepo = new DeckRepository();
            List<Card> deck = await deckRepo.GetDeck(username);

            if (deck == null || deck.Count == 0)
                return "HTTP/1.1 200 OK\r\n\r\n{\"message\": \"Deck is empty\"}";

            string jsonDeck = JsonSerializer.Serialize(deck);
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonDeck}";
        }


        //****************************User manages their deck**********************************
        private async Task<string> SetDeck(Dictionary<string, string> headers, string body)
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return "HTTP/1.1 401 Unauthorized\r\n\r\n{\"message\": \"Authorization header missing\"}";
            }

            string token = headers["Authorization"].Replace("Bearer ", "").Trim();
            string username = token.Replace("-mtcgToken", "");

            List<int>? cardIds;
            try
            {
                cardIds = JsonSerializer.Deserialize<List<int>>(body);
            }
            catch
            {
                return "HTTP/1.1 400 Bad Request\r\n\r\n{\"message\": \"Invalid request body\"}";
            }

            if (cardIds == null || cardIds.Count != 4)
            {
                return "HTTP/1.1 400 Bad Request\r\n\r\n{\"message\": \"Deck must contain exactly 4 cards\"}";
            }

            var deckRepo = new DeckRepository();
            bool success = await deckRepo.SetDeck(username, cardIds);

            return success
                ? "HTTP/1.1 200 OK\r\n\r\n{\"message\": \"Deck updated successfully\"}"
                : "HTTP/1.1 400 Bad Request\r\n\r\n{\"message\": \"Failed to update deck\"}";
        }



        //****************************Show the whole Stack of a User**********************************
        private async Task<string> GetUserStack(Dictionary<string, string> headers)
        {
            if (!headers.ContainsKey("Authorization"))
                return "HTTP/1.1 401 Unauthorized\r\n\r\n{\"message\": \"Authorization header missing\"}";

            string token = headers["Authorization"].Replace("Bearer ", "").Trim();
            string username = token.Replace("-mtcgToken", "");

            var stackRepo = new StackRepository();
            var stack = await stackRepo.GetUserStack(username);

            if (stack == null || stack.Count == 0)
                return "HTTP/1.1 200 OK\r\n\r\n{\"message\": \"User stack is empty\"}";

            string jsonStack = JsonSerializer.Serialize(stack);
            return $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonStack}";
        }

       
    }
}
