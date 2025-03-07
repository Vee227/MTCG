using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCards_Granig.DataLayer
{
    public class UserRepository
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=admin;Password=supersecure;Database=MTCG_DB";

        // Nutzer registrieren
        public async Task<bool> RegisterUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                var query = "INSERT INTO Users (username, password) VALUES (@username, @password)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false; // Falls Nutzername schon existiert
                    }
                }
            }
        }

        // Login prüfen & Token verwalten
        public async Task<string?> LoginUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT password, token FROM Users WHERE username = @username";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string storedPassword = reader.GetString(0);
                            string? existingToken = reader.IsDBNull(1) ? null : reader.GetString(1);

                            if (storedPassword == password)
                            {
                                if (!string.IsNullOrEmpty(existingToken))
                                    return existingToken; // Falls Token existiert, zurückgeben

                                // Neuen Token generieren & speichern
                                string newToken = $"{username}-mtcgToken";
                                await SaveToken(username, newToken);
                                return newToken;
                            }
                        }
                    }
                }
            }
            return null; // Login fehlgeschlagen
        }

        // Speichert den Token in der DB
        private async Task SaveToken(string username, string token)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                var query = "UPDATE Users SET token = @token WHERE username = @username";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@token", token);
                    cmd.Parameters.AddWithValue("@username", username);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Gibt Token zurück (falls existiert)
        public async Task<string?> GetToken(string username)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT token FROM Users WHERE username = @username";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    return await cmd.ExecuteScalarAsync() as string;
                }
            }
        }
    }
}

