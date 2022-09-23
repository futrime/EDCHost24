namespace EdcHost;

/// <summary>
/// The camp
/// </summary>
public enum Camp
{
    NONE = 0,
    A = 1,
    B = 2
};

/// <summary>
/// The game state
/// </summary>
public enum GameState
{
    /// <summary>
    /// The game has not started yet
    /// </summary>
    UNSTART = 0,

    /// <summary>
    /// The game is in progress
    /// </summary>
    RUN = 1,

    /// <summary>
    /// The game is paused
    /// </summary>
    PAUSE = 2,

    /// <summary>
    /// The game is ended
    /// </summary>
    END = 3
};

/// <summary>
/// The game stage
/// </summary>
public enum GameStage
{
    /// <summary>
    /// Undefined stage
    /// </summary>
    NONE = 0,

    /// <summary>
    /// The first half
    /// </summary>
    FIRST_HALF = 1,

    /// <summary>
    /// The second half
    /// </summary>
    SECOND_HALF = 2
};

public enum PackageStatus
{
    UNPICKED = 0,

    PICKED = 1,
    ARRIVED = 2
};
