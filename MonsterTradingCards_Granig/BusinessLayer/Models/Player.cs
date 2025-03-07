using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCards_Granig.BusinessLayer.Models;

namespace MonsterTradingCards_Granig
{
    internal class Player
    {
        public string Username { get; set; }
        public string Password {  get; set; }
        public List<Card> CardStack { get; private set; }
        public List<Card> BestDeck { get; private set; }
        public int Coins { get; private set; }
        private int packageCounter;


        public Player(string username, string password, int packageCounter)
        {
            Username = username;
            Password = password;
            CardStack = new List<Card>();
            BestDeck = new List<Card>();
            Coins = 20;
            this.packageCounter = packageCounter;
        }

        public List<Card> TradeCards(List<Card> cardStack)
        {
            return cardStack;
        }

        public List<Card> BuyPackages(int coins, List<Card> cardStack, int packageCounter)
        {
            return cardStack;
        }

        public void ManageDeck(List<Card> cardStack, List<Card> bestDeck)
        {
        }


    }
}
