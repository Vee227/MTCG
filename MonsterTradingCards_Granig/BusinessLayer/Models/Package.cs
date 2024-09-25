using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig
{
    internal class Package
    {       
        public List<ICard> PackageCards { get; private set; }

        public Package()
        {
            PackageCards = new List<ICard>();
        }
    }
}
