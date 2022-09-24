using System;

namespace EdcHost;

/// <summary>
/// The order for the vehicles to deliver
/// </summary>
public class Order
{
    /// <summary>
    /// The order status
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The order is ready for picking up.
        /// </summary>
        Pending = 0,
        /// <summary>
        /// The order is in delivery.
        /// </summary>
        InDelivery = 1,
        /// <summary>
        /// The order is delivered.
        /// </summary>
        Delivered = 2
    }


    private Dot _departurePosition;
    private Dot _destinationPosition;

    private long _generationTime;
    private long _deliveryTimeLimit;

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
    /// Generate a random order.
    /// </summary>
    /// <param name="area">The area</param>
    /// <param name="generationTimeRange">The time range</param>
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
    }
}