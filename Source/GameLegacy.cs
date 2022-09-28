using System;
using System.Collections.Generic;

namespace EdcHost;

/// <summary>
/// A game
/// </summary>
public class GameLegacy
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
    public const int MaxOrderNumberFisrtHalf = 20;
    public const int MaxOrderNumberSecondHalf = 60;
    public const int MaxDeliveringOrderNumber = 5;
    public const int MinDeliveryTime = 20;
    public const int MaxDeliveryTime = 60;
    public const int OrderRadius = 8;
    public const long MinTakeOrdersTime = 1000;
    public const long MinDeliverOrdersTime = 1000;


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
    // parameters for parking
    public const long MaxParkingTime = 5000;
    // parameters for walls
    public const int LongParkingPenalty = 5;
    // parameters for scores
    public const int OutOfCourtPenalty = 50;
    public const int CollideWallPenalty = 5;
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

    private GameStageType _gameStage = GameStageType.PreMatch;
    private GameStateType _gameState = GameStateType.Unstarted;
    private CampType _camp = CampType.None;
    private long _startTime = 0;
    private long _timePenaltySum = 0;
    private long _gameDuration = GameDurationFirstHalf;
    private long _pauseTime;
    private long _lastTickTime = Utility.SystemTime - 1;
    private Dictionary<CampType, Vehicle> _vehicle;

    private Dictionary<CampType, decimal> _score = new Dictionary<CampType, decimal>
        {{CampType.A,0},
         {CampType.B,0}};

    private int _maxOrderNumber;
    private OrderGenerator _orderGenerator;
    private List<Order> _allOrderList;
    private List<Order> _pendingOrderList = new List<Order>();

    private List<Barrier> _barrierList = new List<Barrier>();
    // The barrier only needs once generation. 
    private bool _haveInitializedBarrier = false;

    private List<Barrier> _wallList = new List<Barrier>();
    private List<ChargingPile> _chargingPileList = new List<ChargingPile>();
    private long? _parkingDuration = 0;
    private long? _lastParkingDuration = 0;

    // SoundPlayer
    private System.Media.SoundPlayer _deliverOrderSound = new System.Media.SoundPlayer(
        System.IO.Directory.GetCurrentDirectory() + @"\Assets\Sounds\Deliver.wav");
    private System.Media.SoundPlayer _takeOrderSound = new System.Media.SoundPlayer(
        System.IO.Directory.GetCurrentDirectory() + @"\Assets\Sounds\Order.wav");

    #endregion


    #region Public methods

    public GameLegacy()
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
        Dot vehiclePosition = (Dot)this._vehicle[this._camp].Position;
        this._parkingDuration = this._vehicle[this._camp].ParkingDuration;

        // Update vehicle's info on each frame
        TakeOrders(vehiclePosition, this._parkingDuration, _vehicle[this._camp].DeliveringOrderList, _pendingOrderList);
        DeliverOrders(vehiclePosition, this._parkingDuration, _vehicle[this._camp].DeliveringOrderList);
        ParkingPenalty(vehiclePosition, this._parkingDuration, this._lastParkingDuration);
        Charge(vehiclePosition);
        BarrierPenalty(vehiclePosition);
        WallPenalty(vehiclePosition);
        this._lastParkingDuration = this._parkingDuration;

        if (this.GameState == GameStateType.Running)
        {
            // Calculate the remaining time
            switch (this.GameStage)
            {
                case GameStageType.FirstHalf:
                    this._gameDuration = GameLegacy.GameDurationFirstHalf;
                    break;
                case GameStageType.SecondHalf:
                    this._gameDuration = GameLegacy.GameDurationSecondHalf;
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

        this._lastTickTime = Utility.SystemTime;
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
    /// <param name="vehiclePosition"></param>
    /// <param name="parkingDuration">The parking time should be more than MinTakeOrdersTime</param>
    /// <param name="deliveringOrder">The current orders on the vehicle</param>
    /// <param name="ordersRemain">The orders that remain on the GUI</param>
    private void TakeOrders(Dot vehiclePosition, long? parkingDuration, List<Order> deliveringOrder, List<Order> ordersRemain)      //拾取外卖
    {
        for (int i = 0; i < ordersRemain.Count; i++)
        {
            Order order = ordersRemain[i];
            if (order.Status == OrderStatusType.Pending && Dot.Distance(order.DeparturePosition, vehiclePosition) <= OrderRadius &&
                deliveringOrder.Count < MaxDeliveringOrderNumber && parkingDuration >= MinTakeOrdersTime)
            {
                order.Take(this.GameTime);
                deliveringOrder.Add(order);
                ordersRemain.Remove(order);

                // add score
                this._score[this._camp] += TakeOrderScore;

                // 拾取后改变order的status
                order.Status = OrderStatusType.InDelivery;

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
    /// <param name="vehiclePosition"></param>
    /// <param name="parkingDuration">The parking time should be more than MinDeliverOrdersTime</param>
    /// <param name="deliveringOrder">The current orders on the vehicle</param>
    private void DeliverOrders(Dot vehiclePosition, long? parkingDuration, List<Order> deliveringOrder)      //送达外卖 
    {
        for (int i = 0; i < deliveringOrder.Count; i++)
        {
            Order order = deliveringOrder[i];
            if (order.Status == OrderStatusType.InDelivery && Dot.Distance(order.DestinationPosition, vehiclePosition) <= OrderRadius &&
                parkingDuration >= MinDeliverOrdersTime)
            {
                order.Deliver(this.GameTime);
                deliveringOrder.Remove(order);

                this._score[this._camp] += DeliverOrderScore;

                // 拾取后改变order的status
                order.Status = OrderStatusType.Delivered;
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
            this._vehicle[this._camp].IncreaseMaxDistance(-OpponentChargingPileDecreaseRate);
        }
    }
    /// <summary>
    /// Decrease the power when the vehicle is in the range of barriers
    /// </summary>
    private void BarrierPenalty(Dot vehiclePosition)
    {
        if (IsInBarrier(vehiclePosition))
            this._vehicle[this._camp].IncreaseMaxDistance(-BarrierDecreasePowerRate);
    }
    /// <summary>
    /// Decrease the power when the vehicle is in the range of barriers
    /// </summary>
    private void WallPenalty(Dot vehiclePosition)
    {
        if (IsInWall(vehiclePosition))
            this._score[this._camp] -= CollideWallPenalty;
    }
    /// <summary>
    /// Parking Penalty
    /// </summary>
    /// <param name="vehiclePosition"></param>
    /// <param name="parkingDuration">The parking time in this frame</param>
    /// <param name="lastParkingDuration">The parking time in last frame</param>
    private void ParkingPenalty(Dot vehiclePosition, long? parkingDuration, long? lastParkingDuration)
    {
        if (parkingDuration != null && lastParkingDuration != null)
            if (parkingDuration / MaxParkingTime != lastParkingDuration / MaxParkingTime &&
                !IsInChargingPileInfluenceScope(this._camp, vehiclePosition))
            {
                this._score[this._camp] -= LongParkingPenalty;
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

        //reset the vehicle
        this._vehicle = new Dictionary<CampType, Vehicle>
        {{CampType.A, new Vehicle(CampType.A)},
         {CampType.B,new Vehicle(CampType.B)}};
        // this._vehicle[CampType.A].Reset();
        // this._vehicle[CampType.B].Reset();

        if (GameStage == GameStageType.FirstHalf)
        {
            this._gameDuration = GameDurationFirstHalf;
            this._maxOrderNumber = MaxOrderNumberFisrtHalf;
        }
        else if (GameStage == GameStageType.SecondHalf)
        {
            this._gameDuration = GameDurationSecondHalf;
            this._maxOrderNumber = MaxOrderNumberSecondHalf;

        }

        if (this._camp == CampType.A)
        {
            // Initial orders on the field, which is only implemented per half game
            this._orderGenerator = new OrderGenerator(this._maxOrderNumber, CoreArea,
                                    (0, _gameDuration), (MinDeliveryTime, MaxDeliveryTime), out _allOrderList);
        }
        else if (this._camp == CampType.B)
        {
            this._orderGenerator.Reset();
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
                int centerX = (targetBarrier.TopLeftPosition.X + targetBarrier.BottomRightPosition.X) / 2;
                int centerY = (targetBarrier.TopLeftPosition.Y + targetBarrier.BottomRightPosition.Y) / 2;

                foreach (Barrier barrier in barrierList)
                {
                    if (barrier != null)
                    {
                        int currentCenterX = (barrier.TopLeftPosition.X + barrier.BottomRightPosition.X) / 2;
                        int currentCenterY = (barrier.TopLeftPosition.Y + barrier.BottomRightPosition.Y) / 2;

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
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.X - WallWidth, CoreArea.TopLeft.Y - WallWidth), new Dot(CoreArea.TopLeft.X, CoreArea.TopLeft.Y + WallLength - WallWidth)));
        //左上横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.X - WallWidth, CoreArea.TopLeft.Y - WallWidth), new Dot(CoreArea.TopLeft.X + WallLength - WallWidth, CoreArea.TopLeft.Y)));
        //右上竖墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.X, CoreArea.TopLeft.Y - WallWidth), new Dot(CoreArea.BottomRight.X + WallWidth, CoreArea.TopLeft.Y + WallLength - WallWidth)));
        //右上横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.X - WallLength + WallWidth, CoreArea.TopLeft.Y - WallWidth), new Dot(CoreArea.BottomRight.X + WallWidth, CoreArea.TopLeft.Y)));
        //左下竖墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.X - WallWidth, CoreArea.BottomRight.Y - WallLength + WallWidth), new Dot(CoreArea.TopLeft.X, CoreArea.BottomRight.Y + WallWidth)));
        //左下横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.TopLeft.X - WallWidth, CoreArea.BottomRight.Y), new Dot(CoreArea.TopLeft.X + WallLength - WallWidth, CoreArea.BottomRight.Y + WallWidth)));
        //右下竖墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.X, CoreArea.BottomRight.Y - WallLength + WallWidth), new Dot(CoreArea.BottomRight.X + WallWidth, CoreArea.BottomRight.Y + WallWidth)));
        //右下横墙
        this._wallList.Add(new Barrier(new Dot(CoreArea.BottomRight.X - WallLength + WallWidth, CoreArea.BottomRight.Y), new Dot(CoreArea.BottomRight.X + WallWidth, CoreArea.BottomRight.Y + WallWidth)));

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

    public decimal GetScore(CampType c, GameStageType gs)
    {
        if (gs == GameStageType.PreMatch)
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
    /// Check if a position is in the walls.
    /// </summary>
    /// <param name="position">The position</param>
    /// <returns>True if the position is in the walls; otherwise false</returns>
    private bool IsInWall(Dot position)
    {
        foreach (var wall in this._wallList)
        {
            if (wall.IsIn(position))
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