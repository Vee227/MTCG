using MonsterTradingCards_Granig;
using System.Text.Json;
using MonsterTradingCards_Granig.BusinessLayer.Models;


namespace MonsterTradingCards_Granig.PresentationLayer
{
    // Diese Klasse verarbeitet die eingehenden Anfragen und gibt entsprechende Antworten zurück
    public class RequestHandler()
    {
        private readonly List<User> users = new();
        
        public string HandleRequest(string request, string httpMethod, string? requestBody)
        {
            {
                if (httpMethod == "POST" && request == "/login")
                { 
                    return HandleLogin(requestBody);
                }

                if (httpMethod == "POST" && request == "/register")
                {
                    return HandleRegister(requestBody);
                }

                return "Unknown request.";
            }
        }

        //Methode, um eine Registrierung zu verwalten
        private string HandleRegister(string? jsonBody)
        {
            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                return "Error: No data provided.";
            }

            var newUser = JsonSerializer.Deserialize<User>(jsonBody);
            if (newUser == null || string.IsNullOrWhiteSpace(newUser.Username) || string.IsNullOrWhiteSpace(newUser.Password))
            {
                return "Error: Invalid user data.";
            }

            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Username == newUser.Username)
                {
                    return "Error: Username already exists.";
                }
            }


            users.Add(newUser);
            return $"User '{newUser.Username}' registered successfully.";
        }


        // Methode, um das Login zu verwalten
        private string HandleLogin(string? jsonBody)
        {
            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                return "Error: No data provided.";
            }

            var loginUser = JsonSerializer.Deserialize<User>(jsonBody);
            var user = users.FirstOrDefault(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

            if (user == null)
            {
                return "Error: Invalid username or password.";
            }

            user.Token = GenerateToken();
            return $"Login successful. Token: {user.Token}";
        }

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}