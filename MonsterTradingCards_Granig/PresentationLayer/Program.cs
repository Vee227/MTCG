using MonsterTradingCards_Granig.PresentationLayer;

class Program
{
    static async Task Main()
    {
        Server server = new Server();
        await server.Start();
    }
}
