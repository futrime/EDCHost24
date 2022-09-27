using System.Collections.Generic;

namespace EdcHost;

/// <summary>
/// An order generator to keep the consistency of the
/// order generation in different rounds.
/// </summary>
public class OrderGenerator
{
    #region Private fields

    private int _nextGeneratedOrderIndex = 0;
    private List<Order> _orderList = new List<Order>();

    #endregion


    #region Public methods

    /// <summary>
    /// Construct an OrderGenerator object.
    /// </summary>
    /// <param name="count">The number of orders to generate</param>
    /// <param name="area">The area to generate</param>
    /// <param name="generationTimeRange">The generation time range</param>
    /// <param name="timeLimitRange">The time limit range</param>
    /// <param name="allOrders">The output value which contains all orders</param>
    public OrderGenerator(
        int count,
        (Dot TopLeft, Dot BottomRight) area,
        (long Lower, long Upper) generationTimeRange,
        (long Lower, long Upper) timeLimitRange,
        out List<Order> allOrders
    )
    {
        // Generate orders
        for (int i = 0; i < count; ++i)
        {
            this._orderList.Add(Order.GenerateRandomOrder(
                area,
                generationTimeRange,
                timeLimitRange
            ));
        }

        // Sort the orders by their generation time
        this._orderList.Sort(
            (order1, order2) =>
                (order1.GenerationTime.CompareTo(order2.GenerationTime))
        );

        allOrders = this._orderList;
    }

    /// <summary>
    /// Generate a new order according to the time.
    /// </summary>
    /// <remarks>
    /// Only the orders not generated and should be
    /// generated before <code>time</code> would be
    /// generated.
    /// </remarks>
    /// <param name="time">The time</param>
    /// <returns>The order; null if unable to generate</returns>
    public Order Generate(long time)
    {
        // Return null if all orders are generated
        if (this._nextGeneratedOrderIndex >= this._orderList.Count)
        {
            return null;
        }

        // Return null if it is not time to generate the next order
        if (this._orderList[this._nextGeneratedOrderIndex].GenerationTime > time)
        {
            return null;
        }

        var order = this._orderList[this._nextGeneratedOrderIndex];
        ++this._nextGeneratedOrderIndex;

        order.Status = Order.StatusType.Pending;

        return order;
    }

    /// <summary>
    /// Reset the generation status.
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < this._orderList.Count; ++i)
        {
            this._orderList[i] = new Order(
                this._orderList[i].DeparturePosition,
                this._orderList[i].DestinationPosition,
                this._orderList[i].GenerationTime,
                deliveryTimeLimit: this._orderList[i].ScheduledDeliveryTime -
                    this._orderList[i].GenerationTime
            );
        }

        this._nextGeneratedOrderIndex = 0;
    }

    #endregion
}