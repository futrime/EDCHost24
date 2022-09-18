using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST22
{
    
    public class Beacon
    {
        //如果设置为const,在Game.cs部分无法使用
        public const int MAX_BEACON_NUM = 3;  //一辆车最大允许放置的信标数目为3

        //CarA放置的信标
        public Dot[] CarABeacon;
        public MineType[] CarABeaconMineType;
        //CarB放置的信标
        public Dot[] CarBBeacon;
        public MineType[] CarBBeaconMineType;
        //CarA放置的信标数量
        public int CarABeaconNum;
        //CarB放置的信标数量
        public int CarBBeaconNum;
        //构造函数
        public Beacon()
        {
            CarABeacon = new Dot[MAX_BEACON_NUM];
            CarABeaconMineType = new MineType[MAX_BEACON_NUM];
            for (int i = 0; i < MAX_BEACON_NUM; i++)
            {
                CarABeacon[i] = new Dot();
                CarABeaconMineType[i] = MineType.A;
            }
            CarBBeacon = new Dot[MAX_BEACON_NUM];
            CarBBeaconMineType = new MineType[MAX_BEACON_NUM];
            for (int i = 0; i < MAX_BEACON_NUM; i++)
            {
                CarBBeacon[i] = new Dot();
                CarBBeaconMineType[i] = MineType.A;
            }
            CarABeaconNum = 0;
            CarBBeaconNum = 0;
        }

        //重设信标
        public void Reset()
        {
            CarABeaconNum = 0;
            CarBBeaconNum = 0;
        }
        //CarA放置信标
        public void CarAAddBeacon(Dot Pos, MineType type)
        {
            //放置的信标不多于MaxBeaconNum
            if (CarABeaconNum < MAX_BEACON_NUM)
            {
                CarABeacon[CarABeaconNum] = Pos;
                CarABeaconMineType[CarABeaconNum] = type;  
                CarABeaconNum++;
            }
        }
        //CarB放置信标
        public void CarBAddBeacon(Dot Pos, MineType type)
        {
            if (CarBBeaconNum < MAX_BEACON_NUM)
            {
                CarBBeacon[CarBBeaconNum] = Pos;
                CarBBeaconMineType[CarBBeaconNum] = type;
                CarBBeaconNum++;
            }
        }
        //得到CarA同六个信标的距离，返回6个数字，前三个为自己放置的信标，后三个为另一组放置的信标，
        //如果某一方未放置够3个信标，未放置的信标所返回的距离为-1
        public double[] GetCarADistance(Dot Pos)
        {
            double[] Distance = new double[MAX_BEACON_NUM * 2];
            for (int i = 0; i < MAX_BEACON_NUM; i++)
            {
                if (i < CarABeaconNum)
                {
                    Distance[i] = Dot.GetDistance(Pos, CarABeacon[i]);
                }
                else
                {
                    Distance[i] = -1;
                }
            }
            for (int i = 0; i < MAX_BEACON_NUM; i++)
            {
                if (i < CarBBeaconNum)
                {
                    Distance[i + MAX_BEACON_NUM] = Dot.GetDistance(Pos, CarBBeacon[i]);
                }
                else
                {
                    Distance[i + MAX_BEACON_NUM] = -1;
                }
            }
            return Distance;
        }
        //得到CarB同六个信标的距离，返回6个数字，前三个为自己放置的信标，后三个为另一组放置的信标，
        //如果某一方未放置够3个信标，未放置的信标所返回的距离为-1
        public double[] GetCarBDistance(Dot Pos)
        {
            double[] Distance = new double[MAX_BEACON_NUM * 2];
            for (int i = 0; i < MAX_BEACON_NUM; i++)
            {
                if (i < CarBBeaconNum)
                {
                    Distance[i] = Dot.GetDistance(Pos, CarBBeacon[i]);
                }
                else
                {
                    Distance[i] = -1;
                }
            }
            for (int i = 0; i < MAX_BEACON_NUM; i++)
            {
                if (i < CarABeaconNum)
                {
                    Distance[i + MAX_BEACON_NUM] = Dot.GetDistance(Pos, CarABeacon[i]);
                }
                else
                {
                    Distance[i + MAX_BEACON_NUM] = -1;
                }
            }
            return Distance;
        }
    }
}
