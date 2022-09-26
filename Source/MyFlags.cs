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
    private static readonly (int Width, int Height) CourtSize = (254, 254);

    /// <summary>
    /// The size of the monitor frame
    /// </summary>
    private static readonly (int Width, int Height) MonitorFrameSize = (0, 0);

    // 调试颜色识别
    public bool showMask;
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
}