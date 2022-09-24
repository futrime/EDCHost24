using System;
using OpenCvSharp;

namespace EdcHost;

public class Utilities
{
    public static int DistanceL(Wall wall, Dot CarPos)
    {
        int sameP, bigP, smallP, sameC, diffC;
        // 相同的坐标，不同坐标中较大的，不同坐标中较小的,
        // 相同坐标对应的小车坐标，不同的坐标对应小车坐标
        if (wall.w1.y == wall.w2.y)
        {
            sameP = wall.w1.y;
            sameC = CarPos.y;
            diffC = CarPos.x;
            if (wall.w1.x > wall.w2.x)
            {
                bigP = wall.w1.x;
                smallP = wall.w2.x;
            }
            else
            {
                bigP = wall.w1.x;
                smallP = wall.w2.x;
            }
        }
        else
        {
            sameP = wall.w1.x;
            sameC = CarPos.x;
            diffC = CarPos.y;
            if (wall.w1.y > wall.w2.y)
            {
                bigP = wall.w1.y;
                smallP = wall.w2.y;
            }
            else
            {
                bigP = wall.w1.y;
                smallP = wall.w2.y;
            }
        }

        //如果小车在两个障碍点之间，计算垂直距离
        if (smallP <= diffC && diffC <= bigP)
        {
            return Math.Abs(sameP - sameC);
        }
        // 否则计算两点距离
        else
        {
            int d1 = Math.Min(Math.Abs(smallP - diffC), Math.Abs(bigP - diffC));
            int d2 = Math.Abs(sameP - sameC);
            return (int)Math.Sqrt(d1 * d1 + d2 * d2);
        }
    }

    public static int DistanceP(Dot p1, Dot p2)
    {
        int d1 = Math.Abs(p1.x - p2.x);
        int d2 = Math.Abs(p1.y - p2.y);
        return (int)Math.Sqrt(d1 * d1 + d2 * d2);
    }

    public static Point Dot2Point(Dot dot)
    {
        Point tmpPt = new Point(dot.x, dot.y);
        return tmpPt;
    }

    public static Dot Point2Dot(Point pt)
    {
        Dot tmpDot = new Dot(pt.X, pt.Y);
        return tmpDot;
    }
}