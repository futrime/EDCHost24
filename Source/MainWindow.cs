using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
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

    public static readonly CampType[] AllCampList = {
        CampType.A,
        CampType.B
    };

    public static readonly ConfigType DefaultConfig = new ConfigType
    {
        Vehicles = new Dictionary<CampType, ConfigType.PerVehicleConfigType> {
            {
                CampType.A,
                new ConfigType.PerVehicleConfigType
                {
                    Locator = new Locator.ConfigType
                    {
                        Hue = (0, 40),
                        Saturation = (100, 255),
                        Value = (100, 255),
                        MinArea = 4M
                    },
                    ShowMask = false,
                    SerialPort = "",
                    Baudrate = 115200
                }
            },
            {
                CampType.B,
                new ConfigType.PerVehicleConfigType
                {
                    Locator = new Locator.ConfigType
                    {
                        Hue = (0, 40),
                        Saturation = (100, 255),
                        Value = (100, 255),
                        MinArea = 4M
                    },
                    ShowMask = false,
                    SerialPort = "",
                    Baudrate = 115200
                }
            }
        },
        Camera = 0
    };

    /// <summary>
    /// The size of icons shown on the monitor
    /// </summary>
    private static readonly OpenCvSharp.Size IconSize = new OpenCvSharp.Size(25, 25);

    private static readonly Dictionary<CampType, Mat> IconCarDict = new Dictionary<CampType, Mat> {
            {CampType.A, new Mat(@"Assets/Icons/VehicleRed.png", ImreadModes.Color)},
            {CampType.B, new Mat(@"Assets/Icons/VehicleBlue.png", ImreadModes.Color)}
        };
    private static readonly Dictionary<CampType, Mat> IconChargingPileDict = new Dictionary<CampType, Mat> {
            {CampType.A, new Mat(@"Assets/Icons/ChargingPileRed.png", ImreadModes.Color)},
            {CampType.B, new Mat(@"Assets/Icons/ChargingPileBlue.png", ImreadModes.Color)}
        };
    private static readonly (Mat Departure, Mat Destination) IconOrder = (
        new Mat(@"Assets/Icons/OrderDeparture.png", ImreadModes.Color),
        new Mat(@"Assets/Icons/OrderDestination.png", ImreadModes.Color)
    );

    /// <summary>
    /// The refresh rate in hertz
    /// </summary>
    private const int RefreshRate = 60;

    /// <summary>
    /// The length of the buffer for serial ports.
    /// </summary>
    private const int SerialPortBufferLength = 1024;

    #endregion


    #region Public properties

    /// <summary>
    /// The camera
    /// </summary>
    public VideoCapture Camera => this._camera;

    /// <summary>
    /// The size of camera frames
    /// </summary>
    public OpenCvSharp.Size CameraFrameSize
    {
        get => this._cameraFrameSize;
        set => this._cameraFrameSize = value;
    }

    /// <summary>
    /// The configurations
    /// </summary>
    public ConfigType Config
    {
        get => this._config;
        set => this._config = value;
    }

    /// <summary>
    /// The coordinate converter
    /// </summary>
    public CoordinateConverter CoordinateConverter
    {
        get => this._coordinateConverter;
        set => this._coordinateConverter = value;
    }

    /// <summary>
    /// The court size.
    /// </summary>
    public OpenCvSharp.Size CourtSize => this._courtSize;

    /// <summary>
    /// The locators
    /// </summary>
    public Dictionary<CampType, Locator> LocatorDict => this._locatorDict;

    /// <summary>
    /// The size of monitor frames
    /// </summary>
    public OpenCvSharp.Size MonitorFrameSize => this._monitorFrameSize;

    /// <summary>
    /// The serial ports
    /// </summary>
    public Dictionary<CampType, SerialPort> SerialPortDict
    {
        get => this._serialPortDict;
        set => this._serialPortDict = value;
    }


    #endregion

    #region Private fields

    private int _calibrationClickCount = 4;
    private VideoCapture _camera = new VideoCapture();
    private OpenCvSharp.Size _cameraFrameSize;
    private ConfigType _config = MainWindow.DefaultConfig;
    private OpenCvSharp.Size _courtSize;
    private CoordinateConverter _coordinateConverter;
    private Game _game = new Game();
    private Dictionary<CampType, Locator> _locatorDict = new Dictionary<CampType, Locator>();
    private Point2f[] _monitorCorners = new Point2f[4];
    private OpenCvSharp.Size _monitorFrameSize;
    private Dictionary<CampType, SerialPort> _serialPortDict = new Dictionary<CampType, SerialPort> {
        {CampType.A, null},
        {CampType.B, null}
    };

    #endregion


    #region Methods

    public MainWindow()
    {
        InitializeComponent();

        // Resize icons
        foreach (var icon in IconCarDict.Values)
        {
            Cv2.Resize(
                src: icon,
                dst: icon,
                dsize: MainWindow.IconSize
            );
        }
        foreach (var icon in IconChargingPileDict.Values)
        {
            Cv2.Resize(
                src: icon,
                dst: icon,
                dsize: MainWindow.IconSize
            );
        }
        Cv2.Resize(
            src: IconOrder.Departure,
            dst: IconOrder.Departure,
            dsize: MainWindow.IconSize
        );
        Cv2.Resize(
            src: IconOrder.Destination,
            dst: IconOrder.Destination,
            dsize: MainWindow.IconSize
        );

        // Setup the camera
        this._camera.Open(this.Config.Camera);
        if (!this._camera.IsOpened())
        {
            MessageBox.Show(
                "No camera found!",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            Application.Exit();
        }
        this._camera.ConvertRgb = true;

        // Load the sizes of camera frames, monitor frames and the court.
        this._cameraFrameSize = new OpenCvSharp.Size(
            this._camera.FrameWidth,
            this._camera.FrameHeight
        );
        this._monitorFrameSize = new OpenCvSharp.Size(
            this.pictureBoxMonitor.Width,
            this.pictureBoxMonitor.Height
        );
        this._courtSize = new OpenCvSharp.Size(
            Game.CourtWidth,
            Game.CourtHeight
        );

        // Setup the coordinate converter
        this._coordinateConverter = new CoordinateConverter(
            cameraFrameSize: this._cameraFrameSize,
            monitorFrameSize: this._monitorFrameSize,
            courtSize: this._courtSize
        );

        // Setup the timer
        this.timer.Interval = 1000 / MainWindow.RefreshRate;
        this.timer.Start();

        // Setup the locators
        foreach (var camp in MainWindow.AllCampList)
        {
            this._locatorDict.Add(
                camp,
                new Locator(
                    config: this.Config.Vehicles[camp].Locator,
                    showMask: this.Config.Vehicles[camp].ShowMask
                )
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
            this.buttonFoul.Enabled = false;
            if (this._calibrationClickCount >= 4)
            {
                this.buttonCalibration.Enabled = true;
            }
            this.buttonSettings.Enabled = true;
            this.buttonStart.Enabled = true;
            this.buttonPause.Enabled = false;
            this.buttonContinue.Enabled = false;
            this.buttonEnd.Enabled = false;
        }
        else if (this._game.GameState == GameStateType.Running)
        {
            if (this._locatorDict[this._game.GetCamp()].TargetPosition != null)
            {
                // Update the position of the current vehicle.
                this._game.Vehicle[this._game.GetCamp()].UpdatePosition(
                    new Dot((Point2i)this._coordinateConverter.CameraToCourt((Point2f)this._locatorDict[this._game.GetCamp()].TargetPosition)),
                    this._game.GameTime
                );
                this._game.Refresh();
            }

            this.buttonFoul.Enabled = true;
            this.buttonCalibration.Enabled = false;
            this.buttonSettings.Enabled = false;
            this.buttonStart.Enabled = false;
            this.buttonPause.Enabled = true;
            this.buttonContinue.Enabled = false;
            this.buttonEnd.Enabled = true;

            this.labelScoreVehicleA.Text = ((int)this._game.GetScore(CampType.A, this._game.GameStage)).ToString();
            this.labelScoreVehicleB.Text = ((int)this._game.GetScore(CampType.B, this._game.GameStage)).ToString();
            this.labelGameTime.Text = Math.Max((decimal)(this._game.RemainingTime) / 1000, (decimal)0).ToString("0.00");
            this.progressBarRemainingPowerRatio.Value = (int)(this._game.GetPowerRatio() * 100);
        }
        else if (this._game.GameState == GameStateType.Paused)
        {
            this.buttonFoul.Enabled = true;
            if (this._calibrationClickCount >= 4)
            {
                this.buttonCalibration.Enabled = true;
            }
            this.buttonSettings.Enabled = false;
            this.buttonStart.Enabled = false;
            this.buttonPause.Enabled = false;
            this.buttonContinue.Enabled = true;
            this.buttonEnd.Enabled = true;
        }
        else if (this._game.GameState == GameStateType.Ended)
        {
            this.buttonFoul.Enabled = false;
            if (this._calibrationClickCount >= 4)
            {
                this.buttonCalibration.Enabled = true;
            }
            this.buttonSettings.Enabled = false;
            if (
                this._game.GameStage == GameStageType.SecondHalf &&
                this._game.GetCamp() == CampType.B
            )
            {
                this.buttonStart.Enabled = false;
            }
            else
            {
                this.buttonStart.Enabled = true;
            }
            this.buttonPause.Enabled = false;
            this.buttonContinue.Enabled = false;
            this.buttonEnd.Enabled = false;
        }

        this.Refresh();
    }

    /// <summary>
    /// Communicate with the slaves
    /// </summary>
    private void Communicate()
    {
        foreach (var camp in MainWindow.AllCampList)
        {
            if (
                this._serialPortDict[camp] == null ||
                !this._serialPortDict[camp].IsOpen
            )
            {
                continue;
            }

            // Read the message
            var buffer = new byte[MainWindow.SerialPortBufferLength];
            var length = this._serialPortDict[camp].Read(
                buffer: buffer,
                offset: 0,
                count: MainWindow.SerialPortBufferLength
            );

            if (length > 0)
            {
                // To be implemented
            }
        }
    }

    #region Methods related to the camera and the monitor

    /// <summary>
    /// Read, process and display a frame from the camera.
    /// </summary>
    private void ProcessCameraFrame()
    {
        // Read the camera frame.
        Mat frame = new Mat();
        if (!Camera.Read(frame))
        {
            return;
        }

        // Update locator images.
        foreach (var locator in this._locatorDict.Values)
        {
            locator.Image = frame;
        }

        // Resize to the size of the monitor
        Cv2.Resize(src: frame, dst: frame, dsize: this._monitorFrameSize);

        // Draw patterns on the monitor.
        this.Draw(ref frame);

        // Update the monitor frame
        this.RefreshMonitor(BitmapConverter.ToBitmap(frame));
    }

    /// <summary>
    /// Draw patterns on the monitor frame.
    /// </summary>
    /// <param name="image">The background picture</param>
    /// <param name="localizer">The localiser</param>
    /// <return>The frame with patterns on it</return>
    private void Draw(ref Mat image)
    {
        // Draw court boundaries
        var corners = this._coordinateConverter.CourtToMonitor(new Point2f[]{
            new Point2f(0, 0),
            new Point2f(0, this._courtSize.Width),
            new Point2f(this._courtSize.Height, this._courtSize.Height),
            new Point2f(this._courtSize.Height, 0)
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

        // Draw Barriers and Walls
        foreach (var barrier in this._game.BarrierList)
        {
            DrawBarrier(ref image, barrier, Scalar.Orange);
        }
        foreach (var wall in this._game.WallList)
        {
            DrawBarrier(ref image, wall, Scalar.Black);
        }

        // Draw charging piles
        foreach (var chargingPile in this._game.ChargingPileList)
        {
            this.DrawIcon(
                image: ref image,
                icon: IconChargingPileDict[chargingPile.Camp],
                position: (Point2i)this._coordinateConverter.CourtToMonitor(chargingPile.Position.ToPoint())
            );
        }

        // Draw departures and destinations of orders
        if (this._game.GameState == GameStateType.Running || this._game.GameState == GameStateType.Paused)
        {
            Vehicle vehicle = this._game.Vehicle[this._game.GetCamp()];

            foreach (Order order in _game.AllOrderList)
            {
                if (order.Status == Order.StatusType.Pending)
                {
                    this.DrawIcon(
                        image: ref image,
                        icon: IconOrder.Departure,
                        position: (Point2i)this._coordinateConverter.CourtToMonitor(order.DeparturePosition.ToPoint())
                    );
                }
                else if (order.Status == Order.StatusType.InDelivery)
                {
                    this.DrawIcon(
                        image: ref image,
                        icon: IconOrder.Destination,
                        position: (Point2i)this._coordinateConverter.CourtToMonitor(order.DestinationPosition.ToPoint())
                    );
                }
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

            this.DrawIcon(
                image: ref image,
                icon: IconCarDict[camp],
                position: (Point2i)this._coordinateConverter.CameraToMonitor((Point2f)this._locatorDict[camp].TargetPosition)
            );
        }

        // Draw corners when calibrating
        if (this._calibrationClickCount < 4) // When calibrating
        {
            var pointList = this._monitorCorners;
            for (int i = 0; i < this._calibrationClickCount; ++i)
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
    }

    /// <summary>
    /// Draw a barrier on an image
    /// </summary>
    /// <param name="image">The image</param>
    /// <param name="barrier">The barrier</param>
    /// <param name="color">The color of the barrier</param>
    private void DrawBarrier(ref Mat image, Barrier barrier, Scalar color)
    {
        Point2f[] cornerInCourtCoordinateList = {
            barrier.TopLeftPosition.ToPoint(),
            new Point2f(barrier.TopLeftPosition.x, barrier.BottomRightPosition.y),
            barrier.BottomRightPosition.ToPoint(),
            new Point2f(barrier.BottomRightPosition.x, barrier.TopLeftPosition.y),
        };

        var cornerInMonitorCoordinateList = this._coordinateConverter.CourtToMonitor(cornerInCourtCoordinateList);

        Cv2.FillConvexPoly(
            image,
            Array.ConvertAll(
                cornerInMonitorCoordinateList,
                item => (Point2i)item
            ),
            color
        );
    }

    /// <summary>
    /// Draw an icon on an image
    /// </summary>
    /// <param name="image">The image</param>
    /// <param name="icon">The icon</param>
    /// <param name="position">
    /// The position of the icon in the camera coordinate
    /// </param>
    private void DrawIcon(ref Mat image, Mat icon, Point2i position)
    {
        var x = Math.Min(Math.Max(position.X - icon.Cols / 2, 0), image.Cols - icon.Cols);
        var y = Math.Min(Math.Max(position.Y - icon.Cols / 2, 0), image.Rows - icon.Rows);

        icon.CopyTo(new Mat(image, new Rect(x, y, icon.Cols, icon.Rows)));
    }

    /// <summary>
    /// Refresh the monitor.
    /// </summary>
    /// <param name="img">
    /// The image to show in the monitor
    /// </param>
    private void RefreshMonitor(Image img)
    {
        pictureBoxMonitor.Image = img;
    }

    #endregion


    #region Methods related to the Windows Form

    private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
    {
        this._camera.Release();

        // Close serial ports
        foreach (var serialPort in this._serialPortDict.Values)
        {
            if (
                serialPort == null ||
                !serialPort.IsOpen
            )
            {
                continue;
            }

            serialPort.Close();
        }
    }

    private void buttonCalibrate_Click(object sender, EventArgs e)
    {
        this._calibrationClickCount = 0;
        this.buttonCalibration.Enabled = false;
    }

    /// <summary>
    /// Click the four corners to calibrate the capturing.
    /// </summary>
    /// <remarks>
    /// Click on the top left corner, top right corner,
    /// bottom left corner, and the bottom right corner
    /// in turn to calibrate the capturing.
    /// </remarks>
    private void pictureBoxMonitor_MouseClick(object sender, MouseEventArgs e)
    {
        // Return if the mouse does not click in the monitor picture box.
        if (
            e.X < 0 || e.X >= this.pictureBoxMonitor.Width ||
            e.Y < 0 || e.Y >= this.pictureBoxMonitor.Height
        )
        {
            return;
        }

        // Return if it is not in calibration mode.
        if (this._calibrationClickCount >= 4)
        {
            return;
        }

        this._monitorCorners[this._calibrationClickCount] = new Point2f(e.X, e.Y);

        ++this._calibrationClickCount;

        // Calibrate if the four corners are confirmed.
        if (this._calibrationClickCount >= 4)
        {
            this._coordinateConverter.Calibrate(_monitorCorners);
            this.buttonCalibration.Enabled = true;
        }
    }

    private void pictureBoxMonitor_Resize(object sender, EventArgs e)
    {
        this._monitorFrameSize = new OpenCvSharp.Size(
            this.pictureBoxMonitor.Width,
            this.pictureBoxMonitor.Height
        );
        if (this._coordinateConverter != null)
            this._coordinateConverter = new CoordinateConverter(
                cameraFrameSize: this._cameraFrameSize,
                monitorFrameSize: this._monitorFrameSize,
                courtSize: this._courtSize,
                calibrationCorners: this._coordinateConverter.CalibrationCorners
            );
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
        if (this._game.GameStage == GameStageType.None &&
            this._game.GetCamp() == CampType.None)
        {
            _game.Start(CampType.A, GameStageType.FirstHalf);
            labelGameHalf.Text = "上半场";
        }
        else if (this._game.GameStage == GameStageType.FirstHalf &&
            this._game.GetCamp() == CampType.A)
        {
            _game.Start(CampType.B, GameStageType.FirstHalf);
            labelGameHalf.Text = "上半场";
        }
        else if (this._game.GameStage == GameStageType.FirstHalf &&
            this._game.GetCamp() == CampType.B)
        {
            _game.Start(CampType.A, GameStageType.SecondHalf);
            labelGameHalf.Text = "下半场";
        }
        else if (this._game.GameStage == GameStageType.SecondHalf &&
            this._game.GetCamp() == CampType.A)
        {
            _game.Start(CampType.B, GameStageType.SecondHalf);
            labelGameHalf.Text = "下半场";
        }
        else
        {
            return;
        }
    }

    private void buttonPause_Click(object sender, EventArgs e)
    {
        this._game.Pause();
    }

    private void buttonContinue_Click(object sender, EventArgs e)
    {
        _game.Continue();
    }

    private void buttonEnd_Click(object sender, EventArgs e)
    {
        _game.End();
    }

    private void buttonReset_Click(object sender, EventArgs e)
    {
        this._game = new Game();
    }

    private void buttonFoul_Click(object sender, EventArgs e)
    {
        _game.GetPenalty();
    }

    private void buttonSettings_Click(object sender, EventArgs e)
    {
        (new Thread(
            () => (new SettingsWindow(this)).ShowDialog()
        )).Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        this.RefreshAll();
        this.Communicate();
    }

    #endregion

    #endregion
}