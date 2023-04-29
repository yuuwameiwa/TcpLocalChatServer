using System.Net;
using System.Net.Sockets;


namespace TcpLocalChatServer
{
    internal class Client
    {
        protected internal string Id { get; } = Guid.NewGuid().ToString();
        protected internal string? UserName { get; set; }

        protected internal StreamWriter Writer { get; }
        protected internal StreamReader Reader { get; }
        protected internal NetworkStream Stream { get; }

        protected TcpClient client;
        protected Server server;

        public Client(TcpClient tcpClient, Server serverObject)
        {
            client = tcpClient;
            server = serverObject;

            Stream = client.GetStream();
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);
        }

        public async Task ProcessAsync()
        {
            try
            {
                UserName = await Reader.ReadLineAsync();
                string? message = $"{UserName} logged in.";

                server.BroadcastMessageAsync(message, Id);
                Console.WriteLine(message);

                while (true)
                {
                    try
                    {
                        message = await Reader.ReadLineAsync();

                        if (message == null) 
                            continue;

                        message = $"{UserName}: {message}";
                        Console.WriteLine(message);

                        await server.BroadcastMessageAsync(message, Id);
                    }
                    catch (Exception ex)
                    {
                        message = $"{UserName} left.";
                        Console.WriteLine(message);
                        await server.BroadcastMessageAsync(message, Id);
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
            }
        }

        protected internal void Close()
        {
            Writer.Close();
            Reader.Close();
            client.Close();
        }
    }
}
