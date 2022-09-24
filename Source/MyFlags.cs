namespace EdcHost;
public class MyFlags
{    
    /// <summary>
    /// The object detection configurations
    /// </summary>
    public struct LocConfigs
    {
        public int hue1Lower;
        public int hue1Upper;
        public int hue2Lower;
        public int hue2Upper;
        public int saturation1Lower;
        public int saturation2Lower;
        public int valueLower;

        /// <summary>
        /// The mininum area of the color block to be detected as a vehicle.
        /// </summary>
        public int areaLower;
    }

    /// <summary>
    /// The size of the camera frame
    /// </summary>
    private static readonly (int Width, int Height) CameraFrameSize = (1280, 960);

    /// <summary>
    /// The size of the court
    /// </summary>
    private static readonly (int Width, int Height) CourtSize = (1280, 960);

    /// <summary>
    /// The size of the monitor frame
    /// </summary>
    private static readonly (int Width, int Height) MonitorFrameSize = (254, 254);

    // 调试颜色识别
    public bool showMask;
    // 比赛是否正在进行
    public bool running;
    // 地图是否被校准
    // 只有校正后才能准确实现logicMap与showMap，camMap间的坐标转换
    // camMap和showMap坐标的对应始终是准确的，但它们与logicMap的顶点不一定重合
    // 因此需要手动在showMap上选中场地的4个顶点以校正
    public bool calibrated;

    public bool videomode;
    public int clickCount;   // 画面被点击的次数

    public LocConfigs configs;

    // 三个画面的大小
    public OpenCvSharp.Size showSize;
    public OpenCvSharp.Size cameraSize;
    public OpenCvSharp.Size logicSize;

    public MyFlags()
    {
        this.showMask = false;
        this.running = false;
        this.calibrated = false;
        this.videomode = false;

        // 初始化色彩识别参数
        this.configs = new LocConfigs();

        // 以下数据待定，根据实际设备确定
        this.cameraSize = new OpenCvSharp.Size(MyFlags.CameraFrameSize.Width, MyFlags.CameraFrameSize.Height);
        this.logicSize = new OpenCvSharp.Size(MyFlags.CourtSize.Width, MyFlags.CourtSize.Height);
        this.showSize = new OpenCvSharp.Size(MyFlags.MonitorFrameSize.Width, MyFlags.MonitorFrameSize.Width);

        // 点击显示画面的次数，用于校正画面
        this.clickCount = 0;
    }

    public void Start()
    {
        this.running = true;
    }

    public void End()
    {
        this.running = false;
    }
}