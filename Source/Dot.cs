using System;

namespace EdcHost;

/// <summary>
/// A 2-dimenstion dot
/// </summary>
public class Dot
{
    public int x;
    public int y;

    public Dot(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(Dot a, Dot b)
    {
        return (a.x == b.x) && (a.y == b.y);
    }

    public static bool operator !=(Dot a, Dot b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Get the Euclidean distance between two dots.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns>The distance</returns>
    public static int Distance(Dot A, Dot B)
    {
        return (int)Math.Sqrt((A.x - B.x) * (A.x - B.x)
            + (A.y - B.y) * (A.y - B.y));
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}