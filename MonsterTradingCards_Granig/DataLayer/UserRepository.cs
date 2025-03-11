using System;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCards_Granig.DataLayer
{
    public class UserRepository
    {
        //private const string ConnectionString = "Host=localhost;Port=5432;Username=admin;Password=supersecure;Database=postgres";

        //****************************Register User**********************************
        public async Task<bool> RegisterUser(string username, string password, string name = "", string bio = "", string image = "")
        {
            await using var conn = await DBConn.GetConnection();

            await using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", conn);
            checkCmd.Parameters.AddWithValue("@username", username);
            var count = (await checkCmd.ExecuteScalarAsync() as long?) ?? 0;


            if (count > 0)
                return false;

            await using var insertCmd = new NpgsqlCommand(@"
            INSERT INTO users (username, password, name, bio, image, coins, elo, games_played) 
            VALUES (@username, @password, @name, @bio, @image, 20, 100, 0)", conn);

            Console.WriteLine($"DEBUG: Username={username}, Password={password}, Name={name}, Bio={bio}, Image={image}");


            insertCmd.Parameters.AddWithValue("@username", username);
            insertCmd.Parameters.AddWithValue("@password", password);
            insertCmd.Parameters.AddWithValue("@name", name);
            insertCmd.Parameters.AddWithValue("@bio", bio);
            insertCmd.Parameters.AddWithValue("@image", image);

            await insertCmd.ExecuteNonQueryAsync();
            return true;
        }

        //****************************Get ALL Users**********************************
        public async Task<List<Dictionary<string, object>>> GetAllUsers()
        {
            List<Dictionary<string, object>> users = new();

            await using var conn = await DBConn.GetConnection();

            await using var cmd = new NpgsqlCommand("SELECT id, username, coins, elo, games_played, bio, image, name FROM users", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new Dictionary<string, object>
        {
            { "id", reader.GetInt32(0) },
            { "username", reader.GetString(1) },
            { "coins", reader.GetInt32(2) },
            { "elo", reader.GetInt32(3) },
            { "games_played", reader.GetInt32(4) },
            { "bio", reader.IsDBNull(5) ? DBNull.Value : reader.GetString(5) },
            { "image", reader.IsDBNull(6) ? DBNull.Value : reader.GetString(6) },
            { "name", reader.IsDBNull(7) ? DBNull.Value : reader.GetString(7) }

        });
            }

            return users;
        }


        //****************************Update User**********************************
        public async Task<bool> UpdateUserProfile(string username, string token, string name, string bio, string image, string password)
        {
            if (token != $"{username}-mtcgToken")
            {
                Console.WriteLine("ERROR: Ungültiger Token für Update!");
                return false;
            }

            await using var conn = await DBConn.GetConnection();

            var query = new StringBuilder("UPDATE users SET ");
            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrEmpty(name))
            {
                query.Append("name = @name, ");
                parameters.Add(new NpgsqlParameter("@name", name));
            }
            if (!string.IsNullOrEmpty(bio))
            {
                query.Append("bio = @bio, ");
                parameters.Add(new NpgsqlParameter("@bio", bio));
            }
            if (!string.IsNullOrEmpty(image))
            {
                query.Append("image = @image, ");
                parameters.Add(new NpgsqlParameter("@image", image));
            }
            if (!string.IsNullOrEmpty(password))
            {
                query.Append("password = @password, ");
                parameters.Add(new NpgsqlParameter("@password", password));
            }

            if (parameters.Count == 0)
            {
                Console.WriteLine("ERROR: Keine gültigen Daten zum Updaten!");
                return false;
            }

            query.Length -= 2; 
            query.Append(" WHERE username = @username");
            parameters.Add(new NpgsqlParameter("@username", username));

            await using var cmd = new NpgsqlCommand(query.ToString(), conn);
            cmd.Parameters.AddRange(parameters.ToArray());

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }


        //****************************Delete User (nur als Admin)**********************************
        public async Task<bool> DeleteUser(string username)
        {
            await using var conn = await DBConn.GetConnection();

            await using var cmd = new NpgsqlCommand("DELETE FROM users WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }



        //****************************Eigene Profildaten abrufen**********************************
        public async Task<Dictionary<string, object>?> GetUser(string username)
        {
            await using var conn = await DBConn.GetConnection();

            var query = "SELECT username, coins, elo, games_played, bio, image, name FROM users WHERE username = @username";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Dictionary<string, object>
                {
                    { "username", reader.GetString(0) },
                    { "coins", reader.GetInt32(1) },
                    { "elo", reader.GetInt32(2) },
                    { "games_played", reader.GetInt32(3) },
                    { "bio", reader.IsDBNull(4) ? DBNull.Value : reader.GetString(4) },
                    { "image", reader.IsDBNull(5) ? DBNull.Value : reader.GetString(5) },
                    { "name", reader.IsDBNull(6) ? DBNull.Value : reader.GetString(6) }

                };

            }

            return null;
        }


        //****************************Login User**********************************
        public async Task<string?> LoginUser(string username, string password)
        {
            await using var conn = await DBConn.GetConnection();
            
            var query = "SELECT password FROM users WHERE username = @username";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string storedPassword = reader.GetString(0);
                if (storedPassword == password)
                {
                    return $"{username}-mtcgToken";
                }
            }

            return null;
        }


    }
}
