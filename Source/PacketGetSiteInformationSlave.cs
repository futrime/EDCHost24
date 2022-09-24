using System;

namespace EdcHost;

/// <summary>
/// The packet for the slaves to get site information
/// </summary>
internal class PacketGetSiteInformationSlave : Packet
{
    private readonly byte PacketId = 0x00;

    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    /// <remarks>
    /// There is no field in this type of packets.
    /// </remarks>
    public PacketGetSiteInformationSlave()
    {
        // Empty
    }

    /// <summary>
    /// Construct a GetSiteInformation packet with a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    /// <exception cref="Exception">
    /// The raw byte array violates the rules.
    /// </exception>
    public PacketGetSiteInformationSlave(byte[] bytes) : this()
    {
        // Validate the packet and extract data
        Packet.ExtractPacketData(bytes);

        byte packetId = bytes[0];
        if (packetId != this.PacketId)
        {
            throw new Exception("The packet ID is incorrect.");
        }
    }

    public override byte[] GetBytes()
    {
        var data = new byte[0];

        var header = Packet.GeneratePacketHeader(this.PacketId, data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }
}