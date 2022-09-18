using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point2i = OpenCvSharp.Point;

namespace EDCHOST22
{
    //还没用上：这些全局函数可以用吗？如果能写成全局函数会比较好
    //商榷：这部分内容可能应该写在Game流程类里
    public static class MyConvert
    {
        //从格点转化为int，传入坐标，返回Dot
        //public static Dot CrossNo2Dot(int CrossNoX, int CrossNoY)
        //{
        //    int x = Game.MAZE_SHORT_BORDER_CM + Game.MAZE_CROSS_DIST_CM / 2 + Game.MAZE_CROSS_DIST_CM * CrossNoX;
        //    int y = Game.MAZE_SHORT_BORDER_CM + Game.MAZE_CROSS_DIST_CM / 2 + Game.MAZE_CROSS_DIST_CM * CrossNoY;
        //    Dot temp = new Dot(x, y);
        //    return temp;
        //}

        public static Point2i Dot2Point(Dot dot)
        {
            Point2i tmpPt = new Point2i(dot.x, dot.y);
            return tmpPt;
        }

        public static Dot Point2Dot(Point2i pt)
        {
            Dot tmpDot = new Dot(pt.X, pt.Y);
            return tmpDot;
        }
    }

}
