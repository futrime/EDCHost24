using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EdcHost;

public class Game
{
    // size of competition area
    // 最大游戏场地
    public const int MAX_SIZE = 254;
    public const int AVAILABLE_MAX_X = 254;
    public const int AVAILABLE_MIN_X = 0;
    public const int AVAILABLE_MAX_Y = 254;
    public const int AVAILABLE_MIN_Y = 0;
    public const int EDGE_DISTANCE = 28;
    public const int ENTRANCE_WIDTH = 36;
    public const int LINE_WIDTH = 2;

    // size of car
    public const int COLLISION_RADIUS = 8;

    // initial amount of package
    public const int INITIAL_PKG_NUM = 1;
    // time interval of packages
    public const int TIME_INTERVAL = 1500;

    // time of first and second half
    public const int FIRST_HALF_TIME = 60000;
    public const int SECOND_HALF_TIME = 180000;

    // Message Token
    public const byte START = 0xff;
    public const byte END = 0;

    // state
    public GameStage mGameStage;
    public GameState mGameState;

    // Time
    // Set time zero as the start time of each race
    private long mStartTime; // system time, update for each race
    private long mGameTime;

    public long GameTime
    {
        get => this.mGameTime;
    }

    private long mTimeRemain;

    public long RemainingTime
    {
        get => this.mTimeRemain;
    }

    /// <summary>
    /// The sum of the time penalty
    /// </summary>
    private int _timePenaltySum = 0;

    // car and package
    private Car mCarA, mCarB;

    private int[] mScoreA, mScoreB;

    private PackageList mPackageFirstHalf;
    private PackageList mPackageSecondHalf;

    // store the packages on the field
    private List<Package> mPackagesRemain;

    // Charge station set by Team A and B
    private Station mChargeStation;

    // which team is racing A or B
    private Camp mCamp;

    // obstacle
    private Obstacle mObstacle;

    private Boundary mBoundary;

    // flags represents whether package list has been generated
    private bool hasFirstPackageListGenerated;
    private bool hasSecondPackageListGenerated;
    public bool DebugMode = false;

    /***********************************************************************
    Interface used for Tracker to display the information of current game
    ***********************************************************************/
    public Game()
    {
        Debug.WriteLine("Call Constructor of Game");

        mGameState = GameState.UNSTART;
        mGameStage = GameStage.NONE;

        mCamp = Camp.NONE;

        mCarA = new Car(Camp.A);
        mCarB = new Car(Camp.B);

        mScoreA = new int[] { 0, 0 };
        mScoreB = new int[] { 0, 0 };

        hasFirstPackageListGenerated = false;
        hasSecondPackageListGenerated = false;

        mChargeStation = new Station();

        mPackagesRemain = new List<Package>();

        mStartTime = -1;
        mGameTime = -1;
        mTimeRemain = 0;

        mBoundary = new Boundary();

        this.mPackageFirstHalf = new PackageList(AVAILABLE_MAX_X, AVAILABLE_MIN_X,
                AVAILABLE_MAX_Y, AVAILABLE_MIN_Y, INITIAL_PKG_NUM, FIRST_HALF_TIME, TIME_INTERVAL, 0);

        this.mPackageSecondHalf = new PackageList(AVAILABLE_MAX_X, AVAILABLE_MIN_X,
                AVAILABLE_MAX_Y, AVAILABLE_MIN_Y, INITIAL_PKG_NUM, FIRST_HALF_TIME, TIME_INTERVAL, 1);
    }


    /***********************************************
    Update on each frame
    ***********************************************/
    public void UpdateOnEachFrame(Dot _CarPos)
    {
        // check the game state
        if (mGameState == GameState.UNSTART || mGameState == GameState.PAUSE || mGameState == GameState.END)
        {
            return;
        }

        if (mCamp == Camp.NONE)
        {
            throw new Exception("The camp is invalid.");
        }

        _UpdateGameTime();

        // Try to generate packages on each refresh
        _GeneratePackage();

        int TimePenalty = 0;

        // Update car's info on each frame
        if (mCamp == Camp.A)
        {
            mCarA.Update(_CarPos, (int)mGameTime, _IsOnBlackLine(_CarPos),
            _IsInObstacle(_CarPos), _IsInOpponentStation(_CarPos),
            _IsInChargeStation(_CarPos), ref mPackagesRemain, out TimePenalty);
        }
        else if (mCamp == Camp.B)
        {
            mCarB.Update(_CarPos, (int)mGameTime, _IsOnBlackLine(_CarPos),
            _IsInObstacle(_CarPos), _IsInOpponentStation(_CarPos),
            _IsInChargeStation(_CarPos), ref mPackagesRemain, out TimePenalty);
        }

        if (this.mGameState == GameState.RUN)
        {
            // Calculate the remaining time
            int gameDuration = 0;
            switch (this.mGameStage)
            {
                case GameStage.FIRST_HALF:
                    gameDuration = Game.FIRST_HALF_TIME;
                    break;
                case GameStage.SECOND_HALF:
                    gameDuration = Game.SECOND_HALF_TIME;
                    break;
                default:
                    break;
            }

            this._timePenaltySum += TimePenalty;
            // this.mTimeRemain = gameDuration - this._timePenaltySum - mGameTime;
            //暂时去掉penalty
            this.mTimeRemain = gameDuration - mGameTime;

            //judge wether to end the game automatiacally
            if (mTimeRemain <= 0)
            {
                mGameState = GameState.END;
                Debug.WriteLine("Time remaining is up to 0. The End.");
            }
        }
    }
    public void SetChargeStation()
    {
        if (mCamp == Camp.A)
        {
            mCarA.SetChargeStation();
            Station.Add(mCarA.CurrentPos(), 0);
        }
        else if (mCamp == Camp.B)
        {
            mCarB.SetChargeStation();
            Station.Add(mCarB.CurrentPos(), 1);
        }
    }

    public void GetMark()
    {
        if (mCamp == Camp.A)
        {
            mCarA.GetMark();
        }
        else if (mCamp == Camp.B)
        {
            mCarB.GetMark();
        }
    }

    // decide which team and stage is going on
    public void Start(Camp _camp, GameStage _GameStage)
    {
        if (mGameState == GameState.RUN)
        {
            Debug.WriteLine("Failed! The current game is going on");
            return;
        }

        if (_GameStage != GameStage.FIRST_HALF && _GameStage != GameStage.SECOND_HALF)
        {
            Debug.WriteLine("Failed to set game stage! Expect input to be GameStage.FIRST_HALF or GameStage.SECOND_HALF");
        }

        // Generate the package list
        if (!hasFirstPackageListGenerated && _GameStage == GameStage.FIRST_HALF)
        {
            // 在生成包裹后生成障碍物 保证与包裹有一定距离
            mObstacle = new Obstacle();
        }

        if (!hasSecondPackageListGenerated && _GameStage == GameStage.SECOND_HALF)
        {
            // 在生成包裹后生成障碍物 保证与包裹有一定距离
            mObstacle = new Obstacle();
        }

        // set state param of game
        mGameState = GameState.RUN;
        mGameStage = _GameStage;
        mCamp = _camp;

        if (mCamp == Camp.A)
        {
            mScoreA[(int)mGameStage - 1] = 0;
        }
        else if (mCamp == Camp.B)
        {
            mScoreB[(int)mGameStage - 1] = 0;
        }

        // initial packages on the field
        _InitialPackagesRemain();

        if (mGameStage == GameStage.FIRST_HALF)
        {
            mTimeRemain = FIRST_HALF_TIME;
        }
        else if (mGameStage == GameStage.SECOND_HALF)
        {
            mTimeRemain = SECOND_HALF_TIME;
        }

        mStartTime = _GetCurrentTime();
        mGameTime = 0;
    }

    public void Pause()
    {
        if (mGameState != GameState.RUN)
        {
            Debug.WriteLine("Pause failed! No race is going on.");
            return;
        }
        mGameState = GameState.PAUSE;
    }

    public void Continue()
    {
        if (mGameState != GameState.PAUSE)
        {
            Debug.WriteLine("Continue Failed! No race is suspended");
        }
        mGameState = GameState.RUN;
    }

    // finish on a manul mode
    public void End()
    {
        if (mGameState != GameState.RUN)
        {
            Debug.WriteLine("Failed! There is no game going on");
        }

        //Reset Car and Save Score
        if (mCamp == Camp.A)
        {
            mScoreA[(int)mGameStage - 1] = mCarA.GetScore();
            mCarA.Reset();
        }
        else if (mCamp == Camp.B)
        {
            mScoreB[(int)mGameStage - 1] = mCarB.GetScore();
            mCarB.Reset();
        }

        // Reset pointer which used to genrate packages
        if (mGameStage == GameStage.FIRST_HALF)
        {
            mPackageFirstHalf.ResetPointer();
        }
        else if (mGameStage == GameStage.SECOND_HALF)
        {
            mPackageSecondHalf.ResetPointer();
        }

        // set state param of game
        mGameState = GameState.UNSTART;
        mGameStage = GameStage.NONE;
        mCamp = Camp.NONE;

        mPackagesRemain.Clear();

        mStartTime = -1;
        mGameTime = -1;
    }

    public byte[] Message()
    {
        byte[] MyMessage = new byte[100];
        int Index = 0;
        // Game Stage
        MyMessage[Index++] = (byte)mGameStage;
        // Game State
        MyMessage[Index++] = (byte)mGameState;

        // GameTime 
        MyMessage[Index++] = (byte)((mGameTime / 100) >> 8);
        MyMessage[Index++] = (byte)(mGameTime / 100);

        // TimeRemain
        MyMessage[Index++] = (byte)((mTimeRemain / 100) >> 8);
        MyMessage[Index++] = (byte)(mTimeRemain / 100);


        // Obstacle
        // Add your code here...
        foreach (Wall item in Obstacle.mpWallList)
        {
            MyMessage[Index++] = (byte)(item.w1.x);
            MyMessage[Index++] = (byte)(item.w1.y);
            MyMessage[Index++] = (byte)(item.w2.x);
            MyMessage[Index++] = (byte)(item.w2.y);
        }

        if (mCamp == Camp.A)
        {
            // Your Charge Station
            MyMessage[Index++] = (byte)Station.Index(0, 0).x;
            MyMessage[Index++] = (byte)Station.Index(0, 0).y;
            MyMessage[Index++] = (byte)Station.Index(1, 0).x;
            MyMessage[Index++] = (byte)Station.Index(1, 0).y;
            MyMessage[Index++] = (byte)Station.Index(2, 0).x;
            MyMessage[Index++] = (byte)Station.Index(2, 0).y;

            // Oppenont's Charge Station
            MyMessage[Index++] = (byte)Station.Index(0, 1).x;
            MyMessage[Index++] = (byte)Station.Index(0, 1).y;
            MyMessage[Index++] = (byte)Station.Index(1, 1).x;
            MyMessage[Index++] = (byte)Station.Index(1, 1).y;
            MyMessage[Index++] = (byte)Station.Index(2, 1).x;
            MyMessage[Index++] = (byte)Station.Index(2, 1).y;

            // Score
            MyMessage[Index++] = (byte)(mCarA.GetScore() >> 8);
            MyMessage[Index++] = (byte)(mCarA.GetScore());

            // Car Position
            MyMessage[Index++] = (byte)(mCarA.CurrentPos().x);
            MyMessage[Index++] = (byte)(mCarA.CurrentPos().y);

            // Car's Package List
            MyMessage[Index++] = (byte)(mCarA.GetPackageCount());
            // Destinaton, Scheduled Time
            for (int i = 0; i < 5; i++)
            {
                MyMessage[Index++] = (byte)(mCarA.GetPackageOnCar(i).Destination().x);
                MyMessage[Index++] = (byte)(mCarA.GetPackageOnCar(i).Destination().y);
                MyMessage[Index++] = (byte)(mCarA.GetPackageOnCar(i).ScheduledDeliveryTime() / 100 >> 8);
                MyMessage[Index++] = (byte)(mCarA.GetPackageOnCar(i).ScheduledDeliveryTime());
            }
        }
        else if (mCamp == Camp.B)
        {
            // Your Charge Station
            MyMessage[Index++] = (byte)Station.Index(0, 1).x;
            MyMessage[Index++] = (byte)Station.Index(0, 1).y;
            MyMessage[Index++] = (byte)Station.Index(1, 1).x;
            MyMessage[Index++] = (byte)Station.Index(1, 1).y;
            MyMessage[Index++] = (byte)Station.Index(2, 1).x;
            MyMessage[Index++] = (byte)Station.Index(2, 1).y;

            // Oppenont's Charge Station
            MyMessage[Index++] = (byte)Station.Index(0, 0).x;
            MyMessage[Index++] = (byte)Station.Index(0, 0).y;
            MyMessage[Index++] = (byte)Station.Index(1, 0).x;
            MyMessage[Index++] = (byte)Station.Index(1, 0).y;
            MyMessage[Index++] = (byte)Station.Index(2, 0).x;
            MyMessage[Index++] = (byte)Station.Index(2, 0).y;

            // Score
            MyMessage[Index++] = (byte)(mCarB.GetScore() >> 8);
            MyMessage[Index++] = (byte)(mCarB.GetScore());

            // Car Position
            MyMessage[Index++] = (byte)(mCarB.CurrentPos().x);
            MyMessage[Index++] = (byte)(mCarB.CurrentPos().y);

            // Car's Package List
            // Total numbrt of picked packages
            MyMessage[Index++] = (byte)(mCarB.GetPackageCount());
            // Destinaton, Scheduled Time
            for (int i = 0; i < 5; i++)
            {
                MyMessage[Index++] = (byte)(mCarB.GetPackageOnCar(i).Destination().x);
                MyMessage[Index++] = (byte)(mCarB.GetPackageOnCar(i).Destination().y);
                MyMessage[Index++] = (byte)(mCarB.GetPackageOnCar(i).ScheduledDeliveryTime() / 100 >> 8);
                MyMessage[Index++] = (byte)(mCarB.GetPackageOnCar(i).ScheduledDeliveryTime());
            }
        }

        // Packages Generate on this frame
        // Indentity Code, Departure, Destination, Scheduled Time
        if (mGameStage == GameStage.FIRST_HALF)
        {
            MyMessage[Index++] = (byte)(mPackageFirstHalf.LastGenerationPackage().IndentityCode());
            MyMessage[Index++] = (byte)(mPackageFirstHalf.LastGenerationPackage().Departure().x);
            MyMessage[Index++] = (byte)(mPackageFirstHalf.LastGenerationPackage().Departure().y);
            MyMessage[Index++] = (byte)(mPackageFirstHalf.LastGenerationPackage().Destination().x);
            MyMessage[Index++] = (byte)(mPackageFirstHalf.LastGenerationPackage().Destination().y);
            MyMessage[Index++] = (byte)(mPackageFirstHalf.LastGenerationPackage().ScheduledDeliveryTime() / 100 >> 8);
            MyMessage[Index++] = (byte)(mPackageFirstHalf.LastGenerationPackage().ScheduledDeliveryTime() / 100);
        }
        else if (mGameStage == GameStage.SECOND_HALF)
        {
            MyMessage[Index++] = (byte)(mPackageSecondHalf.LastGenerationPackage().IndentityCode());
            MyMessage[Index++] = (byte)(mPackageSecondHalf.LastGenerationPackage().Departure().x);
            MyMessage[Index++] = (byte)(mPackageSecondHalf.LastGenerationPackage().Departure().y);
            MyMessage[Index++] = (byte)(mPackageSecondHalf.LastGenerationPackage().Destination().x);
            MyMessage[Index++] = (byte)(mPackageSecondHalf.LastGenerationPackage().Destination().y);
            MyMessage[Index++] = (byte)(mPackageSecondHalf.LastGenerationPackage().ScheduledDeliveryTime() / 100 >> 8);
            MyMessage[Index++] = (byte)(mPackageSecondHalf.LastGenerationPackage().ScheduledDeliveryTime() / 100);
        }

        return MyMessage;
    }

    /***********************************************************************
    Interface used for Tracker to display the information of current game
    ***********************************************************************/
    public List<Package> PackagesOnStage()
    {
        return mPackagesRemain;
    }

    public Camp GetCamp()
    {
        return mCamp;
    }

    public int GetScore(Camp c, GameStage gs)
    {
        if (gs == GameStage.NONE)
        {
            return 0;
        }

        switch (c)
        {
            case Camp.A:
                return mScoreA[(int)gs - 1];

            case Camp.B:
                return mScoreB[(int)gs - 1];

            default:
                break;
        }

        return 0;
    }

    public int GetMileage()
    {
        if (mCamp == Camp.A)
        {
            return mCarA.GetMileage();
        }
        else if (mCamp == Camp.B)
        {
            return mCarB.GetMileage();
        }
        else
        {
            return 0;
        }
    }
    public Car GetCar(Camp c)
    {
        if (c == Camp.A)
        {
            return mCarA;
        }
        else if (c == Camp.B)
        {
            return mCarB;
        }
        else
        {
            return null;
        }
    }
    /***********************************************************************
    Private Functions
    ***********************************************************************/

    /***********************************************
    Initialize and Generate Package
    ***********************************************/
    private bool _InitialPackagesRemain()
    {
        mPackagesRemain.Clear();

        if (mGameStage == GameStage.FIRST_HALF)
        {
            // 开局仅添加INITIAL_PKG_NUM个package
            for (int i = 0; i < INITIAL_PKG_NUM; i++)
            {
                mPackagesRemain.Add(mPackageFirstHalf.Index(i));
            }
            return true;
        }
        else if (mGameStage == GameStage.SECOND_HALF)
        {
            // 开局仅添加INITIAL_PKG_NUM个package
            for (int i = 0; i < INITIAL_PKG_NUM; i++)
            {
                mPackagesRemain.Add(mPackageSecondHalf.Index(i));
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool _GeneratePackage()
    {
        if (this.mPackageFirstHalf.mPointer < PackageList.MaxPackageNumber &&
            mGameStage == GameStage.FIRST_HALF &&
            mGameTime >= mPackageFirstHalf.NextGenerationPackage().GenerationTime()
        )
        {
            mPackagesRemain.Add(mPackageFirstHalf.GeneratePackage());
            return true;
        }
        else if (this.mPackageSecondHalf.mPointer < PackageList.MaxPackageNumber &&
            mGameStage == GameStage.SECOND_HALF &&
            mGameTime >= mPackageSecondHalf.NextGenerationPackage().GenerationTime()
        )
        {
            mPackagesRemain.Add(mPackageSecondHalf.GeneratePackage());
            return true;
        }
        else
        {
            return false;
        }
    }


    /***********************************************
    Time
    ***********************************************/
    private void _UpdateGameTime()
    {
        this.mGameTime = _GetCurrentTime() - mStartTime;
    }

    private static long _GetCurrentTime()
    {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }


    /***********************************************
    Judge Collision of illegal locations
    ***********************************************/
    private bool _IsOnBlackLine(Dot _CarPos)
    {
        return Boundary.isCollided(_CarPos, COLLISION_RADIUS);
    }

    private bool _IsInObstacle(Dot _CarPos)
    {
        return Obstacle.isCollided(_CarPos, COLLISION_RADIUS);
    }

    private bool _IsInOpponentStation(Dot _CarPos)
    {
        if (mCamp == Camp.A)
        {
            return Station.isCollided(_CarPos, 2, COLLISION_RADIUS);
        }
        else if (mCamp == Camp.B)
        {
            return Station.isCollided(_CarPos, 1, COLLISION_RADIUS);
        }
        else
        {
            throw new Exception("No team is racing now");
        }
    }

    private bool _IsInChargeStation(Dot _CarPos)
    {
        if (mCamp == Camp.NONE)
        {
            throw new Exception("No team is racing now");
        }
        else
        {
            return Station.isCollided(_CarPos, (int)mCamp, COLLISION_RADIUS);
        }
    }
}