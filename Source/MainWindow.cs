using System;
using System.Collections.Generic;
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
    #region Parameters

    private static readonly CampType[] AllCampList = {
        CampType.A,
        CampType.B
    };

    /// <summary>
    /// The size of icons shown on the monitor
    /// </summary>
    private const int IconSize = 10;

    /// <summary>
    /// The refresh rate in hertz
    /// </summary>
    private const int RefreshRate = 10;

    #endregion


    #region Public properties

    /// <summary>
    /// A list of available serial ports
    /// </summary>
    public string[] AvailableSerialPortList => this._availableSerialPortList;

    /// <summary>
    /// The camera
    /// </summary>
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

    #endregion

    #region Private fields

    private string[] _availableSerialPortList = SerialPort.GetPortNames();
    private VideoCapture _camera = new VideoCapture();
    private CoordinateConverter _coordinateConverter;
    private ConfigTypeLegacy _flags = new ConfigTypeLegacy();
    private Game _game = new Game();
    private Dictionary<CampType, Locator> _locatorDict = new Dictionary<CampType, Locator>();
    private Point2f[] _monitorCorners = new Point2f[4];
    private SerialPort _serialPortVehicleA = null;
    private SerialPort _serialPortVehicleB = null;

    #endregion


    #region Methods

    public MainWindow()
    {
        InitializeComponent();

        // Setup the camera
        this._camera.Open(0);
        Flags.CameraFrameSize.Width = this._camera.FrameWidth;
        Flags.CameraFrameSize.Height = this._camera.FrameHeight;
        Flags.MonitorFrameSize.Width = MonitorPictureBox.Width;
        Flags.MonitorFrameSize.Height = MonitorPictureBox.Height;
        CoordinateConverter = new CoordinateConverter(Flags);
        this._camera.FrameWidth = Flags.CameraFrameSize.Width;
        this._camera.FrameHeight = Flags.CameraFrameSize.Height;
        this._camera.ConvertRgb = true;

        // Setup the timer
        this.Timer.Interval = 1000 / MainWindow.RefreshRate;
        this.Timer.Start();

        // Setup locators
        foreach (var camp in MainWindow.AllCampList)
        {
            this._locatorDict.Add(
                camp, new Locator(new Locator.ConfigType(), false)
            );
        }
    }

    /// <summary>
    /// Refresh everything.
    /// </summary>
    private void RefreshAll()
    {
        this.ProcessCameraFrame();

        if (this._game.GameState == GameStateType.Unstarted)
        {
            this.FoulButton.Enabled = false;
            this.CalibrateButton.Enabled = true;
            this.SettingsButton.Enabled = true;
            this.StartButton.Enabled = true;
            this.PauseButton.Enabled = false;
            this.ContinueButton.Enabled = false;
            this.EndButton.Enabled = false;
        }
        else if (this._game.GameState == GameStateType.Running)
        {
            // update Car_pos;
            this._game.Vehicle[this._game.GetCamp()].Position = new Dot((Point2i)CoordinateConverter.CameraToCourt((Point2f)this._locatorDict[this._game.GetCamp()].TargetPosition))

            if (this._locatorDict[this._game.GetCamp()].TargetPosition != null)
            {
                this._game.Refresh();
            }

            this.FoulButton.Enabled = true;
            this.CalibrateButton.Enabled = false;
            this.SettingsButton.Enabled = false;
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
            this.FoulButton.Enabled = true;
            this.CalibrateButton.Enabled = true;
            this.SettingsButton.Enabled = false;
            this.StartButton.Enabled = false;
            this.PauseButton.Enabled = false;
            this.ContinueButton.Enabled = true;
            this.EndButton.Enabled = true;
        }
        else if (this._game.GameState == GameStateType.Ended)
        {
            this.FoulButton.Enabled = false;
            this.CalibrateButton.Enabled = true;
            this.SettingsButton.Enabled = false;
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

    /// <summary>
    /// Read, process and display a frame from the camera.
    /// </summary>
    private void ProcessCameraFrame()
    {
        // Read the camera frame.
        Mat cameraFrame = new Mat();
        if (!Camera.Read(cameraFrame))
        {
            return;
        }

        // Update locator images.
        foreach (var locator in this._locatorDict.Values)
        {
            locator.Image = cameraFrame;
        }

        // Draw patterns on the monitor.
        cameraFrame = this.Draw(cameraFrame);

        // Resize to the size of the monitor
        Cv2.Resize(cameraFrame, cameraFrame, this.Flags.MonitorFrameSize);

        // Update the monitor frame
        this.BeginInvoke(new Action<Image>(RefreshMonitor), BitmapConverter.ToBitmap(cameraFrame));
    }

    /// <summary>
    /// Draw patterns on the monitor frame.
    /// </summary>
    /// <param name="image">The background picture</param>
    /// <param name="localizer">The localiser</param>
    /// <return>The frame with patterns on it</return>
    private Mat Draw(Mat image)
    {
        // Read icons
        var iconCarA = new Mat(@"Assets/Icons/VehicleRed.png", ImreadModes.Color);
        var iconCarB = new Mat(@"Assets/Icons/VehicleBlue.png", ImreadModes.Color);
        var iconCarDict = new Dictionary<CampType, Mat> {
            {CampType.A, iconCarA},
            {CampType.B, iconCarB}
        };
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

        // Draw court boundaries
        var corners = this.CoordinateConverter.CourtToCamera(new Point2f[]{
            new Point2f(0, 0),
            new Point2f(0, this.Flags.CourtSize.Width),
            new Point2f(this.Flags.CourtSize.Height, this.Flags.CourtSize.Height),
            new Point2f(this.Flags.CourtSize.Height, 0)
        });
        for (int i = 0; i < 4; ++i)
        {
            Cv2.Line(
                image,
                (int)corners[i].X,
                (int)corners[i].Y,
                (int)corners[(i + 1) % 4].X,
                (int)corners[(i + 1) % 4].Y,
                color: new Scalar(0x00, 0x00, 0x00)
            );
        }

        // Draw corners when calibrating
        if (this._flags.CalibrationClickCount < 4)
        {
            var pointList = this.CoordinateConverter.MonitorToCamera(this._monitorCorners);
            for (int i = 0; i < this._flags.CalibrationClickCount; ++i)
            {
                var point = pointList[i];
                Cv2.Line(
                    image,
                    (int)(point.X - 10), (int)(point.Y),
                    (int)(point.X + 10), (int)(point.Y),
                    color: new Scalar(0x00, 0xff, 0x00)
                );
                Cv2.Line(
                    image,
                    (int)(point.X), (int)(point.Y - 10),
                    (int)(point.X), (int)(point.Y + 10),
                    color: new Scalar(0x00, 0xff, 0x00)
                );
            }
        }

        // Draw Barriers and Walls
        foreach (var barrier in this._game.BarrierList)
        {
            DrawBarrier(image, barrier, Scalar.Gray);
        }
        foreach (var wall in this._game.WallList)
        {
            DrawBarrier(image, wall, Scalar.Black);
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
        foreach (var camp in MainWindow.AllCampList)
        {
            // If the vehicle cannot be detected
            if (this._locatorDict[camp].TargetPosition == null)
            {
                continue;
            }

            var position = (Point2i)this._locatorDict[camp].TargetPosition;

            position.X = Math.Max(position.X, 0);
            position.X = Math.Min(position.X, image.Cols - iconCarB.Cols);

            position.Y = Math.Max(position.Y, 0);
            position.Y = Math.Min(position.Y, image.Rows - iconCarB.Rows);

            iconCarDict[camp].CopyTo(new Mat(image, new Rect(position.X, position.Y, iconCarA.Cols, iconCarA.Rows)));
        }

        return image;
    }

    private void DrawBarrier(Mat image, Barrier barrier, Scalar color)
    {
        Point2f[] cornerInCourtCoordinateList = {
            barrier.TopLeftPosition.ToPoint(),
            new Point2f(barrier.TopLeftPosition.x, barrier.BottomRightPosition.y),
            barrier.BottomRightPosition.ToPoint(),
            new Point2f(barrier.BottomRightPosition.x, barrier.TopLeftPosition.y),
        };

        var cornerInCameraCoordinateList = this.CoordinateConverter.CourtToCamera(cornerInCourtCoordinateList);

        Cv2.FillConvexPoly(
            image,
            Array.ConvertAll(
                cornerInCameraCoordinateList,
                item => (Point2i)item
            ),
            color
        );
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

    private void OnFormClosed(object sender, FormClosedEventArgs e)
    {
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
    /// Click the four corners to calibrate the capturing.
    /// </summary>
    /// <remarks>
    /// Click on the top left corner, top right corner,
    /// bottom left corner, and the bottom right corner
    /// in turn to calibrate the capturing.
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

    private void OnPauseButtonClick(object sender, EventArgs e)
    {
        this._game.Pause();
    }

    private void OnContinueButtonClick(object sender, EventArgs e)
    {
        _game.Continue();
    }

    private void OnEndButtonClick(object sender, EventArgs e)
    {
        _game.End();
    }

    private void OnResetButtonClick(object sender, EventArgs e)
    {
        this._game = new Game();
    }

    private void OnFoulButtonClick(object sender, EventArgs e)
    {
        _game.GetMark();
    }

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

    #endregion
}