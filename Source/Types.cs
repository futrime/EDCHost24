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
/// The game stage type
/// </summary>
public enum GameStageType
{
    /// <summary>
    /// Undefined stage
    /// </summary>
    None,

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
public struct ConfigType
{
    public struct PerVehicleConfigType
    {
        public Locator.ConfigType Locator;
        public bool ShowMask;
        public string SerialPort;
        public int Baudrate;
    }

    public Dictionary<CampType, PerVehicleConfigType> Vehicles;
    public int Camera;
}