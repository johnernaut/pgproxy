using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using pgproxy.Messages;

namespace pgproxy;

public class TcpConnection
{
    private readonly TcpClient _clientConn;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public TcpConnection(TcpClient clientConn)
    {
        _clientConn = clientConn;
    }

    public void Run()
    {
        Task.Run(async () =>
        {
            try
            {
                using (_clientConn)
                {
                    // get a stream object for reading and writing to the client
                    await using var clientStream = _clientConn.GetStream();

                    Stream serverStream;
                    IReader startUpMsg;
                    while (true)
                    {
                        startUpMsg = HandleStartupMessage(clientStream);
                        if (startUpMsg is SSLRequest)
                        {
                            clientStream.Write(Encoding.ASCII.GetBytes("N"));
                        }
                        else if (startUpMsg is StartupMessage startUp)
                        {
                            foreach (var parameter in startUp.StartupParameters)
                            {
                                Console.WriteLine("Startup params: {0} = {1}", parameter.Key, parameter.Value);
                            }

                            var serverConn = new TcpClient("127.0.0.1", 5432);
                            serverStream = serverConn.GetStream();
                            break;
                        }
                    }

                    await serverStream.WriteAsync(startUpMsg.Read(), _cancellationTokenSource.Token);
                    await using (_cancellationTokenSource.Token.Register(() => serverStream.Close(), true))
                    {
                        var proxyTask = await Task.WhenAny(
                            CopyToAsync(serverStream, clientStream, 4096, _cancellationTokenSource.Token),
                            CopyToAsync(clientStream, serverStream, 4096, _cancellationTokenSource.Token));
                        await proxyTask;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occurred during TCP stream : {0}", ex);
            }
        });
    }

    private async Task CopyToAsync(Stream source, Stream destination, int bufferSize = 4096,
        CancellationToken cancellationToken = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            while (true)
            {
                var bytesRead = await source.ReadAsync(new Memory<byte>(buffer), cancellationToken)
                    .ConfigureAwait(false);
                if (bytesRead == 0) break;
                await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private IReader HandleStartupMessage(Stream stream)
    {
        var head = new byte[4];
        _ = stream.Read(head);
        var result = BinaryPrimitives.ReadUInt32BigEndian(head);

        var data = new Byte[result - 4];
        _ = stream.Read(data);

        var startupMessage = new StartupMessage();
        return startupMessage.ReadStartupMessage(data);
    }
}