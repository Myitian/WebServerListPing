namespace WebServerListPing.Requests;
public struct StatusRequest : IRequestPacket
{
    public readonly byte PacketID => 0x00;
    public readonly void WritePacket(Stream stream)
    {
        PacketUtil.WriteByte(stream, 1);
        PacketUtil.WriteByte(stream, 0);
    }
}
