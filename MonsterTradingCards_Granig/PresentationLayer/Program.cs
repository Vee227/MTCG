using MonsterTradingCards_Granig.PresentationLayer;
namespace InterfVSAbstVCompDemo.PresentationLayer;

public class Program
{
    public static void Main(string[] args)
    {
        
        // Startet den TCP-Server, um auf Verbindungen zu lauschen.
        var server = new Server();
        server.Start();
    }
}