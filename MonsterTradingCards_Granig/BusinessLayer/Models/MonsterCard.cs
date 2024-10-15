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
        public ElementType Element { get; private set; } 

        public MonsterCard(string name, int damage, ElementType element)
        {
            Name = name;
            Damage = damage;
            Element = element;
        }

        public int Attack(ElementType opponentElement, int opponentDamage) //hab ich weiter ausgebaut
        {
            if(this.Element == ElementType.Fire)
            {
                return Damage + 100; 
            }
            else if(this.Element == ElementType.Water)
            {
                return Damage + 70;
            }
            else if(this.Element == ElementType.Normal) {
                return Damage + 20;
            }

            return Damage;


    }
}
