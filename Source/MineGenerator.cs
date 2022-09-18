using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST22
{
    public class MineGenerator    //进行各回合金矿的生成
    {
        public const int COURTMINENUM = 2;       // 同时存在的金矿数
        public const int MINELISTNUM = 500;      //第二回合可取用的金矿总数
        
        public Mine[] MineArray1;        // 第一回合设置金矿的数组
        public MineType[] ParkType;            // 编号为i的停车点存储的金矿种类
        public Mine[] MineArray2;        // 第二回合金矿数组
        public int Mine_id;              // 第二回合该取下标为Mine_id的金矿了

        public MineGenerator()         // 构造函数
        {
            
            Mine_id = 0;

            MineArray1 = new Mine[COURTMINENUM];
            for (int i = 0; i < COURTMINENUM; i++)
            {
                MineArray1[i] = new Mine();
            }
            MineArray2 = new Mine[MINELISTNUM];
            for (int i = 0; i < MINELISTNUM; i++)
            {
                MineArray2[i] = new Mine();
            }
            Random ran = new Random();
            ParkType = new MineType[Court.TOTAL_PARKING_AREA];
            for (int i = 0;i < Court.TOTAL_PARKING_AREA; i++)
            {
                int temp;
                int cnt = 0;
                do
                {
                    temp = ran.Next(0,4);
                    cnt = 0;
                    for (int j = 0; j < i; j++)
                    {
                        cnt += ((int)ParkType[j] == (int)temp) ? 1 : 0;
                    }
                } while (cnt >= 2);
                ParkType[i] = (MineType)temp;
            }
            
            //生成第一回合要用到的两个矿
            int stage1_mine1_x = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
            int stage1_mine1_y = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
            int stage1_mine1_d = ran.Next(Court.MIN_MINE_DEPTH, Court.MAX_MINE_DEPTH + 1);   //单参数Next含上界
            Dot stage1_mine1_xy = new Dot(stage1_mine1_x, stage1_mine1_y);
            MineType stage1_mine1_type = (MineType)ran.Next(0, 4);
            Mine stage1_mine1 = new Mine(stage1_mine1_xy, stage1_mine1_d, stage1_mine1_type);

            int stage1_mine2_x = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
            int stage1_mine2_y = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
            Dot stage1_mine2_xy = new Dot(stage1_mine2_x, stage1_mine2_y);
            int stage1_mine2_d = ran.Next(Court.MIN_MINE_DEPTH, Court.MAX_MINE_DEPTH + 1);
            MineType stage1_mine2_type = (MineType)ran.Next(0, 4);
            while (Dot.InCollisionZone(stage1_mine1_xy, stage1_mine2_xy, Court.MINE_LOWERDIST_CM))
            {
                stage1_mine2_x = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
                stage1_mine2_y = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
                stage1_mine2_xy = new Dot(stage1_mine2_x, stage1_mine2_y);
            }
            Mine stage1_mine2 = new Mine(stage1_mine2_xy, stage1_mine2_d, stage1_mine2_type);

            MineArray1[0] = stage1_mine1;
            MineArray1[1] = stage1_mine2;

        }

        //返回停车点对应的存储类型
        public MineType[] GetParkType()
        {
            return ParkType;
        }

        //返回第一回合的金矿组
        public Mine[] GetStage1Mine()
        {
            return MineArray1;
        }

        //检查i号金矿与前四个大于金矿最小距离
        public bool MinesApart(Dot mine_xy, int i)
        {
            bool flag = true;
            for (int j = i >= 4 ? i - 4 : 0; j < i; j++)
            {
                if (Dot.InCollisionZone(mine_xy, MineArray2[j].Pos, Court.MINE_LOWERDIST_CM))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }


        //生成第二回合的金矿组
        public void GenerateStage2(Beacon beacon)
        {
            Random ran = new Random();
            Dot[] beacon_loc = new Dot[beacon.CarABeaconNum + beacon.CarBBeaconNum];
            int count = 0;
            for (int i = 0; i < beacon.CarABeaconNum; i++)
            {
                beacon_loc[count++] = beacon.CarABeacon[i];
            }
            for (int i = 0; i < beacon.CarBBeaconNum; i++)
            {
                beacon_loc[count++] = beacon.CarBBeacon[i];
            }

            for (int i = 0; i < MINELISTNUM; i++)
            {
                int stage2_mine_x = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
                int stage2_mine_y = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
                int stage2_mine_d = ran.Next(Court.MIN_MINE_DEPTH, Court.MAX_MINE_DEPTH + 1);
                Dot stage2_mine_xy = new Dot(stage2_mine_x, stage2_mine_y);
                while (!MinesApart(stage2_mine_xy, i) || Dot.InCollisionZones(stage2_mine_xy, beacon_loc,16))
                {
                    stage2_mine_x = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
                    stage2_mine_y = ran.Next(Court.BORDER_CM, Court.MAX_SIZE_CM + 1 - Court.BORDER_CM);
                    stage2_mine_xy = new Dot(stage2_mine_x, stage2_mine_y);
                }
                Mine stage2_mine = new Mine(stage2_mine_xy, stage2_mine_d, (MineType)ran.Next(0,4));
                MineArray2[i] = stage2_mine;
            }
        }

        //第二回合中，返回下一个列表中的金矿
        public Mine GetNextMine()
        {
            return MineArray2[Mine_id++];
        }

        //恢复mine_id为0供B车使用
        public void Reset()
        {
            Mine_id = 0;
        }
    }
}