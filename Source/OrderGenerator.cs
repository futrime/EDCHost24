using System.Collections.Generic;

namespace EdcHost;

/// <summary>
/// The order generator for generating orders in a round of game
/// </summary>
public class OrderGenerator
{
    private int _nextGeneratedOrderIndex = 0;
    private List<Order> _orderList = new List<Order>();

    /// <summary>
    /// The number of orders generated
    /// </summary>
    public int Count => this._orderList.Count;


    /// <summary>
    /// Construct an OrderGenerator object.
    /// </summary>
    /// <param name="count">The number of orders to generate</param>
    /// <param name="area">The area to generate</param>
    /// <param name="generationTimeRange">The generation time range</param>
    /// <param name="timeLimitRange">The time limit range</param>
    public OrderGenerator(
        int count,
        (Dot TopLeft, Dot BottomRight) area,
        (long Lower, long Upper) generationTimeRange,
        (long Lower, long Upper) timeLimitRange
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
        if (this._nextGeneratedOrderIndex >= this._orderList.Count)
        {
            return null;
        }
        if (this._orderList[this._nextGeneratedOrderIndex].GenerationTime > time)
        {
            return null;
        }

        var order = this._orderList[this._nextGeneratedOrderIndex];
        ++this._nextGeneratedOrderIndex;

        return order;
    }

    /// <summary>
    /// Reset the generation status.
    /// </summary>
    public void Reset()
    {
        this._nextGeneratedOrderIndex = 0;
    }
}