using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;
using System.Diagnostics;

namespace EDCHOST22
{
    // 比赛状态：未开始、正常进行中、暂停、结束
    public enum GameState { UNSTART = 0, NORMAL = 1, PAUSE = 2, END = 3 };

    // 比赛阶段：第一回合A车，第一回合B车，第二回合A车，第二回合B车，比赛结束
    public enum GameStage { FIRST_A = 0, FIRST_B, SECOND_B, SECOND_A ,END};

    public class Game
    {
        public const int MINE_COUNT_MAX = 2;        // 场上最多同时有2个矿

        public bool DebugMode;          // 调试模式，最大回合数 = 1,000,000
        public GameState mGameState;     // 比赛状态
        public GameStage mGameStage;     // 比赛阶段
        public Camp UpperCamp;          // 当前回合需先上场的一方
        public Car CarA, CarB;          // 定义小车
        public Beacon mBeacon;          // 信标
        public int mPrevTime;           // 时间均改为以毫秒为单位,记录上一次的时间，精确到毫秒，实时更新
        public int mGameTime;           // 时间均改为以毫秒为单位
        public FileStream FoulTimeFS;   // 犯规记录
        public int mLastOnBeaconTime;   // 上一次触碰信标的时间，防止多次重复判断撞信标
        public MineGenerator mMineGenerator;    // 用于生成金矿序列
        public Mine[] mMineArray;        // 当前在场上的矿的数组
        public int[] mMineInMaze;          // 两矿是否在场上（1在场上，0已被收集运走）
        public Dot[] mParkDots;             // 场上停车点的坐标数组
        public int mIsOverTime;             // 是否是加时赛
        public int mSecCount;             // 数到1sec


        // 构造一个新的Game类，默认为CampA是先上半场上一阶段进行
        public Game()
        {
            Debug.WriteLine("开始执行Game构造函数");
            mGameState = GameState.UNSTART;
            mGameStage = GameStage.FIRST_A;
            UpperCamp = Camp.A;
            CarA = new Car(Camp.A, 0);
            CarB = new Car(Camp.B, 0);
            mBeacon = new Beacon();
            mPrevTime = GetCurrentTime();
            mGameTime = 0;
            mLastOnBeaconTime = -10;
            Debug.WriteLine("Game构造函数FIRST_A执行完毕");
            mMineGenerator = new MineGenerator();
            mMineArray = mMineGenerator.GetStage1Mine();
            mMineInMaze = new int[MineGenerator.COURTMINENUM];
            mIsOverTime = 0;
            for (int i = 0; i < MineGenerator.COURTMINENUM; i++)
            {
                mMineInMaze[i] = 1;
            }
            mSecCount = 0;
            mParkDots = Court.GetParkDots();
        }

        #region 辅助函数

        // 获取当前时间（秒）
        public int GetCurrentTime()
        {
            System.DateTime currentTime = System.DateTime.Now;
            int time = currentTime.Hour * 3600000 + currentTime.Minute * 60000 + currentTime.Second * 1000;
            //Debug.WriteLine("H, M, S: {0}, {1}, {2}", currentTime.Hour, currentTime.Minute, currentTime.Second);
            //Debug.WriteLine("GetCurrentTime，Time = {0}", time); 
            return time;
        }

        // 获取有效Beacon的数组
        // flag：0返回A和B设置的全部有效信标；1返回A设置的有效信标；2返回B设置的有效信标
        public Dot[] GetBeacon(int flag)
        {
            if (flag == 0)
            {
                Dot[] ret = new Dot[mBeacon.CarABeaconNum + mBeacon.CarBBeaconNum];
                int idx = 0;
                for (int i = 0; i < mBeacon.CarABeaconNum; i++)
                {
                    ret[idx] = new Dot(mBeacon.CarABeacon[i].x, mBeacon.CarABeacon[i].y);
                    idx++;
                }
                for (int i = 0; i< mBeacon.CarBBeaconNum; i++)
                {
                    ret[idx++] = new Dot(mBeacon.CarBBeacon[i].x, mBeacon.CarBBeacon[i].y);
                    idx++;
                }
                return ret;
            }
            else if (flag == 1)
            {
                Dot[] ret = new Dot[mBeacon.CarABeaconNum];
                int idx = 0;
                for (int i = 0; i < mBeacon.CarABeaconNum; i++)
                {
                    ret[idx] = new Dot(mBeacon.CarABeacon[i].x, mBeacon.CarABeacon[i].y);
                    idx++;
                }
                return ret;

            }
            else if (flag == 2)
            {
                Dot[] ret = new Dot[mBeacon.CarBBeaconNum];
                int idx = 0;
                for (int i = 0; i < mBeacon.CarBBeaconNum; i++)
                {
                    ret[idx] = new Dot(mBeacon.CarBBeacon[i].x, mBeacon.CarBBeacon[i].y);
                    idx++;
                }
                return ret;
            }
            else
            {
                return new Dot[0];
            }
        }

        #endregion


        #region 自动更新

        // 判断车是否在中心矿区内（及进入矿区自动加分）
        public void JudgeAIsInMaze()
        {
            //Debug.WriteLine("开始执行JudgeAIsInMaze");
            if (CarA.mPos.x >= Court.BORDER_CM
                && CarA.mPos.x <= Court.BORDER_CM + Court.MAZE_SIZE_CM
                && CarA.mPos.y >= Court.BORDER_CM
                && CarA.mPos.y <= Court.BORDER_CM + Court.MAZE_SIZE_CM)
            {
                //Debug.WriteLine("A 在 Maze 中");
                CarA.mIsInMaze = 1;
                if (CarA.WhetherCarIn == 0)
                {
                    CarA.SetCarIn();
                }
            }
            else
            {
                //Debug.WriteLine("A 不在 Maze 中");
                CarA.mIsInMaze = 0;
            }
        }
        public void JudgeBIsInMaze()
        {
            //Debug.WriteLine("开始执行JudgeAIsInMaze");
            if (CarB.mPos.x >= Court.BORDER_CM
                && CarB.mPos.x <= Court.BORDER_CM + Court.MAZE_SIZE_CM
                && CarB.mPos.y >= Court.BORDER_CM
                && CarB.mPos.y <= Court.BORDER_CM + Court.MAZE_SIZE_CM)
            {
                //Debug.WriteLine("A 在 Maze 中");
                CarB.mIsInMaze = 1;
                if (CarB.WhetherCarIn == 0)
                {
                    CarB.SetCarIn();
                }
            }
            else
            {
                //Debug.WriteLine("A 不在 Maze 中");
                CarB.mIsInMaze = 0;
            }
        }

        // 判断是否在场地内
        public void JudgeAIsInField()
        {
            //Debug.WriteLine("开始执行JudgeAIsInField");
            if (CarA.mPos.x >= 0
                && CarA.mPos.x <= Court.MAX_SIZE_CM
                && CarA.mPos.y >= 0
                && CarA.mPos.y <= Court.MAX_SIZE_CM)
            {
                //Debug.WriteLine("A 在 Field 中");
                CarA.mIsInField = 1;
            }
            else
            {
                //Debug.WriteLine("A 不在 Field 中");
                CarA.mIsInField = 0;
            }
        }
        public void JudgeBIsInField()
        {
            //Debug.WriteLine("开始执行JudgeBIsInField");
            if (CarB.mPos.x >= 0
                && CarB.mPos.x <= Court.MAX_SIZE_CM
                && CarB.mPos.y >= 0
                && CarB.mPos.y <= Court.MAX_SIZE_CM)
            {
                //Debug.WriteLine("B 在 Field 中");
                CarB.mIsInField = 1;
            }
            else
            {
                //Debug.WriteLine("B 不在 Field 中");
                CarB.mIsInField = 0;
            }
        }

        // 更新游戏时间
        public void UpdateGameTime()
        {
            if (mGameState == GameState.NORMAL)
            {
                mGameTime = GetCurrentTime() - mPrevTime + mGameTime;
            }
            mPrevTime = GetCurrentTime();
        }

        // 更新车第一回合的超时
        public void UpdateCarAOverTime()
        {
            if (mGameState == GameState.NORMAL && mGameStage == GameStage.FIRST_A)
            {
                if (mGameTime > 60000)
                {
                    CarA.setOverTime((mGameTime - 60000) / 1000);
                }
            }
        }
        public void UpdateCarBOverTime()
        {
            if (mGameState == GameState.NORMAL && mGameStage == GameStage.FIRST_B)
            {
                if (mGameTime > 60000)
                {
                    CarB.setOverTime((mGameTime - 60000) / 1000);
                }
            }
        }


        // 更新车的发送位置
        public void UpdateCarATransPos()
        {
            if (mGameState == GameState.NORMAL)
            {
                if (mGameStage == GameStage.FIRST_A)
                {
                    CarA.mTransPos = CarA.mPos;
                    CarA.mRightPos = 1;
                }
                else if (mGameStage == GameStage.SECOND_A && CarA.mIsInMaze != 1)
                {
                    CarA.mTransPos = CarA.mPos;
                    CarA.mRightPos = 1;
                }
                else
                {
                    CarA.mTransPos.SetInfo(-10, -10);
                    CarA.mRightPos = 0;
                }
            }
        }
        public void UpdateCarBTransPos()
        {
            if (mGameState == GameState.NORMAL)
            {
                if (mGameStage == GameStage.FIRST_B)
                {
                    CarB.mTransPos = CarB.mPos;
                    CarB.mRightPos = 1;
                }
                else if (mGameStage == GameStage.SECOND_B && CarB.mIsInMaze != 1)
                {
                    CarB.mTransPos = CarB.mPos;
                    CarB.mRightPos = 1;
                }
                else
                {
                    CarB.mTransPos.SetInfo(-10, -10);
                    CarB.mRightPos = 0;
                }
            }
        }

        // 更新车碰到对家信标
        public void CheckCarAOnBeacon()
        {
            if (mGameStage == GameStage.SECOND_A)
            {
                Dot[] beaconArray = GetBeacon(2);
                if (Dot.InCollisionZones(CarA.mPos, beaconArray) && 
                    (!Dot.InCollisionZones(CarA.mLastPos, beaconArray)) &&
                    mGameTime - mLastOnBeaconTime >= 1000)
                {
                    CarA.AddCrossBeacon();
                    Debug.WriteLine("A车撞到了B放置的信标，位置 x {0}, y {1}", CarA.mPos.x, CarA.mPos.y);
                    mLastOnBeaconTime = mGameTime;
                }
            }
        }
        public void CheckCarBOnBeacon()
        {
            if (mGameStage == GameStage.SECOND_B)
            {
                Dot[] beaconArray = GetBeacon(1);
                if (Dot.InCollisionZones(CarB.mPos, beaconArray) &&
                    (!Dot.InCollisionZones(CarB.mLastPos, beaconArray)) &&
                    mGameTime - mLastOnBeaconTime >= 1000)
                {
                    CarB.AddCrossBeacon();
                    Debug.WriteLine("B车撞到了A放置的信标，位置 x {0}, y {1}", CarB.mPos.x, CarB.mPos.y);
                    mLastOnBeaconTime = mGameTime;
                }
            }
        }

        // 更新车收集到了金矿
        public void CheckCarAGetMine()
        {
            if (mGameState == GameState.NORMAL)
            {
                if (mGameStage == GameStage.FIRST_A)
                {
                    if (CarA.IsMineStateFull())
                    {
                        return;
                    }
                    int flag = -1;
                    for (int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        if (Dot.InCollisionZone(CarA.mPos, mMineArray[i].Pos) && mMineInMaze[i] == 1)
                        {
                            flag = i;
                            break;
                        }
                    }
                    if (flag != -1)
                    {
                        mMineInMaze[flag] = 0;
                        CarA.AddMineLoad(0);
                        CarA.AddMineState();
                    }
                }
                else if (mGameStage == GameStage.SECOND_A)
                {
                    if (CarA.IsMineStateFull())
                    {
                        return;
                    }
                    int flag = -1;
                    for (int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        if (Dot.InCollisionZone(CarA.mPos, mMineArray[i].Pos) && mMineInMaze[i] == 1)
                        {
                            flag = i;
                            break;
                        }
                    }
                    if (flag != -1)
                    {
                        mMineInMaze[flag] = 0;
                        CarA.AddMineLoad(1, mMineArray[flag].Type);
                        CarA.AddMineState(mMineArray[flag].Type);
                    }
                }
                else
                {
                    return;
                }
            }
        }
        public void CheckCarBGetMine()
        {
            if (mGameState == GameState.NORMAL)
            {
                if (mGameStage == GameStage.FIRST_B)
                {
                    if (CarB.IsMineStateFull())
                    {
                        return;
                    }
                    int flag = -1;
                    for (int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        if (Dot.InCollisionZone(CarB.mPos, mMineArray[i].Pos) && mMineInMaze[i] == 1)
                        {
                            flag = i;
                            break;
                        }
                    }
                    if (flag != -1)
                    {
                        mMineInMaze[flag] = 0;
                        CarB.AddMineLoad(0);
                        CarB.AddMineState();
                    }
                }
                else if (mGameStage == GameStage.SECOND_B)
                {
                    if (CarB.IsMineStateFull())
                    {
                        return;
                    }
                    int flag = -1;
                    for (int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        if (Dot.InCollisionZone(CarB.mPos, mMineArray[i].Pos) && mMineInMaze[i] == 1)
                        {
                            flag = i;
                            break;
                        }
                    }
                    if (flag != -1)
                    {
                        mMineInMaze[flag] = 0;
                        CarB.AddMineLoad(1, mMineArray[flag].Type);
                        CarB.AddMineState(mMineArray[flag].Type);
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // 更新车是否完成矿的运送
        public void CheckCarAUnloadMine()
        {
            if (mGameState == GameState.NORMAL)
            {
                if (mGameStage == GameStage.FIRST_A)
                {
                    if (CarA.mMineStateSum() == MINE_COUNT_MAX &&
                        Dot.InCollisionZones(CarA.mPos, mParkDots))
                    {
                        mSecCount++;
                    }
                    if (mSecCount >= 9)
                    {
                        mSecCount = 0;
                        CarA.AddMineUnload(0);
                        CarA.ClearMineState();
                        if (mGameTime < 60000)
                        {
                            CarA.SetAheadSec((60000 - mGameTime) / 1000);
                        }
                    }
                }
                else if (mGameStage == GameStage.SECOND_A)
                {
                    int BeaconFlag = -1;
                    for (int i = 0; i < mBeacon.CarABeaconNum; i++)
                    {
                        if (Dot.InCollisionZone(CarA.mPos, mBeacon.CarABeacon[i]))
                        {
                            BeaconFlag = i;
                            break;
                        }
                    }
                    if (BeaconFlag == -1 || BeaconFlag == mBeacon.CarABeaconNum)
                    {
                        int ParkFlag = -1;
                        for (int i = 0; i < Court.TOTAL_PARKING_AREA; i++)
                        {
                            if (Dot.InCollisionZone(CarA.mPos, mParkDots[i]))
                            {
                                ParkFlag = i;
                                break;
                            }
                        }
                        if (ParkFlag != -1 && ParkFlag != Court.TOTAL_PARKING_AREA && CarA.mMineState[(int)mMineGenerator.ParkType[ParkFlag]] != 0)
                        {
                            CarA.AddMineUnload(1, mMineGenerator.ParkType[ParkFlag]);
                            CarA.ClearMineTypeState(mMineGenerator.ParkType[ParkFlag]);
                        }
                    }
                    else
                    {
                        if (CarA.mMineState[(int)mBeacon.CarABeaconMineType[BeaconFlag]] != 0)
                        {
                            CarA.AddMineUnload(1, mBeacon.CarABeaconMineType[BeaconFlag]);
                            CarA.ClearMineTypeState(mBeacon.CarABeaconMineType[BeaconFlag]);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }
        public void CheckCarBUnloadMine()
        {
            if (mGameState == GameState.NORMAL)
            {
                if (mGameStage == GameStage.FIRST_B)
                {
                    if (CarB.mMineStateSum() == MINE_COUNT_MAX &&
                        Dot.InCollisionZones(CarB.mPos, mParkDots))
                    {
                        mSecCount++;
                    }
                    if (mSecCount >= 9)
                    {
                        mSecCount = 0;
                        CarB.AddMineUnload(0);
                        CarB.ClearMineState();
                        if (mGameTime < 60000)
                        {
                            CarB.SetAheadSec((60000 - mGameTime) / 1000);
                        }
                    }
                }
                else if (mGameStage == GameStage.SECOND_B)
                {
                    int BeaconFlag = -1;
                    for (int i = 0; i < mBeacon.CarBBeaconNum; i++)
                    {
                        if (Dot.InCollisionZone(CarB.mPos, mBeacon.CarBBeacon[i]))
                        {
                            BeaconFlag = i;
                            break;
                        }
                    }
                    if (BeaconFlag == -1 || BeaconFlag == mBeacon.CarBBeaconNum)
                    {
                        int ParkFlag = -1;
                        for (int i = 0; i < Court.TOTAL_PARKING_AREA; i++)
                        {
                            if (Dot.InCollisionZone(CarB.mPos, mParkDots[i]))
                            {
                                ParkFlag = i;
                                break;
                            }
                        }
                        if (ParkFlag != -1 && ParkFlag != Court.TOTAL_PARKING_AREA && CarB.mMineState[(int)mMineGenerator.ParkType[ParkFlag]] != 0)
                        {
                            CarB.AddMineUnload(1, mMineGenerator.ParkType[ParkFlag]);
                            CarB.ClearMineTypeState(mMineGenerator.ParkType[ParkFlag]);
                        }
                    }
                    else
                    {
                        if (CarB.mMineState[(int)mBeacon.CarBBeaconMineType[BeaconFlag]] != 0)
                        {
                            CarB.AddMineUnload(1, mBeacon.CarBBeaconMineType[BeaconFlag]);
                            CarB.ClearMineTypeState(mBeacon.CarBBeaconMineType[BeaconFlag]);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // 更新矿的分布
        public void UpdateMine()
        {
            if (mGameStage == GameStage.FIRST_A ||
                mGameStage == GameStage.FIRST_B ||
                mGameStage == GameStage.END ||
                mGameState != GameState.NORMAL)
            {
                return;
            }
            Debug.WriteLine("开始执行UpdateMine");
            for (int  i = 0; i < MINE_COUNT_MAX; i++)
            {
                if (mMineInMaze[i] == 0)
                {
                    mMineInMaze[i] = 1;
                    mMineArray[i] = mMineGenerator.GetNextMine();
                    Debug.WriteLine($"更新矿物位置x = {0}, y = {1}", mMineArray[i].Pos.x, mMineArray[i].Pos.y);
                }
            }
            Debug.WriteLine("执行完毕UpdateMine");
        }

        // 转换到下一阶段
        public void CheckNextStage()
        {
            if (mGameStage == GameStage.FIRST_A)
            {
                int flag = 0;
                for (int i = 0; i < MINE_COUNT_MAX; i++)
                {
                    flag += mMineInMaze[i];
                }
                if (mGameTime > 180000 || (flag == 0 && CarA.mMineStateSum() == 0))
                {
                    mGameState = GameState.UNSTART;
                    mGameStage++;
                    for(int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        mMineInMaze[i] = 1;
                    }
                    UpperCamp = Camp.B;
                    CarA.mTaskState = 1;
                    if (FoulTimeFS != null)
                    {
                        byte[] data = Encoding.Default.GetBytes($"nextStage\r\n");
                        FoulTimeFS.Write(data, 0, data.Length);
                        // 如果不加以下两行的话，数据无法写到文件中
                        FoulTimeFS.Flush();
                        //FoulTimeFS.Close();
                    }
                    Debug.WriteLine("成功进入下一个stage");
                }
            }
            else if (mGameStage == GameStage.FIRST_B)
            {
                int flag = 0;
                for (int i = 0; i < MINE_COUNT_MAX; i++)
                {
                    flag += mMineInMaze[i];
                }
                if (mGameTime > 180000 || (flag == 0 && CarB.mMineStateSum() == 0))
                {
                    mGameState = GameState.UNSTART;
                    mMineGenerator.GenerateStage2(mBeacon);
                    for (int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        mMineInMaze[i] = 1;
                        mMineArray[i] = mMineGenerator.GetNextMine();
                    }
                    mGameStage++;
                    CarB.mTaskState = 1;
                    if (FoulTimeFS != null)
                    {
                        byte[] data = Encoding.Default.GetBytes($"nextStage\r\n");
                        FoulTimeFS.Write(data, 0, data.Length);
                        // 如果不加以下两行的话，数据无法写到文件中
                        FoulTimeFS.Flush();
                        //FoulTimeFS.Close();
                    }
                    Debug.WriteLine("成功进入下一个stage");
                }
            }
            else if (mGameStage == GameStage.SECOND_B)
            {
                if (mGameTime > 120000 / (1 + mIsOverTime))
                {
                    mGameState = GameState.UNSTART;
                    mMineGenerator.Reset();
                    for (int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        mMineInMaze[i] = 1;
                        mMineArray[i] = mMineGenerator.GetNextMine();
                    }
                    mLastOnBeaconTime = -10;
                    UpperCamp = Camp.A;
                    mGameStage++;
                    if (FoulTimeFS != null)
                    {
                        byte[] data = Encoding.Default.GetBytes($"nextStage\r\n");
                        FoulTimeFS.Write(data, 0, data.Length);
                        // 如果不加以下两行的话，数据无法写到文件中
                        FoulTimeFS.Flush();
                        //FoulTimeFS.Close();
                    }
                    Debug.WriteLine("成功进入下一个stage");
                }
            }
            else if (mGameStage == GameStage.SECOND_A)
            {
                if (mGameTime > 120000 / (1 + mIsOverTime))
                {
                    mGameState = GameState.END;
                    mMineGenerator.Reset();
                    for (int i = 0; i < MINE_COUNT_MAX; i++)
                    {
                        mMineInMaze[i] = 1;
                        mMineArray[i] = mMineGenerator.GetNextMine();
                    }
                    mLastOnBeaconTime = -10;
                    mGameStage++;
                    if (FoulTimeFS != null)
                    {
                        byte[] data = Encoding.Default.GetBytes($"end\r\n");
                        FoulTimeFS.Write(data, 0, data.Length);
                        // 如果不加以下两行的话，数据无法写到文件中
                        FoulTimeFS.Flush();
                        //FoulTimeFS.Close();
                    }
                    Debug.WriteLine("比赛结束");
                }
            }
            else
            {
                return;
            }
        }



        // Game的更新
        public void Update()
        {
            if (mGameState == GameState.NORMAL)
            {
                UpdateGameTime();
                UpdateMine();
                if (mGameStage == GameStage.FIRST_A || mGameStage == GameStage.SECOND_A)
                {
                    JudgeAIsInField();
                    JudgeAIsInMaze();
                    CheckCarAGetMine();
                    CheckCarAOnBeacon();
                    CheckCarAUnloadMine();
                    UpdateCarATransPos();
                    UpdateCarAOverTime();
                }
                else if (mGameStage == GameStage.FIRST_B || mGameStage == GameStage.SECOND_B)
                {
                    JudgeBIsInField();
                    JudgeBIsInMaze();
                    CheckCarBGetMine();
                    CheckCarBOnBeacon();
                    CheckCarBUnloadMine();
                    UpdateCarBTransPos();
                    UpdateCarBOverTime();
                }
                CheckNextStage();
            }
        }

        #endregion


        #region 按键触发

        // 开始按键
        // 开始任意一个stage的比赛
        public void Start()
        {
            if (mGameState == GameState.UNSTART)
            {
                mGameState = GameState.NORMAL;
                mGameTime = 0;
                mPrevTime = GetCurrentTime();
                mSecCount = 0;
                CarA.ClearMineState();
                CarB.ClearMineState();
                Debug.WriteLine("start");
            }
        }

        // 设置信标
        public void SetBeacon(MineType type)
        {
            if (mGameState == GameState.END || mGameState == GameState.PAUSE || mGameState == GameState.UNSTART ||
                mGameStage == GameStage.END || mGameStage == GameStage.SECOND_A || mGameStage == GameStage.SECOND_B)
            {
                return;
            }
            if (UpperCamp == Camp.A && mGameStage == GameStage.FIRST_A)
            {
                if (mBeacon.CarABeaconNum >= Beacon.MAX_BEACON_NUM)
                {
                    return;
                }
                if (CarA.mIsInMaze != 1)
                {
                    return;
                }
                mBeacon.CarAAddBeacon(CarA.mPos, type);
                CarA.AddBeaconCount();
            }
            else if (UpperCamp == Camp.B && mGameStage == GameStage.FIRST_B)
            {
                if (mBeacon.CarBBeaconNum >= Beacon.MAX_BEACON_NUM)
                {
                    return;
                }
                if (CarB.mIsInMaze != 1)
                {
                    return;
                }
                mBeacon.CarBAddBeacon(CarB.mPos, type);
                CarB.AddBeaconCount();
            }
        }

        // 暂停按键
        public void Pause()
        {
            mGameState = GameState.PAUSE;
        }

        // 继续按键
        public void Continue()
        {
            mGameState = GameState.NORMAL;
            mPrevTime = GetCurrentTime();
        }

        // 重置按键，初始化比赛
        // 仅在比赛暂停(GameState.PAUSE)或未开始(GameState.UNSTART)或结束(GameState.END)时可以初始化比赛
        public void Reset()
        {
            if (mGameState == GameState.NORMAL)
            {
                return;
            }
            Debug.WriteLine("初始化比赛");
            mGameState = GameState.UNSTART;
            mGameStage = GameStage.FIRST_A;
            UpperCamp = Camp.A;
            CarA = new Car(Camp.A, 0);
            CarB = new Car(Camp.B, 0);
            mBeacon = new Beacon();
            mPrevTime = GetCurrentTime();
            mGameTime = 0;
            mLastOnBeaconTime = -10;
            mMineGenerator = new MineGenerator();
            mMineArray = mMineGenerator.GetStage1Mine();
            mMineInMaze = new int[MineGenerator.COURTMINENUM];
            mIsOverTime = 0;
            for (int i = 0; i < MineGenerator.COURTMINENUM; i++)
            {
                mMineInMaze[i] = 1;
            }
            mSecCount= 0;
            Debug.WriteLine("Game构造函数FIRST_A执行完毕");
            mParkDots = Court.GetParkDots();
        }

        // 进行加时赛
        // 仅比赛结束(GameState.END)时可以使用
        public void OverTime()
        {
            if (mGameState != GameState.END)
            {
                return;
            }
            mIsOverTime = 1;
            mGameStage = GameStage.SECOND_B;
            mGameState = GameState.UNSTART;
            mMineGenerator.GenerateStage2(mBeacon);
            mMineGenerator.Reset();
            for (int i = 0; i < MINE_COUNT_MAX; i++)
            {
                mMineInMaze[i] = 1;
                mMineArray[i] = mMineGenerator.GetNextMine();
            }
            UpperCamp = Camp.B;
            mPrevTime = GetCurrentTime();
            mGameTime = 0;
            mLastOnBeaconTime = -10;
        }

        #endregion


        #region 通信
        public byte[] PackCarAMessage()//已更新到最新通信协议
        {
            byte[] message = new byte[48]; //上位机传递多少信息
            int messageCnt = 0;
            int gTimeTrans = mGameTime / 100;
            message[messageCnt++] = (byte)((gTimeTrans) >> 8);
            message[messageCnt++] = (byte)(gTimeTrans);
            message[messageCnt++] = (byte)((((byte)mGameState << 6) & 0xC0) | (((byte)CarA.mTaskState << 5) & 0x20) |
                (((byte)mMineInMaze[0] << 4) & 0x10) | (((byte)mMineInMaze[1] << 3) & 0x08));
            message[messageCnt++] = (byte)(CarA.mTransPos.x);
            message[messageCnt++] = (byte)(CarA.mTransPos.y);
            message[messageCnt++] = (byte)((((byte)mMineArray[0].Type << 6) & 0xC0) | (((byte)mMineArray[1].Type << 4) & 0x30) | ((byte)CarA.mMineStateSum()));
            message[messageCnt++] = (byte)((((byte)CarA.mMineState[(int)MineType.A] << 4) & 0xF0)| ((byte)CarA.mMineState[(int)MineType.B]));
            message[messageCnt++] = (byte)((((byte)CarA.mMineState[(int)MineType.C] << 4) & 0xF0) | ((byte)CarA.mMineState[(int)MineType.D]));
            message[messageCnt++] = (byte)((((byte)mBeacon.CarABeaconMineType[0] << 6) & 0xC0) | (((byte)mBeacon.CarABeaconMineType[1] << 4) & 0x30) | (((byte)mBeacon.CarABeaconMineType[2] << 2) & 0x0C));
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarA.mPos) >> 24);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarA.mPos) >> 16);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarA.mPos) >> 8);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarA.mPos));
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarA.mPos) >> 24);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarA.mPos) >> 16);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarA.mPos) >> 8);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarA.mPos));
            if (mGameStage == GameStage.SECOND_A)
            {
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[0], CarA.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[0], CarA.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[1], CarA.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[1], CarA.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[2], CarA.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[2], CarA.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[0], CarA.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[0], CarA.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[1], CarA.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[1], CarA.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[2], CarA.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[2], CarA.mPos)));
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    message[messageCnt++] = (byte)0x00;
                }
            }
            message[messageCnt++] = (byte)((((byte)((int)mMineGenerator.ParkType[0]) << 6) & 0xC0) | (((byte)((int)mMineGenerator.ParkType[1]) << 4) & 0x30) | (((byte)((int)mMineGenerator.ParkType[2]) << 2) & 0x0C) | (byte)((int)mMineGenerator.ParkType[3]));
            message[messageCnt++] = (byte)((((byte)((int)mMineGenerator.ParkType[4]) << 6) & 0xC0) | (((byte)((int)mMineGenerator.ParkType[5]) << 4) & 0x30) | (((byte)((int)mMineGenerator.ParkType[6]) << 2) & 0x0C) | (byte)((int)mMineGenerator.ParkType[7]));
            message[messageCnt++] = (byte)(((CarA.mIsInMaze << 7) & 0x80)
                | ((((mGameState == GameState.NORMAL) && (CarA.mIsInField != 0) && ((mGameStage == GameStage.FIRST_A) || ((mGameStage == GameStage.SECOND_A) && (CarA.mIsInMaze != 1))) ? 1 : 0) << 6) & 0x40)
                | ((((0 < mBeacon.CarABeaconNum) ? 1 : 0) * ((mGameStage == GameStage.SECOND_A || mGameStage == GameStage.FIRST_A) ? 1 : 0) << 5) & 0x20)
                | ((((1 < mBeacon.CarABeaconNum) ? 1 : 0) * ((mGameStage == GameStage.SECOND_A || mGameStage == GameStage.FIRST_A) ? 1 : 0) << 4) & 0x10)
                | ((((2 < mBeacon.CarABeaconNum) ? 1 : 0) * ((mGameStage == GameStage.SECOND_A || mGameStage == GameStage.FIRST_A) ? 1 : 0) << 3) & 0x08)
                | ((((0 < mBeacon.CarBBeaconNum) ? 1 : 0) * ((mGameStage == GameStage.SECOND_A || mGameStage == GameStage.FIRST_A) ? 1 : 0) << 2) & 0x04)
                | ((((1 < mBeacon.CarBBeaconNum) ? 1 : 0) * ((mGameStage == GameStage.SECOND_A || mGameStage == GameStage.FIRST_A) ? 1 : 0) << 1) & 0x02)
                | (((2 < mBeacon.CarBBeaconNum) ? 1 : 0) * ((mGameStage == GameStage.SECOND_A || mGameStage == GameStage.FIRST_A) ? 1 : 0) & 0x01));
            message[messageCnt++] = (byte)(CarA.MyScore >> 8);
            message[messageCnt++] = (byte)(CarA.MyScore);
            if (mGameStage == GameStage.SECOND_A)
            {
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[0].x);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[0].y);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[1].x);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[1].y);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[2].x);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[2].y);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[0].x);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[0].y);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[1].x);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[1].y);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[2].x);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[2].y);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    message[messageCnt++] = (byte)0x00;
                }
            }
            message[messageCnt++] = 0x0D;
            message[messageCnt++] = 0x0A;
            return message;
        }
        public byte[] PackCarBMessage()//已更新到最新通信协议
        {
            byte[] message = new byte[48]; //上位机传递多少信息
            int messageCnt = 0;
            int gTimeTrans = mGameTime / 100;
            message[messageCnt++] = (byte)((gTimeTrans) >> 8);
            message[messageCnt++] = (byte)(gTimeTrans);
            message[messageCnt++] = (byte)((((byte)mGameState << 6) & 0xC0) | (((byte)CarB.mTaskState << 5) & 0x20) |
                (((byte)mMineInMaze[0] << 4) & 0x10) | (((byte)mMineInMaze[1] << 3) & 0x08));
            message[messageCnt++] = (byte)(CarB.mTransPos.x);
            message[messageCnt++] = (byte)(CarB.mTransPos.y);
            message[messageCnt++] = (byte)((((byte)mMineArray[0].Type << 6) & 0xC0) | (((byte)mMineArray[1].Type << 4) & 0x30) | ((byte)CarB.mMineStateSum()));
            message[messageCnt++] = (byte)((((byte)CarB.mMineState[(int)MineType.A] << 4) & 0xF0) | ((byte)CarB.mMineState[(int)MineType.B]));
            message[messageCnt++] = (byte)((((byte)CarB.mMineState[(int)MineType.C] << 4) & 0xF0) | ((byte)CarB.mMineState[(int)MineType.D]));
            message[messageCnt++] = (byte)((((byte)mBeacon.CarBBeaconMineType[0] << 6) & 0xC0) | (((byte)mBeacon.CarBBeaconMineType[1] << 4) & 0x30) | (((byte)mBeacon.CarBBeaconMineType[2] << 2) & 0x0C));
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarB.mPos) >> 24);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarB.mPos) >> 16);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarB.mPos) >> 8);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[0], CarB.mPos));
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarB.mPos) >> 24);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarB.mPos) >> 16);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarB.mPos) >> 8);
            message[messageCnt++] = (byte)(Mine.GetIntensity(mMineArray[1], CarB.mPos));
            if (mGameStage == GameStage.SECOND_B)
            {
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[0], CarB.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[0], CarB.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[1], CarB.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[1], CarB.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[2], CarB.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarBBeacon[2], CarB.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[0], CarB.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[0], CarB.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[1], CarB.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[1], CarB.mPos)));
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[2], CarB.mPos)) >> 8);
                message[messageCnt++] = (byte)((int)(Dot.GetDistance(mBeacon.CarABeacon[2], CarB.mPos)));
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    message[messageCnt++] = (byte)0x00;
                }
            }
            message[messageCnt++] = (byte)((((byte)((int)mMineGenerator.ParkType[0]) << 6) & 0xC0) | (((byte)((int)mMineGenerator.ParkType[1]) << 4) & 0x30) | (((byte)((int)mMineGenerator.ParkType[2]) << 2) & 0x0C) | (byte)((int)mMineGenerator.ParkType[3]));
            message[messageCnt++] = (byte)((((byte)((int)mMineGenerator.ParkType[4]) << 6) & 0xC0) | (((byte)((int)mMineGenerator.ParkType[5]) << 4) & 0x30) | (((byte)((int)mMineGenerator.ParkType[6]) << 2) & 0x0C) | (byte)((int)mMineGenerator.ParkType[7]));
            message[messageCnt++] = (byte)(((CarB.mIsInMaze << 7) & 0x80)
                | ((((mGameState == GameState.NORMAL) && (CarB.mIsInField != 0) && ((mGameStage == GameStage.FIRST_B) || ((mGameStage == GameStage.SECOND_B) && (CarB.mIsInMaze != 1))) ? 1 : 0) << 6) & 0x40)
                | ((((0 < mBeacon.CarBBeaconNum) ? 1 : 0) << 5) * ((mGameStage == GameStage.SECOND_B || mGameStage == GameStage.FIRST_B) ? 1 : 0) & 0x20)
                | ((((1 < mBeacon.CarBBeaconNum) ? 1 : 0) << 4) * ((mGameStage == GameStage.SECOND_B || mGameStage == GameStage.FIRST_B) ? 1 : 0) & 0x10)
                | ((((2 < mBeacon.CarBBeaconNum) ? 1 : 0) << 3) * ((mGameStage == GameStage.SECOND_B || mGameStage == GameStage.FIRST_B) ? 1 : 0) & 0x08)
                | ((((0 < mBeacon.CarABeaconNum) ? 1 : 0) << 2) * ((mGameStage == GameStage.SECOND_B || mGameStage == GameStage.FIRST_B) ? 1 : 0) & 0x04)
                | ((((1 < mBeacon.CarABeaconNum) ? 1 : 0) << 1) * ((mGameStage == GameStage.SECOND_B || mGameStage == GameStage.FIRST_B) ? 1 : 0) & 0x02)
                | (((2 < mBeacon.CarABeaconNum) ? 1 : 0) * ((mGameStage == GameStage.SECOND_B) ? 1 : 0) & 0x01));
            message[messageCnt++] = (byte)(CarB.MyScore >> 8);
            message[messageCnt++] = (byte)(CarB.MyScore);
            if (mGameStage == GameStage.SECOND_B)
            {
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[0].x);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[0].y);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[1].x);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[1].y);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[2].x);
                message[messageCnt++] = (byte)(mBeacon.CarBBeacon[2].y);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[0].x);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[0].y);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[1].x);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[1].y);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[2].x);
                message[messageCnt++] = (byte)(mBeacon.CarABeacon[2].y);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    message[messageCnt++] = (byte)0x00;
                }
            }
            message[messageCnt++] = 0x0D;
            message[messageCnt++] = 0x0A;
            return message;
        }

        #endregion
    }
}