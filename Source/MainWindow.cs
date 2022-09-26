using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point2i = OpenCvSharp.Point;

namespace EdcHost;

/// <summary>
/// A main window
/// </summary>
public partial class MainWindow : Form
{
    /// <summary>
    /// The size of icons shown on the monitor
    /// </summary>
    private const int IconSize = 25;


    /// <summary>
    /// A list of available serial ports
    /// </summary>
    public string[] AvailableSerialPortList => this._availableSerialPortList;
    public VideoCapture Camera => this._camera;
    /// <summary>
    /// The coordinate converter
    /// </summary>
    public CoordinateConverter CoordinateConverter
    {
        get => this._coordinateConverter;
        set => this._coordinateConverter = value;
    }
    /// <summary>
    /// The configurations
    /// </summary>
    public ConfigTypeLegacy Flags => this._flags;
    /// <summary>
    /// The serial port of the vehicle of camp A
    /// </summary>
    public SerialPort SerialPortVehicleA
    {
        get => this._serialPortVehicleA;
        set => this._serialPortVehicleA = value;
    }
    /// <summary>
    /// The serial port of the vehicle of camp B
    /// </summary>
    public SerialPort SerialPortVehicleB
    {
        get => this._serialPortVehicleB;
        set => this._serialPortVehicleB = value;
    }

    private string[] _availableSerialPortList;
    private VideoCapture _camera = new VideoCapture();
    private CoordinateConverter _coordinateConverter;
    private ConfigTypeLegacy _flags = new ConfigTypeLegacy();
    private Game _game = new Game();
    private Point2f[] _monitorCorners = new Point2f[4];
    private SerialPort _serialPortVehicleA = null;
    private SerialPort _serialPortVehicleB = null;
    private LocaliserLegacy _vehicleLocalizer = new LocaliserLegacy();



    public MainWindow()
    {
        InitializeComponent();

        // Setup Windows Forms controls
        ScoreABackgroundLabel.SendToBack();
        ScoreBBackgroundLabel.SendToBack();
        GameRoundLabel.Text = "上半场";

        // Setup the monitor
        Camera.Open(0);

        // 相机画面大小设为视频帧大小
        Flags.CameraFrameSize.Width = Camera.FrameWidth;
        Flags.CameraFrameSize.Height = Camera.FrameHeight;

        // 显示大小设为界面组件大小
        Flags.MonitorFrameSize.Width = MonitorPictureBox.Width;
        Flags.MonitorFrameSize.Height = MonitorPictureBox.Height;

        // 以既有的flags参数初始化坐标转换器
        CoordinateConverter = new CoordinateConverter(Flags);

        this._availableSerialPortList = SerialPort.GetPortNames();

        Camera.FrameWidth = Flags.CameraFrameSize.Width;
        Camera.FrameHeight = Flags.CameraFrameSize.Height;
        Camera.ConvertRgb = true;

        // 设置定时器的触发间隔为 100ms
        timerMsg100ms.Interval = 100;

        // 启动计时器，执行给迷宫外的小车定时发信息的任务
        timerMsg100ms.Start();
    }

    /// <summary>
    /// Refresh everything.
    /// </summary>
    private void RefreshAll()
    {
        this.ProcessCameraFrame();

        if (this._game.GameState == GameStateType.Unstarted)
        {
            this.StartButton.Enabled = true;
            this.PauseButton.Enabled = false;
            this.ContinueButton.Enabled = false;
            this.EndButton.Enabled = false;
        }
        else if (this._game.GameState == GameStateType.Running)
        {
            var vehiclePositionList = (this._vehicleLocalizer.GetCentres(this._game.GetCamp()));
            if (vehiclePositionList.Count > 0)
            {
                // Convert Camera Coordinate to Court Coordinate
                this._game.Refresh(new Dot((Point2i)CoordinateConverter.CameraToCourt((Point2f)vehiclePositionList[0])));
            }

            this.StartButton.Enabled = false;
            this.PauseButton.Enabled = true;
            this.ContinueButton.Enabled = false;
            this.EndButton.Enabled = true;

            this.ScoreALabel.Text = this._game.GetScore(CampType.A, this._game.GameStage).ToString();
            this.ScoreBLabel.Text = this._game.GetScore(CampType.B, this._game.GameStage).ToString();
            this.GameTimeLabel.Text = Math.Max((decimal)(this._game.RemainingTime) / 1000, (decimal)0).ToString("0.00");
        }
        else if (this._game.GameState == GameStateType.Paused)
        {
            this.StartButton.Enabled = false;
            this.PauseButton.Enabled = false;
            this.ContinueButton.Enabled = true;
            this.EndButton.Enabled = true;
        }
        else if (this._game.GameState == GameStateType.Ended)
        {
            this.StartButton.Enabled = true;
            this.PauseButton.Enabled = false;
            this.ContinueButton.Enabled = false;
            this.EndButton.Enabled = false;
        }

        this.Refresh();
    }

    /// <summary>
    /// Communicate with the slaves
    /// </summary>
    private void Communicate()
    {
        // To be implemented
    }

    #region Methods related to the camera and the monitor

    // 从视频帧中读取一帧，进行图像处理、绘图和数值更新
    private void ProcessCameraFrame()
    {
        Mat cameraFrame = new Mat();
        Mat monitorFrame = new Mat();
        // 从视频流中读取一帧相机画面videoFrame
        if (!Camera.Read(cameraFrame))
        {
            return;
        }

        this._vehicleLocalizer.Locate(cameraFrame, Flags);

        cameraFrame = this.Draw(cameraFrame, _vehicleLocalizer);

        Cv2.Resize(cameraFrame, monitorFrame, this.Flags.MonitorFrameSize);

        this.BeginInvoke(new Action<Image>(RefreshMonitor), BitmapConverter.ToBitmap(monitorFrame));
    }

    /// <summary>
    /// Draw patterns on the monitor frame.
    /// </summary>
    /// <param name="image">The background picture</param>
    /// <param name="localizer">The localiser</param>
    private Mat Draw(Mat image, LocaliserLegacy localizer)
    {
        // Read icons
        var iconCarA = new Mat(@"Assets/Icons/VehicleRed.png", ImreadModes.Color);
        var iconCarB = new Mat(@"Assets/Icons/VehicleBlue.png", ImreadModes.Color);
        var iconChargingPileRed = new Mat(@"Assets/Icons/ChargingPileRed.png", ImreadModes.Color);
        var iconChargingPileBlue = new Mat(@"Assets/Icons/ChargingPileBlue.png", ImreadModes.Color);
        var iconOrderDeparture = new Mat(@"Assets/Icons/OrderDeparture.png", ImreadModes.Color);
        var iconOrderDestination = new Mat(@"Assets/Icons/OrderDestination.png", ImreadModes.Color);

        Cv2.Resize(src: iconCarA, dst: iconCarA, dsize: new OpenCvSharp.Size(IconSize, IconSize));
        Cv2.Resize(src: iconCarB, dst: iconCarB, dsize: new OpenCvSharp.Size(IconSize, IconSize));
        Cv2.Resize(
            src: iconChargingPileRed, dst: iconChargingPileRed,
            dsize: new OpenCvSharp.Size(IconSize, IconSize)
        );
        Cv2.Resize(
            src: iconChargingPileBlue, dst: iconChargingPileBlue,
            dsize: new OpenCvSharp.Size(IconSize, IconSize)
        );
        Cv2.Resize(
            src: iconOrderDeparture, dst: iconOrderDeparture,
            dsize: new OpenCvSharp.Size(IconSize, IconSize)
        );
        Cv2.Resize(
            src: iconOrderDestination, dst: iconOrderDestination,
            dsize: new OpenCvSharp.Size(IconSize, IconSize)
        );

        // Draw crosses at the corners.
        foreach (Point2f pt in this.CoordinateConverter.MonitorToCamera(this._monitorCorners))
        {
            Cv2.Line(
                image,
                (int)(pt.X - 20), (int)(pt.Y),
                (int)(pt.X + 20), (int)(pt.Y),
                color: new Scalar(0x00, 0xff, 0x00),
                thickness: 3
            );
            Cv2.Line(
                image,
                (int)(pt.X), (int)(pt.Y - 20),
                (int)(pt.X), (int)(pt.Y + 20),
                color: new Scalar(0x00, 0xff, 0x00),
                thickness: 3
            );
        }

        // Draw Barriers and Walls
        if (this._game.GameState == GameStateType.Running || this._game.GameState == GameStateType.Paused)
        {
            // define a public function
            void DrawBarrier(Barrier barrier, Scalar barrierColor, int edgeThickness)
            {
                Dot topLeftPosition = barrier.TopLeftPosition;
                Dot bottomRightPosition = barrier.BottomRightPosition;

                Point2f[] pointsInCourtCoordination = { topLeftPosition.ToPoint(), bottomRightPosition.ToPoint() };

                Point2i[] pointsInCameraCoordination = Array.ConvertAll(
                    CoordinateConverter.CourtToCamera(pointsInCourtCoordination), item => (Point2i)item
                );

                Cv2.Rectangle(image, pointsInCameraCoordination[0], pointsInCameraCoordination[1], color: barrierColor, thickness: edgeThickness);

                for (int i = pointsInCameraCoordination[0].X; i < pointsInCameraCoordination[1].X; i += 2)
                {
                    Point2i upperPoint = new Point2i(i, pointsInCameraCoordination[0].Y);
                    Point2i lowerPoint = new Point2i(i, pointsInCameraCoordination[1].Y);
                    Cv2.Line(image, upperPoint, lowerPoint, color: barrierColor, 1);
                }
            }
            // Draw Barriers 
            foreach (var barrier in this._game.BarrierList)
            {
                DrawBarrier(barrier, Scalar.Yellow, 2);
            }
            // Draw Walls
            foreach (var wall in this._game.WallList)
            {
                DrawBarrier(wall, Scalar.Black, 1);
            }
        }

        // Draw charging piles
        foreach (var chargingPile in this._game.ChargingPileList)
        {
            var position = new Dot((Point2i)this.CoordinateConverter.CourtToCamera(chargingPile.Position.ToPoint()));

            Mat icon = new Mat();
            switch (chargingPile.Camp)
            {
                case CampType.A:
                    icon = iconChargingPileRed;
                    break;

                case CampType.B:
                    icon = iconChargingPileBlue;
                    break;

                default:
                    break;
            }

            int x = Math.Min(Math.Max(position.x - 10, 0), image.Cols - iconChargingPileRed.Cols);
            int y = Math.Min(Math.Max(position.y - 10, 0), image.Cols - iconChargingPileRed.Cols);

            icon.CopyTo(new Mat(image, new Rect(x, y, icon.Cols, icon.Rows)));
        }

        // Draw departures and destinations of orders
        if (this._game.GameState == GameStateType.Running || this._game.GameState == GameStateType.Paused)
        {
            // 找到当前的车队
            Vehicle current_car = this._game.Vehicle[CampType.A];
            // 现在车上载有的外卖数量 
            // int order_number_on_car = current_car.DeliveringOrderList.Count;
            foreach (Order ord in _game.AllOrderList)
            {
                Order.StatusType currentOrderStatus = ord.Status;
                //判断此外卖是否在车上
                int Tx, Ty, Tcol, Trow;
                // 若小车没有接收外卖，显示起点
                Mat target_img = null;
                if (Order.StatusType.Pending == currentOrderStatus)
                {
                    target_img = iconOrderDeparture;
                    //修正坐标
                    Point2f[] converted_cord = CoordinateConverter.CourtToCamera(new Point2f[] { (Point2f)ord.DeparturePosition.ToPoint() });
                    Tx = (int)converted_cord[0].X - 10;
                    Ty = (int)converted_cord[0].Y - 10;

                }
                // 若小车装载此外卖，显示终点
                else if (Order.StatusType.InDelivery == currentOrderStatus)
                {
                    target_img = iconOrderDestination;
                    Point2f[] converted_cord = CoordinateConverter.CourtToCamera(new Point2f[] { (Point2f)ord.DestinationPosition.ToPoint() });
                    Tx = (int)converted_cord[0].X - 10;
                    Ty = (int)converted_cord[0].Y - 10;
                }
                else
                {
                    continue;
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
                if (Tx + Tcol > image.Cols)
                {
                    Tcol = image.Cols - Tx;
                }
                if (Ty + Trow > image.Rows)
                {
                    Trow = image.Rows - Ty;
                }
                // 可能会出错 位置生成不正确
                Mat Pos = new Mat(image, new Rect(Tx, Ty, Tcol, Trow));
                target_img.CopyTo(Pos);

            }
        }

        // Draw vehicles
        foreach (Point2i c1 in localizer.GetCentres(CampType.A).ToArray())
        {
            var Tx = c1.X;
            Tx = Math.Max(Tx, 0);
            Tx = Math.Min(Tx, image.Cols - iconCarB.Cols);

            var Ty = c1.Y;
            Ty = Math.Max(Ty, 0);
            Ty = Math.Min(Ty, image.Rows - iconCarB.Rows);

            Mat Pos = new Mat(image, new Rect(Tx, Ty, iconCarA.Cols, iconCarA.Rows));
            iconCarA.CopyTo(Pos);
        }
        foreach (Point2i c2 in localizer.GetCentres(CampType.B).ToArray())
        {
            var Tx = c2.X;
            Tx = Math.Max(Tx, 0);
            Tx = Math.Min(Tx, image.Cols - iconCarB.Cols);

            var Ty = c2.Y;
            Ty = Math.Max(Ty, 0);
            Ty = Math.Min(Ty, image.Rows - iconCarB.Rows);

            Mat Pos = new Mat(image, new Rect(Tx, Ty, iconCarB.Cols, iconCarB.Rows));
            iconCarB.CopyTo(Pos);
        }

        return image;
    }

    /// <summary>
    /// Refresh the monitor.
    /// </summary>
    /// <param name="img">
    /// The image to show in the monitor
    /// </param>
    private void RefreshMonitor(Image img)
    {
        MonitorPictureBox.Image = img;
    }

    #endregion


    #region Methods related to the Windows Form

    private void OnLoad(object sender, EventArgs e)
    {
        if (File.Exists(@"Data/data.txt"))
        {
            FileStream fsRead = new FileStream(@"Data/data.txt", FileMode.Open);
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            fsRead.Read(heByte, 0, heByte.Length);
            string myStr = System.Text.Encoding.UTF8.GetString(heByte);
            string[] str = myStr.Split(' ');

            Flags.LocatorConfig.MinHueVehicleA = Convert.ToInt32(str[0]);
            Flags.LocatorConfig.MaxHueVehicleA = Convert.ToInt32(str[1]);
            Flags.LocatorConfig.MinHueVehicleB = Convert.ToInt32(str[2]);
            Flags.LocatorConfig.MaxHueVehicleB = Convert.ToInt32(str[3]);
            Flags.LocatorConfig.MinSaturationVehicleA = Convert.ToInt32(str[4]);
            Flags.LocatorConfig.MinSaturationVehicleB = Convert.ToInt32(str[5]);
            Flags.LocatorConfig.valueLower = Convert.ToInt32(str[6]);
            Flags.LocatorConfig.MinArea = Convert.ToInt32(str[7]);

            fsRead.Close();
        }
    }

    /// <summary>
    /// OnFormClose
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnFormClosed(object sender, FormClosedEventArgs e)
    {
        timerMsg100ms.Stop();
        //threadCamera.Join();
        Camera.Release();
        if (SerialPortVehicleA != null && SerialPortVehicleA.IsOpen)
            SerialPortVehicleA.Close();
        if (SerialPortVehicleB != null && SerialPortVehicleB.IsOpen)
            SerialPortVehicleB.Close();
    }

    private void OnCalibrateButtonClick(object sender, EventArgs e)
    {
        lock (Flags)
        {
            Flags.CalibrationClickCount = 0;
            for (int i = 0; i < 4; ++i)
                _monitorCorners[i].X = _monitorCorners[i].Y = 0;
        }
    }

    /// <summary>
    /// Click the four corners to calibrate the capture.
    /// </summary>
    /// <remarks>
    /// Click on the top left corner, top right corner, 
    /// </remarks>
    private void OnMonitorMouseClick(object sender, MouseEventArgs e)
    {
        int widthView = MonitorPictureBox.Width;
        int heightView = MonitorPictureBox.Height;

        int xMouse = e.X;
        int yMouse = e.Y;

        int idx = -1;

        lock (Flags)
        {
            if (Flags.CalibrationClickCount < 4)
            {
                Flags.CalibrationClickCount++;
                idx = Flags.CalibrationClickCount - 1;
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
                CoordinateConverter.Calibrate(_monitorCorners);
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
        if (this._game.GameStage == GameStageType.None &&
            this._game.GetCamp() == CampType.None)
        {
            _game.Start(CampType.A, GameStageType.FirstHalf);
            GameRoundLabel.Text = "上半场";
        }
        else if (this._game.GameStage == GameStageType.FirstHalf &&
            this._game.GetCamp() == CampType.A)
        {
            _game.Start(CampType.B, GameStageType.FirstHalf);
            GameRoundLabel.Text = "上半场";
        }
        else if (this._game.GameStage == GameStageType.FirstHalf &&
            this._game.GetCamp() == CampType.B)
        {
            _game.Start(CampType.A, GameStageType.SecondHalf);
            GameRoundLabel.Text = "下半场";
        }
        else if (this._game.GameStage == GameStageType.SecondHalf &&
            this._game.GetCamp() == CampType.A)
        {
            _game.Start(CampType.B, GameStageType.SecondHalf);
            GameRoundLabel.Text = "下半场";
        }
        else
        {
            return;
        }
    }

    // Pause
    private void OnPauseButtonClick(object sender, EventArgs e)
    {
        this._game.Pause();
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

    private void OnResetButtonClick(object sender, EventArgs e)
    {
        this._game = new Game();
    }

    // Get foul mark
    private void OnFoulButtonClick(object sender, EventArgs e)
    {
        _game.GetMark();
    }

    // 打开设置调试窗口
    private void OnSettingsButtonClick(object sender, EventArgs e)
    {
        lock (Flags)
        {
            SettingsWindow st = new SettingsWindow(ref this._flags, ref _game, this);
            st.Show();
        }
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        this.RefreshAll();
        this.Communicate();
    }

    #endregion
}