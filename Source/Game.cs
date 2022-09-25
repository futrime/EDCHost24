using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EdcHost;

public class Game
{
    // size of competition area
    // 最大游戏场地
    public const int MAX_SIZE = 254;
    public const int AVAILABLE_MAX_X = 254;
    public const int AVAILABLE_MAX_Y = 254;

    // size of car
    public const int COLLISION_RADIUS = 8;

    // time of first and second half
    public const int FIRST_HALF_TIME = 60000;
    public const int SECOND_HALF_TIME = 180000;


    public const int MaxOrderNumber = 20;

    public const int MinDeliveryTime = 20;
    public const int MaxDeliveryTime = 60;

    public const int MaxBarrierNum = 8;
    // 障碍物的最小边长
    public const int BarrierMinLength = 12;
    // 障碍物的最大边长
    public const int BarrierMaxLength = 16;

    public const int BarrierMinDistanceFromBarrier = 20;


    /// <summary>
    /// The system time
    /// </summary>
    public long SystemTime
    {
        get
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }

    // state
    public GameStage _gameStage;
    public GameState _gameState;

    // Time
    // Set time zero as the start time of each race
    private long _startTime; // system time, update for each race

    public long GameTime
    {
        get
        {
            if (this._gameState != GameState.RUN)
            {
                return 0;
            }

            return Math.Max(this.SystemTime - this._startTime, 0);
        }
    }

    public long RemainingTime
    {
        get {
            if (this._gameState != GameState.RUN)
            {
                return 0;
            }

            return Math.Max(this._gameDuration - this.GameTime, 0);
        }
    }

    /// <summary>
    /// The sum of the time penalty
    /// </summary>
    private int _timePenaltySum = 0;

    // car and package
    private Car mCarA, mCarB;

    private int[] mScoreA, mScoreB;


    // store the packages on the field
    private OrderGenerator _orderGenerator;

    /// <summary>
    /// All orders in orderGenerator
    /// </summary>
    private List<Order> _allOrderList;
    public List<Order> AllOrderList => _allOrderList;

    /// <summary>
    /// orders remained on GUI
    /// </summary>
    private List<Order> _pendingOrderList;

    // which team is racing A or B
    private Camp _camp;

    private List<Barrier> _barrierList;
    public List<Barrier> BarrierList => _barrierList;

    private List<ChargingPile> _chargingPileList = new List<ChargingPile>();

    private int _gameDuration = FIRST_HALF_TIME;


    public Game()
    {
        Debug.WriteLine("Call Constructor of Game");

        _gameState = GameState.UNSTART;
        _gameStage = GameStage.NONE;

        _camp = Camp.NONE;

        mCarA = new Car(Camp.A);
        mCarB = new Car(Camp.B);

        mScoreA = new int[] { 0, 0 };
        mScoreB = new int[] { 0, 0 };

        _pendingOrderList = new List<Order>();

        _startTime = -1;


        // this.mPackageFirstHalf = new PackageList(AVAILABLE_MAX_X, AVAILABLE_MIN_X,
        //         AVAILABLE_MAX_Y, AVAILABLE_MIN_Y, INITIAL_PKG_NUM, FIRST_HALF_TIME, TIME_INTERVAL, 0);

        // this.mPackageSecondHalf = new PackageList(AVAILABLE_MAX_X, AVAILABLE_MIN_X,
        //         AVAILABLE_MAX_Y, AVAILABLE_MIN_Y, INITIAL_PKG_NUM, FIRST_HALF_TIME, TIME_INTERVAL, 1);
    }


    /***********************************************
    Update on each frame
    ***********************************************/
    public void UpdateOnEachFrame(Dot _CarPos)
    {
        // check the game state
        if (_gameState == GameState.UNSTART || _gameState == GameState.PAUSE || _gameState == GameState.END)
        {
            return;
        }

        if (_camp == Camp.NONE)
        {
            throw new Exception("The camp is invalid.");
        }

        // Try to generate packages on each refresh
        Order ord = _orderGenerator.Generate(GameTime);
        if (ord != null)
        {
            _pendingOrderList.Add(ord);
        }

        int TimePenalty = 0;

        // Update car's info on each frame
        if (_camp == Camp.A)
        {
            mCarA.Update(_CarPos, (int)GameTime,
            IsInBarrier(_CarPos), this.IsInChargingPileInfluenceScope(Camp.B, _CarPos),
            this.IsInChargingPileInfluenceScope(Camp.A, _CarPos), ref _pendingOrderList, out TimePenalty);
        }
        else if (_camp == Camp.B)
        {
            mCarA.Update(_CarPos, (int)GameTime,
            IsInBarrier(_CarPos), this.IsInChargingPileInfluenceScope(Camp.A, _CarPos),
            this.IsInChargingPileInfluenceScope(Camp.B, _CarPos), ref _pendingOrderList, out TimePenalty);
        }

        if (this._gameState == GameState.RUN)
        {
            // Calculate the remaining time
            switch (this._gameStage)
            {
                case GameStage.FIRST_HALF:
                    this._gameDuration = Game.FIRST_HALF_TIME;
                    break;
                case GameStage.SECOND_HALF:
                    this._gameDuration = Game.SECOND_HALF_TIME;
                    break;
                default:
                    break;
            }

            this._timePenaltySum += TimePenalty;

            //judge wether to end the game automatiacally
            if (RemainingTime <= 0)
            {
                _gameState = GameState.END;
                Debug.WriteLine("Time remaining is up to 0. The End.");
            }
        }
    }
    public void SetChargeStation()
    {
        if (_camp == Camp.A)
        {
            mCarA.SetChargeStation();
            Station.Add(mCarA.CurrentPos(), 0);
        }
        else if (_camp == Camp.B)
        {
            mCarB.SetChargeStation();
            Station.Add(mCarB.CurrentPos(), 1);
        }
    }

    public void GetMark()
    {
        if (_camp == Camp.A)
        {
            mCarA.GetMark();
        }
        else if (_camp == Camp.B)
        {
            mCarB.GetMark();
        }
    }

    // decide which team and stage is going on
    public void Start(Camp _camp, GameStage _GameStage)
    {
        if (_gameState == GameState.RUN)
        {
            Debug.WriteLine("Failed! The current game is going on");
            return;
        }

        if (_GameStage != GameStage.FIRST_HALF && _GameStage != GameStage.SECOND_HALF)
        {
            Debug.WriteLine("Failed to set game stage! Expect input to be GameStage.FIRST_HALF or GameStage.SECOND_HALF");
        }

        // set state param of game
        _gameState = GameState.RUN;
        _gameStage = _GameStage;
        this._camp = _camp;

        if (this._camp == Camp.A)
        {
            mScoreA[(int)_gameStage - 1] = 0;
        }
        else if (this._camp == Camp.B)
        {
            mScoreB[(int)_gameStage - 1] = 0;
        }

        if (_gameStage == GameStage.FIRST_HALF)
        {
            this._gameDuration = FIRST_HALF_TIME;
        }
        else if (_gameStage == GameStage.SECOND_HALF)
        {
            this._gameDuration = SECOND_HALF_TIME;
        }

        _startTime = this.SystemTime;

        // initial packages on the field
        // 暂定时限为10-20s!!
        _orderGenerator = new OrderGenerator(MaxOrderNumber, (new Dot(0, 0), new Dot(MAX_SIZE, MAX_SIZE)),
                                            (0, _gameDuration), (MinDeliveryTime, MaxDeliveryTime), out _allOrderList);

        this._pendingOrderList.Clear();

        // 在生成包裹后生成障碍物 保证障碍物与包裹有一定距离
        _barrierList = new List<Barrier>();

        // Check if the new barrier is valid: Every Barrier should be away from the others.
        bool AwayFromObstacles(Barrier targetBarrier, List<Barrier> barrierList)
        {
            int centerX = (targetBarrier.TopLeftPosition.x + targetBarrier.BottomRightPosition.x) / 2;
            int centerY = (targetBarrier.TopLeftPosition.x + targetBarrier.BottomRightPosition.x) / 2;

            foreach (Barrier barrier in barrierList)
            {
                if (barrier != null)
                {
                    int currentCenterX = (barrier.TopLeftPosition.x + barrier.TopLeftPosition.x) / 2;
                    int currentCenterY = (barrier.BottomRightPosition.y + barrier.BottomRightPosition.y) / 2;

                    //判断与障碍物的距离
                    if (Math.Sqrt((centerX - currentCenterX) * (centerX - currentCenterX) +
                        (centerY - currentCenterY) * (centerY - currentCenterY)) < BarrierMinDistanceFromBarrier)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        // the number of valid barrier
        int currentBarrierNumber = 0;
        while (currentBarrierNumber < MaxBarrierNum)
        {
            Barrier barrier = Barrier.GenerateRandomBarrier((new Dot(0, 0), new Dot(MAX_SIZE, MAX_SIZE)),
            (new Dot(BarrierMinLength, BarrierMinLength), new Dot(BarrierMaxLength, BarrierMaxLength)));

            if (AwayFromObstacles(barrier, _barrierList))
            {
                currentBarrierNumber += 1;
                _barrierList.Add(barrier);
            }
        }
    }

    public void Pause()
    {
        if (_gameState != GameState.RUN)
        {
            Debug.WriteLine("Pause failed! No race is going on.");
            return;
        }
        _gameState = GameState.PAUSE;
    }

    public void Continue()
    {
        if (_gameState != GameState.PAUSE)
        {
            Debug.WriteLine("Continue Failed! No race is suspended");
        }
        _gameState = GameState.RUN;
    }

    // finish on a manul mode
    public void End()
    {
        if (_gameState != GameState.RUN)
        {
            Debug.WriteLine("Failed! There is no game going on");
        }

        //Reset Car and Save Score
        if (_camp == Camp.A)
        {
            mScoreA[(int)_gameStage - 1] = mCarA.GetScore();
            mCarA.Reset();
        }
        else if (_camp == Camp.B)
        {
            mScoreB[(int)_gameStage - 1] = mCarB.GetScore();
            mCarB.Reset();
        }

        // Reset pointer which used to genrate packages
        // if (mGameStage == GameStage.FIRST_HALF)
        // {
        //     mPackageFirstHalf.ResetPointer();
        // }
        // else if (mGameStage == GameStage.SECOND_HALF)
        // {
        //     mPackageSecondHalf.ResetPointer();
        // }
        _orderGenerator.Reset();

        // set state param of game
        _gameState = GameState.UNSTART;
        _gameStage = GameStage.NONE;
        _camp = Camp.NONE;

        _pendingOrderList.Clear();

        _startTime = -1;
    }

    public List<Order> OrdersRemain()
    {
        return _pendingOrderList;
    }

    public Camp GetCamp()
    {
        return _camp;
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
        if (_camp == Camp.A)
        {
            return mCarA.GetMileage();
        }
        else if (_camp == Camp.B)
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

    /// <summary>
    /// Check if a position is in a barrier.
    /// </summary>
    /// <param name="position">The position</param>
    /// <returns>True if the position is in a barrier; otherwise false</returns>
    private bool IsInBarrier(Dot position)
    {
        foreach (var barrier in this._barrierList)
        {
            if (barrier.IsIn(position))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if a position is in the influence scope of the charging piles of a camp.
    /// </summary>
    /// <param name="camp">The camp</param>
    /// <param name="position">The position</param>
    /// <returns>True if the position is in the scope; otherwise false</returns>
    private bool IsInChargingPileInfluenceScope(Camp camp, Dot position)
    {
        foreach (var chargingPile in this._chargingPileList)
        {
            if (chargingPile.Camp == camp && chargingPile.IsInInfluenceScope(position))
            {
                return true;
            }
        }

        return false;
    }
}