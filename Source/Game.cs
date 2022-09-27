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
    public const int MaxDeliveringOrderNumber = 5;
    public const int MinDeliveryTime = 20;
    public const int MaxDeliveryTime = 60;
    public const int OrderRadius = 8;

    // Parameters for barriers
    public const int MaxBarrierNum = 5;
    public const int MinBarrierLength = 20;
    public const int MaxBarrierLength = 40;
    public const int MinDistanceBetweenBarriers = 40;
    /// <summary>
    /// The decreasing speed in the barrier: 20cm per second
    /// </summary>
    public const int BarrierDecreasePowerRate = 2;

    // Parameters for charging piles
    /// <summary>
    /// The charging speed in own charging piles: 200cm per second
    /// </summary>
    public const int OwnChargingPileIncreaseRate = 20;
    /// <summary>
    /// The decreasing speed in opponent charging piles: 200cm per second
    /// </summary>
    public const int OpponentChargingPileDecreaseRate = 20;

    // parameters for scores
    public const int OutOfCourtPenalty = 50;
    public const int StartVehicleScore = 10;
    public const int TakeOrderScore = 5;
    public const int DeliverOrderScore = 10;

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

    public Dictionary<CampType, Vehicle> Vehicle => _vehicle;

    #endregion

    #region Private fields

    private GameStageType _gameStage = GameStageType.None;
    private GameStateType _gameState = GameStateType.Unstarted;
    private CampType _camp = CampType.None;
    private long _startTime = 0;
    private long _timePenaltySum = 0;
    private long _gameDuration = GameDurationFirstHalf;
    private long _pauseTime;
    private Dictionary<CampType, Vehicle> _vehicle;

    private Dictionary<CampType, int> _score = new Dictionary<CampType, int>
        {{CampType.A,0},
         {CampType.B,0}};

    private OrderGenerator _orderGenerator;
    private List<Order> _allOrderList;
    private List<Order> _pendingOrderList = new List<Order>();

    private List<Barrier> _barrierList = new List<Barrier>();
    // The barrier only needs once generation. 
    private bool _haveInitializedBarrier = false;

    private List<Barrier> _wallList = new List<Barrier>();
    private List<ChargingPile> _chargingPileList = new List<ChargingPile>();

    // SoundPlayer
    private System.Media.SoundPlayer _deliverOrderSound = new System.Media.SoundPlayer(
        System.IO.Directory.GetCurrentDirectory() + @"\Assets\Sounds\Deliver.wav");
    private System.Media.SoundPlayer _takeOrderSound = new System.Media.SoundPlayer(
        System.IO.Directory.GetCurrentDirectory() + @"\Assets\Sounds\Order.wav");

    #endregion


    #region Public methods

    public Game()
    {
        // Empty
    }

    public void Refresh()
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
        Dot vehiclePosition = this._vehicle[this._camp].Position;

        // Update vehicle's info on each frame
        TakeOrders(vehiclePosition, _vehicle[this._camp].DeliveringOrderList, _pendingOrderList);
        DeliverOrders(vehiclePosition, _vehicle[this._camp].DeliveringOrderList);
        Charge(vehiclePosition);
        BarrierPenalty(vehiclePosition);


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
    /// <summary>
    /// Get the penalty when the vehicle is out of the court
    /// </summary>
    public void GetPenalty()
    {
        _score[this._camp] -= OutOfCourtPenalty;
    }
    /// <summary>
    /// Take the order
    /// </summary>
    /// <param name="VehiclePosition"></param>
    /// <param name="deliveringOrder">The current orders on the vehicle</param>
    /// <param name="ordersRemain">The orders that remain on the GUI</param>
    private void TakeOrders(Dot VehiclePosition, List<Order> deliveringOrder, List<Order> ordersRemain)      //拾取外卖
    {
        for (int i = 0; i < ordersRemain.Count; i++)
        {
            Order order = ordersRemain[i];
            if (order.Status == Order.StatusType.Pending && Dot.Distance(order.DeparturePosition, VehiclePosition) <= OrderRadius &&
                deliveringOrder.Count < MaxDeliveringOrderNumber)
            {
                order.Take(this.GameTime);
                deliveringOrder.Add(order);
                ordersRemain.Remove(order);

                this._score[this._camp] += TakeOrderScore;

                // 拾取后改变order的status
                order.Status = Order.StatusType.InDelivery;

                // play sound
                _takeOrderSound.Load();
                _takeOrderSound.Play();
                break;
            }
        }
    }
    /// <summary>
    /// Deliver the order
    /// </summary>
    /// <param name="VehiclePosition"></param>
    /// <param name="deliveringOrder">The current orders on the vehicle</param>
    private void DeliverOrders(Dot VehiclePosition, List<Order> deliveringOrder)      //送达外卖 
    {
        for (int i = 0; i < deliveringOrder.Count; i++)
        {
            Order order = deliveringOrder[i];
            if (order.Status == Order.StatusType.InDelivery && Dot.Distance(order.DestinationPosition, VehiclePosition) <= OrderRadius)
            {
                order.Deliver(this.GameTime);
                deliveringOrder.Remove(order);

                this._score[this._camp] += DeliverOrderScore;

                // 拾取后改变order的status
                order.Status = Order.StatusType.Delivered;
                _deliverOrderSound.Load();
                _deliverOrderSound.Play();
                break;
            }
        }
    }
    /// <summary>
    /// Charge the vehicle
    /// </summary>
    private void Charge(Dot vehiclePosition)
    {
        CampType opponentCamp = (this._camp == CampType.A ? CampType.B : CampType.A);
        // judge whether the vehicle is in the range of charging piles

        if (IsInChargingPileInfluenceScope(this._camp, vehiclePosition))
        {
            this._vehicle[this._camp].IncreaseMaxDistance(OwnChargingPileIncreaseRate);
        }
        // is in opponent camp's charging piles
        if (IsInChargingPileInfluenceScope(opponentCamp, vehiclePosition))
        {
            this._vehicle[this._camp].DecreaseMaxDistance(OpponentChargingPileDecreaseRate);
        }
    }
    /// <summary>
    /// Decrease the power when the vehicle is in the range of barriers
    /// </summary>
    private void BarrierPenalty(Dot vehiclePosition)
    {
        if (IsInBarrier(vehiclePosition))
            this._vehicle[this._camp].DecreaseMaxDistance(BarrierDecreasePowerRate);
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

        //reset the vehicle
        this._vehicle = new Dictionary<CampType, Vehicle>
        {{CampType.A, new Vehicle(CampType.A)},
         {CampType.B,new Vehicle(CampType.B)}};
        // this._vehicle[CampType.A].Reset();
        // this._vehicle[CampType.B].Reset();

        if (this._camp == CampType.A)
        {
            // Initial orders on the field, which is only implemented per half game
            this._orderGenerator = new OrderGenerator(MaxOrderNumber, CoreArea,
                                    (0, _gameDuration), (MinDeliveryTime, MaxDeliveryTime), out _allOrderList);
        }
        else if (this._camp == CampType.B)
        {
            this._orderGenerator.Reset();
        }

        if (GameStage == GameStageType.FirstHalf)
        {
            this._gameDuration = GameDurationFirstHalf;
        }
        else if (GameStage == GameStageType.SecondHalf)
        {
            this._gameDuration = GameDurationSecondHalf;
        }

        this._startTime = this.SystemTime;

        // Clear the order list
        this._pendingOrderList.Clear();

        if (!this._haveInitializedBarrier)
        {
            _haveInitializedBarrier = true;

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
                        if (Dot.Distance(new Dot(currentCenterX, currentCenterY), new Dot(centerX, centerY)) < MinDistanceBetweenBarriers)
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
                return _score[CampType.A];

            case CampType.B:
                return _score[CampType.B];

            default:
                break;
        }

        return 0;
    }
    public decimal GetPowerRatio()
    {
        return this._vehicle[this._camp].RemainingPowerRatio;
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