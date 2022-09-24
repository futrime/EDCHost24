using System;

namespace EdcHost;

internal class PacketSetChargingPile : Packet
{
    private readonly byte PacketId = 0x02;

    private Dot ChargingPilePos;
    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    /// <remarks>
    /// There is no field in this type of packets.
    /// </remarks>
    public PacketSetChargingPile(Dot ChargingPilePos)
    {
        this.ChargingPilePos = ChargingPilePos;
    }

    /// <summary>
    /// Construct a GetSiteInformation packet with a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    /// <exception cref="ArgumentException">
    /// The raw byte array violates the rules.
    /// </exception>
    public PacketSetChargingPile(byte[] bytes)
    {
        // Validate the packet and extract data
        Packet.ExtractPacketData(bytes);

        byte packetId = bytes[0];
        if (packetId != this.PacketId)
        {
            throw new Exception("The packet ID is incorrect.");
        }

        this.ChargingPilePos = new Dot(BitConverter.ToInt32(bytes, 6),
                                       BitConverter.ToInt32(bytes, 6 + 4));

    }

    public override byte[] GetBytes()
    {
        var header = new byte[6];
        var data = new byte[8];

        BitConverter.GetBytes(ChargingPilePos.x).CopyTo(data, 0);
        BitConverter.GetBytes(ChargingPilePos.y).CopyTo(data, 4);

        header[0] = this.PacketId;
        BitConverter.GetBytes(data.Length).CopyTo(header, 1);
        header[5] = Packet.CalculateChecksum(data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }
}