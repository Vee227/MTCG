using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.BusinessLayer.Models
{
    public class Deck
    {
        public string Username { get; set; } 
        public List<Guid> CardIds { get; set; } 

        public Deck(string username, List<Guid> cardIds)
        {
            Username = username;
            CardIds = cardIds;
        }
    }
}
