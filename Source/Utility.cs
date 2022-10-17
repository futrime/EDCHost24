using System;

namespace EdcHost;

/// <summary>
/// Utilities for the program
/// </summary>
static class Utility
{
    #region Public properties.

    /// <summary>
    /// A random number generator.
    /// </summary>
    public static Random RandomGenerator = new Random((int)Utility.SystemTime);

    /// <summary>
    /// The system time.
    /// </summary>
    public static long SystemTime
    {
        get
        {
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }
    }

    #endregion

    #region Private fields.

    // Note that static local variables are not permitted in C#.
    private static int _lastUniqueId = 0;

    #endregion


    #region Public methods.

    /// <summary>
    /// Generate a unique ID.
    /// </summary>
    /// <returns>The unique ID.</returns>
    public static int GenerateUniqueId()
    {
        ++Utility._lastUniqueId;

        return Utility._lastUniqueId;
    }

    #endregion
}