namespace EdcHost;

/// <summary>
/// A charging pile
/// </summary>
public class ChargingPile
{
    /// <summary>
    /// The charging range radius in centimeters
    /// </summary>
    public const decimal InfluenceScopeRadius = 20;


    /// <summary>
    /// The camp
    /// </summary>
    public Camp Camp => this._camp;

    /// <summary>
    /// The position
    /// </summary>
    public Dot Position => this._position;

    private Camp _camp;
    private Dot _position;


    /// <summary>
    /// Construct a charging pile.
    /// </summary>
    /// <param name="position">The position</param>
    public ChargingPile(Camp camp, Dot position)
    {
        this._camp = camp;
        this._position = position;
    }

    /// <summary>
    /// Check if the vehicle is in the influence scope of the charging pile.
    /// </summary>
    /// <param name="position">The position of the vehicle</param>
    /// <returns>
    /// True if the vehicle is in the influence scope; otherwise false
    /// </returns>
    public bool IsInInfluenceScope(Dot position)
    {
        return Dot.Distance(position, this._position) <
            ChargingPile.InfluenceScopeRadius;
    }
}