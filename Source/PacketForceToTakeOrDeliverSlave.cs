using System;

namespace EdcHost;

/// <summary>
/// A packet for slaves to force to take or deliver an order.
/// </summary>
public class PacketForceToTakeOrDeliverSlave : Packet
{
    #region Static, const and readonly fields.

    /// <summary>
    /// The packet ID.
    /// </summary>
    public const byte PacketId = 0x06;

    #endregion


    #region Fields and properties

    /// <summary>
    /// The action
    /// </summary>
    public ForceToTakeOrDeliverOrderType Action => _action;

    private ForceToTakeOrDeliverOrderType _action;

    #endregion


    #region Constructors and finalizers.

    /// <summary>
    /// Constructs a ForceToTakeOrDeliverSlave packet with fields.
    /// </summary>
    /// <param name="action">The action</param>
    public PacketForceToTakeOrDeliverSlave(ForceToTakeOrDeliverOrderType action)
    {
        _action = action;
    }

    /// <summary>
    /// Constructs a ForceToTakeOrDeliverSlave packet from a raw
    /// byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    public PacketForceToTakeOrDeliverSlave(byte[] bytes)
    {
        // Validate the packet and extract data.
        var data = Packet.ExtractPacketData(bytes);

        // Check the packet ID.
        byte packetId = bytes[2];
        if (packetId != this.GetPacketId())
        {
            throw new Exception("The packet ID is incorrect.");
        }

        if (data[0] == 0)
        {
            _action = ForceToTakeOrDeliverOrderType.Take;
        }
        else
        {
            _action = ForceToTakeOrDeliverOrderType.Deliver;
        }
    }

    #endregion


    #region Methods

    public override byte[] GetBytes()
    {
        var data = new byte[1];
        if (_action == ForceToTakeOrDeliverOrderType.Take)
        {
            data[0] = 0;
        }
        else
        {
            data[0] = 1;
        }

        var header = Packet.GeneratePacketHeader(PacketForceToTakeOrDeliverSlave.PacketId, data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }

    public override byte GetPacketId()
    {
        return PacketForceToTakeOrDeliverSlave.PacketId;
    }

    #endregion
}