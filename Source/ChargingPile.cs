namespace EdcHost;

/// <summary>
/// A charging pile
/// </summary>
public class ChargingPile
{
    #region Parameters

    /// <summary>
    /// The default value for the radius of the influence scope
    /// </summary>
    const int DefaultInfluenceScopeRadius = 20;

    #endregion

    #region Public properties

    /// <summary>
    /// The camp
    /// </summary>
    public CampType Camp => this._camp;

    /// <summary>
    /// The position
    /// </summary>
    public Dot Position => this._position;

    #endregion

    #region Private fields

    private CampType _camp;
    private decimal _influenceScopeRadius;
    private Dot _position;

    #endregion


    #region Public methods

    /// <summary>
    /// Construct a charging pile.
    /// </summary>
    /// <param name="position">The position</param>
    public ChargingPile(
        CampType camp,
        Dot position,
        long influenceScopeRadius = ChargingPile.DefaultInfluenceScopeRadius)
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

    #endregion
}