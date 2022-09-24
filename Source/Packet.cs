namespace EdcHost;

internal abstract class Packet
{
    protected byte _packetId;

    public static byte CalculateChecksum(byte[] bytes) {
        byte checksum = 0x00;
        foreach (var byte_item in bytes)
        {
            checksum ^= byte_item;
        }
        return checksum;
    }

    public abstract byte[] GetBytes();
}