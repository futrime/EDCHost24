using System.Collections.Generic;
using System.Linq;

namespace EdcHost;

public class Vehicle
{
    #region Parameters

    /// <summary>
    /// The default value of the charging rate in centimeters
    /// per millisecond
    /// </summary>
    private const decimal DefaultChargingRate = 0.1M;

    /// <summary>
    /// The default value of the max distance in centimeters
    /// </summary>
    private const int DefaultInitialMaxDistance = 1000;

    #endregion


    #region Public properties

    public CampType Camp => this._camp;

    public List<Order> DeliveringOrderList => this._deliveringOrderList;

    public int Distance
    {
        get
        {
            int distance = 0;

            for (int i = 1; i < this._path.Count; ++i)
            {
                distance += Dot.Distance(this._path[i - 1], this._path[i]);
            }

            return distance;
        }
    }

    public int MaxDistance =>
        (int)(this._chargingTime * this._chargingRate) + this._initialMaxDistance;

    public List<Dot> Path => this._path;

    public Dot Position
    {
        get
        {
            if (this.Path.Count == 0)
            {
                return null;
            }

            return this.Path.Last();
        }
    }

    #endregion

    #region Private fields

    private CampType _camp;
    private decimal _chargingRate;
    private long _chargingTime = 0;
    private List<Order> _deliveringOrderList = new List<Order>();
    private int _initialMaxDistance;
    private List<Dot> _path = new List<Dot>();

    #endregion


    #region Public methods

    public Vehicle(
        CampType camp,
        decimal chargingRate = Vehicle.DefaultChargingRate,
        int initialMaxDistance = Vehicle.DefaultInitialMaxDistance
    )
    {
        this._camp = camp;
        this._chargingRate = chargingRate;
        this._initialMaxDistance = initialMaxDistance;
    }

    public void Reset()
    {
        this._path.Clear();
    }

    #endregion
}