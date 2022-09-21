using System;
using System.Math;
namespace EDCHOST24
{
    public class Boundary
    {
        // 存储所有黑实线边界数据的数组
        private Wall[] mWall;
        private readonly int mMap;
        private readonly int mSize;
        private readonly int mEntrance;
        private readonly int mWidth;

        // 构造函数
        //四个参数分别是：地图的边长，分界线到地图边缘的最短距离，入口的大小，双实线的宽度
        //这里，由于围墙具有对称性，所以我们只计算四段围墙，计算时，将小车坐标对称变换到第一象限
        public Boundary(int map, int size, int entrance, int width)
        {
            Wall[] mWall = new Wall[4];
            mMap = map;
            mSize  =size;
            mEntrance = entrance;
            mWidth = width;
            // 双实线内侧到边界的距离
        int size1 = size + width;
            // 双实线的长度
            int height = (map - 2 * size - entrance) / 2;
            mWall[0] = Wall(Dot(size, size), Dot(size, size + height));
            mWall[1] = Wall(Dot(size1, size1), Dot(size1, size + height));
            mWall[2] = Wall(Dot(size, size), Dot(size + height, size));
            mWall[3] = Wall(Dot(size1, size1), Dot(size + height, size1));
        }

        public static bool isCollided(Dot CarPos, int radius = 8)
        {
            if (Carpos.x > mMap / 2)
            {
                CarPos.x = mMap - CarPos.x;
            }
            if (Carpos.y > mMap / 2)
            {
                CarPos.y = mMap - CarPos.y;
            }
            foreach (Wall wall in mWall)
            {
                if (Utility.DistanceL(wall, CarPos) < radius)
                {
                    return true;
                }
            }
            return false;
        }
    }

}