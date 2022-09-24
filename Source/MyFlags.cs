namespace EdcHost;
public class MyFlags
{
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

    // 图像识别参数
    // HSV颜色模型：Hue为色调，Saturation为饱和度，Value为亮度
    public struct LocConfigs
    {
        public int hue1Lower;
        public int hue1Upper;
        public int hue2Lower;
        public int hue2Upper;
        public int saturation1Lower;
        public int saturation2Lower;
        public int valueLower;
        // 小车面积的最小值，低于这个值的检测对象会被过滤掉
        public int areaLower;
    }
    public LocConfigs configs;

    // 三个画面的大小
    public OpenCvSharp.Size showSize;
    public OpenCvSharp.Size cameraSize;
    public OpenCvSharp.Size logicSize;



    //将Init()替换为默认构造函数
    public MyFlags()
    {
        showMask = false;
        running = false;
        calibrated = false;
        videomode = false;

        // 初始化色彩识别参数
        configs = new LocConfigs();



        // 设置3张地图的大小
        const int MAX_SIZE_CM = 254;
        // 以下数据待定，根据实际设备确定
        showSize = new OpenCvSharp.Size(1280, 960);
        cameraSize = new OpenCvSharp.Size(1280, 960);
        logicSize = new OpenCvSharp.Size(MAX_SIZE_CM, MAX_SIZE_CM);

        // 点击显示画面的次数，用于校正画面
        clickCount = 0;
    }

    public void Start()
    {
        running = true;
    }

    public void End()
    {
        running = false;
    }
}