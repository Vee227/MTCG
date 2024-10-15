using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig
{
    internal class SpellCard : ICard
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }

        public ElementType Element{ get; private set; } 

        public SpellCard(string name, int damage, ElementType element)
        {
            Name = name;
            Damage = damage;
            Element = element;
        }

        public int Attack(ElementType opponentElement, int opponentDamage) //ebenfalls überarbeitet
        {
            if (opponentElement == ElementType.Fire && this.Element == ElementType.Water)
            {
                return Damage * 2;
            }
            else if (opponentElement == ElementType.Water && this.Element == ElementType.Fire)
            {
                return Damage / 2;
            }

            return Damage;
        }

    }
}
