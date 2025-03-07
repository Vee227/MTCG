using MonsterTradingCards_Granig.RoutingLayer;
using System;
using System.Collections.Generic;
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
            _listener = new TcpListener(IPAddress.Any, 10001);
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server started, listening on port 10001...");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Task.Run(() => HandleClient(client));
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"Received Request: \n{request}");


                    string[] lines = request.Split("\r\n");
                    if (lines.Length == 0)
                    {
                        SendResponse(stream, "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nInvalid request");
                        return;
                    }

                    string firstLine = lines[0]; 
                    string[] parts = firstLine.Split(' ');

                    if (parts.Length < 2)
                    {
                        SendResponse(stream, "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nInvalid request format");
                        return;
                    }

                    string method = parts[0]; 
                    string path = parts[1].Trim(); 

                  
                    Dictionary<string, string> headers = ExtractHeaders(lines);
                    string body = ExtractRequestBody(lines);

                
                    Router router = new Router();
                    string response = await router.HandleRequest(method, path, body, headers);

                    if (string.IsNullOrEmpty(response))
                    {
                        Console.WriteLine("Fehler: Keine Antwort vom Router! Sende 500-Fehler.");
                        response = "HTTP/1.1 500 Internal Server Error\r\nContent-Type: text/plain\r\n\r\nServer Error: No response generated";
                    }

                    SendResponse(stream, response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private static string ExtractRequestBody(string[] requestLines)
        {
            int jsonStartIndex = Array.FindIndex(requestLines, line => line.StartsWith("{"));
            if (jsonStartIndex != -1)
            {
                return string.Join("\n", requestLines[jsonStartIndex..]);
            }
            return "";
        }

        private static Dictionary<string, string> ExtractHeaders(string[] requestLines)
        {
            Dictionary<string, string> headers = new();
            foreach (string line in requestLines)
            {
                if (line.Contains(": "))
                {
                    var parts = line.Split(": ", 2);
                    headers[parts[0]] = parts[1];
                }
            }
            return headers;
        }

        private static void SendResponse(NetworkStream stream, string response)
        {
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
            stream.Flush();
        }
    }

    class Program
    {
        static void Main()
        {
            Server server = new Server();
            server.Start();
        }
    }
}
