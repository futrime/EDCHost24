using System;

namespace EdcHost;

internal abstract class Packet
{
    public static byte CalculateChecksum(byte[] bytes)
    {
        byte checksum = 0x00;
        foreach (var byte_item in bytes)
        {
            checksum ^= byte_item;
        }
        return checksum;
    }

    public static byte[] ExtractPacketData(byte[] bytes)
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

        if (dataLength < bytes.Length - 6)
        {
            throw new Exception("The data length of the packet is incorrect.");
        }
        if (checksum != Packet.CalculateChecksum(data))
        {
            throw new Exception("The data of the packet is broken.");
        }

        return data;
    }

    public abstract byte[] GetBytes();
}