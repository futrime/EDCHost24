using System;

namespace EdcHost;

/// <summary>
/// The order for the vehicles to deliver
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

    private const int OverTimePenalty = 5; // per second
    /// <summary>
    /// The order is delivered.
    /// </summary>
    private const int ScheduledScore = 25;

    private Dot _departurePosition;
    private Dot _destinationPosition;

    private long _generationTime;
    private long _deliveryTimeLimit;
    private long _firstCollisionTime;

    private StatusType _status = StatusType.Ungenerated;

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
    /// The delivery time limit
    /// </summary>
    public long DeliveryTime => this._deliveryTimeLimit;
    /// <summary>
    /// The time of first collision with car 
    /// </summary>
    public long FirstCollisionTime => this._firstCollisionTime;
    /// <summary>
    /// The order status
    /// </summary>
    public StatusType Status
    {
        get => this._status;
        set => this._status = value;
    }


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
        this._departurePosition = departurePosition;
        this._destinationPosition = destinationPosition;
        this._generationTime = generationTime;
        this._deliveryTimeLimit = deliveryTimeLimit;
        this._firstCollisionTime = -1;
        this._status = StatusType.Pending;
    }
    /// <summary>
    /// Only if the _firstCollisionTime is -1, it can be revised
    /// </summary>
    public bool AddFirstCollisionTime(long time)
    {
        if (this._firstCollisionTime == -1)
        {
            this._firstCollisionTime = time;
            return true;
        }
        // failed to revise
        else
        {
            return false;
        }
    }
    /// <summary>
    /// Get Order score, 
    /// </summary>
    public int GetPackageScore(int arrivalTime)
    {
        int orderScore = 0;

        if (arrivalTime <= this.DeliveryTime)
        {
            orderScore = ScheduledScore;
        }
        else
        {
            orderScore = ScheduledScore + (int)((this.DeliveryTime - arrivalTime) * OverTimePenalty / 1000);
        }

        return orderScore;
    }

    public double Distance2Departure(Dot dot)
    {
        return Dot.Distance(dot, this._departurePosition);
    }

    public double Distance2Destination(Dot dot)
    {
        return Dot.Distance(dot, this._destinationPosition);
    }
}