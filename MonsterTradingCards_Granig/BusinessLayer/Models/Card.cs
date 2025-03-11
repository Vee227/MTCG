using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCards_Granig.BusinessLayer.Models
{
    public class Card
    {
            public int Id { get; set; }
            public required string Name { get; set; }
            public required string Type { get; set; } 
            public required string Element { get; set; }
            public double Damage { get; set; }
            public string? Owner { get; set; } 
            //public int? PackageId { get; set; }

    }
}
