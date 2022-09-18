using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST22
{
    public enum MineType
    {
        A = 0, B, C, D  // 0,1,2,3
    }
    public class Mine      // 此类第一回合、第二回合通用，两个回合中用同一个MineGenerator生成矿
    {
        public const int MINE_TYPE = 4;
        public const int A = 1000000000;    // 金矿强度值计算参数

        public Dot Pos;       // 金矿初始位置
        public int Depth;           // 金矿初始深度
        public MineType Type;       // 金矿的种类

        //构造函数（含参）
        public Mine(Dot start_dt, int depth_, MineType type_)
        {
            Pos = start_dt;
            Depth = depth_;
            Type = type_;
        }

        // 构造函数（无参）
        public Mine()
        {
            Dot temp = new Dot(0, 0);
            Pos = temp;
            Depth = 0;
            Type = MineType.A;
        }

        //用于修改点位和类型的接口
        public void ResetInfo(Dot start_dt, int depth_, MineType type_)
        {
            Pos = start_dt;
            Depth = depth_;
            Type = type_;
        }

        
        // 获取某金矿对任意点处的强度
        static public int GetIntensity(Mine m, Dot d)
        {
            int depth = m.Depth < 1 ? 1 : m.Depth;
            int delta_x = Math.Abs(m.Pos.x - d.x) < 1 ? 1 : Math.Abs(m.Pos.x - d.x);
            int delta_y = Math.Abs(m.Pos.y - d.y) < 1 ? 1 : Math.Abs(m.Pos.y - d.y);
            return Convert.ToInt32(A * 1.0 / (Math.Pow(depth, 2) + Math.Pow(delta_x, 2) + Math.Pow(delta_y, 2)));
        }
    }
}
