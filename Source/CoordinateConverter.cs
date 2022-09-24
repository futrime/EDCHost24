using System;
using OpenCvSharp;

namespace EdcHost;

/// <summary>
/// The transformation between different coordinate systems
/// </summary>
/// <remarks>
/// This is the transformation between camera frame coordinate, monitor frame coordinate,
/// and the court coordinate.
/// </remarks>
public class CoordinateConverter : IDisposable
{
    // The transformation matrices
    private Mat cam2logic;
    private Mat logic2cam;
    private Mat show2cam;
    private Mat cam2show;

    /// <summary>
    /// The positions of the corners of the court in the court coordinate system
    /// </summary>
    private Point2f[] logicCorners;

    /// <param name="myFlags">The information of the game</param>
    public CoordinateConverter(MyFlags myFlags)
    {
        Point2f[] camCorners = {
            new Point2f(0, 0),
            new Point2f(myFlags.cameraSize.Width, 0),
            new Point2f(0, myFlags.cameraSize.Height),
            new Point2f(myFlags.cameraSize.Width, myFlags.cameraSize.Height)
        };
        Point2f[] showCorners = {
            new Point2f(0, 0),
            new Point2f(myFlags.showSize.Width, 0),
            new Point2f(0, myFlags.showSize.Height),
            new Point2f(myFlags.showSize.Width, myFlags.showSize.Height)
        };

        this.logicCorners = new Point2f[] {
            new Point2f(0, 0),
            new Point2f(myFlags.logicSize.Width, 0),
            new Point2f(0, myFlags.logicSize.Height),
            new Point2f(myFlags.logicSize.Width, myFlags.logicSize.Height)
        };

        // Get the position transformations between camera frames and monitor frames
        this.cam2show = Cv2.GetPerspectiveTransform(camCorners, showCorners);
        this.show2cam = Cv2.GetPerspectiveTransform(showCorners, camCorners);

        // Get the position transformations between camera frames and the court
        this.cam2logic = Cv2.GetPerspectiveTransform(camCorners, logicCorners);
        this.logic2cam = Cv2.GetPerspectiveTransform(logicCorners, camCorners);
    }

    public void Dispose()
    {
        this.Dispose(true);
    }

    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            cam2logic.Dispose();
            logic2cam.Dispose();
            show2cam.Dispose();
            cam2show.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Calibrate the coordinate transformation.
    /// </summary>
    /// <param name="corners">A list of the four corners of the court in the camera view</param>
    /// <param name="myFlags">The information of the game</param>
    public void UpdateCorners(Point2f[] corners, MyFlags myFlags)
    {
        // Return if the corner list is invalid
        if (corners == null)
        {
            return;
        }
        if (corners.Length != 4)
        {
            return;
        }

        corners = Cv2.PerspectiveTransform(corners, show2cam);

        // Get the position transformations between camera frames and the court
        this.cam2logic = Cv2.GetPerspectiveTransform(corners, this.logicCorners);
        this.logic2cam = Cv2.GetPerspectiveTransform(this.logicCorners, corners);

        // 标记摄像机画面为已校正
        myFlags.calibrated = true;
    }

    #region Transformations

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

    #endregion
}