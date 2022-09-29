using System;

namespace EdcHost;

/// <summary>
/// Utilities for the program
/// </summary>
static class Utility
{
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
}