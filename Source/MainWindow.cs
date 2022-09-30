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
                    Locator = new LocatorConfigType
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
                    Locator = new LocatorConfigType
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
    /// The length of the buffer for serial ports.
    /// </summary>
    private const int SerialPortBufferLength = 1024;

    #endregion


    #region Public properties

    /// <summary>
    /// The camera
    /// </summary>
    public VideoCapture Camera
    {
        get => this._camera;
        set => this._camera = value;
    }

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

        // Initialize the label texts
        this.labelScoreVehicleA.Text = "0.000";
        this.labelScoreVehicleB.Text = "0.000";
        this.labelGameTime.Text = "0.00";
        this.labelGameHalf.Text = "Pre-match";

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
            Game.CourtArea.BottomRight.X,
            Game.CourtArea.BottomRight.Y
        );

        // Setup the coordinate converter
        this._coordinateConverter = new CoordinateConverter(
            cameraFrameSize: this._cameraFrameSize,
            monitorFrameSize: this._monitorFrameSize,
            courtSize: this._courtSize
        );

        // Setup the timer
        this.timer.Interval = (int)(1000 / this._camera.Fps);
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

        // Refresh the window to update content.
        this.Refresh();
    }

    /// <summary>
    /// Refresh everything.
    /// </summary>
    private void RefreshAll()
    {
        // Update the timer interval.
        if (this.timer.Interval != (int)(this._camera.Fps))
        {
            this.timer.Interval = Math.Max((int)(this._camera.Fps), 1);
        }

        this.ProcessCameraFrame();

        if (this._game.GameState == GameStatusType.Unstarted)
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
        else if (this._game.GameState == GameStatusType.Running)
        {
            if (this._locatorDict[(CampType)this._game.Camp].TargetPosition != null)
            {
                // Update the position of the current vehicle.
                if (this._game.GameTime == null)
                {
                    throw new Exception("The game time is null.");
                }

                // Update the position of the vehicle of the current camp.
                this._game.Vehicle[(CampType)this._game.Camp].UpdatePosition(
                    new Dot((Point2i)this._coordinateConverter.CameraToCourt((Point2f)this._locatorDict[(CampType)this._game.Camp].TargetPosition)),
                    (long)this._game.GameTime
                );

                // Refresh the game.
                this._game.Refresh();
            }

            this.buttonFoul.Enabled = true;
            this.buttonCalibration.Enabled = false;
            this.buttonSettings.Enabled = false;
            this.buttonStart.Enabled = false;
            this.buttonPause.Enabled = true;
            this.buttonContinue.Enabled = false;
            this.buttonEnd.Enabled = true;

            this.labelScoreVehicleA.Text = this._game.Score[CampType.A].ToString("0.000");
            this.labelScoreVehicleB.Text = this._game.Score[CampType.B].ToString("0.000");
            this.labelGameTime.Text = Math.Max((decimal)(this._game.RemainingTime) / 1000, (decimal)0).ToString("0.00");
            this.progressBarRemainingPowerRatio.Value = (int)(this._game.Vehicle[(CampType)this._game.Camp].RemainingPowerRatio * 100);
        }
        else if (this._game.GameState == GameStatusType.Paused)
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
        else if (this._game.GameState == GameStatusType.Ended)
        {
            this.buttonFoul.Enabled = false;
            if (this._calibrationClickCount >= 4)
            {
                this.buttonCalibration.Enabled = true;
            }
            this.buttonSettings.Enabled = false;
            if (
                this._game.GameStage == GameStageType.SecondHalf &&
                this._game.Camp == CampType.B
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

            // Read the message.
            var buffer = new byte[MainWindow.SerialPortBufferLength];
            var length = this._serialPortDict[camp].Read(
                buffer: buffer,
                offset: 0,
                count: MainWindow.SerialPortBufferLength
            );
            var bytesRead = new byte[length];
            buffer.CopyTo(bytesRead, 0);

            // Process the message
            if (length > 0)
            {
                try
                {
                    Packet packetFromSlave = Packet.Make(bytesRead);

                    if (packetFromSlave.GetPacketId() == PacketGetGameInformationSlave.PacketId)
                    {
                        // Find own charging pile list
                        List<Dot> ownChargingPiles = new List<Dot> { },
                            opponentChargingPiles = new List<Dot> { };
                        foreach (var chargingPile in this._game.ChargingPileList)
                        {
                            if (chargingPile.Camp == this._game.Camp)
                            {
                                ownChargingPiles.Add(chargingPile.Position);
                            }
                            else
                            {
                                opponentChargingPiles.Add(chargingPile.Position);
                            }
                        }

                        var gameInfoPacket = new PacketGetGameInformationHost(
                                            gameStage: this._game.GameStage,
                                            barrierList: this._game.BarrierList,
                                            duration: (long)Game.GameDuration[this._game.GameStage],
                                            ownChargingPiles: ownChargingPiles,
                                            opponentChargingPiles: opponentChargingPiles
                                        );
                        var bytesToWrite = gameInfoPacket.GetBytes();

                        this._serialPortDict[camp].Write(bytesToWrite, 0, bytesToWrite.Length);
                    }
                    else if (packetFromSlave.GetPacketId() == PacketSetChargingPileSlave.PacketId)
                    {
                        this._game.SetChargingPile();
                    }
                }
                catch (System.Exception)
                {
                    // Do nothing.
                }
            }


            // Send default packet.
            // Only send if the serial port is not writing.
            if (this._serialPortDict[camp].BytesToWrite == 0)
            {
                // Get the order in delivery list.
                var orderInDeliveryList = new List<Order>();
                foreach (var order in this._game.OrderList)
                {
                    if (order.Status == OrderStatusType.InDelivery)
                    {
                        orderInDeliveryList.Add(order);
                    }
                }

                var packet = new PacketGetStatusHost(
                    gameStatus: this._game.GameState,
                    gameTime: this._game.GameTime.GetValueOrDefault(0),
                    score: (double)this._game.Score[camp],
                    vehiclePosition: this._game.Vehicle[camp].Position.GetValueOrDefault(new Dot(0, 0)),
                    remainingDistance: this._game.Vehicle[camp].RemainingDistance,
                    orderInDeliveryList: orderInDeliveryList,
                    latestPendingOrder: this._game.OrderList[this._game.OrderList.Count - 1]
                );

                var bytesToWrite = packet.GetBytes();

                this._serialPortDict[camp].Write(bytesToWrite, 0, bytesToWrite.Length);
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

        // Avoid rendering the monitor frame when the main window is minimized.
        if (this._monitorFrameSize.Width != 0 &&
            this._monitorFrameSize.Height != 0)
        {
            // Resize to the size of the monitor
            Cv2.Resize(src: frame, dst: frame, dsize: this._monitorFrameSize);

            // Draw patterns on the monitor.
            this.Draw(ref frame);

            // Update the monitor frame
            this.RefreshMonitor(BitmapConverter.ToBitmap(frame));
        }
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
        foreach (var wall in Game.WallList)
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
        if (this._game.GameState == GameStatusType.Running || this._game.GameState == GameStatusType.Paused)
        {
            Vehicle vehicle = this._game.Vehicle[(CampType)this._game.Camp];

            foreach (Order order in _game.OrderList)
            {
                if (order.Status == OrderStatusType.Pending)
                {
                    this.DrawIcon(
                        image: ref image,
                        icon: IconOrder.Departure,
                        position: (Point2i)this._coordinateConverter.CourtToMonitor(order.DeparturePosition.ToPoint())
                    );
                }
                else if (order.Status == OrderStatusType.InDelivery)
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
            new Point2f(barrier.TopLeftPosition.X, barrier.BottomRightPosition.Y),
            barrier.BottomRightPosition.ToPoint(),
            new Point2f(barrier.BottomRightPosition.X, barrier.TopLeftPosition.Y),
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
        if (this._game.GameStage == GameStageType.PreMatch &&
            this._game.Camp == null)
        {
            _game.Start(CampType.A, GameStageType.FirstHalf);
            labelGameHalf.Text = "First Half";
        }
        else if (this._game.GameStage == GameStageType.FirstHalf &&
            this._game.Camp == CampType.A)
        {
            _game.Start(CampType.B, GameStageType.FirstHalf);
            labelGameHalf.Text = "First Half";
        }
        else if (this._game.GameStage == GameStageType.FirstHalf &&
            this._game.Camp == CampType.B)
        {
            _game.Start(CampType.A, GameStageType.SecondHalf);
            labelGameHalf.Text = "Second Half";
        }
        else if (this._game.GameStage == GameStageType.SecondHalf &&
            this._game.Camp == CampType.A)
        {
            _game.Start(CampType.B, GameStageType.SecondHalf);
            labelGameHalf.Text = "Second Half";
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
        _game.SetFoul();
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