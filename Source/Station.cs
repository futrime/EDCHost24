using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EDCHOST24
{
    class Station //所有站点
    {
        static private int MAX_STATION = 3;


        static private List<Dot> mStationList1 = null; //一个包含站点位置信息的list
        static private List<Dot> mStationList2 = null;

        public Station() //构造函数
        {
            List<Dot> mStationList1 = new List<Dot>();
            List<Dot> mStationList2 = new List<Dot>();
        }

        public static void Reset()
        {
            mStationList1.Clear();
            mStationList2.Clear();
        }

        public static void Add(Dot _inPos, int _Type = 0)
        {
            if (_Type == 0 && mStationList1.Count < MAX_STATION)
            {
                mStationList1.Add(_inPos);
                Debug.WriteLine("Car A set charge station, {0}, {1}", _inPos.x, _inPos.y);
            }
            else if (_Type == 1 && mStationList2.Count < MAX_STATION)
            {
                mStationList2.Add(_inPos);
                Debug.WriteLine("Car B set charge station, {0}, {1}", _inPos.x, _inPos.y);
            }
        }

        public static bool isCollided(Dot _CarPos, int _Type, int r = 8)
        {
            if (_Type == 0)
            {
                foreach (Dot item in mStationList1)
                {
                    if (Dot.Distance(item, _CarPos) <= r)
                    {
                        return true;
                    }
                }
            }
            else if (_Type == 1)
            {
                foreach (Dot item in mStationList2)
                {
                    if (Dot.Distance(item, _CarPos) <= r)
                    {
                        return true;
                    }
                }
            }
            else if (_Type == 2)
            {
                foreach (Dot item in mStationList1)
                {
                    if (Dot.Distance(item, _CarPos) <= r)
                    {
                        return true;
                    }
                }

                foreach (Dot item in mStationList2)
                {
                    if (Dot.Distance(item, _CarPos) <= r)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Dot Index(int i, int _Type)
        {
            if (_Type == 0 && i < mStationList1.Count)
            {
                return mStationList1[i];
            }
            else if (_Type == 1 && i <  mStationList2.Count)
            {
                return mStationList2[i];
            }
            else
            {
                return new Dot(0xff, 0xff);
            }
        }

        public static List<Dot> StationOnStage(int _Type)
        {
            if (_Type == 0)
            {
                return mStationList1;
            }
            else if (_Type == 1)
            {
                return mStationList2;
            }
            else
            {
                return new List<Dot>();
            }
        }
    }

}