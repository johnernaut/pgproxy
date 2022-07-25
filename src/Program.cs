namespace pgproxy;

internal class Program
{
    private static void Main(string[] args)
    {
        var server = new Server();
        server.Serve();

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }
}