using System;

namespace EdcHost;

static class Utility
{
    public static long SystemTime =>
        DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
}