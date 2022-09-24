using System;

namespace EdcHost;

internal class PacketGetSiteInformation : Packet
{
    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    /// <remarks>
    /// There is no field in this type of packets.
    /// </remarks>
    public PacketGetSiteInformation()
    {
        this._packetId = 0x00;
    }

    /// <summary>
    /// Construct a GetSiteInformation packet with a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    /// <exception cref="ArgumentException">
    /// The raw byte array violates the rules.
    /// </exception>
    public PacketGetSiteInformation(byte[] bytes): this()
    {
        // Validate the byte array
        if (bytes.Length < 6)
        {
            throw new Exception("The header of the packet is broken.");
        }

        byte packetId = bytes[0];
        uint dataLength = BitConverter.ToUInt32(bytes, 1);
        byte checksum = bytes[5];
        var data = new byte[dataLength];
        Array.Copy(bytes, data, 6);

        if (packetId != this._packetId)
        {
            throw new Exception("The packet ID is incorrect.");
        }
        if (dataLength != bytes.Length - 6) {
            throw new Exception("The data length of the packet is incorrect.");
        }
        if (checksum != Packet.CalculateChecksum(data)) {
            throw new Exception("The data of the packet is broken.");
        }
    }

    public override byte[] GetBytes()
    {
        var header = new byte[6];
        var data = new byte[0];

        header[0] = this._packetId;
        BitConverter.GetBytes(data.Length).CopyTo(header, 1);
        header[5] = Packet.CalculateChecksum(data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }
}