namespace EdcHost;

/// <summary>
/// A charging pile
/// </summary>
public class ChargingPile
{
    /// <summary>
    /// The default influence scope radius in centimeters
    /// </summary>
    public const decimal DefaultInfluenceScopeRadius = 20;


    /// <summary>
    /// The camp
    /// </summary>
    public CampType Camp => this._camp;

    /// <summary>
    /// The position
    /// </summary>
    public Dot Position => this._position;

    private CampType _camp;
    private decimal _influenceScopeRadius;
    private Dot _position;


    /// <summary>
    /// Construct a charging pile.
    /// </summary>
    /// <param name="position">The position</param>
    public ChargingPile(
        CampType camp,
        Dot position,
        decimal influenceScopeRadius = ChargingPile.DefaultInfluenceScopeRadius)
    {
        this._camp = camp;
        this._influenceScopeRadius = influenceScopeRadius;
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
            this._influenceScopeRadius;
    }
}