using System.Collections.Generic;

namespace EdcHost;

/// <summary>
/// The camp type
/// </summary>
public enum CampType
{
    None,
    A,
    B
};

/// <summary>
/// The configuration type
/// </summary>
public struct ConfigType
{
    public struct PerVehicleConfigType
    {
        public LocatorConfigType Locator;
        public bool ShowMask;
        public string SerialPort;
        public int Baudrate;
    }

    public Dictionary<CampType, PerVehicleConfigType> Vehicles;
    public int Camera;
}

/// <summary>
/// The type of the distance between two dots.
/// </summary>
public enum DotDistanceType
{
    Euclidean,
    Manhattan
}

/// <summary>
/// The game stage type
/// </summary>
public enum GameStageType
{
    /// <summary>
    /// The pre-match stage.
    /// </summary>
    PreMatch,

    /// <summary>
    /// The first half
    /// </summary>
    FirstHalf,

    /// <summary>
    /// The second half
    /// </summary>
    SecondHalf
};

/// <summary>
/// The game state type
/// </summary>
public enum GameStateType
{
    /// <summary>
    /// The game has not started yet
    /// </summary>
    Unstarted,

    /// <summary>
    /// The game is in progress
    /// </summary>
    Running,

    /// <summary>
    /// The game is paused
    /// </summary>
    Paused,

    /// <summary>
    /// The game is ended
    /// </summary>
    Ended
};

/// <summary>
/// The configuration type
/// </summary>
public struct LocatorConfigType
{
    public (int Min, int Max) Hue;
    public (int Min, int Max) Saturation;
    public (int Min, int Max) Value;
    public decimal MinArea;
}

/// <summary>
/// The order status enum type
/// </summary>
public enum OrderStatusType
{
    /// <summary>
    /// The order is not generated.
    /// </summary>
    Ungenerated,
    /// <summary>
    /// The order is ready for picking up.
    /// </summary>
    Pending,
    /// <summary>
    /// The order is in delivery.
    /// </summary>
    InDelivery,
    /// <summary>
    /// The order is delivered.
    /// </summary>
    Delivered
}