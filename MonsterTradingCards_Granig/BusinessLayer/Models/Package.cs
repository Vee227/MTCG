using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCards_Granig.BusinessLayer.Models;

namespace MonsterTradingCards_Granig
{
    internal class Package
    {       
        public List<Card> PackageCards { get; private set; }

        public Package()
        {
            PackageCards = new List<Card>();
        }
    }
}
