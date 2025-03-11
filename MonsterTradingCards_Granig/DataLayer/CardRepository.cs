using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using MonsterTradingCards_Granig.BusinessLayer.Models;

namespace MonsterTradingCards_Granig.DataLayer
{
    public class CardRepository
    {
        public async Task<bool> CreatePackage(List<Card> cards)
        {
            await using var conn = await DBConn.GetConnection(); // Holt die Verbindung aus DBConn
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Zuerst ein neues Package erstellen
                await using var packageCmd = new NpgsqlCommand(
                    "INSERT INTO packages (sold, created_at) VALUES (FALSE, CURRENT_TIMESTAMP) RETURNING id;",
                    conn, transaction);
                var packageId = (await packageCmd.ExecuteScalarAsync() as long?) ?? 0;

                if (packageId == 0)
                    throw new Exception("Fehler beim Erstellen des Packages");

                // Karten erstellen und Verknüpfung mit dem Package setzen
                foreach (var card in cards)
                {
                    await using var cardCmd = new NpgsqlCommand(
                        "INSERT INTO cards (name, type, element, damage) VALUES (@name, @type, @element, @damage) RETURNING id;",
                        conn, transaction);
                    cardCmd.Parameters.AddWithValue("@name", card.Name);
                    cardCmd.Parameters.AddWithValue("@type", card.Type);
                    cardCmd.Parameters.AddWithValue("@element", card.Element);
                    cardCmd.Parameters.AddWithValue("@damage", card.Damage);

                    var cardId = (await cardCmd.ExecuteScalarAsync() as long?) ?? 0;
                    if (cardId == 0)
                        throw new Exception("Fehler beim Erstellen der Karte");

                    // In die Zwischentabelle eintragen
                    await using var linkCmd = new NpgsqlCommand(
                        "INSERT INTO package_cards (package_id, card_id) VALUES (@package_id, @card_id);",
                        conn, transaction);
                    linkCmd.Parameters.AddWithValue("@package_id", packageId);
                    linkCmd.Parameters.AddWithValue("@card_id", cardId);
                    await linkCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Erstellen des Packages: {ex.Message}");
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}


