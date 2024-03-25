namespace WebServerListPing.Requests;
public struct Handshake(
    int protocolVersion = -1,
    string serverAddress = "",
    ushort serverPort = 25565,
    NextState nextState = NextState.Status) : IRequestPacket
{
    public readonly byte PacketID => 0x00;
    public int ProtocolVersion = protocolVersion;
    public string ServerAddress = serverAddress;
    public ushort ServerPort = serverPort;
    public NextState NextState = nextState;

    public readonly void WritePacket(Stream stream)
    {
        using (MemoryStream ms = new())
        {
            PacketUtil.WriteVarInt(ms, PacketID);
            PacketUtil.WriteVarInt(ms, ProtocolVersion);
            PacketUtil.WriteString(ms, ServerAddress);
            PacketUtil.WriteUInt16(ms, ServerPort);
            PacketUtil.WriteVarInt(ms, (int)NextState);

            PacketUtil.WriteVarLong(stream, ms.Length);
            ms.Position = 0;
            ms.CopyTo(stream);
        }
    }
}

public enum NextState
{
    Status = 1,
    Login
}
