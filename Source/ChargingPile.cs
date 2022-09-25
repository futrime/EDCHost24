namespace EdcHost;

/// <summary>
/// A charging pile
/// </summary>
public class ChargingPile
{
    /// <summary>
    /// The charging range radius in centimeters
    /// </summary>
    private const decimal ChargingRangeRadius = 20;

    /// <summary>
    /// The position
    /// </summary>
    public Dot Position => this._position;

    private Dot _position;


    /// <summary>
    /// Construct a charging pile.
    /// </summary>
    /// <param name="position">The position</param>
    public ChargingPile(Dot position)
    {
        this._position = position;
    }

    /// <summary>
    /// Check if the vehicle can be charged at the position.
    /// </summary>
    /// <param name="position">The position of the vehicle</param>
    /// <returns>
    /// True if the vehicle can be charged; otherwise false
    /// </returns>
    public bool CanCharge(Dot position)
    {
        return Dot.Distance(position, this._position) <
            ChargingPile.ChargingRangeRadius;
    }
}