using System;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCards_Granig.DataLayer
{
    public class UserRepository
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=admin;Password=supersecure;Database=postgres";

        public async Task<bool> RegisterUser(string username, string password)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            await using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", conn);
            checkCmd.Parameters.AddWithValue("@username", username);
            var count = (long)await checkCmd.ExecuteScalarAsync();

            if (count > 0)
                return false; 

            await using var insertCmd = new NpgsqlCommand("INSERT INTO users (username, password) VALUES (@username, @password)", conn);
            insertCmd.Parameters.AddWithValue("@username", username);
            insertCmd.Parameters.AddWithValue("@password", password); 

            await insertCmd.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<string?> LoginUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT password FROM users WHERE username = @username";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string storedPassword = reader.GetString(0);
                            if (storedPassword == password) 
                            {
                                return $"{username}-mtcgToken"; 
                            }
                        }
                    }
                }
            }
            return null; 
        }

    }
}
