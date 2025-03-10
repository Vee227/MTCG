using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.BusinessLayer.Models
{
    public class Stack
    {
        public int UserId { get; set; }
        public List<Guid> CardIds { get; set; } = new List<Guid>();
    }
}

