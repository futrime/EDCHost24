using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST22
{
    //0813xhl把struct改成了class
    public class Dot //点
    {
        public int x;
        public int y;
        //构造函数
        //8-14 yd添加了默认构造值
        public Dot(int _x = 0, int _y = 0) { x = _x; y = _y; }

        //运算符重载
        public static bool operator ==(Dot a, Dot b)
        {
            return (a.x == b.x) && (a.y == b.y);
        }

        public static bool operator !=(Dot a, Dot b)
        {
            return !(a == b);
        }

        public void SetInfo(int x_, int y_)
        {
            this.x = x_;
            this.y = y_;
        }

        // 两点间距
        public static double GetDistance(Dot d1, Dot d2)        
        {
            return Math.Sqrt(Math.Pow(d1.x - d2.x, 2) + Math.Pow(d1.y - d2.y, 2));
        }

        // 两点间距是否小于某定值dist
        // dist默认值为碰撞半径，也可设为金矿间最小间距Court.MINE_LOWERDIST_CM
        // 用于判断碰撞或金矿间距
        public static bool InCollisionZone(Dot d1, Dot d2, int dist = Court.COINCIDE_ERR_DIST_CM)      
        {
            return GetDistance(d1, d2) < dist;
        }

        // 判断d是否与dArray中的某一个点的间距小于某定值dist
        // dist参数含义同上
        public static bool InCollisionZones(Dot d, Dot[] dArray, int dist = Court.COINCIDE_ERR_DIST_CM)   
        {
            foreach(Dot temp in dArray)
            {
                if(InCollisionZone(d, temp, dist))//有一个点满足条件即可返回true
                {
                    return true;
                }
            }
            //每个点均不满足则返回false
            return false;
        }

    }
}
