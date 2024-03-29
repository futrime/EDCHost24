using System;

namespace EdcHost;

/// <summary>
/// An order
/// </summary>
public class Order
{
    #region Public properties

    /// <summary>
    /// The commission.
    /// </summary>
    public float Commission => this._commission;

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
    /// The ID.
    /// </summary>
    public int Id => this._id;

    /// <summary>
    /// The overtiom duraiton.
    /// </summary>
    public long? OvertimeDuration
    {
        get
        {
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
        get
        {
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

    private float _commission;
    private long? _deliveryTime = null;
    private long _deliveryTimeLimit;
    private Dot _departurePosition;
    private long? _departureTime = null;
    private Dot _destinationPosition;
    private long _generationTime;
    private int _id;
    private OrderStatusType _status = OrderStatusType.Ungenerated;

    #endregion


    #region Public methods

    /// <summary>
    /// Generate a random order.
    /// </summary>
    /// <param name="area">The area</param>
    /// <param name="generationTimeRange">The generation time range</param>
    /// <param name="timeLimitRange">The time limit range</param>
    /// <param name="commissionRange">The commission range</param>
    /// <returns></returns>
    public static Order GenerateRandomOrder(
        (Dot TopLeft, Dot BottomRight) area,
        (long Lower, long Upper) generationTimeRange,
        (long Lower, long Upper) timeLimitRange,
        (decimal Lower, decimal Upper) commissionRange
    )
    {
        var departurePosition = new Dot(
            Utility.RandomGenerator.Next(area.TopLeft.X + 10, area.BottomRight.X - 10),
            Utility.RandomGenerator.Next(area.TopLeft.Y + 10, area.BottomRight.Y - 10)
        );
        var destinationPosition = new Dot(
            Utility.RandomGenerator.Next(area.TopLeft.X + 10, area.BottomRight.X - 10),
            Utility.RandomGenerator.Next(area.TopLeft.Y + 10, area.BottomRight.Y - 10)
        );
        var generationTime = Utility.RandomGenerator.NextInt64(generationTimeRange.Lower, generationTimeRange.Upper);
        var timeLimit = Utility.RandomGenerator.NextInt64(timeLimitRange.Lower, timeLimitRange.Upper);
        var commission = commissionRange.Lower + (decimal)Utility.RandomGenerator.NextDouble() * (commissionRange.Upper - commissionRange.Lower);

        return new Order(departurePosition, destinationPosition, generationTime, timeLimit, (float)commission);
    }

    /// <summary>
    /// Constructs an Order object.
    /// </summary>
    /// <param name="departurePosition">The departure position</param>
    /// <param name="destinationPosition">The destination position</param>
    /// <param name="generationTime">The generation time</param>
    /// <param name="deliveryTimeLimit">The delivery time limit</param>
    /// <param name="commission">The commission</param>
    /// <param name="id">The id, empty for auto-generation</param>
    public Order(
        Dot departurePosition,
        Dot destinationPosition,
        long generationTime,
        long deliveryTimeLimit,
        float commission,
        int? id = null
    )
    {
        // Validate the delivery time limit
        // change the <= into <,because the delivery time limit of the default order is 0. 10-23 ZYR
        if (deliveryTimeLimit < 0)
        {
            throw new Exception("The delivery time limit is invalid.");
        }

        this._departurePosition = departurePosition;
        this._destinationPosition = destinationPosition;
        this._generationTime = generationTime;
        this._deliveryTimeLimit = deliveryTimeLimit;
        this._commission = commission;
        this._id = id.GetValueOrDefault(Utility.GenerateUniqueId());
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
    /// <summary>
    /// Reset the generation time of the order.
    /// </summary>
    /// <param name="time">The current time</param>
    public void ResetGenerationTime((long Lower, long Upper) generationTimeRange)
    {
        this._generationTime = Utility.RandomGenerator.NextInt64(generationTimeRange.Lower, generationTimeRange.Upper);
    }
    #endregion
}