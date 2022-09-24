using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Cvt = EdcHost.MyConvert;
using Point2i = OpenCvSharp.Point;

namespace EdcHost;
public partial class Tracker : Form
{
    // 不合理的坐标
    public static Point2i InvalidPos = new Point2i(-1, -1);

    // 比赛所用参数和场上状况
    public MyFlags flags = null;
    public VideoCapture capture = null;

    // 设定的显示画面四角坐标
    private Point2f[] showCornerPts = null;

    // 坐标转换器
    public CoordinateConverter coordCvt;
    // 定位器
    private Localiser localiser;

    // 以下坐标均为相机坐标
    // 车A坐标
    private Point2i camCarA;
    // 车B坐标
    private Point2i camCarB;
    // 物资坐标
    private Point2i[] camPkgs;

    // 以下坐标均为逻辑坐标
    private Point2i logicCarA;
    private Point2i logicCarB;

    // 以下均为显示坐标
    private Point2i showCarA;
    private Point2i showCarB;

    // 游戏逻辑
    private Game game;
    // 视频输出流
    private VideoWriter vw = null;
    // 与小车进行蓝牙通讯的端口
    public SerialPort serial1, serial2;
    // 可用的端口名称
    public string[] validPorts;


    public Tracker()
    {
        InitializeComponent();

        // Setup Windows Forms controls
        label_RedBG.SendToBack();
        label_BlueBG.SendToBack();
        label_GameCount.Text = "上半场";

        //flags参数类
        flags = new MyFlags();
        flags.Start();

        // Setup the monitor
        capture = new VideoCapture();
        capture.Open(0);

        // 相机画面大小设为视频帧大小
        flags.cameraSize.Width = capture.FrameWidth;
        flags.cameraSize.Height = capture.FrameHeight;

        // 显示大小设为界面组件大小
        flags.showSize.Width = pbCamera.Width;
        flags.showSize.Height = pbCamera.Height;

        // 用于存储鼠标点击的画面中场地的4个角的坐标
        showCornerPts = new Point2f[4];

        // 以既有的flags参数初始化坐标转换器
        coordCvt = new CoordinateConverter(flags);

        // 定位小车位置的类
        localiser = new Localiser();

        // 相机坐标初始化
        camCarA = new Point2i();
        camCarB = new Point2i();
        camPkgs = new Point2i[0];

        // 逻辑坐标初始化
        logicCarA = new Point2i();
        logicCarB = new Point2i();

        // 显示坐标初始化
        showCarA = new Point2i();
        showCarB = new Point2i();

        buttonStart.Enabled = true;
        buttonPause.Enabled = false;
        button_Continue.Enabled = false;

        validPorts = SerialPort.GetPortNames();

        //Game.LoadMap();
        game = new Game();

        // 如果视频流开启，开始进行计时器事件
        if (capture.IsOpened())
        {
            // 设置帧大小
            capture.FrameWidth = flags.cameraSize.Width;
            capture.FrameHeight = flags.cameraSize.Height;
            capture.ConvertRgb = true;

            // 设置定时器的触发间隔为 100ms
            timerMsg100ms.Interval = 100;

            // 启动计时器，执行给迷宫外的小车定时发信息的任务
            timerMsg100ms.Start();
        }

        Debug.WriteLine("Tracker Initialize Finished\n");
    }

    // 进行界面刷新、读取摄像头图像、与游戏逻辑交互的周期性函数
    private void Flush()
    {
        VideoProcess();

        Dot CarPosA = Cvt.Point2Dot(this.logicCarA);
        Dot CarPosB = Cvt.Point2Dot(this.logicCarB);

        // Refresh the game
        if (game.GetCamp() == Camp.A)
        {
            game.UpdateOnEachFrame(CarPosA);
        }
        else if (game.GetCamp() == Camp.B)
        {
            game.UpdateOnEachFrame(CarPosB);
        }

        this.labelAScore.Text = this.game.GetScore(Camp.A, this.game.mGameStage).ToString();
        this.labelBScore.Text = this.game.GetScore(Camp.B, this.game.mGameStage).ToString();
        this.Refresh();
    }

    // 当Tracker被加载时调用此函数
    // 读取data.txt文件中存储的hue,saturation,value等的默认值
    private void Tracker_Load(object sender, EventArgs e)
    {
        if (File.Exists("data.txt"))
        {
            FileStream fsRead = new FileStream("data.txt", FileMode.Open);
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            fsRead.Read(heByte, 0, heByte.Length);
            string myStr = System.Text.Encoding.UTF8.GetString(heByte);
            string[] str = myStr.Split(' ');

            flags.configs.hue1Lower = Convert.ToInt32(str[0]);
            flags.configs.hue1Upper = Convert.ToInt32(str[1]);
            flags.configs.hue2Lower = Convert.ToInt32(str[2]);
            flags.configs.hue2Upper = Convert.ToInt32(str[3]);
            flags.configs.saturation1Lower = Convert.ToInt32(str[4]);
            flags.configs.saturation2Lower = Convert.ToInt32(str[5]);
            flags.configs.valueLower = Convert.ToInt32(str[6]);
            flags.configs.areaLower = Convert.ToInt32(str[7]);

            fsRead.Close();
        }
    }

    /// <summary>
    /// Sends a message to the vehicle.
    /// </summary>
    private void SendMessage()
    {
        byte[] Message = game.Message();

        if (game.GetCamp() == Camp.A)
        {
            if (serial1 != null && serial1.IsOpen)
            {
                serial1.Write(Message, 0, 82);
            }
        }
        else if (game.GetCamp() == Camp.B)
        {
            if (serial2 != null && serial2.IsOpen)
            {
                serial2.Write(Message, 0, 82);
            }
        }

        validPorts = SerialPort.GetPortNames();
    }

    #region 图像处理与界面显示

    // 从视频帧中读取一帧，进行图像处理、绘图和数值更新
    private void VideoProcess()
    {
        if (flags.running)
        {
            // 多个using连在一起写可能是共用最后一个using的作用域（没查到相关资料）
            using (Mat videoFrame = new Mat())
            using (Mat showFrame = new Mat())
            {
                // 从视频流中读取一帧相机画面videoFrame
                if (capture.Read(videoFrame))
                {
                    // 调用坐标转换器，将flags中设置的人员出发点从逻辑坐标转换为显示坐标
                    // coordCvt.PeopleFilter(flags);

                    // 调用定位器，进行图像处理，得到小车位置中心点集
                    // 第一个形参videoFrame传入的是指针，所以videoFrame已被修改（画上了红蓝圆点）
                    localiser.Locate(videoFrame, flags);

                    // 调用定位器，得到小车的坐标
                    localiser.GetCarLocations(out camCarA, out camCarB);

                    // 小车的相机坐标数组
                    Point2f[] camCars = { camCarA, camCarB };

                    // 转换成显示坐标数组
                    // 此转换与只与showMap和camMap有关，不需要图像被校正
                    Point2f[] showCars = coordCvt.CameraToShow(camCars);

                    showCarA = (Point2i)showCars[0];
                    showCarB = (Point2i)showCars[1];

                    // 如果画面已经被手工校正，则可以获取小车的逻辑坐标
                    if (flags.calibrated)
                    {
                        // 将小车坐标从相机坐标转化成逻辑坐标
                        Point2f[] logicCars = coordCvt.CameraToLogic(camCars);

                        logicCarA = (Point2i)logicCars[0];
                        logicCarB = (Point2i)logicCars[1];
                    }
                    else  // 否则将小车的坐标设为（-1，-1）
                    {
                        logicCarA = InvalidPos;
                        logicCarB = InvalidPos;
                    }

                    // 在显示的画面上绘制小车，乘客，物资等对应的图案
                    PaintPattern(videoFrame, localiser);

                    // 将摄像头视频帧缩放成显示帧
                    // Resize函数的最后一个参数是缩放函数的插值算法
                    // InterpolationFlags.Cubic 表示双三次插值法，放大图像时效果较好，但速度较慢
                    Cv2.Resize(videoFrame, showFrame, flags.showSize, 0, 0, InterpolationFlags.Cubic);

                    // 更新界面组件的画面显示
                    BeginInvoke(new Action<Image>(UpdateCameraPicture), BitmapConverter.ToBitmap(showFrame));
                    // 输出视频
                    if (flags.videomode == true)
                    {
                        vw.Write(showFrame);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draw patterns on the picture.
    /// </summary>
    /// <param name="mat">The background picture</param>
    /// <param name="loc">The localiser</param>
    public void PaintPattern(Mat mat, Localiser loc)
    {
        // Draw cross patterns at the corners.
        foreach (Point2f pt in coordCvt.ShowToCamera(showCornerPts))
        {
            Cv2.Line(mat, (int)(pt.X - 10), (int)(pt.Y),
                (int)(pt.X + 10), (int)(pt.Y), new Scalar(0x00, 0xff, 0x98));
            Cv2.Line(mat, (int)(pt.X), (int)(pt.Y - 10),
                (int)(pt.X), (int)(pt.Y + 10), new Scalar(0x00, 0xff, 0x98));
        }

        // Read the icons
        Mat Icon_CarA, Icon_CarB, Icon_Package, Icon_Person, Icon_Zone, Icon_Obstacle;
        Icon_CarA = new Mat(@"Assets\Icons\VehicleRed.png", ImreadModes.Color);
        Icon_CarB = new Mat(@"Assets\Icons\VehicleBlue.png", ImreadModes.Color);
        Icon_Package = new Mat(@"Assets\Icons\Package.png", ImreadModes.Color);
        Icon_Person = new Mat(@"Assets\Icons\Person.png", ImreadModes.Color);
        Icon_Zone = new Mat(@"Assets\Icons\Zone.png", ImreadModes.Color);
        //障碍物的图标 暂定
        Icon_Obstacle = new Mat(@"Assets\Icons\Obstacle.png", ImreadModes.Color);

        Cv2.Resize(src: Icon_CarA, dst: Icon_CarA, dsize: new OpenCvSharp.Size(20, 20));
        Cv2.Resize(src: Icon_CarB, dst: Icon_CarB, dsize: new OpenCvSharp.Size(20, 20));
        Cv2.Resize(src: Icon_Package, dst: Icon_Package, dsize: new OpenCvSharp.Size(22, 22));
        Cv2.Resize(src: Icon_Person, dst: Icon_Person, dsize: new OpenCvSharp.Size(20, 20));
        Cv2.Resize(src: Icon_Zone, dst: Icon_Zone, dsize: new OpenCvSharp.Size(22, 22));
        Cv2.Resize(src: Icon_Obstacle, dst: Icon_Obstacle, dsize: new OpenCvSharp.Size(20, 20));

        // Draw vehicle icons
        foreach (Point2i c1 in loc.GetCentres(Camp.A))
        {
            if (c1.X >= 0 && c1.X <= Game.AVAILABLE_MAX_X && c1.Y >= 0 && c1.Y <= Game.AVAILABLE_MAX_Y)
            {
                // Point2f[] converted_cord = coordCvt.ShowToCamera(new Point2f[] { (Point2f)c1 });
                int Tx = c1.X;
                int Ty = c1.Y;
                // int Tx = (int)converted_cord[0].X - 10;
                // int Ty = (int)converted_cord[0].Y - 10;
                int Tcol = Icon_CarA.Cols;
                int Trow = Icon_CarA.Rows;
                if (Tx < 0)
                {
                    Tx = 0;
                }
                if (Ty < 0)
                {
                    Ty = 0;
                }
                if (Tx + Tcol > mat.Cols)
                {
                    Tx = mat.Cols - Tcol;
                }
                if (Ty + Trow > mat.Rows)
                {
                    Ty = mat.Rows - Trow;
                }

                Mat Pos = new Mat(mat, new Rect(Tx, Ty, Tcol, Trow));
                Icon_CarA.CopyTo(Pos);
                //暂时只画一个车，如果要画多个车，删去break
                break;
            }
        }

        foreach (Point2i c2 in loc.GetCentres(Camp.B))
        {
            if (c2.X >= 0 && c2.X <= Game.AVAILABLE_MAX_X && c2.Y >= 0 && c2.Y <= Game.AVAILABLE_MAX_Y)
            {
                int Tx = c2.X;
                int Ty = c2.Y;
                int Tcol = Icon_CarB.Cols;
                int Trow = Icon_CarB.Rows;
                if (Tx < 0)
                {
                    Tx = 0;
                }
                if (Ty < 0)
                {
                    Ty = 0;
                }
                if (Tx + Tcol > mat.Cols)
                {
                    Tx = mat.Cols - Tcol;
                }
                if (Ty + Trow > mat.Rows)
                {
                    Ty = mat.Rows - Trow;
                }

                Mat Pos = new Mat(mat, new Rect(Tx, Ty, Tcol, Trow));
                Icon_CarB.CopyTo(Pos);
                //暂时只画一个车，如果要画多个车，删去break
                break;
            }
        }

        // Draw charging piles
        List<Dot> stationlistA = Station.StationOnStage(0);
        List<Dot> stationlistB = Station.StationOnStage(1);

        int station_numA = stationlistA.Count;
        int station_numB = stationlistB.Count;

        if (station_numA != 0)
        {
            List<Point2f> logicDots2A = new List<Point2f>();
            foreach (Dot dot in stationlistA)
            {
                logicDots2A.Add(Cvt.Dot2Point(dot));
            }
            List<Point2f> showDots2A = new List<Point2f>(coordCvt.LogicToCamera(logicDots2A.ToArray()));
            // 第一阶段，只绘制本阶段的充电桩
            // 第二阶段，绘制双方的充电桩
            // 这里将A车的绘制成红色，B车绘制成绿色
            if ((game.mGameStage == GameStage.FIRST_HALF && game.GetCamp() == Camp.A)
                || game.mGameStage == GameStage.SECOND_HALF)
            {
                int x = (int)showDots2A[0].X;
                int y = (int)showDots2A[0].Y;
                Cv2.Circle(mat, x, y, 5, new Scalar(0xff, 0x00, 0x00), -1);
                //Debug.WriteLine("Paint the package of campa");
            }
        }
        else if (station_numB != 0)
        {
            List<Point2f> logicDots2B = new List<Point2f>();
            // 第一阶段，只绘制本阶段的充电桩
            // 第二阶段，绘制双方的充电桩
            // 这里将A车的绘制成红色，B车绘制成绿色
            foreach (Dot dot in stationlistB)
            {
                logicDots2B.Add(Cvt.Dot2Point(dot));
            }

            List<Point2f> showDots2B = new List<Point2f>(coordCvt.LogicToCamera(logicDots2B.ToArray()));


            if ((game.mGameStage == GameStage.FIRST_HALF && game.GetCamp() == Camp.B)
                || game.mGameStage == GameStage.SECOND_HALF)
            {
                int x = (int)showDots2B[0].X;
                int y = (int)showDots2B[0].Y;
                Cv2.Circle(mat, x, y, 5, new Scalar(0x00, 0xff, 0x00), -1);
                //Debug.WriteLine("Paint the package of campb");
            }
        }

        // Draw obstacles
        if (Obstacle.IsLabySet == true && game.mGameState == GameState.RUN)
        {
            for (int i = 0; i < Obstacle.mpWallList.Length; i++)
            {
                Dot StartDot = Obstacle.mpWallList[i].w1;
                Dot EndDot = Obstacle.mpWallList[i].w2;

                Point2f[] logicDots = { Cvt.Dot2Point(StartDot), Cvt.Dot2Point(EndDot) };

                if (flags.calibrated)
                {
                    Point2f[] showDots = coordCvt.LogicToCamera(logicDots);
                    // 将Point2f转换为Point2i
                    Point2i[] resultDots = new Point2i[2];
                    resultDots[0] = (Point2i)showDots[0];
                    resultDots[1] = (Point2i)showDots[1];

                    // Cv2.Line(mat, (int)showDots[0].X, (int)showDots[0].Y,
                    //     (int)showDots[1].X, (int)showDots[1].Y,
                    //     new Scalar(35, 35, 139), 5);

                    Cv2.Rectangle(mat, resultDots[0], resultDots[1], color: Scalar.Red, 2);
                    // 画竖直直线 从坐标x=resultDots[0].X+4开始画(随便定的)
                    for (int k = 4; k < resultDots[1].X - resultDots[0].X; k += 5)
                    {
                        Point2i upperPoint = new Point2i(resultDots[0].X + k, resultDots[0].Y);
                        Point2i lowerPoint = new Point2i(resultDots[0].X + k, resultDots[1].Y);
                        Cv2.Line(mat, upperPoint, lowerPoint, color: Scalar.Orange, 1);
                    }
                }
            }
        }

        if (Obstacle.IsLabySet == true && GameState.RUN == game.mGameState)
        {
            // 找到当前的车队
            Car current_car = this.game.GetCar(this.game.GetCamp());
            // 现在车上载有的外卖数量 
            int package_number_on_car = current_car.GetPackageCount();
            foreach (Package package in game.PackagesOnStage())
            {

                PackageStatus current_package_status = package.Status;

                //判断此外卖是否在车上

                int Tx, Ty, Tcol, Trow;
                // 若小车没有接收外卖，显示起点
                Mat target_img = null;
                if (PackageStatus.UNPICKED == current_package_status)
                {
                    target_img = Icon_Package;
                    //修正坐标
                    Point2f[] converted_cord = coordCvt.LogicToCamera(new Point2f[] { (Point2f)MyConvert.Dot2Point(package.mDeparture) });
                    Tx = (int)converted_cord[0].X - 10;
                    Ty = (int)converted_cord[0].Y - 10;

                }
                // 若小车没有装载此外卖，显示终点
                else if (PackageStatus.PICKED == current_package_status)
                {
                    target_img = Icon_Zone;
                    Point2f[] converted_cord = coordCvt.LogicToCamera(new Point2f[] { (Point2f)MyConvert.Dot2Point(package.mDestination) });
                    Tx = (int)converted_cord[0].X - 10;
                    Ty = (int)converted_cord[0].Y - 10;
                }
                else
                {
                    break;
                }
                // 修正位置
                Tcol = target_img.Cols;
                Trow = target_img.Rows;
                if (Tx < 0)
                {
                    Tx = 0;
                }
                if (Ty < 0)
                {
                    Ty = 0;
                }
                // 
                if (Tx + Tcol > mat.Cols)
                {
                    Tcol = mat.Cols - Tx;
                }
                if (Ty + Trow > mat.Rows)
                {
                    Trow = mat.Rows - Ty;
                }
                // 可能会出错 位置生成不正确
                Mat Pos = new Mat(mat, new Rect(Tx, Ty, Tcol, Trow));
                target_img.CopyTo(Pos);

            }
        }
    }

    /// <summary>
    /// Refresh the picture in the monitor.
    /// </summary>
    /// <param name="img">The picture</param>
    private void UpdateCameraPicture(Image img)
    {
        pbCamera.Image = img;
    }

    #endregion


    #region 与界面控件有关的函数

    // 当Tracker界面被关闭时，处理一些接口的关闭
    private void Tracker_FormClosed(object sender, FormClosedEventArgs e)
    {
        lock (flags)
        {
            flags.End();
        }
        timerMsg100ms.Stop();
        //threadCamera.Join();
        capture.Release();
        if (serial1 != null && serial1.IsOpen)
            serial1.Close();
        if (serial2 != null && serial2.IsOpen)
            serial2.Close();
    }

    // 重置画面
    private void btnReset_Click(object sender, EventArgs e)
    {
        lock (flags)
        {
            flags.clickCount = 0;
            flags.calibrated = false;
            for (int i = 0; i < 4; ++i)
                showCornerPts[i].X = showCornerPts[i].Y = 0;
        }
    }

    // 通过鼠标点击屏幕上的地图的4个角以校正画面
    // 当显示画面被点击时触发
    // C#中，X轴从左向右，Y轴从上向下
    // 左上角（0,0）；     右上角（width，0）
    // 左下角（0,height）；右下角（width，height）
    private void pbCamera_MouseClick(object sender, MouseEventArgs e)
    {
        int widthView = pbCamera.Width;
        int heightView = pbCamera.Height;

        int xMouse = e.X;
        int yMouse = e.Y;

        int idx = -1;

        lock (flags)
        {
            if (flags.clickCount < 4)
            {
                flags.clickCount++;
                idx = flags.clickCount - 1;
            }
        }

        // 如果画面已经被点击了4次，则不再重复校正
        if (idx == -1) return;

        if (xMouse >= 0 && xMouse < widthView && yMouse >= 0 && yMouse < heightView)
        {
            showCornerPts[idx].X = xMouse;
            showCornerPts[idx].Y = yMouse;
            if (idx == 3)
            {
                coordCvt.UpdateCorners(showCornerPts, flags);
                MessageBox.Show(
                      $"边界点设置完成\n"
                    + $"0: {showCornerPts[0].X,5}, {showCornerPts[0].Y,5}\t"
                    + $"1: {showCornerPts[1].X,5}, {showCornerPts[1].Y,5}\n"
                    + $"2: {showCornerPts[2].X,5}, {showCornerPts[2].Y,5}\t"
                    + $"3: {showCornerPts[3].X,5}, {showCornerPts[3].Y,5}");
            }
        }
    }


    private void buttonStart_Click(object sender, EventArgs e)
    {
        //状态+1
        this.game.mGameStage += 1;

        if (this.game.mGameStage == GameStage.FIRST_HALF)
        {
            switch (this.game.GetCamp())
            {
                case Camp.NONE:
                    game.Start(Camp.A, GameStage.FIRST_HALF);
                    break;

                case Camp.A:
                    game.Start(Camp.B, GameStage.FIRST_HALF);
                    break;

                case Camp.B:
                    game.Start(Camp.A, GameStage.SECOND_HALF);
                    break;

                default:
                    break;
            }
        }
        else if (this.game.mGameStage == GameStage.SECOND_HALF)
        {
            switch (this.game.GetCamp())
            {
                case Camp.A:
                    game.Start(Camp.B, GameStage.SECOND_HALF);
                    break;

                default:
                    break;
            }
        }
    }

    // Pause
    private void buttonPause_Click(object sender, EventArgs e)
    {
        game.Pause();
    }

    // Continue
    private void buttonContinue_Click(object sender, EventArgs e)
    {
        game.Continue();
    }

    // End
    private void buttonEnd_Click(object sender, EventArgs e)
    {
        game.End();
    }

    private void buttonRestart_click(object sender, EventArgs e)
    {
        game = new Game();
    }

    // Get foul mark
    private void buttonFoul_Click(object sender, EventArgs e)
    {
        game.GetMark();
    }

    private void buttonSetStation_Click(object sender, EventArgs e)
    {
        game.SetChargeStation();
    }

    // 打开设置调试窗口
    private void button_set_Click(object sender, EventArgs e)
    {
        lock (flags)
        {
            SetWindow st = new SetWindow(ref flags, ref game, this);
            st.Show();
        }
    }
    #endregion

    #region 由定时器控制的函数
    //计时器事件，每100ms触发一次，向小车发送信息
    private void timerMsg100ms_Tick(object sender, EventArgs e)
    {
        Flush();
        // 如果A车在场地内且在迷宫外  ???这是上上一届的规则吧 --张琰然

        // 如果游戏开始了才发信息
        if (GameState.RUN == game.mGameState)
            SendMessage();
    }

    #endregion
}