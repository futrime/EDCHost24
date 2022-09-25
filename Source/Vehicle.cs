using System.Collections.Generic;
using System.Linq;

namespace EdcHost;

public class Vehicle
{
    public const int RUN_CREDIT = 10;          //小车启动可以得到10分;
    public const int PICK_CREDIT = 5;          //接到一笔订单得5分;
    public const int CHARGE_CREDIT = 5;        // credit for set a charge station
    public const int ON_BLACk_LINE_PENALTY = 10;
    public const int IN_OPPONENT_STATION_PENALTY = 10; // in cm per frame
    public const int IN_OBSTACLE_PENALTY = 10; // in cm per frame
    public const int MARK_PENALTY = 50;
    public const int MAX_PKG_COUNT = 5;
    public const int ENERGY_EXHAUSTION_PENALTY = 50; // 50 ms per cm



    public const int COLLISION_RADIUS = 8;
    public const int COLLISION_DETECTION_TIME = 1000; // in ms
    public const int MAX_MILEAGE = 2000; // in cm


    private Queue<Dot> mQueuePos;   // series of location
    public Camp mCamp;               //A or B get、set直接两个封装好的函数
    private int mScore;               //得分
    private int mMileage;              //小车续航里程
    private List<Order> _deliveringOrder; // orders that are being delivered by car

    //Flag of whether the car is able to run
    private bool mIsAbleToRun;


    // Flags of Location
    // Locations where car would get penalty
    private bool mIsOnBlackLine;
    private bool mIsInOpponentChargeStation;
    private bool mIsInObstacle;


    public Queue<bool> mFlagIsInChargeStation;


    private int mGameTime;


    /********************************************
    Interface
    *********************************************/
    public Vehicle(Camp c)
    {
        mQueuePos = new Queue<Dot>(10);
        mCamp = c;
        mScore = 0;
        mMileage = MAX_MILEAGE;
        _deliveringOrder = new List<Order>();

        // Flags
        mIsAbleToRun = false;
        mIsOnBlackLine = false;
        mIsInOpponentChargeStation = false;
        mIsInObstacle = false;
        mFlagIsInChargeStation = new Queue<bool>(10);

        mGameTime = -1;
    }

    public void Reset()
    {
        mQueuePos.Clear();

        mScore = 0;
        mMileage = MAX_MILEAGE;
        _deliveringOrder.Clear();

        // Flags
        mIsAbleToRun = false;
        mIsOnBlackLine = false;
        mIsInOpponentChargeStation = false;
        mIsInObstacle = false;
        mFlagIsInChargeStation.Clear();

        mGameTime = -1;
    }

    public void Update(Dot _CarPos, int _GameTime,
        bool _IsInObstacle, bool _IsInOpponentStation, bool _IsInChargeStation,
        ref List<Order> ordersRemain, out int _TimePenalty)
    {
        mGameTime = _GameTime;

        UpdatePos(_CarPos);
        int temp_TimePenalty = 0;
        //至少获取了两个位置之后(0.1s)才有后面的操作
        if (_GameTime > 2 * 100)
        {
            if (!mIsAbleToRun)
            {
                AbleToRun();
            }

            _TimePenalty = 0;

            //action
            TakeOrders(_CarPos, ref ordersRemain);
            DeliverPackage(_CarPos);
            //这里不能直接写_TimePenalty，因为它必须写在最外层，故用临时变量temp_TimePenalty作为out
            UpdateMileage(out temp_TimePenalty);
            Charge(_IsInChargeStation);

            // Penalty
            // OnBlackLinePenaly(_IsOnBlackLine);
            InOpponentStationPenalty(_IsInOpponentStation);
            InObstaclePenalty(_IsInObstacle);
        }
        _TimePenalty = temp_TimePenalty;

    }

    public int GetScore()
    {
        return mScore;
    }

    public void GetMark()
    {
        mScore -= MARK_PENALTY;
    }

    public void SetChargeStation()
    {
        mScore += CHARGE_CREDIT;
    }

    public Dot CurrentPos()
    {
        return mQueuePos.ElementAt(mQueuePos.Count - 1);
    }
    /// <summary>
    /// Get the last position
    /// </summary>
    public Dot LastPos()
    {
        if (mQueuePos.Count() > 0)
        {
            return mQueuePos.ElementAt(mQueuePos.Count - 2);
        }
        else
        {
            return null;
        }
    }

    public Order GetPackageOnCar(int index)
    {
        if (index >= _deliveringOrder.Count)
        {
            return null;
        }
        else
        {
            return _deliveringOrder[index];
        }
    }

    public int GetOrderCount()
    {
        return _deliveringOrder.Count;
    }
    public int GetMileage()
    {
        return mMileage;
    }

    /********************************************
    Private Functions
    *********************************************/

    private void UpdatePos(Dot _CarPos)
    {
        mQueuePos.Enqueue(_CarPos);
    }

    private void AbleToRun()
    {
        if (!mIsAbleToRun && mQueuePos.Count() > 1 &&
        Dot.Distance(mQueuePos.ElementAt(mQueuePos.Count - 1), mQueuePos.ElementAt(mQueuePos.Count - 2)) > 0)
        {
            mScore += RUN_CREDIT;
            mIsAbleToRun = true;
        }
    }

    private void TakeOrders(Dot _CarPos, ref List<Order> ordersRemain)      //拾取外卖
    {
        for (int i = 0; i < ordersRemain.Count; i++)
        {
            Order ord = ordersRemain[i];
            if (ord.Status == Order.StatusType.Pending && Dot.Distance(ord.DeparturePosition, _CarPos) <= COLLISION_RADIUS &&
                _deliveringOrder.Count <= MAX_PKG_COUNT)
            {
                // ord.AddFirstCollisionTime(this.mGameTime);
                ord.Take(mGameTime);
                _deliveringOrder.Add(ord);
                ordersRemain.Remove(ord);
                mScore += PICK_CREDIT;
                // 拾取后改变pkg的packagestatus
                ord.Status = Order.StatusType.InDelivery;
                break;
            }
        }
    }

    private void DeliverPackage(Dot _CarPos)      //送达外卖 
    {
        for (int i = 0; i < _deliveringOrder.Count; i++)
        {
            Order ord = _deliveringOrder[i];
            if (ord.Status == Order.StatusType.InDelivery && Dot.Distance(ord.DestinationPosition, _CarPos) <= COLLISION_RADIUS)
            {
                ord.Deliver(mGameTime);
                _deliveringOrder.Remove(ord);
                mScore += ord.Score;
                // if (ord.DepartureFirstCollisionTime != -1 &&
                //     this.mGameTime - ord.DepartureFirstCollisionTime > COLLISION_DETECTION_TIME)
                // {
                //     // 送达后改变pkg的packagestatus
                //     ord.Status = Order.StatusType.Delivered;

                //     _deliveringOrder.Remove(ord);
                //     mScore += ord.GetScore(mGameTime);
                // }
            }
        }
    }

    private void UpdateMileage(out int _Time_Penalty)
    {
        int temp_TimePenalty = 0;
        if (mQueuePos.Count() > 1)
        {
            int DeltaDistance = Dot.Distance(mQueuePos.ElementAt(mQueuePos.Count - 1), mQueuePos.ElementAt(mQueuePos.Count - 2));
            mMileage -= DeltaDistance;
            if (mMileage < 0)
            {
                temp_TimePenalty = DeltaDistance * ENERGY_EXHAUSTION_PENALTY;
            }
            else
            {
                temp_TimePenalty = 0;
            }
        }
        _Time_Penalty = temp_TimePenalty;
    }

    private void Charge(bool IsInChargeStation)
    {
        mFlagIsInChargeStation.Enqueue(IsInChargeStation);
        for (int i = 0; i < mFlagIsInChargeStation.Count(); i++)
        {
            if (!mFlagIsInChargeStation.ElementAt(i))
            {
                return;
            }
        }

        mMileage = MAX_MILEAGE;
    }

    private void OnBlackLinePenaly(bool IsOnBlackLine)
    {
        if (IsOnBlackLine && !mIsOnBlackLine)
        {
            mIsOnBlackLine = IsOnBlackLine;
        }
        else if (!IsOnBlackLine && mIsOnBlackLine)
        {
            mIsOnBlackLine = IsOnBlackLine;
            mScore -= ON_BLACk_LINE_PENALTY;
        }
    }

    private void InOpponentStationPenalty(bool IsInOpponentStation)
    {
        if (IsInOpponentStation)
        {
            mMileage -= IN_OPPONENT_STATION_PENALTY;
        }
    }

    private void InObstaclePenalty(bool IsInObstacle)
    {
        if (mIsInObstacle)
        {
            mMileage -= IN_OBSTACLE_PENALTY;
        }
    }
}