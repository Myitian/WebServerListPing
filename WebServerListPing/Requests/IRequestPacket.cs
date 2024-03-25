namespace WebServerListPing.Requests;
public interface IRequestPacket
{
    byte PacketID { get; }
    void WritePacket(Stream stream);
}
