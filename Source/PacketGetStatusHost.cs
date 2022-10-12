using System;
using System.Collections.Generic;

namespace EdcHost;

public class PacketGetStatusHost : Packet
{
    #region Static, const and readonly fields.

    /// <summary>
    /// The packet ID.
    /// </summary>
    public const byte PacketId = 0x05;

    private GameStatusType _gameStatus;
    private long _gameTime;
    private double _score;
    private Dot _vehiclePosition;
    private int _remainingDistance;
    private List<Order> _orderInDeliveryList;
    private Order _latestPendingOrder;

    #endregion


    #region Constructors and finalizers.

    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    public PacketGetStatusHost(
        GameStatusType gameStatus,
        long gameTime,
        double score,
        Dot vehiclePosition,
        int remainingDistance,
        List<Order> orderInDeliveryList,
        Order latestPendingOrder
    )
    {
        this._gameStatus = gameStatus;
        this._gameTime = gameTime;
        this._score = score;
        this._vehiclePosition = vehiclePosition;
        this._remainingDistance = remainingDistance;
        this._orderInDeliveryList = orderInDeliveryList;
        this._latestPendingOrder = latestPendingOrder;
    }

    #endregion

    /// <summary>
    /// Construct a GetSiteInformation packet with a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    /// <exception cref="ArgumentException">
    /// The raw byte array violates the rules.
    /// </exception>
    public PacketGetStatusHost(byte[] bytes)
    {
        // Validate the packet and extract data
        byte[] data = Packet.ExtractPacketData(bytes);

        byte packetId = bytes[0];
        if (packetId != PacketGetStatusHost.PacketId)
        {
            throw new Exception("The packet ID is incorrect.");
        }
        int currentIndex = 0;
        // status
        this._gameStatus = (GameStatusType)data[currentIndex];
        currentIndex += 1;
        // time
        this._gameTime = BitConverter.ToInt64(data, currentIndex);
        currentIndex += 8;
        // score
        this._score = BitConverter.ToDouble(data, currentIndex);
        currentIndex += 8;
        // car
        this._vehiclePosition.X = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;
        this._vehiclePosition.Y = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;
        // mileage
        this._remainingDistance = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        // DeliveryList
        int orderInDeliveryListLength = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        //Note that only according to bytes, the _orderList is probably incomplete (with regard to variable 'generationTime' and 'StatusType')
        this._orderInDeliveryList = new List<Order>() { };
        for (int i = 0; i < orderInDeliveryListLength + 1; i++)
        {
            // (orderInDeliveryListLength + 1) represents (orderInDeliveryList + lastestPendingOrder)

            Dot departurePosition = new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4));
            currentIndex += 4 * 2;
            Dot destinationPosition = new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4));
            currentIndex += 4 * 2;

            long deliveryTimeLimit = BitConverter.ToInt64(data, currentIndex);
            currentIndex += 8;

            // Not used because there's no interface in the constructor of class Order
            // bool isTaken = BitConverter.ToBoolean(data, currentIndex);
            // currentIndex += 1;

            int commission = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            int order_id = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            long generationTime = 0;

            // Warning: this constructor might make false order ids because it generates <new> Order
            if (i != orderInDeliveryListLength)
                this._orderInDeliveryList.Add(new Order(departurePosition, destinationPosition, generationTime, deliveryTimeLimit, commission));
            else
                this._latestPendingOrder = new Order(departurePosition, destinationPosition, generationTime, deliveryTimeLimit, commission);
        }


    }

    #region Methods.

    public override byte[] GetBytes()
    {
        var data = new byte[
            1 + 8 + 8 + 8 + 4 +
            (4 + 28 * this._orderInDeliveryList.Count) + 28
        ];

        int index = 0;

        // The game status.
        switch (this._gameStatus)
        {
            case GameStatusType.Unstarted:
            case GameStatusType.Paused:
            case GameStatusType.Ended:
                BitConverter.GetBytes((byte)0).CopyTo(data, index);
                break;

            case GameStatusType.Running:
                BitConverter.GetBytes((byte)1).CopyTo(data, index);
                break;

            default:
                break;
        }
        index += 1;

        // The game time.
        BitConverter.GetBytes(this._gameTime).CopyTo(data, index);
        index += 8;

        // The score.
        BitConverter.GetBytes(this._score).CopyTo(data, index);
        index += 8;

        #region The vehicle position.
        // The x.
        BitConverter.GetBytes(this._vehiclePosition.X).CopyTo(data, index);
        index += 4;
        // The y.
        BitConverter.GetBytes(this._vehiclePosition.Y).CopyTo(data, index);
        index += 4;
        #endregion

        // The remaining distance.
        BitConverter.GetBytes(this._remainingDistance).CopyTo(data, index);
        index += 4;

        #region The order in delivery list.
        BitConverter.GetBytes(this._orderInDeliveryList.Count).CopyTo(data, index);
        index += 4;
        foreach (var order in this._orderInDeliveryList)
        {
            #region The departure position.
            // The x.
            BitConverter.GetBytes(order.DeparturePosition.X).CopyTo(data, index);
            index += 4;
            // The y.
            BitConverter.GetBytes(order.DeparturePosition.Y).CopyTo(data, index);
            index += 4;
            #endregion

            #region The destination position.
            // The x.
            BitConverter.GetBytes(order.DestinationPosition.X).CopyTo(data, index);
            index += 4;
            // The y.
            BitConverter.GetBytes(order.DestinationPosition.Y).CopyTo(data, index);
            index += 4;
            #endregion

            // The delivery time limit.
            BitConverter.GetBytes(order.DeliveryTimeLimit).CopyTo(data, index);
            index += 8;

            // The order ID.
            BitConverter.GetBytes(order.Id).CopyTo(data, index);
            index += 4;
        }
        #endregion

        #region The latest pending order.
        #region The departure position.
        // The x.
        BitConverter.GetBytes(this._latestPendingOrder.DeparturePosition.X).CopyTo(data, index);
        index += 4;
        // The y.
        BitConverter.GetBytes(this._latestPendingOrder.DeparturePosition.Y).CopyTo(data, index);
        index += 4;
        #endregion

        #region The destination position.
        // The x.
        BitConverter.GetBytes(this._latestPendingOrder.DestinationPosition.X).CopyTo(data, index);
        index += 4;
        // The y.
        BitConverter.GetBytes(this._latestPendingOrder.DestinationPosition.Y).CopyTo(data, index);
        index += 4;
        #endregion

        // The delivery time limit.
        BitConverter.GetBytes(this._latestPendingOrder.DeliveryTimeLimit).CopyTo(data, index);
        index += 8;

        // The order ID.
        BitConverter.GetBytes(this._latestPendingOrder.Id).CopyTo(data, index);
        index += 4;
        #endregion

        // Generate the header.
        var header = Packet.GeneratePacketHeader(PacketGetStatusHost.PacketId, data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }

    public override byte GetPacketId()
    {
        return PacketGetStatusHost.PacketId;
    }

    #endregion
}