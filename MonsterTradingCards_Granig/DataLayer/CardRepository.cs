using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using MonsterTradingCards_Granig.BusinessLayer.Models; 

namespace MonsterTradingCards_Granig.DataLayer
{
    public class CardRepository
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=admin;Password=supersecure;Database=postgres";

        public async Task<List<Card>> GetCardsByUser(string username)
        {
            List<Card> cards = new List<Card>();

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT c.id, c.name, c.damage, c.element_type, c.card_type 
                    FROM cards c 
                    JOIN users u ON c.owner_id = u.id 
                    WHERE u.username = @username";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var card = new Card(
                                reader.GetInt32(0), 
                                reader.GetString(1), 
                                reader.GetInt32(2), 
                                reader.GetString(3), 
                                reader.GetString(4), 
                                reader.GetInt32(5)  
                            );
                            cards.Add(card);
                        }

                    }
                }
            }
            return cards;
        }

        public async Task<bool> AddCard(string name, int damage, int elementType, string cardType, string username)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                var userIdQuery = "SELECT id FROM users WHERE username = @username";
                int ownerId;

                using (var cmd = new NpgsqlCommand(userIdQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null) return false;
                    ownerId = Convert.ToInt32(result);
                }

                var insertQuery = "INSERT INTO cards (name, damage, element_type, card_type, owner_id) VALUES (@name, @damage, @elementType, @cardType, @ownerId)";

                using (var cmd = new NpgsqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@damage", damage);
                    cmd.Parameters.AddWithValue("@elementType", elementType);
                    cmd.Parameters.AddWithValue("@cardType", cardType);
                    cmd.Parameters.AddWithValue("@ownerId", ownerId);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}
