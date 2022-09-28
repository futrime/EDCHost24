using System;
using System.Collections.Generic;
using System.Linq;

namespace EdcHost;

/// <summary>
/// A vehicle
/// </summary>
public class Vehicle
{
    #region Parameters

    /// <summary>
    /// The default value of the max distance in centimeters
    /// </summary>
    private const int DefaultInitialMaxDistance = 1000;

    #endregion


    #region Public properties

    /// <summary>
    /// The camp
    /// </summary>
    public CampType Camp => this._camp;

    /// <summary>
    /// A list of the orders carried by the vehicle
    /// </summary>
    public List<Order> DeliveringOrderList => this._deliveringOrderList;

    /// <summary>
    /// The distance the vehicle has travelled
    /// </summary>
    public int Distance
    {
        get
        {
            int distance = 0;

            for (int i = 1; i < this._path.Count; ++i)
            {
                distance += Dot.Distance(this._path[i - 1], this._path[i]);
            }

            // The distance should not exceed the max distance.
            distance = Math.Min(distance, this._maxDistance);

            return distance;
        }
    }

    /// <summary>
    /// True if the power is exhausted; otherwise false
    /// </summary>
    public bool IsPowerExhausted
    {
        get
        {
            return (this.MaxDistance == this.Distance);
        }
    }

    /// <summary>
    /// The maximum distance the vehicle can travel
    /// </summary>
    public int MaxDistance => this._maxDistance;

    /// <summary>
    /// The duration that the vehicle has not moved.
    /// </summary>
    public long? ParkingDuration
    {
        get {
            if (
                this._lastGameTime != null &&
                this._lastStartParkingTime != null
            )
            {
                return ((long)this._lastGameTime - (long)this._lastStartParkingTime);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// The path the vehicle has travelled
    /// </summary>
    public List<Dot> Path => this._path;

    /// <summary>
    /// The position of the vehicle
    /// </summary>
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

    /// <summary>
    /// The distance remaining the vehicle can go
    /// </summary>
    public int RemainingDistance
    {
        get
        {
            return this.MaxDistance - this.Distance;
        }
    }

    /// <summary>
    /// The ratio of the remaining power to the full
    /// power
    /// </summary>
    public decimal RemainingPowerRatio
    {
        get
        {
            return (decimal)this.RemainingDistance / this._initialMaxDistance;
        }
    }

    #endregion

    #region Private fields

    private CampType _camp;
    private List<Order> _deliveringOrderList = new List<Order>();
    private readonly int _initialMaxDistance;
    private long? _lastGameTime = null;
    private long? _lastStartParkingTime = null;
    private int _maxDistance;
    private List<Dot> _path = new List<Dot>();

    #endregion


    #region Public methods

    /// <summary>
    /// Construct a Vehicle object.
    /// </summary>
    /// <param name="camp">The camp</param>
    /// <param name="initialMaxDistance">The initial maximum distance</param>
    public Vehicle(
        CampType camp,
        int initialMaxDistance = Vehicle.DefaultInitialMaxDistance
    )
    {
        this._camp = camp;
        this._initialMaxDistance = initialMaxDistance;
        this._maxDistance = this._initialMaxDistance;
    }

    /// <summary>
    /// Increase the maximum distance.
    /// </summary>
    /// <param name="increment">
    /// The increment of the maximum distance
    /// </param>
    public void IncreaseMaxDistance(int increment)
    {
        this._maxDistance += increment;
        this._maxDistance = Math.Min(this._maxDistance, this.Distance + this._initialMaxDistance);
        this._maxDistance = Math.Max(this._maxDistance, this.Distance);
    }

    /// <summary>
    /// Update the position of the vehicle.
    /// </summary>
    /// <param name="position">The position</param>
    /// <param name="gameTime">The game time</param>
    public void UpdatePosition(Dot position, long gameTime)
    {
        this.Path.Add(position);

        this._lastGameTime = gameTime;

        // If there are at least two positions in the path.
        if (
            this.Path.Count > 1 &&
            this.Path[this.Path.Count - 1] == this.Path[this.Path.Count - 2]
        )
        {
            // If the vehicle starts to park, update the last start parking time.
            if (this._lastStartParkingTime == null)
            {
                this._lastStartParkingTime = gameTime;
            }
        }
    }

    #endregion
}