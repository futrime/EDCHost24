namespace EdcHost;

/// <summary>
/// The camp
/// </summary>
public enum Camp
{
    None,
    A,
    B
};

/// <summary>
/// The game stage
/// </summary>
public enum GameStage
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
/// The game state
/// </summary>
public enum GameState
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