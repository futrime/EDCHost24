using System;

namespace EdcHost;

/// <summary>
/// Utilities for the program
/// </summary>
static class Utility
{
    public static long SystemTime
    {
        get
        {
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }
    }
}