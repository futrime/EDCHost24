using System;
using System.Collections.Generic;

namespace EdcHost;

/// <summary>
/// A game
/// </summary>
public class Game
{
    #region Parameters

    // Parameters for the game
    public const int GameDurationFirstHalf = 60000;
    public const int GameDurationSecondHalf = 180000;

    // Parameters for the court
    public const int CourtWidth = 254;
    public const int CourtHeight = 254;
    public const int BufferAreaLength = 28;
    public const int BufferEdgeLength = 2;
    public const int WallLength = 80;
    public const int WallWidth = 2;
    public static readonly (Dot TopLeft, Dot BottomRight) CoreArea = (
        new Dot(
            BufferEdgeLength + BufferAreaLength + WallWidth,
            BufferEdgeLength + BufferAreaLength + WallWidth
        ),
        new Dot(
            CourtWidth - (BufferEdgeLength + BufferAreaLength + WallWidth),
            CourtHeight - (BufferEdgeLength + BufferAreaLength + WallWidth)
        )
    );

    // Parameters for orders
    public const int MaxOrderNumber = 20;
    public const int MinDeliveryTime = 20;
    public const int MaxDeliveryTime = 60;

    // Parameters for barriers
    public const int MaxBarrierNum = 5;
    public const int MinBarrierLength = 20;
    public const int MaxBarrierLength = 40;
    public const int MinDistanceBetweenBarriers = 40;

    #endregion


    #region Properties

    /// <summary>
    /// The game stage
    /// </summary>
    public GameStageType GameStage => this._gameStage;

    /// <summary>
    /// The game state
    /// </summary>
    public GameStateType GameState => this._gameState;

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

    /// <summary>
    /// The game time
    /// </summary>
    public long GameTime
    {
        get
        {
            if (this.GameState != GameStateType.Running)
            {
                return 0;
            }
            // To fix the bug that time still runs when 'Pause' button is pressed,
            // we should minus this.ContinueTime - this.PauseTime
            return Math.Max(this.SystemTime - this._startTime, 0);
        }
    }

    /// <summary>
    /// The remaining time
    /// </summary>
    public long RemainingTime
    {
        get
        {
            if (this.GameState != GameStateType.Running)
            {
                return 0;
            }

            return Math.Max(this._gameDuration - this.GameTime, 0);
        }
    }

    /// <summary>
    /// A list of all charging piles
    /// </summary>
    public List<ChargingPile> ChargingPileList => this._chargingPileList;

    /// <summary>
    /// A list of all orders
    /// </summary>
    public List<Order> AllOrderList => _allOrderList;

    /// <summary>
    /// A list of barriers
    /// </summary>
    public List<Barrier> BarrierList => _barrierList;

    /// <summary>
    /// A list of walls
    /// </summary>
    public List<Barrier> WallList => _wallList;

    #endregion

    #region Private fields

    private GameStageType _gameStage = GameStageType.None;
    private GameStateType _gameState = GameStateType.Unstarted;
    private CampType _camp = CampType.None;
    private long _startTime = 0;
    private long _timePenaltySum = 0;
    private long _gameDuration = GameDurationFirstHalf;
    private long _pauseTime;
    private VehicleLegacy _vehicleA = new VehicleLegacy(CampType.A);
    private VehicleLegacy _vehicleB = new VehicleLegacy(CampType.B);
    private int[] _scoreA = { 0, 0 };
    private int[] _scoreB = { 0, 0 };
    private OrderGenerator _orderGenerator;
    private List<Order> _allOrderList;
    private List<Order> _pendingOrderList = new List<Order>();
    private List<Barrier> _barrierList;
    private List<Barrier> _wallList;
    private List<ChargingPile> _chargingPileList = new List<ChargingPile>();

    #endregion


    #region Public methods

    public Game()
    {
        // Empty
    }

    public void Refresh(Dot _CarPos)
    {
        if (this.GameState != GameStateType.Running)
        {
            return;
        }

        if (_camp == CampType.None)
        {
            throw new Exception("The camp is invalid.");
        }

        // Try to generate packages on each refresh
        Order order = _orderGenerator.Generate(GameTime);
        if (order != null)
        {
            _pendingOrderList.Add(order);
        }

        int TimePenalty = 0;

        // Update car's info on each frame
        if (_camp == CampType.A)
        {
            // Update A's info
            _vehicleA.Update(_CarPos, (int)GameTime,
            IsInBarrier(_CarPos), this.IsInChargingPileInfluenceScope(CampType.B, _CarPos),
            this.IsInChargingPileInfluenceScope(CampType.A, _CarPos), ref _pendingOrderList, out TimePenalty);

            // Refresh the score
            _scoreA[(int)GameStage - 1] = _vehicleA.GetScore();
        }
        else if (_camp == CampType.B)
        {
            // Update B's info
            _vehicleB.Update(_CarPos, (int)GameTime,
            IsInBarrier(_CarPos), this.IsInChargingPileInfluenceScope(CampType.A, _CarPos),
            this.IsInChargingPileInfluenceScope(CampType.B, _CarPos), ref _pendingOrderList, out TimePenalty);

            // Refresh the score
            _scoreB[(int)GameStage - 1] = _vehicleB.GetScore();
        }

        if (this.GameState == GameStateType.Running)
        {
            // Calculate the remaining time
            switch (this.GameStage)
            {
                case GameStageType.FirstHalf:
                    this._gameDuration = Game.GameDurationFirstHalf;
                    break;
                case GameStageType.SecondHalf:
                    this._gameDuration = Game.GameDurationSecondHalf;
                    break;
                default:
                    break;
            }

            this._timePenaltySum += TimePenalty;

            if (RemainingTime <= 0)
            {
                this._gameState = GameStateType.Ended;
            }

        }
    }

    public void GetMark()
    {
        if (_camp == CampType.A)
        {
            _vehicleA.GetMark();
        }
        else if (_camp == CampType.B)
        {
            _vehicleB.GetMark();
        }
    }

    // decide which team and stage is going on
    public void Start(CampType _camp, GameStageType _GameStage)
    {
        if (GameState == GameStateType.Running)
        {
            return;
        }

        // set state param of game
        this._gameState = GameStateType.Running;
        this._gameStage = _GameStage;
        this._camp = _camp;

        if (this._camp == CampType.A)
        {
            _scoreA[(int)GameStage - 1] = 0;
        }
        else if (this._camp == CampType.B)
        {
            _scoreB[(int)GameStage - 1] = 0;
        }

        if (GameStage == GameStageType.FirstHalf)
        {
            this._gameDuration = GameDurationFirstHalf;
        }
        else if (GameStage == GameStageType.SecondHalf)
        {
            this._gameDuration = GameDurationSecondHalf;
        }

        _startTime = this.SystemTime;

        // initial packages on the field
        _orderGenerator = new OrderGenerator(MaxOrderNumber, CoreArea,
                                            (0, _gameDuration), (MinDeliveryTime, MaxDeliveryTime), out _allOrderList);

        this._pendingOrderList.Clear();

        // 在生成包裹后生成障碍物 保证障碍物与包裹有一定距离
        _barrierList = new List<Barrier>();

        // Check if the new barrier is valid: Every Barrier should be away from the others.
        bool AwayFromBarriers(Barrier targetBarrier, List<Barrier> barrierList)
        {
            int centerX = (targetBarrier.TopLeftPosition.x + targetBarrier.BottomRightPosition.x) / 2;
            int centerY = (targetBarrier.TopLeftPosition.y + targetBarrier.BottomRightPosition.y) / 2;

            foreach (Barrier barrier in barrierList)
            {
                if (barrier != null)
                {
                    int currentCenterX = (barrier.TopLeftPosition.x + barrier.BottomRightPosition.x) / 2;
                    int currentCenterY = (barrier.TopLeftPosition.y + barrier.BottomRightPosition.y) / 2;

                    //判断与障碍物的距离
                    if (Math.Sqrt((centerX - currentCenterX) * (centerX - currentCenterX) +
                        (centerY - currentCenterY) * (centerY - currentCenterY)) < MinDistanceBetweenBarriers)
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
            Barrier barrier = Barrier.GenerateRandomBarrier(CoreArea,
            (new Dot(MinBarrierLength, MinBarrierLength), new Dot(MaxBarrierLength, MaxBarrierLength)));

            if (AwayFromBarriers(barrier, _barrierList))
            {
                currentBarrierNumber += 1;
                this._barrierList.Add(barrier);
            }
        }

        // Generate walls  不得不分开处理，循环实在不好写
        this._wallList = new List<Barrier>();
        //左上竖墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.x - WallWidth, CoreArea.TopLeft.y - WallWidth), new Dot(CoreArea.TopLeft.x, CoreArea.TopLeft.y + WallLength - WallWidth)));
        //左上横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.x - WallWidth, CoreArea.TopLeft.y - WallWidth), new Dot(CoreArea.TopLeft.x + WallLength - WallWidth, CoreArea.TopLeft.y)));
        //右上竖墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.x, CoreArea.TopLeft.y - WallWidth), new Dot(CoreArea.BottomRight.x + WallWidth, CoreArea.TopLeft.y + WallLength - WallWidth)));
        //右上横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.x - WallLength + WallWidth, CoreArea.TopLeft.y - WallWidth), new Dot(CoreArea.BottomRight.x + WallWidth, CoreArea.TopLeft.y)));
        //左下竖墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.x - WallWidth, CoreArea.BottomRight.y - WallLength + WallWidth), new Dot(CoreArea.TopLeft.x, CoreArea.BottomRight.y + WallWidth)));
        //左下横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.x - WallWidth, CoreArea.BottomRight.y), new Dot(CoreArea.TopLeft.x + WallLength - WallWidth, CoreArea.BottomRight.y + WallWidth)));
        //右下竖墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.x, CoreArea.BottomRight.y - WallLength + WallWidth), new Dot(CoreArea.BottomRight.x + WallWidth, CoreArea.BottomRight.y + WallWidth)));
        //右下横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.x - WallLength + WallWidth, CoreArea.BottomRight.y), new Dot(CoreArea.BottomRight.x + WallWidth, CoreArea.BottomRight.y + WallWidth)));

    }

    public void Pause()
    {
        if (GameState != GameStateType.Running)
        {
            return;
        }
        this._gameState = GameStateType.Paused;
        // To fix the bug that time still runs when 'Pause' button is pressed  
        this._pauseTime = this.SystemTime;
    }

    public void Continue()
    {
        this._gameState = GameStateType.Running;
        this._startTime += this.SystemTime - this._pauseTime;
    }

    public void End()
    {
        if (GameState != GameStateType.Running)
        {
            return;
        }

        this._gameState = GameStateType.Ended;
    }

    public CampType GetCamp()
    {
        return _camp;
    }

    public int GetScore(CampType c, GameStageType gs)
    {
        if (gs == GameStageType.None)
        {
            return 0;
        }

        switch (c)
        {
            case CampType.A:
                return _scoreA[(int)gs - 1];

            case CampType.B:
                return _scoreB[(int)gs - 1];

            default:
                break;
        }

        return 0;
    }

    public VehicleLegacy GetCar(CampType c)
    {
        if (c == CampType.A)
        {
            return _vehicleA;
        }
        else if (c == CampType.B)
        {
            return _vehicleB;
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region Private methods

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
    private bool IsInChargingPileInfluenceScope(CampType camp, Dot position)
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

    #endregion
}