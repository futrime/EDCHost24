using System;
using OpenCvSharp;

namespace EdcHost;
// 坐标转换器：将三种坐标（摄像头坐标、显示坐标、逻辑坐标）上的点坐标进行相互转换
// 摄像头坐标：摄像头直接捕捉到的视频帧对应的坐标
// 显示坐标：界面上的组件大小所决定的显示画面帧对应的坐标
// 逻辑坐标：规则文档中描述的场地大小对应的坐标
public class CoordinateConverter : IDisposable
{
    // 投影变换中的变换矩阵
    private Mat cam2logic;
    private Mat logic2cam;
    private Mat show2cam;
    private Mat cam2show;
    private Mat show2logic;
    private Mat logic2show;

    // 逻辑画面、摄像头画面、显示画面的四个角坐标
    // 顺序依次为左上、右上、左下、右下
    private Point2f[] logicCorners;
    private Point2f[] camCorners;
    private Point2f[] showCorners;

    // 释放托管资源
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ((IDisposable)(cam2logic)).Dispose();
            ((IDisposable)(logic2cam)).Dispose();
            ((IDisposable)(show2cam)).Dispose();
            ((IDisposable)(cam2show)).Dispose();
            ((IDisposable)(show2logic)).Dispose();
            ((IDisposable)(logic2show)).Dispose();
        }

    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // 设置3种画面的分别的4个角的坐标，并计算出投影变化矩阵
    public CoordinateConverter(MyFlags myFlags)
    {
        // 相机拍摄的地图
        camCorners = new Point2f[4];
        // 逻辑的地图
        logicCorners = new Point2f[4];
        // 在屏幕上显示的地图
        showCorners = new Point2f[4];

        cam2logic = new Mat();
        show2cam = new Mat();
        logic2show = new Mat();
        show2logic = new Mat();
        cam2show = new Mat();
        logic2cam = new Mat();

        // 逻辑画面四角坐标设置
        logicCorners[0].X = 0;
        logicCorners[0].Y = 0;
        logicCorners[1].X = myFlags.logicSize.Width;
        logicCorners[1].Y = 0;
        logicCorners[2].X = 0;
        logicCorners[2].Y = myFlags.logicSize.Height;
        logicCorners[3].X = myFlags.logicSize.Width;
        logicCorners[3].Y = myFlags.logicSize.Height;

        // 显示画面四角坐标设置
        showCorners[0].X = 0;
        showCorners[0].Y = 0;
        showCorners[1].X = myFlags.showSize.Width;
        showCorners[1].Y = 0;
        showCorners[2].X = 0;
        showCorners[2].Y = myFlags.showSize.Height;
        showCorners[3].X = myFlags.showSize.Width;
        showCorners[3].Y = myFlags.showSize.Height;

        // 摄像头画面四角坐标设置
        camCorners[0].X = 0;
        camCorners[0].Y = 0;
        camCorners[1].X = myFlags.cameraSize.Width;
        camCorners[1].Y = 0;
        camCorners[2].X = 0;
        camCorners[2].Y = myFlags.cameraSize.Height;
        camCorners[3].X = myFlags.cameraSize.Width;
        camCorners[3].Y = myFlags.cameraSize.Height;

        // 通过投影变换函数计算变换矩阵
        show2cam = Cv2.GetPerspectiveTransform(showCorners, camCorners);
        cam2show = Cv2.GetPerspectiveTransform(camCorners, showCorners);
    }

    // 传入鼠标点击的画面上的4个点，校正摄像机画面
    public void UpdateCorners(Point2f[] corners, MyFlags myFlags)
    {
        // 如果传入的不是4个点则返回
        if (corners == null) return;
        if (corners.Length != 4) return;
        else showCorners = corners;
        // showCorners被更新为鼠标点击的4个点（左上、右上、左下、右下）

        // logicCorners不需要改变
        // 直接计算showCorners和logicCorners间的变换矩阵
        logic2show = Cv2.GetPerspectiveTransform(logicCorners, showCorners);
        show2logic = Cv2.GetPerspectiveTransform(showCorners, logicCorners);

        // 将显示画面投影变换成摄像头画面，同时更新摄像头画面的四个角标
        // 通过修正后的showCorners和先前已计算过的showCorners至camCorners的变换矩阵
        // 计算出实际上摄像头拍到的场地边界camCorners
        camCorners = Cv2.PerspectiveTransform(showCorners, show2cam);
        cam2logic = Cv2.GetPerspectiveTransform(camCorners, logicCorners);
        logic2cam = Cv2.GetPerspectiveTransform(logicCorners, camCorners);

        // 标记摄像机画面为已校正
        myFlags.calibrated = true;
    }

    #region 投影变换函数

    // 输入某一地图上一串坐标序列，通过投影矩阵的作用，输出另一地图对应的坐标序列
    public Point2f[] ShowToCamera(Point2f[] ptsShow)
    {
        return Cv2.PerspectiveTransform(ptsShow, show2cam);
    }

    public Point2f[] CameraToShow(Point2f[] ptsCamera)
    {
        return Cv2.PerspectiveTransform(ptsCamera, cam2show);
    }

    public Point2f[] CameraToLogic(Point2f[] ptsCamera)
    {
        return Cv2.PerspectiveTransform(ptsCamera, cam2logic);
    }

    public Point2f[] LogicToCamera(Point2f[] ptsLogic)
    {
        return Cv2.PerspectiveTransform(ptsLogic, logic2cam);
    }

    public Point2f[] LogicToShow(Point2f[] ptsLogic)
    {
        return Cv2.PerspectiveTransform(ptsLogic, logic2show);
    }

    public Point2f[] ShowToLogic(Point2f[] ptsShow)
    {
        return Cv2.PerspectiveTransform(ptsShow, show2logic);
    }

    #endregion

    /*
            // 将flags中人员的起始位置从逻辑坐标转换为显示坐标
            public void PeopleFilter(MyFlags flags)
            {
                // 如果图像还未被校正，直接返回
                if (!flags.calibrated) return;

                // 因为被困人员同一时间在场上只有1个，其实只要计算1个坐标变换
                // 但是还是将这1个坐标构造成了坐标点列方便调用已有函数
                Point2f[] res = LogicToCamera(new Point2f[] { flags.logicPsgStart });

                // 计算被困人员在画面地图上的坐标
                flags.logicPsgStart = res[0];
            }*/
}