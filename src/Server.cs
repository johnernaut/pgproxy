using System.Net;
using System.Net.Sockets;

namespace pgproxy;

public class Server
{
    public async void Serve()
    {
        var clientConn = new TcpListener(localaddr: IPAddress.Parse("127.0.0.1"), port: 6432);

        try
        {
            Console.WriteLine("Listening on port 6432...");
            clientConn.Start();

            while (true)
            {
                Console.WriteLine("Waiting for connection...");
                var client = await clientConn.AcceptTcpClientAsync();
                Console.WriteLine("Connected!");

                var tcpConnection = new TcpConnection(client);
                tcpConnection.Run();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception occured: {0}", e);
        }
        finally
        {
            // Stop listening for new clients.
            clientConn.Stop();
        }
    }
}