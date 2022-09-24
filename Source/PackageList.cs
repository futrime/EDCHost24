using System;
using System.Collections.Generic;

namespace EdcHost;

/// <summary>
/// A list of packages
/// </summary>
public class PackageList
{
    public const int MaxPackageNumber = 20;

    public static List<Package> mPackageList;
    // public static int InitialPackageNumber = 1;
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
        #region Initialize fields

        mPackageList = new List<Package>();
        X_MAX = _X_MAX;
        X_MIN = _X_MIN;
        Y_MAX = _Y_MAX;
        Y_MIN = _Y_MIN;
        LIMITED_TIME = _LIMITED_TIME;
        TIME_INTERVAL = _TIME_INTERVAL;
        mPointer = _INITIAL_AMOUNT;

        #endregion


        var random = new Random((int)DateTime.Now.Ticks);
        // 生成package的时间下限，值会随循环变动
        int begin_time = 0;
        // 步长，包裹生成时间为(begin_time, begin_time + step_time)
        int step_time = this.LIMITED_TIME / PackageList.MaxPackageNumber;

        for (int i = 0; i < PackageList.MaxPackageNumber; ++i)
        {
            Dot departure = new Dot(random.Next(X_MIN, X_MAX), random.Next(Y_MIN, Y_MAX));
            Dot destination = new Dot(random.Next(X_MIN, X_MAX), random.Next(Y_MIN, Y_MAX));

            int generationTime = random.Next(begin_time, begin_time + step_time);
            begin_time += step_time;

            mPackageList.Add(new Package(departure, destination, generationTime, i));
        }
    }

    public Package Index(int i)
    {
        return mPackageList[i];
    }

    public int Amount()
    {
        return mPackageList.Count;
    }

    public Package GeneratePackage()
    {
        Package package = PackageList.mPackageList[this.mPointer];
        ++this.mPointer;
        return package;
    }


    public Package LastGenerationPackage()
    {
        return mPackageList[mPointer - 1];
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
            return !(Obstacle.isCollided(_dot));
        }
        else if (_Type == 1)
        {
            return !(Obstacle.isCollided(_dot) || Station.isCollided(_dot, 0));
        }

        return false;
    }
}