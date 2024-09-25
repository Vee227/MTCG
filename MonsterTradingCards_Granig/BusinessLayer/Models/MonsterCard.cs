using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig
{
    internal class MonsterCard : ICard
    {
        public string Name {  get; private set; }
        public int Damage { get; private set; }

        public MonsterCard(string name, int damage)
        {
            Name = name;
            Damage = damage;
        }

        public int Attack(ElementType elementType, int damage)
        {
            return damage;
        }

    }
}
