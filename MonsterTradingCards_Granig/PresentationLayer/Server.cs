using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCards_Granig.PresentationLayer
{
    // Diese Klasse stellt den TCP-Server dar, der auf Anfragen wartet
    public class Server
    {
        private readonly RequestHandler _requestHandler;

        public Server()
        {
            _requestHandler = new RequestHandler();
        }

        public void Start()
        {
            // Starte den TCP-Listener auf Port 8080, der eingehende Verbindungen akzeptiert
            TcpListener listener = new TcpListener(IPAddress.Any, 8080);
            listener.Start();
            Console.WriteLine("Server started, listening on port 8080...");

            while (true)
            {
                // Akzeptiere eingehende TCP-Verbindungen
                using TcpClient client = listener.AcceptTcpClient();
                using NetworkStream? stream = client.GetStream();

                // Erstellt einen Puffer (ein Array von Bytes), um die eingehenden Daten zu speichern.
                // Die Größe des Puffers wird basierend auf der maximalen Empfangsgröße des Clients festgelegt.
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Parse die HTTP-Anfrage aus dem eingehenden TCP-Datenstrom
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Konvertieren der bytes aus buffer in string
                string[] requestLines = request.Split("\r\n");
                string httpMethod = requestLines[0].Split(' ')[0]; // Erhalte die HTTP-Methode (GET, POST, DELETE)
                string requestUrl = requestLines[0].Split(' ')[1]; // Erhalte die URL der Anfrage

                // Parse Body for POST requests
                string jsonBody = httpMethod == "POST" ? request.Split("\r\n\r\n")[1] : null;

                string responseString = _requestHandler.HandleRequest(requestUrl, httpMethod, jsonBody);

                // Erstelle eine HTTP-Antwort und sende diese zurück
                byte[] responseBuffer = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Length: {responseString.Length}\r\n\r\n{responseString}");
                stream.Write(responseBuffer, 0, responseBuffer.Length); // Startet bei der Position 0 des Buffers und sendet die volle Länge des Buffers.
            }
        }
    }
}
