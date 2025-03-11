using MonsterTradingCards_Granig.BusinessLayer.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.DataLayer
{
    class DeckRepository
    {
        //****************************Show the deck of a user**********************************
        public async Task<List<Card>> GetDeck(string username)
        {
            List<Card> deck = new List<Card>();

            try
            {
                await using var conn = await DBConn.GetConnection();
                string query = @"
                                SELECT c.id, c.name, c.type, c.element, c.damage 
                                FROM cards c
                                INNER JOIN deck d ON c.id = d.card_id
                                INNER JOIN users u ON d.user_id = u.id
                                WHERE u.username = @username;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    deck.Add(new Card
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Type = reader.GetString(2),
                        Element = reader.GetString(3),
                        Damage = reader.GetDouble(4)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen des Decks: {ex.Message}");
            }

            return deck;
        }


        //****************************User chooses their deck**********************************
        public async Task<bool> SetDeck(string username, List<int> cardIds)
        {
            await using var conn = await DBConn.GetConnection();

            var userCmd = new NpgsqlCommand("SELECT id FROM users WHERE username = @username", conn);
            userCmd.Parameters.AddWithValue("@username", username);
            var userId = await userCmd.ExecuteScalarAsync();

            if (userId == null)
            {
                Console.WriteLine("ERROR: Benutzer nicht gefunden.");
                return false;
            }

            int userIdValue = Convert.ToInt32(userId);

            var checkCmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM cards WHERE id = ANY(@cardIds::INTEGER[])",
                conn
            );
            checkCmd.Parameters.AddWithValue("@cardIds", cardIds);
            checkCmd.Parameters.AddWithValue("@userId", userIdValue);

            var result = await checkCmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
            {
                Console.WriteLine("ERROR: Kein gültiges Ergebnis für die Kartenüberprüfung erhalten.");
                return false;
            }

            long count = (long)result;


            if (count != 4)
            {
                Console.WriteLine("ERROR: Nicht alle Karten gehören dem Benutzer.");
                return false;
            }

            var deleteCmd = new NpgsqlCommand("DELETE FROM deck WHERE user_id = @userId", conn);
            deleteCmd.Parameters.AddWithValue("@userId", userIdValue);
            await deleteCmd.ExecuteNonQueryAsync();

            foreach (var cardId in cardIds)
            {
                var insertCmd = new NpgsqlCommand(
                    "INSERT INTO deck (user_id, card_id) VALUES (@userId, @cardId)",
                    conn
                );
                insertCmd.Parameters.AddWithValue("@userId", userIdValue);
                insertCmd.Parameters.AddWithValue("@cardId", cardId);
                await insertCmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine($"DEBUG: Deck erfolgreich für Benutzer {username} aktualisiert.");
            return true;
        }
    }
}
