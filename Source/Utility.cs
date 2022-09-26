using System;

namespace EdcHost;

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