using System;
using System.Collections.Generic;
namespace EdcHost;

internal class PacketSendStatusInformationHost : Packet
{
    /// <summary>
    /// An order that consists of 4 elements
    /// </summary>
    public class Order
    {
        private Dot _departurePos;
        private Dot _destinationPos;
        private double _scheduledTime;
        private bool _isTaken;

        public Order(Dot departurePos, Dot destinationPos, double scheduledTime, bool isTaken)
        {
            this._departurePos = departurePos;
            this._destinationPos = destinationPos;
            this._scheduledTime = scheduledTime;
            this._isTaken = isTaken;
        }
    };

    private readonly byte PacketId = 0x05;
    private GameState _currentState;
    private double _currentTime;
    private int _currentScore;
    private Dot _carPos;
    private double _mileage;
    private List<Order> _orderList;

    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    /// <remarks>
    /// Note that we should convert package list into order list
    /// </remarks>
    public PacketSendStatusInformationHost(GameState currentState, double currentTime, int currentScore,
        Dot carPos, double mileage, List<Package> packageList)
    {
        this._currentScore = currentScore;
        this._currentTime = currentTime;
        this._currentScore = currentScore;
        this._carPos = carPos;
        this._mileage = mileage;

        // Wait class 'Order' to be finished
        // for (int i = 0; i < orderList.Count; i++)
        // {

        // }
    }

    /// <summary>
    /// Construct a GetSiteInformation packet with a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    /// <exception cref="ArgumentException">
    /// The raw byte array violates the rules.
    /// </exception>
    public PacketSendStatusInformationHost(byte[] bytes)
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
        var header = new byte[6];
        var data = new byte[0];

        header[0] = this.PacketId;
        BitConverter.GetBytes(data.Length).CopyTo(header, 1);
        header[5] = Packet.CalculateChecksum(data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }
}