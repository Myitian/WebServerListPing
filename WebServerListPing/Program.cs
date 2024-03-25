using WebServerListPing.Requests;
using System.Net.Sockets;

namespace WebServerListPing;
class Program
{
    static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        WebApplication app = builder.Build();
        app.MapMethods("/{target}", ["GET", "HEAD"], Ping);
        app.Run();
    }

    static IResult Ping(string target)
    {
        string id = "ID:" + (0xFFFFF & (ulong)DateTime.UtcNow.Ticks).ToString("X").PadLeft(5, '0');
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Request: {target}");
        int colon = target.IndexOf(':', StringComparison.Ordinal);
        string host;
        ushort port;
        if (colon == -1)
        {
            host = target;
            port = 25565;
        }
        else
        {
            host = target[..colon];
            if (!ushort.TryParse(target[(colon + 1)..], out port))
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Incorrect Port!");
                return Results.Text("Incorrect Port", statusCode: StatusCodes.Status400BadRequest);
            }
        }
        try
        {
            using TcpClient tcp = new(host, port);
            NetworkStream networkStream = tcp.GetStream();
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Sending Handshake Packet...");
            Handshake handshake = new(-1, host, port);
            handshake.WritePacket(networkStream);
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Sending Status Request Packet...");
            StatusRequest statusRequest = new();
            statusRequest.WritePacket(networkStream);

            PacketUtil.ReadVarInt(networkStream); // PacketLength
            PacketUtil.ReadVarInt(networkStream); // PacketID
            int strlen = PacketUtil.ReadVarInt(networkStream);
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Response: {strlen}");
            byte[] buffer = new byte[strlen];
            networkStream.ReadExactly(buffer);
            return Results.Bytes(buffer, "application/json");
        }
        catch (SocketException e)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Exception: {e.GetType()}");
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Message: {e.Message}");
            return Results.Text("Cannot Establish TCP Connect", statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Exception: {e.GetType()}");
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({id}) Message: {e.Message}");
            return Results.Text($"{e.GetType()}\n{e.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}