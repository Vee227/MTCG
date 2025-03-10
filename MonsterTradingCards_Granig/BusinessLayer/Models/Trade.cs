using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.BusinessLayer.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OfferedCardId { get; set; }
        public string RequestedType { get; set; }
        public int RequestedMinDamage { get; set; }

        public Trade(int id, int userId, int offeredCardId, string requestedType, int requestedMinDamage)
        {
            Id = id;
            UserId = userId;
            OfferedCardId = offeredCardId;
            RequestedType = requestedType;
            RequestedMinDamage = requestedMinDamage;
        }
    }
}
