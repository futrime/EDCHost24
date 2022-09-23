using System;
//using Math; unneeded

namespace EdcHost;
public class Dot
{
    public int x;
    public int y;

    public Dot(int _x = 0, int _y = 0)
    {
        x = _x;
        y = _y;
    }

    public static bool operator ==(Dot a, Dot b)
    {
        return (a.x == b.x) && (a.y == b.y);
    }

    public static bool operator !=(Dot a, Dot b)
    {
        return !(a == b);
    }

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