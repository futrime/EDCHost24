using System;

namespace EdcHost;

/// <summary>
/// An order
/// </summary>
public class Order
{
    /// <summary>
    /// The order status enum type
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// The order is not generated.
        /// </summary>
        Ungenerated,
        /// <summary>
        /// The order is ready for picking up.
        /// </summary>
        Pending,
        /// <summary>
        /// The order is in delivery.
        /// </summary>
        InDelivery,
        /// <summary>
        /// The order is delivered.
        /// </summary>
        Delivered
    }


    private const int OvertimeScorePenaltyPerSecond = 5;
    private const int MaxScore = 25;


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
    /// The scheduled delivery time
    /// </summary>
    public long ScheduledDeliveryTime => this._deliveryTimeLimit + this._generationTime;
    /// <summary>
    /// The order status
    /// </summary>
    public StatusType Status
    {
        get => this._status;
        set => this._status = value;
    }
    /// <summary>
    /// The score obtained in the order
    /// </summary>
    public int Score
    {
        get
        {
            if (this._deliveryTime == null)
            {
                return 0;
            }

            var overtimeLength = Math.Max((long)this._deliveryTime - this.ScheduledDeliveryTime, 0);
            return Order.MaxScore - (int)(overtimeLength / 1000) * Order.OvertimeScorePenaltyPerSecond;
        }
    }

    private Dot _departurePosition;
    private Dot _destinationPosition;
    private long _generationTime;
    private long _deliveryTimeLimit;
    private long? _departureTime = null;
    private long? _deliveryTime = null;
    private StatusType _status = StatusType.Ungenerated;


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
            random.Next(area.TopLeft.x, area.BottomRight.x),
            random.Next(area.TopLeft.y, area.BottomRight.y)
        );
        var destinationPosition = new Dot(
            random.Next(area.TopLeft.x, area.BottomRight.x),
            random.Next(area.TopLeft.y, area.BottomRight.y)
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
        if (this._status != StatusType.InDelivery)
        {
            throw new Exception("The order cannot be delivered.");
        }

        this._status = StatusType.Delivered;
        this._deliveryTime = time;
    }

    /// <summary>
    /// Take the order.
    /// </summary>
    /// <param name="time">The current time</param>
    public void Take(long time)
    {
        if (this._status != StatusType.Pending)
        {
            throw new Exception("The order cannot be taken.");
        }

        this._status = StatusType.InDelivery;
        this._departureTime = time;
    }
}