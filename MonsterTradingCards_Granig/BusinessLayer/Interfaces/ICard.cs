using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig
{
    internal interface ICard
    {
        public string Name {  get; }
        public int Damage { get; }


        int Attack(ElementType elementType, int Damage);
    }
}

