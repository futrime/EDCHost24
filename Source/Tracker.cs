using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point2i = OpenCvSharp.Point;

namespace EdcHost;
public partial class Tracker : Form
{
    public MyFlags _flags = null;
    public VideoCapture _camera = null;

    // 设定的显示画面四角坐标
    private Point2f[] _monitorCorners = null;

    // 坐标转换器
    public CoordinateConverter _coordinateConverter;
    // 定位器
    private Localiser _vehicleLocalizer;

    // 以下坐标均为相机坐标
    // 车A坐标
    private Point2i _carAPositionInCameraCoordinate;
    // 车B坐标
    private Point2i _carBPositionInCameraCoordinate;
    // 物资坐标
    private Point2i[] camPkgs;

    // 以下坐标均为逻辑坐标
    private Point2i _carAPosition;
    private Point2i _carBPosition;

    // 以下均为显示坐标
    private Point2i _carAPositionInMonitorCoordinate;
    private Point2i _carBPositionInMonitorCoordinate;

    // 游戏逻辑
    private Game _game;
    // 视频输出流
    private VideoWriter _videoWriter = null;
    // 与小车进行蓝牙通讯的端口
    public SerialPort _serialPortCarA, _serialPortCarB;
    // 可用的端口名称
    public string[] _availableSerialPortList;


    public Tracker()
    {
        InitializeComponent();

        // Setup Windows Forms controls
        label_RedBG.SendToBack();
        label_BlueBG.SendToBack();
        label_GameCount.Text = "上半场";

        //flags参数类
        _flags = new MyFlags();
        _flags.Start();

        // Setup the monitor
        _camera = new VideoCapture();
        _camera.Open(0);

        // 相机画面大小设为视频帧大小
        _flags.cameraSize.Width = _camera.FrameWidth;
        _flags.cameraSize.Height = _camera.FrameHeight;

        // 显示大小设为界面组件大小
        _flags.showSize.Width = pbCamera.Width;
        _flags.showSize.Height = pbCamera.Height;

        // 用于存储鼠标点击的画面中场地的4个角的坐标
        _monitorCorners = new Point2f[4];

        // 以既有的flags参数初始化坐标转换器
        _coordinateConverter = new CoordinateConverter(_flags);

        // 定位小车位置的类
        _vehicleLocalizer = new Localiser();

        // 相机坐标初始化
        _carAPositionInCameraCoordinate = new Point2i();
        _carBPositionInCameraCoordinate = new Point2i();
        camPkgs = new Point2i[0];

        // 逻辑坐标初始化
        _carAPosition = new Point2i();
        _carBPosition = new Point2i();

        // 显示坐标初始化
        _carAPositionInMonitorCoordinate = new Point2i();
        _carBPositionInMonitorCoordinate = new Point2i();

        buttonStart.Enabled = true;
        buttonPause.Enabled = false;
        button_Continue.Enabled = false;

        _availableSerialPortList = SerialPort.GetPortNames();

        //Game.LoadMap();
        _game = new Game();

        // 如果视频流开启，开始进行计时器事件
        if (_camera.IsOpened())
        {
            // 设置帧大小
            _camera.FrameWidth = _flags.cameraSize.Width;
            _camera.FrameHeight = _flags.cameraSize.Height;
            _camera.ConvertRgb = true;

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
        this.VideoProcess();

        var carAPosition = new Dot(this._carAPosition);
        var carBPosition = new Dot(this._carBPosition);

        // Refresh the game
        if (this._game.GetCamp() == Camp.A)
        {
            this._game.UpdateOnEachFrame(carAPosition);
        }
        else if (this._game.GetCamp() == Camp.B)
        {
            this._game.UpdateOnEachFrame(carBPosition);
        }

        // Refresh the controls in the tracker form
        this.labelAScore.Text = this._game.GetScore(Camp.A, this._game.mGameStage).ToString();
        this.labelBScore.Text = this._game.GetScore(Camp.B, this._game.mGameStage).ToString();

        decimal currentTime = (decimal)this._game.RemainingTime / 1000;
        this.GameTimeLabel.Text = (currentTime < 0 ? 0 : currentTime).ToString("0.00");

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

            _flags.configs.hue1Lower = Convert.ToInt32(str[0]);
            _flags.configs.hue1Upper = Convert.ToInt32(str[1]);
            _flags.configs.hue2Lower = Convert.ToInt32(str[2]);
            _flags.configs.hue2Upper = Convert.ToInt32(str[3]);
            _flags.configs.saturation1Lower = Convert.ToInt32(str[4]);
            _flags.configs.saturation2Lower = Convert.ToInt32(str[5]);
            _flags.configs.valueLower = Convert.ToInt32(str[6]);
            _flags.configs.areaLower = Convert.ToInt32(str[7]);

            fsRead.Close();
        }
    }

    /// <summary>
    /// Sends a message to the vehicle.
    /// </summary>
    private void SendMessage()
    {
        byte[] Message = new byte[] { };

        if (_game.GetCamp() == Camp.A)
        {
            if (_serialPortCarA != null && _serialPortCarA.IsOpen)
            {
                _serialPortCarA.Write(Message, 0, 82);
            }
        }
        else if (_game.GetCamp() == Camp.B)
        {
            if (_serialPortCarB != null && _serialPortCarB.IsOpen)
            {
                _serialPortCarB.Write(Message, 0, 82);
            }
        }

        _availableSerialPortList = SerialPort.GetPortNames();
    }

    #region 图像处理与界面显示

    // 从视频帧中读取一帧，进行图像处理、绘图和数值更新
    private void VideoProcess()
    {
        if (_flags.running)
        {
            // 多个using连在一起写可能是共用最后一个using的作用域（没查到相关资料）
            Mat videoFrame = new Mat();
            Mat showFrame = new Mat();
            // 从视频流中读取一帧相机画面videoFrame
            if (_camera.Read(videoFrame))
            {
                // 调用坐标转换器，将flags中设置的人员出发点从逻辑坐标转换为显示坐标
                // coordCvt.PeopleFilter(flags);

                // 调用定位器，进行图像处理，得到小车位置中心点集
                // 第一个形参videoFrame传入的是指针，所以videoFrame已被修改（画上了红蓝圆点）
                _vehicleLocalizer.Locate(videoFrame, _flags);

                // 调用定位器，得到小车的坐标
                _vehicleLocalizer.GetCarLocations(out _carAPositionInCameraCoordinate, out _carBPositionInCameraCoordinate);

                // 小车的相机坐标数组
                Point2f[] camCars = { _carAPositionInCameraCoordinate, _carBPositionInCameraCoordinate };

                // 转换成显示坐标数组
                // 此转换与只与showMap和camMap有关，不需要图像被校正
                Point2f[] showCars = _coordinateConverter.CameraToMonitor(camCars);

                _carAPositionInMonitorCoordinate = (Point2i)showCars[0];
                _carBPositionInMonitorCoordinate = (Point2i)showCars[1];

                // 将小车坐标从相机坐标转化成逻辑坐标
                Point2f[] logicCars = _coordinateConverter.CameraToCourt(camCars);

                _carAPosition = (Point2i)logicCars[0];
                _carBPosition = (Point2i)logicCars[1];

                // 在显示的画面上绘制小车，乘客，物资等对应的图案
                videoFrame = PaintPattern(videoFrame, _vehicleLocalizer);

                // 将摄像头视频帧缩放成显示帧
                // Resize函数的最后一个参数是缩放函数的插值算法
                // InterpolationFlags.Cubic 表示双三次插值法，放大图像时效果较好，但速度较慢
                Cv2.Resize(videoFrame, showFrame, _flags.showSize, 0, 0, InterpolationFlags.Cubic);

                // 更新界面组件的画面显示
                BeginInvoke(new Action<Image>(UpdateCameraPicture), BitmapConverter.ToBitmap(showFrame));
                // 输出视频
                if (_flags.videomode == true)
                {
                    _videoWriter.Write(showFrame);
                }
            }
        }
    }

    /// <summary>
    /// Draw patterns on the monitor frame.
    /// </summary>
    /// <param name="mat">The background picture</param>
    /// <param name="loc">The localiser</param>
    public Mat PaintPattern(Mat mat, Localiser loc)
    {
        // Draw cross patterns at the corners.
        foreach (Point2f pt in this._coordinateConverter.MonitorToCamera(this._monitorCorners))
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

        const int IconSize = 25;
        Cv2.Resize(src: Icon_CarA, dst: Icon_CarA, dsize: new OpenCvSharp.Size(IconSize, IconSize));
        Cv2.Resize(src: Icon_CarB, dst: Icon_CarB, dsize: new OpenCvSharp.Size(IconSize, IconSize));
        Cv2.Resize(src: Icon_Package, dst: Icon_Package, dsize: new OpenCvSharp.Size(IconSize, IconSize));
        Cv2.Resize(src: Icon_Person, dst: Icon_Person, dsize: new OpenCvSharp.Size(IconSize, IconSize));
        Cv2.Resize(src: Icon_Zone, dst: Icon_Zone, dsize: new OpenCvSharp.Size(IconSize, IconSize));
        Cv2.Resize(src: Icon_Obstacle, dst: Icon_Obstacle, dsize: new OpenCvSharp.Size(IconSize, IconSize));

        // Draw vehicle icons
        if (_game.GetCamp() == Camp.A)
        {
            foreach (Point2i c1 in this._coordinateConverter.CourtToCamera(Array.ConvertAll(loc.GetCentres(Camp.A).ToArray(), item => (Point2f)item)))
            {
                //优化一下，避免画面显示的车 位移太大了，只找到和上一次距离较近的车
                //待优化
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
        }
        else if (_game.GetCamp() == Camp.B)
        {
            foreach (Point2i c2 in this._coordinateConverter.CourtToCamera(Array.ConvertAll(loc.GetCentres(Camp.B).ToArray(), item => (Point2f)item)))
            {
                _game.GetCar(Camp.B).LastPos();
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
                logicDots2A.Add(dot.ToPoint());
            }
            List<Point2f> showDots2A = new List<Point2f>(_coordinateConverter.CourtToCamera(logicDots2A.ToArray()));
            // 第一阶段，只绘制本阶段的充电桩
            // 第二阶段，绘制双方的充电桩
            // 这里将A车的绘制成红色，B车绘制成绿色
            if ((_game.mGameStage == GameStage.FIRST_HALF && _game.GetCamp() == Camp.A)
                || _game.mGameStage == GameStage.SECOND_HALF)
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
                logicDots2B.Add(dot.ToPoint());
            }

            List<Point2f> showDots2B = new List<Point2f>(_coordinateConverter.CourtToCamera(logicDots2B.ToArray()));


            if ((_game.mGameStage == GameStage.FIRST_HALF && _game.GetCamp() == Camp.B)
                || _game.mGameStage == GameStage.SECOND_HALF)
            {
                int x = (int)showDots2B[0].X;
                int y = (int)showDots2B[0].Y;
                Cv2.Circle(mat, x, y, 5, new Scalar(0x00, 0xff, 0x00), -1);
                //Debug.WriteLine("Paint the package of campb");
            }
        }

        // Draw Barriers
        if (_game.mGameState == GameState.RUN)
        {
            for (int i = 0; i < _game.BarrierList.Count; i++)
            {
                Dot StartDot = _game.BarrierList[i].TopLeftPosition;
                Dot EndDot = _game.BarrierList[i].BottomRightPosition;

                Point2f[] pointsInCourtCoordination = { StartDot.ToPoint(), EndDot.ToPoint() };

                Point2i[] pointsInCameraCoordination = Array.ConvertAll(
                    _coordinateConverter.CourtToCamera(pointsInCourtCoordination), item => (Point2i)item
                );

                Cv2.Rectangle(mat, pointsInCameraCoordination[0], pointsInCameraCoordination[1], color: Scalar.Red, 2);
                for (int k = 4; k < pointsInCameraCoordination[1].X - pointsInCameraCoordination[0].X; k += 5)
                {
                    Point2i upperPoint = new Point2i(pointsInCameraCoordination[0].X + k, pointsInCameraCoordination[0].Y);
                    Point2i lowerPoint = new Point2i(pointsInCameraCoordination[0].X + k, pointsInCameraCoordination[1].Y);
                    Cv2.Line(mat, upperPoint, lowerPoint, color: Scalar.Orange, 1);
                }
            }
        }

        if (GameState.RUN == _game.mGameState)
        {
            // 找到当前的车队
            Car current_car = this._game.GetCar(this._game.GetCamp());
            // 现在车上载有的外卖数量 
            int order_number_on_car = current_car.GetOrderCount();
            foreach (Order ord in _game.AllOrders)
            {
                Order.StatusType currentOrderStatus = ord.Status;
                //判断此外卖是否在车上

                int Tx, Ty, Tcol, Trow;
                // 若小车没有接收外卖，显示起点
                Mat target_img = null;
                if (Order.StatusType.Pending == currentOrderStatus)
                {
                    target_img = Icon_Package;
                    //修正坐标
                    Point2f[] converted_cord = _coordinateConverter.CourtToCamera(new Point2f[] { (Point2f)ord.DeparturePosition.ToPoint() });
                    Tx = (int)converted_cord[0].X - 10;
                    Ty = (int)converted_cord[0].Y - 10;

                }
                // 若小车装载此外卖，显示终点
                else if (Order.StatusType.InDelivery == currentOrderStatus)
                {
                    target_img = Icon_Zone;
                    Point2f[] converted_cord = _coordinateConverter.CourtToCamera(new Point2f[] { (Point2f)ord.DestinationPosition.ToPoint() });
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

        return mat;
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

    /// <summary>
    /// OnFormClose
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnFormClosed(object sender, FormClosedEventArgs e)
    {
        lock (_flags)
        {
            _flags.End();
        }
        timerMsg100ms.Stop();
        //threadCamera.Join();
        _camera.Release();
        if (_serialPortCarA != null && _serialPortCarA.IsOpen)
            _serialPortCarA.Close();
        if (_serialPortCarB != null && _serialPortCarB.IsOpen)
            _serialPortCarB.Close();
    }

    private void OnCalibrateButtonClick(object sender, EventArgs e)
    {
        lock (_flags)
        {
            _flags.clickCount = 0;
            for (int i = 0; i < 4; ++i)
                _monitorCorners[i].X = _monitorCorners[i].Y = 0;
        }
    }

    // 通过鼠标点击屏幕上的地图的4个角以校正画面
    // 当显示画面被点击时触发
    // C#中，X轴从左向右，Y轴从上向下
    // 左上角（0,0）；     右上角（width，0）
    // 左下角（0,height）；右下角（width，height）
    private void OnMonitorMouseClick(object sender, MouseEventArgs e)
    {
        int widthView = pbCamera.Width;
        int heightView = pbCamera.Height;

        int xMouse = e.X;
        int yMouse = e.Y;

        int idx = -1;

        lock (_flags)
        {
            if (_flags.clickCount < 4)
            {
                _flags.clickCount++;
                idx = _flags.clickCount - 1;
            }
        }

        // 如果画面已经被点击了4次，则不再重复校正
        if (idx == -1) return;

        if (xMouse >= 0 && xMouse < widthView && yMouse >= 0 && yMouse < heightView)
        {
            _monitorCorners[idx].X = xMouse;
            _monitorCorners[idx].Y = yMouse;
            if (idx == 3)
            {
                _coordinateConverter.Calibrate(_monitorCorners);
                MessageBox.Show(
                      $"边界点设置完成\n"
                    + $"0: {_monitorCorners[0].X,5}, {_monitorCorners[0].Y,5}\t"
                    + $"1: {_monitorCorners[1].X,5}, {_monitorCorners[1].Y,5}\n"
                    + $"2: {_monitorCorners[2].X,5}, {_monitorCorners[2].Y,5}\t"
                    + $"3: {_monitorCorners[3].X,5}, {_monitorCorners[3].Y,5}");
            }
        }
    }


    private void OnStartButtonClick(object sender, EventArgs e)
    {
        if (this._game.mGameStage == GameStage.NONE &&
            this._game.GetCamp() == Camp.NONE)
        {
            _game.Start(Camp.A, GameStage.FIRST_HALF);
            label_GameCount.Text = "上半场";
        }
        else if (this._game.mGameStage == GameStage.FIRST_HALF &&
            this._game.GetCamp() == Camp.A)
        {
            _game.Start(Camp.B, GameStage.FIRST_HALF);
            label_GameCount.Text = "上半场";
        }
        else if (this._game.mGameStage == GameStage.FIRST_HALF &&
            this._game.GetCamp() == Camp.B)
        {
            _game.Start(Camp.A, GameStage.SECOND_HALF);
            label_GameCount.Text = "下半场";
        }
        else if (this._game.mGameStage == GameStage.SECOND_HALF &&
            this._game.GetCamp() == Camp.A)
        {
            _game.Start(Camp.B, GameStage.SECOND_HALF);
            label_GameCount.Text = "下半场";
        }
        else
        {
            throw new Exception("The game stage or the camp is invalid.");
        }
    }

    // Pause
    private void OnPauseButtonClick(object sender, EventArgs e)
    {
        _game.Pause();
    }

    // Continue
    private void OnContinueButtonClick(object sender, EventArgs e)
    {
        _game.Continue();
    }

    // End
    private void OnEndButtonClick(object sender, EventArgs e)
    {
        _game.End();
    }

    private void OnRestartButtonClick(object sender, EventArgs e)
    {
        _game = new Game();
    }

    // Get foul mark
    private void OnFoulButtonClick(object sender, EventArgs e)
    {
        _game.GetMark();
    }

    // 打开设置调试窗口
    private void OnSettingsButtonClick(object sender, EventArgs e)
    {
        lock (_flags)
        {
            SetWindow st = new SetWindow(ref _flags, ref _game, this);
            st.Show();
        }
    }
    #endregion

    #region 由定时器控制的函数

    private void OnTimerTick(object sender, EventArgs e)
    {
        this.Flush();

        if (GameState.RUN == _game.mGameState)
        {
            this.SendMessage();
        }
    }

    #endregion
}