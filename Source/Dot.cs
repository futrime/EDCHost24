using System;
using OpenCvSharp;

namespace EdcHost;

/// <summary>
/// A 2-dimenstion dot
/// </summary>
public struct Dot
{
    /// <summary>
    /// The x-coordinate of the dot
    /// </summary>
    public int X
    {
        get => this._x;
        set => this._x = value;
    }
    /// <summary>
    /// The y-coordinate of the dot
    /// </summary>
    public int Y
    {
        get => this._y;
        set => this._y = value;
    }

    private int _x;
    private int _y;


    /// <summary>
    /// Get the Manhattan distance between two dots.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns>
    /// The distance. Null if the distance type is wrong.
    /// </returns>
    public static decimal? Distance(
        Dot A,
        Dot B,
        DotDistanceType distanceType = DotDistanceType.Manhattan
    )
    {
        switch (distanceType)
        {
            case DotDistanceType.Euclidean:
                return (decimal)Math.Sqrt(
                    Math.Pow(A.X - B.X, 2) +
                    Math.Pow(A.Y - B.Y, 2)
                );

            case DotDistanceType.Manhattan:
                return (decimal)(
                    Math.Abs(A.X - B.X) +
                    Math.Abs(A.Y - B.Y)
                );

            default:
                return null;
        }
    }

    public Dot(int x, int y)
    {
        this._x = x;
        this._y = y;
    }

    public Dot(Point point)
    {
        this._x = point.X;
        this._y = point.Y;
    }

    public static bool operator ==(Dot a, Dot b)
    {
        return (a.X == b.X) && (a.Y == b.Y);
    }

    public static bool operator !=(Dot a, Dot b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Convert the dot to a Point object
    /// </summary>
    /// <returns>The Point object</returns>
    public Point ToPoint()
    {
        return new Point(this._x, this._y);
    }
}