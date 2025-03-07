using MonsterTradingCards_Granig.DataLayer;

namespace MonsterTradingCards_Granig.PresentationLayer
{


    public class Program
    {
        
        public static async Task/*void*/ Main(string[] args)
        {
            DBConn.Connection();

            var server = new Server();
            server.Start();
        }

    }

}