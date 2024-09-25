using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig
{
    internal class Player
    {
        public string Username { get; set; }
        public string Password {  get; set; }
        public List<ICard> CardStack { get; private set; }
        public List<ICard> BestDeck { get; private set; }
        public int Coins { get; private set; }
        private int packageCounter;


        public Player(string username, string password, int packageCounter)
        {
            Username = username;
            Password = password;
            CardStack = new List<ICard>();
            BestDeck = new List<ICard>();
            Coins = 20;
            this.packageCounter = packageCounter;
        }

        public List<ICard> TradeCards(List<ICard> cardStack)
        {
            return cardStack;
        }

        public List<ICard> BuyPackages(int coins, List<ICard> cardStack, int packageCounter)
        {
            return cardStack;
        }

        public void ManageDeck(List<ICard> cardStack, List<ICard> bestDeck)
        {
        }


    }
}
