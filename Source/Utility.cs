using System;
using System.Math;
namespace EDCHOST24
{
    public class Utility
    {
        // 计算点到线段的最小距离
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
                    samllP = wall.w2.x;
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
                    samllP = wall.w2.y;
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
                return Abs(sameP - sameC);
            }
            // 否则计算两点距离
            else
            {
                int d1 = Min(Abs(smallP - diffC), Abs(bigP - diffC));
                int d2 = Abs(sameP - sameC);
                return Sqrt(d1 ** d1 + d2 ** d2);
            }
        }

        // 计算两点之间的距离
        public static int DistanceP(Dot p1, Dot p2)
        {
            int d1 = Abs(p1.x - p2.x);
            int d2 = Abs(p1.y - p2.y);
            return (int)Sqrt(d1 * d1 + d2 * d2);
        }
    }
}