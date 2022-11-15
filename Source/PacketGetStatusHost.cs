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
    private int _gameTime;
    private float _score;
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
        int gameTime,
        float score,
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

        byte packetId = bytes[2];
        if (packetId != PacketGetStatusHost.PacketId)
        {
            throw new Exception("The packet ID is incorrect.");
        }
        int currentIndex = 0;
        // status
        this._gameStatus = (GameStatusType)data[currentIndex];
        currentIndex += 1;
        // time
        this._gameTime = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;
        // score
        this._score = BitConverter.ToSingle(data, currentIndex);
        currentIndex += 4;
        // car
        this._vehiclePosition.X = BitConverter.ToInt16(data, currentIndex);
        currentIndex += 2;
        this._vehiclePosition.Y = BitConverter.ToInt16(data, currentIndex);
        currentIndex += 2;
        // mileage
        this._remainingDistance = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        // DeliveryList
        byte orderInDeliveryListLength = data[currentIndex];
        currentIndex += 1;

        //Note that only according to bytes, the _orderList is probably incomplete (with regard to variable 'generationTime' and 'StatusType')
        this._orderInDeliveryList = new List<Order>() { };
        for (int i = 0; i < orderInDeliveryListLength + 1; i++)
        {
            // (orderInDeliveryListLength + 1) represents (orderInDeliveryList + lastestPendingOrder)

            Dot departurePosition = new Dot(BitConverter.ToInt16(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 2));
            currentIndex += 2 * 2;
            Dot destinationPosition = new Dot(BitConverter.ToInt16(data, currentIndex), BitConverter.ToInt16(data, currentIndex + 2));
            currentIndex += 2 * 2;

            int deliveryTimeLimit = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            // Not used because there's no interface in the constructor of class Order
            // bool isTaken = BitConverter.ToBoolean(data, currentIndex);
            // currentIndex += 1;

            float commission = BitConverter.ToSingle(data, currentIndex);
            currentIndex += 4;

            int order_id = BitConverter.ToInt16(data, currentIndex);
            currentIndex += 2;

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
            1 + 4 + 4 + 4 + 4 +
            (1 + 18 * this._orderInDeliveryList.Count) + 18 * 1
        ];

        int index = 0;

        // The game status.
        switch (this._gameStatus)
        {
            case GameStatusType.Unstarted:
            case GameStatusType.Paused:
            case GameStatusType.Ended:
                data[index] = ((byte)0);
                break;

            case GameStatusType.Running:
                data[index] = ((byte)1);
                break;

            default:
                break;
        }
        index += 1;

        // The game time.
        BitConverter.GetBytes(this._gameTime).CopyTo(data, index);
        index += 4;

        // The score.
        BitConverter.GetBytes(this._score).CopyTo(data, index);
        index += 4;

        #region The vehicle position.
        // The x.
        BitConverter.GetBytes((short)this._vehiclePosition.X).CopyTo(data, index);
        index += 2;
        // The y.
        BitConverter.GetBytes((short)this._vehiclePosition.Y).CopyTo(data, index);
        index += 2;
        #endregion

        // The remaining distance.
        BitConverter.GetBytes(this._remainingDistance).CopyTo(data, index);
        index += 4;

        #region The order in delivery list.
        // If the count is too large, throw an exception
        if (this._orderInDeliveryList.Count > 0xff)
        {
            throw new ArgumentException("Length of orderInDeliveryList is larger than 0xff");
        }
        data[index] = ((byte)this._orderInDeliveryList.Count);
        index += 1;
        foreach (var order in this._orderInDeliveryList)
        {
            #region The departure position.
            // The x.
            BitConverter.GetBytes((short)order.DeparturePosition.X).CopyTo(data, index);
            index += 2;
            // The y.
            BitConverter.GetBytes((short)order.DeparturePosition.Y).CopyTo(data, index);
            index += 2;
            #endregion

            #region The destination position.
            // The x.
            BitConverter.GetBytes((short)order.DestinationPosition.X).CopyTo(data, index);
            index += 2;
            // The y.
            BitConverter.GetBytes((short)order.DestinationPosition.Y).CopyTo(data, index);
            index += 2;
            #endregion

            // The delivery time limit.
            BitConverter.GetBytes(order.DeliveryTimeLimit).CopyTo(data, index);
            index += 4;

            // The commission
            BitConverter.GetBytes(order.Commission).CopyTo(data, index);
            index += 4;

            // The order ID.
            BitConverter.GetBytes((short)order.Id).CopyTo(data, index);
            index += 2;
        }
        #endregion


        #region The latest pending order.
        // Used to judge whether the lastest pending order is null, if it is, then a null order with id -1 will be created;
        if (this._latestPendingOrder != null)
        {
            #region The departure position.

            // The x.
            BitConverter.GetBytes((short)this._latestPendingOrder.DeparturePosition.X).CopyTo(data, index);
            index += 2;
            // The y.
            BitConverter.GetBytes((short)this._latestPendingOrder.DeparturePosition.Y).CopyTo(data, index);
            index += 2;
            #endregion

            #region The destination position.
            // The x.
            BitConverter.GetBytes((short)this._latestPendingOrder.DestinationPosition.X).CopyTo(data, index);
            index += 2;
            // The y.
            BitConverter.GetBytes((short)this._latestPendingOrder.DestinationPosition.Y).CopyTo(data, index);
            index += 2;
            #endregion

            // The delivery time limit.
            BitConverter.GetBytes(this._latestPendingOrder.DeliveryTimeLimit).CopyTo(data, index);
            index += 4;

            // The commission
            BitConverter.GetBytes(this._latestPendingOrder.Commission).CopyTo(data, index);
            index += 4;

            // The order ID.
            BitConverter.GetBytes((short)this._latestPendingOrder.Id).CopyTo(data, index);
            index += 2;
        }
        else
        {
            // The bytes of a null order is 0, except for -1 id
            index += 4 * 4;
            // The order ID.
            BitConverter.GetBytes((short)-1).CopyTo(data, index);
            index += 2;
        }
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