using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Station = EDCHOST24.Station;
using Labyrinth = EDCHOST24.Labyrinth;
using Boundary = EDCHOST24.Boundary;

namespace EDCHOST24
{
    // STL : Storage the Package
    public class PackageList //存储预备要用的物资信息
    {
        private static List<Package> mPackageList = null;

        private int X_MAX;
        private int X_MIN;
        private int Y_MAX;
        private int Y_MIN;
        private int LIMITED_TIME;
        private int TIME_INTERVAL;

        // point to the next generated package 
        public int mPointer;

        // stage represents first or second half of the race
        // stage == 0, first half
        // stage == 1, second half
        public PackageList(int _X_MAX, int _X_MIN, int _Y_MAX, int _Y_MIN, 
            int _INITIAL_AMOUNT, int _LIMITED_TIME, int _TIME_INTERVAL, int stage)
        {
            mPointer = _INITIAL_AMOUNT;

            X_MAX = _X_MAX;
            X_MIN = _X_MIN;
            Y_MAX = _Y_MAX;
            Y_MIN = _Y_MIN;
            LIMITED_TIME = _LIMITED_TIME;
            TIME_INTERVAL = _TIME_INTERVAL;

            mPackageList = new List<Package>();
            Random NRand = new Random();

            // initialize package at the beginning of game
            for (int i = 0; i < _INITIAL_AMOUNT; i++)
            {
                Dot Departure = new Dot(NRand.Next(X_MIN, X_MAX), NRand.Next(Y_MIN, Y_MAX));
                Dot Destination = new Dot(NRand.Next(X_MIN, X_MAX), NRand.Next(Y_MIN, Y_MAX));

                if (!(IsPosLegal(Departure, stage) && IsPosLegal(Destination, stage)))
                {
                    i--;
                    continue;
                }

                mPackageList.Add(new Package(Departure, Destination, 0, i));
            }


            // generate the time series for packages
            int LastGenerationTime = 0;
            for (int i = _INITIAL_AMOUNT; LastGenerationTime + TIME_INTERVAL <= LIMITED_TIME; i++)
            {
                Dot Departure = new Dot(NRand.Next(X_MIN, X_MAX), NRand.Next(Y_MIN, Y_MAX));
                Dot Destination = new Dot(NRand.Next(X_MIN, X_MAX), NRand.Next(Y_MIN, Y_MAX));

                if (!(IsPosLegal(Departure, stage) && IsPosLegal(Destination, stage)))
                {
                    i--;
                    continue;
                }

                int GenerationTime = NRand.Next(LastGenerationTime, LastGenerationTime + TIME_INTERVAL);

                LastGenerationTime = GenerationTime;
                mPackageList.Add(new Package(Departure, Destination, 0, i));
            }
        }


        public Package Index(int i)
        {
            return mPackageList[i];
        }

        public int Amount()
        {
            return mPackageList.Count();
        }

        public Package GeneratePackage()
        {
            return mPackageList[mPointer++];
        }

        
        public Package LastGenerationPackage()
        {
            return  mPackageList[mPointer - 1];
        }
        

        public Package NextGenerationPackage()
        {
            return mPackageList[mPointer];
        }

        public void ResetPointer()
        {
            mPointer = 0;
        }

        private static bool IsPosLegal(Dot _dot, int _Type = 0)
        {
            if (_Type == 0)
            {
                return !(Boundary.isCollided(_dot) || Labyrinth.isCollided(_dot));
            }
            else if (_Type == 1)
            {
                return !(Boundary.isCollided(_dot) || Labyrinth.isCollided(_dot) || Station.isCollided(_dot, 0));
            }

            return false;
        }
    }
}