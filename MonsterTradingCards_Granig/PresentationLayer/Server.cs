using MonsterTradingCards_Granig.RoutingLayer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.PresentationLayer
{
    public class Server
    {
        private readonly TcpListener _listener;

        public Server()
        {
            _listener = new TcpListener(IPAddress.Any, 8081); // Server auf Port 8081 starten
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server started, listening on port 8081...");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Task.Run(() => HandleClient(client)); // Startet einen neuen Thread für jeden Client
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"Received: \n{request}");


                    // Request zerlegen
                    string[] lines = request.Split("\r\n");
                    string firstLine = lines[0]; // z.B. "POST /users HTTP/1.1"
                    string[] parts = firstLine.Split(' ');

                    if (parts.Length < 3)
                    {
                        // Falls die Anfrage nicht korrekt ist
                        SendResponse(stream, "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nInvalid request");
                        return;
                    }

                    string method = parts[0]; // "POST"
                    string path = parts[1].Trim(); // Entfernt %0A oder Leerzeichen


                    // Den Body der Anfrage extrahieren
                    string body = lines.Length > 1 ? lines[lines.Length - 1] : "";

                    // Anfrage an den Router weiterleiten
                    Router router = new Router();
                    string response = await router.HandleRequest(method, path, body);

                    // Antwort an den Client senden
                    SendResponse(stream, response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close(); // Verbindung nach der Verarbeitung schließen
            }

        }
        //Method to send HTTP responses
        private static void SendResponse(NetworkStream stream, string response)
        {
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
            stream.Flush();
        }
    }
}
