using System;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCards_Granig.DataLayer
{
    public static class DBConn
    {
        private static readonly string ConnectionString =
            "Host=localhost;Port=5432;Username=admin;Password=supersecure;Database=postgres";

        public static async Task<NpgsqlConnection> GetConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                await connection.OpenAsync();
                return connection; // Verbindung wird geöffnet zurückgegeben
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der DB-Verbindung: {ex.Message}");
                throw; // Fehler weitergeben, damit er nicht untergeht
            }
        }
    }
}
