using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

namespace TcpLocalChatServer
{
    public class Server
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8888);
        List<Client> clients = new List<Client>();

        public async Task ListenAsync()
        {
            try
            {
                listener.Start();
                Console.WriteLine("Server is started. \nListening...");

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();

                    Client client = new Client(tcpClient, this);
                    clients.Add(client);
                    Task.Run(client.ProcessAsync);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        protected internal async Task BroadcastMessageAsync(string message, string id)
        {
            List<string> clientsMatches = new List<string>();
            if (message.Contains('@'))
            {
                foreach (Match match in Regex.Matches(message, @"(?<=@)\w+\b"))
                    clientsMatches.Add(match.Value);
            }

            if (clientsMatches != null && clientsMatches.Count > 0)
            {
                foreach (Client client in clients)
                {
                    if (client.Id != id && clientsMatches.Contains(client.UserName))
                    {
                        await client.Writer.WriteLineAsync(message);
                        await client.Writer.FlushAsync();
                    }
                }
            }
            else
            {
                foreach (Client client in clients)
                {
                    if (client.Id != id)
                    {
                        await client.Writer.WriteLineAsync(message);
                        await client.Writer.FlushAsync();
                    }
                }
            }
        }

        protected internal void RemoveConnection(string id)
        {
            Client? client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
            client?.Close();
        }

        private void Disconnect()
        {
            foreach (Client client in clients)
                client.Close();

            listener.Stop();
        }
    }
}
