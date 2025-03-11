using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using MonsterTradingCards_Granig.BusinessLayer.Models;

namespace MonsterTradingCards_Granig.DataLayer
{
        public class CardRepository
        {

            //****************************Admin creates a package**********************************
            public async Task<bool> CreatePackage(List<Card> cards)
            {
                await using var conn = await DBConn.GetConnection();
                await using var transaction = await conn.BeginTransactionAsync();

                try
                {
                    Console.WriteLine("DEBUG: Starte das Erstellen eines neuen Packages...");

                    await using var packageCmd = new NpgsqlCommand(
                        "INSERT INTO packages (sold, created_at) VALUES (FALSE, CURRENT_TIMESTAMP) RETURNING id;",
                        conn, transaction);

                    var packageResult = await packageCmd.ExecuteScalarAsync();
                    if (packageResult == null || packageResult is DBNull)
                    {
                        Console.WriteLine("ERROR: Package konnte nicht erstellt werden.");
                        throw new Exception("Fehler beim Erstellen des Packages");
                    }

                    int packageId = (int)packageResult;
                    Console.WriteLine($"DEBUG: Erstellt Package mit ID = {packageId}");

                    foreach (var card in cards)
                    {
                        Console.WriteLine($"DEBUG: Füge Karte ein: Name={card.Name}, PackageID={packageId}");

                        await using var cardCmd = new NpgsqlCommand(
                            "INSERT INTO cards (name, type, element, damage, owner) VALUES (@name, @type, @element, @damage, NULL) RETURNING id;",
                            conn, transaction);

                        cardCmd.Parameters.AddWithValue("@name", card.Name);
                        cardCmd.Parameters.AddWithValue("@type", card.Type);
                        cardCmd.Parameters.AddWithValue("@element", card.Element);
                        cardCmd.Parameters.AddWithValue("@damage", card.Damage);

                        var cardResult = await cardCmd.ExecuteScalarAsync();

                        if (cardResult == null || cardResult is DBNull)
                        {
                            Console.WriteLine("ERROR: Karte konnte nicht erstellt werden.");
                            throw new Exception("Fehler beim Erstellen der Karte");
                        }

                        int cardId = (int)cardResult;
                        Console.WriteLine($"DEBUG: Erstellt Karte mit ID = {cardId}");

                        await using var linkCmd = new NpgsqlCommand(
                            "INSERT INTO package_cards (package_id, card_id) VALUES (@packageId, @cardId);",
                            conn, transaction);

                        linkCmd.Parameters.AddWithValue("@packageId", packageId);
                        linkCmd.Parameters.AddWithValue("@cardId", cardId);

                        await linkCmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"DEBUG: Karte {cardId} mit Package {packageId} verknüpft");
                    }

                    await transaction.CommitAsync();
                    Console.WriteLine("DEBUG: Package erfolgreich erstellt und Karten zugeordnet.");
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"ERROR: {ex.Message}");
                    return false;
                }
            }

        //****************************Buy a package**********************************
        public async Task<bool> BuyPackage(string username)
        {
            await using var conn = await DBConn.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                await using var checkCmd = new NpgsqlCommand(
                    "SELECT coins FROM users WHERE username = @username FOR UPDATE;",
                    conn, transaction);
                checkCmd.Parameters.AddWithValue("@username", username);

                object? result = await checkCmd.ExecuteScalarAsync();
                if (result == null || Convert.ToInt32(result) < 5)
                {
                    Console.WriteLine("ERROR: Nicht genug Coins!");
                    return false;
                }

                await using var packageCmd = new NpgsqlCommand(
                    "SELECT id FROM packages WHERE sold = FALSE ORDER BY created_at ASC LIMIT 1 FOR UPDATE;",
                    conn, transaction);

                object? packageResult = await packageCmd.ExecuteScalarAsync();
                if (packageResult == null)
                {
                    Console.WriteLine("ERROR: Keine verfügbaren Packages!");
                    return false;
                }

                int packageId = Convert.ToInt32(packageResult);

                await using var updatePackageCmd = new NpgsqlCommand(
                    "UPDATE packages SET sold = TRUE WHERE id = @packageId;",
                    conn, transaction);
                updatePackageCmd.Parameters.AddWithValue("@packageId", packageId);
                await updatePackageCmd.ExecuteNonQueryAsync();

                await using var updateCardsCmd = new NpgsqlCommand(
                    "UPDATE cards SET owner = @username WHERE id IN (SELECT card_id FROM package_cards WHERE package_id = @packageId);",
                    conn, transaction);
                updateCardsCmd.Parameters.AddWithValue("@username", username);
                updateCardsCmd.Parameters.AddWithValue("@packageId", packageId);
                await updateCardsCmd.ExecuteNonQueryAsync();

                await using var updateCoinsCmd = new NpgsqlCommand(
                    "UPDATE users SET coins = coins - 5 WHERE username = @username;",
                    conn, transaction);
                updateCoinsCmd.Parameters.AddWithValue("@username", username);
                await updateCoinsCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                Console.WriteLine("DEBUG: Package erfolgreich gekauft!");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"ERROR: {ex.Message}");
                return false;
            }
        }

        

              

    }
}




