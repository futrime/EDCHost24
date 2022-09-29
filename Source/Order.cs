using System;

namespace EdcHost;

/// <summary>
/// An order
/// </summary>
public class Order
{
    #region Public properties

    /// <summary>
    /// The delivery time limit.
    /// </summary>
    public long DeliveryTimeLimit => this._deliveryTimeLimit;

    /// <summary>
    /// The departure position
    /// </summary>
    public Dot DeparturePosition => this._departurePosition;

    /// <summary>
    /// The destination position
    /// </summary>
    public Dot DestinationPosition => this._destinationPosition;

    /// <summary>
    /// The generation time
    /// </summary>
    public long GenerationTime => this._generationTime;

    /// <summary>
    /// The overtiom duraiton.
    /// </summary>
    public long? OvertimeDuration
    {
        get {
            if (this.ScheduledDeliveryTime == null)
            {
                return null;
            }

            return Math.Max((long)this._deliveryTime - (long)this.ScheduledDeliveryTime, 0);
        }
    }

    /// <summary>
    /// The scheduled delivery time
    /// </summary>
    public long? ScheduledDeliveryTime
    {
        get {
            if (this._departureTime == null)
            {
                return null;
            }

            return this._deliveryTimeLimit + this._departureTime;
        }
    }

    /// <summary>
    /// The order status
    /// </summary>
    public OrderStatusType Status
    {
        get => this._status;
        set => this._status = value;
    }

    #endregion

    #region Private fields

    private Dot _departurePosition;
    private Dot _destinationPosition;
    private long _generationTime;
    private long _deliveryTimeLimit;
    private long? _departureTime = null;
    private long? _deliveryTime = null;
    private OrderStatusType _status = OrderStatusType.Ungenerated;

    #endregion


    #region Public methods

    /// <summary>
    /// Generate a random order.
    /// </summary>
    /// <param name="area">The area</param>
    /// <param name="generationTimeRange">The generation time range</param>
    /// <param name="timeLimitRange">The time limit range</param>
    /// <returns></returns>
    public static Order GenerateRandomOrder(
        (Dot TopLeft, Dot BottomRight) area,
        (long Lower, long Upper) generationTimeRange,
        (long Lower, long Upper) timeLimitRange
    )
    {
        var random = new Random((int)DateTime.Now.Ticks);

        var departurePosition = new Dot(
            random.Next(area.TopLeft.X, area.BottomRight.X),
            random.Next(area.TopLeft.Y, area.BottomRight.Y)
        );
        var destinationPosition = new Dot(
            random.Next(area.TopLeft.X, area.BottomRight.X),
            random.Next(area.TopLeft.Y, area.BottomRight.Y)
        );
        var generationTime = random.NextInt64(generationTimeRange.Lower, generationTimeRange.Upper);
        var timeLimit = random.NextInt64(timeLimitRange.Lower, timeLimitRange.Upper);

        return new Order(departurePosition, destinationPosition, generationTime, timeLimit);
    }

    /// <summary>
    /// Construct an Order object.
    /// </summary>
    /// <param name="departurePosition">The departure position</param>
    /// <param name="destinationPosition">The destination position</param>
    /// <param name="generationTime">The generation time</param>
    /// <param name="deliveryTimeLimit">The delivery time limit</param>
    public Order(
        Dot departurePosition,
        Dot destinationPosition,
        long generationTime,
        long deliveryTimeLimit
    )
    {
        // Validate the delivery time limit
        if (deliveryTimeLimit <= 0)
        {
            throw new Exception("The delivery time limit is invalid.");
        }

        this._departurePosition = departurePosition;
        this._destinationPosition = destinationPosition;
        this._generationTime = generationTime;
        this._deliveryTimeLimit = deliveryTimeLimit;
    }

    /// <summary>
    /// Deliver the order.
    /// </summary>
    /// <param name="time">The current time</param>
    public void Deliver(long time)
    {
        if (this._status != OrderStatusType.InDelivery)
        {
            throw new Exception("The order cannot be delivered.");
        }

        this._status = OrderStatusType.Delivered;
        this._deliveryTime = time;
    }

    /// <summary>
    /// Take the order.
    /// </summary>
    /// <param name="time">The current time</param>
    public void Take(long time)
    {
        if (this._status != OrderStatusType.Pending)
        {
            throw new Exception("The order cannot be taken.");
        }

        this._status = OrderStatusType.InDelivery;
        this._departureTime = time;
    }

    #endregion
}