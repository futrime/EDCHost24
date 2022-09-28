using System;
using System.Collections.Generic;

namespace EdcHost;

/// <summary>
/// A game
/// </summary>
public class Game
{
    #region Parameters.

    #region Parameters related to the game and the scoring machanism.

    /// <summary>
    /// The game duration of each game part.
    /// </summary>
    public static readonly Dictionary<GameStageType, long?> GameDuration =
        new Dictionary<GameStageType, long?> {
            { GameStageType.FirstHalf, 60000 },
            { GameStageType.SecondHalf, 180000 },
            { GameStageType.PreMatch, null }
        };

    /// <summary>
    /// The score obtained when having moved into the inner court.
    /// </summary>
    public const decimal ScoreMoveIntoInnerCourt = 10M;

    /// <summary>
    /// The score obtained when having an order delivered.
    /// </summary>
    public const decimal ScoreDeliverOrder = 20M;

    /// <summary>
    /// The score obtained when setting a charging pile.
    /// </summary>
    public const decimal ScoreSetChargingPile = 5M;

    /// <summary>
    /// The score obtained when a delivery is over time per
    /// second.
    /// </summary>
    public const decimal ScoreDeliveryOvertimeRate = -5M;

    /// <summary>
    /// The score obtained when gaining a foul flag.
    /// </summary>
    public const decimal ScoreFoul = -50M;

    #endregion

    #region Parameters related to the court.

    /// <summary>
    /// The court area.
    /// </summary>
    public static readonly (Dot TopLeft, Dot BottomRight) CourtArea = (
        new Dot(0, 0),
        new Dot(254, 254)
    );

    /// <summary>
    /// The inner court area.
    /// </summary>
    public static readonly (Dot TopLeft, Dot BottomRight) InnerCourtArea = (
        new Dot(32, 32),
        new Dot(222, 222)
    );

    /// <summary>
    /// The walls.
    /// </summary>
    public static readonly Barrier[] WallList = {
        // Walls on the top.
        new Barrier(new Dot(30, 30), new Dot(112, 32)),
        new Barrier(new Dot(142, 30), new Dot(224, 32)),
        // Walls on the bottom.
        new Barrier(new Dot(30, 222), new Dot(112, 224)),
        new Barrier(new Dot(142, 222), new Dot(224, 224)),
        // Walls on the left.
        new Barrier(new Dot(30, 30), new Dot(32, 112)),
        new Barrier(new Dot(30, 142), new Dot(32, 224)),
        // Walls on the right.
        new Barrier(new Dot(222, 30), new Dot(224, 112)),
        new Barrier(new Dot(222, 142), new Dot(224, 224))
    };

    #endregion

    #region Parameters related to the barriers.

    /// <summary>
    /// The range of the areas of the barriers.
    /// </summary>
    public static readonly (int Min, int Max) BarrierAreaRange = (
        400, 4000
    );

    /// <summary>
    /// The number of the barriers.
    /// </summary>
    public const int BarrierNumber = 5;

    /// <summary>
    /// The range of the side lengths of the barriers.
    /// </summary>
    public static readonly (int Min, int Max) BarrierSideLengthRange = (
        5, 200
    );

    /// <summary>
    /// The reduction rate of the max distances of vehicles in barriers
    /// in centimeter per second.
    /// </summary>
    public const decimal BarrierDischargingRate = 0.1M;

    #endregion

    #region Parameters related to the charging piles.

    /// <summary>
    /// The increasing rate of the max distances of vehicles in the influence
    /// scope of their own charging piles in centimeter per second.
    /// </summary>
    public const decimal ChargingPileChargingRate = 0.1M;

    /// <summary>
    /// The reduction rate of the max distances of vehicles in the influence
    /// scope of their opponents' charging piles in centimeter per second.
    /// </summary>
    public const decimal ChargingPileDischargingRate = 0.1M;

    /// <summary>
    /// The radius of the scope where vehicles charge from a
    /// charging pile.
    /// </summary>
    public const int ChargingPileChargingScopeRadius = 20;

    /// <summary>
    /// The radius of the scope where vehicles discharge from
    /// a charging pile of their opponents.
    /// </summary>
    public const int ChargingPileDischargingScopeRadius = 20;

    #endregion

    #region Parameters related to orders.

    /// <summary>
    /// The radius of the scope where vehicles can contact a order.
    /// </summary>
    public const int OrderContactScopeRadius = 8;

    /// <summary>
    /// The delay for vehicles to deliver an order.
    /// </summary>
    public const long OrderDeliverDelay = 1000;

    /// <summary>
    /// The capacity of the orders on a vehicle.
    /// </summary>
    public const int OrderDeliveryCapacity = 5;

    /// <summary>
    /// The range of the delivery durations of orders.
    /// </summary>
    public static readonly (long Min, long Max) OrderDeliveryDurationRange = (
        20, 60
    );

    /// <summary>
    /// The order number of each game part.
    /// </summary>
    public static readonly Dictionary<GameStageType, int?> OrderNumber =
        new Dictionary<GameStageType, int?>
        {
            { GameStageType.FirstHalf, 10 },
            { GameStageType.SecondHalf, 60 },
            { GameStageType.PreMatch, null }
        };

    #endregion

    #region Parameters related to vehicles

    /// <summary>
    /// The initial max distance of vehicles.
    /// </summary>
    public const int VehicleInitialMaxDistance = 1000;

    /// <summary>
    /// The increasing rate of the max distances of vehicles out of power
    /// in centimeter per second.
    /// </summary>
    public const decimal VehicleAutoChargingRate = 0.02M;

    #endregion

    #endregion


    #region Public properties

    /// <summary>
    /// A list of the barriers.
    /// </summary>
    public List<Barrier> BarrierList => this._barrierList;

    /// <summary>
    /// The camp of the current vehicle. Null if the game has not 
    /// started.
    /// </summary>
    public CampType? Camp => this._camp;

    /// <summary>
    /// A list of the charing piles.
    /// </summary>
    public List<ChargingPile> ChargingPileList => this._chargingPileList;

    /// <summary>
    /// The game stage.
    /// </summary>
    public GameStageType GameStage => this._gameStage;

    /// <summary>
    /// The game state.
    /// </summary>
    public GameStateType GameState => this._gameState;

    /// <summary>
    /// The game time. Null if the game has not started.
    /// </summary>
    public long? GameTime
    {
        get
        {
            if (this._startTime == null)
            {
                return null;
            }

            return (Utility.SystemTime - (long)this._startTime);
        }
    }

    /// <summary>
    /// A list of the generated orders.
    /// </summary>
    public List<Order> OrderList => this._orderList;

    /// <summary>
    /// The remaining time of the game. Null if the game has not
    /// started.
    /// </summary>
    public long? RemainingTime
    {
        get
        {
            if (this.GameTime == null)
            {
                return null;
            }

            return (Game.GameDuration[this._gameStage] - this.GameTime);
        }
    }

    /// <summary>
    /// The scores of different camps.
    /// </summary>
    public Dictionary<CampType, decimal> Score => this._score;

    /// <summary>
    /// The vehicles of different camps.
    /// </summary>
    public Dictionary<CampType, Vehicle> Vehicle => this._vehicle;

    #endregion

    #region Private properties and fields

    private readonly List<Barrier> _barrierList;

    private CampType? _camp = null;

    private List<ChargingPile> _chargingPileList = new List<ChargingPile>();

    private GameStageType _gameStage = GameStageType.PreMatch;

    private GameStateType _gameState = GameStateType.Unstarted;

    // Minus one to prevent division by zero.
    private long _lastTickTime = Utility.SystemTime - 1;

    private OrderGenerator _orderGenerator = null;

    private List<Order> _orderList = new List<Order>();

    private long? _pauseTime = null;

    private List<Order> _pendingOrderList = new List<Order>();

    private Dictionary<CampType, decimal> _score =
        new Dictionary<CampType, decimal>
        {
            { CampType.A, 0M },
            { CampType.B, 0M }
        };

    private long? _startTime = null;

    private Dictionary<CampType, Vehicle> _vehicle = null;

    #endregion


    #region Public methods

    /// <summary>
    /// Construct a Game object.
    /// </summary>
    public Game()
    {
        // Generate barriers
        this._barrierList = new List<Barrier>();
        for (int i = 0; i < Game.BarrierNumber; ++i)
        {
            bool isGenerated = false;
            while (!isGenerated)
            {
                var width = Utility.RandomGenerator.Next(
                    Game.BarrierSideLengthRange.Min,
                    Game.BarrierSideLengthRange.Max
                );
                var height = Utility.RandomGenerator.Next(
                    Game.BarrierSideLengthRange.Min,
                    Game.BarrierSideLengthRange.Max
                );

                // Restrict the area of the barriers.
                if (
                    width * height < Game.BarrierAreaRange.Min ||
                    width * height > Game.BarrierAreaRange.Max
                )
                {
                    continue;
                }

                var x = Utility.RandomGenerator.Next(
                    Game.InnerCourtArea.TopLeft.X,
                    Game.InnerCourtArea.BottomRight.X - width
                );
                var y = Utility.RandomGenerator.Next(
                    Game.InnerCourtArea.TopLeft.Y,
                    Game.InnerCourtArea.BottomRight.Y - height
                );

                this._barrierList.Add(new Barrier(
                    new Dot(x, y), new Dot(x + width, y + height)
                ));

                isGenerated = true;
            }
        }
    }

    /// <summary>
    /// Refresh the game.
    /// </summary>
    public void Refresh()
    {
        // The game should only refresh when running.
        if (
            this._gameState != GameStateType.Running
        )
        {
            return;
        }

        // Validate the fields.
        if (this._camp == null)
        {
            throw new Exception("The camp is invalid.");
        }
        if (this._startTime == null)
        {
            throw new Exception("The start time is not recorded.");
        }
        if (this._gameStage == GameStageType.PreMatch)
        {
            throw new Exception("The game runs in pre-match stage.");
        }

        // Attempt to generate an order
        var order = this._orderGenerator.Generate((long)this.GameTime);
        if (order != null)
        {
            this._orderList.Add(order);
        }

        // End the game if the time is up.
        if ((long)this.RemainingTime <= 0)
        {
            this.End();
        }

        // Update the time of the last tick.
        this._lastTickTime = Utility.SystemTime;
    }

    /// <summary>
    /// Start a part of the game.
    /// </summary>
    /// <param name="camp">The camp participating in the game.</param>
    /// <param name="gameStage">The game stage.</param>
    public void Start(CampType camp, GameStageType gameStage)
    {
        if (
            this._gameState != GameStateType.Unstarted &&
            this._gameState != GameStateType.Ended
        )
        {
            throw new Exception("The game has started.");
        }

        // Validate the parameters
        if (gameStage == GameStageType.PreMatch)
        {
            throw new Exception("The game stage is invalid.");
        }

        this._gameState = GameStateType.Running;

        // Set the metadata.
        this._camp = camp;
        this._gameStage = gameStage;

        // Set the vehicles.
        this._vehicle = new Dictionary<CampType, Vehicle>
        {
            { CampType.A, new Vehicle(CampType.A) },
            { CampType.B, new Vehicle(CampType.B) }
        };

        // Set the order generator.
        switch (this._camp)
        {
            case CampType.A:
                var tmp = new List<Order>();
                this._orderGenerator = new OrderGenerator(
                    count: (int)Game.OrderNumber[this._gameStage],
                    area: Game.InnerCourtArea,
                    generationTimeRange: (0, (long)Game.GameDuration[this._gameStage]),
                    timeLimitRange: Game.OrderDeliveryDurationRange,
                    out tmp // ???
                );
                break;

            case CampType.B:
                if (this._orderGenerator == null)
                {
                    throw new Exception("The order generator is null.");
                }
                this._orderGenerator.Reset();
                break;

            default:
                throw new Exception("The camp is invalid.");
        }
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    public void Pause()
    {
        if (this._gameState != GameStateType.Running)
        {
            throw new Exception("The game is not running.");
        }

        this._gameState = GameStateType.Paused;

        // Record the time when start to pause.
        this._pauseTime = Utility.SystemTime;
    }

    /// <summary>
    /// Continue the game.
    /// </summary>
    public void Continue()
    {
        if (this._gameState != GameStateType.Paused)
        {
            throw new Exception("The game is not paused.");
        }

        this._gameState = GameStateType.Running;

        // To reduce the paused time in the game time.
        this._startTime += Utility.SystemTime - this._pauseTime;
    }

    /// <summary>
    /// End the game.
    /// </summary>
    public void End()
    {
        if (this._gameState != GameStateType.Running)
        {
            throw new Exception("The game is not running.");
        }

        this._gameState = GameStateType.Ended;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Check if a position is in barrier area.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>
    /// True if the poisition is in barrier area; otherwise false.
    /// </returns>
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
    /// Check if a position is in wall area.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>
    /// True if the poisition is in wall area; otherwise false.
    /// </returns>
    private bool IsInWall(Dot position)
    {
        foreach (var wall in Game.WallList)
        {
            if (wall.IsIn(position))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Check if a position is in the influence scope of charging
    /// piles of a camp.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="camp">The camp.</param>
    /// <param name="reverse">
    /// True if to check the position is in the influence scope of 
    /// charging piles of other camps.
    /// </param>
    /// <returns>
    /// True if the position is in the influence scope; otherwise false.
    /// </returns>
    private bool IsInChargingPileInfluenceScope(
        Dot position,
        CampType camp,
        bool reverse = false
    )
    {
        foreach (var chargingPile in this._chargingPileList)
        {
            if (
                (
                    (
                        chargingPile.Camp == camp && !reverse
                    ) ||
                    (
                        chargingPile.Camp != camp && reverse
                    )
                ) &&
                chargingPile.IsInInfluenceScope(position)
            )
            {
                return true;
            }
        }
        return false;
    }

    #endregion
}