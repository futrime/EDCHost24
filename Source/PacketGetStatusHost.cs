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