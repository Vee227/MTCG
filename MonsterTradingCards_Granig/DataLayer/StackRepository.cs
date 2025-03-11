using MonsterTradingCards_Granig.BusinessLayer.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.DataLayer
{
    class StackRepository
    {
        //****************************Show the whole Stack of a User**********************************
        public async Task<List<Card>> GetUserStack(string username)
        {
            var stack = new List<Card>();

            await using var conn = await DBConn.GetConnection();
            await using var cmd = new NpgsqlCommand(
                "SELECT id, name, type, element, damage FROM cards WHERE owner = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                stack.Add(new Card
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Element = reader.GetString(3),
                    Damage = reader.GetFloat(4)
                });
            }

            return stack;
        }
    }
}
