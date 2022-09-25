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
    private Mat _transformationCameraToCourt;
    private Mat _transformationCourtToCamera;
    private Mat _transformationMonitorToCamera;
    private Mat _transformationCameraToMonitor;

    /// <summary>
    /// The positions of the corners of the court in the court coordinate system
    /// </summary>
    private Point2f[] _courtCorners;

    /// <summary>
    /// Construct a coordinate converter.
    /// </summary>
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

        this._courtCorners = new Point2f[] {
            new Point2f(0, 0),
            new Point2f(myFlags.logicSize.Width, 0),
            new Point2f(0, myFlags.logicSize.Height),
            new Point2f(myFlags.logicSize.Width, myFlags.logicSize.Height)
        };

        // Get the position transformations between camera frames and monitor frames
        this._transformationCameraToMonitor = Cv2.GetPerspectiveTransform(camCorners, showCorners);
        this._transformationMonitorToCamera = Cv2.GetPerspectiveTransform(showCorners, camCorners);

        // Get the position transformations between camera frames and the court
        this._transformationCameraToCourt = Cv2.GetPerspectiveTransform(camCorners, _courtCorners);
        this._transformationCourtToCamera = Cv2.GetPerspectiveTransform(_courtCorners, camCorners);
    }

    /// <summary>
    /// Dispose the converter.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
    }

    /// <summary>
    /// Dispose the converter.
    /// </summary>
    /// <param name="disposing">
    /// True if to dispose
    /// </param>
    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transformationCameraToCourt.Dispose();
            _transformationCourtToCamera.Dispose();
            _transformationMonitorToCamera.Dispose();
            _transformationCameraToMonitor.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Calibrate the coordinate transformation.
    /// </summary>
    /// <param name="corners">The four corners of the court in the monitor coordinate system</param>
    public void Calibrate(Point2f[] corners)
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

        corners = Cv2.PerspectiveTransform(corners, _transformationMonitorToCamera);

        // Get the position transformations between camera frames and the court
        this._transformationCameraToCourt = Cv2.GetPerspectiveTransform(corners, this._courtCorners);
        this._transformationCourtToCamera = Cv2.GetPerspectiveTransform(this._courtCorners, corners);
    }

    /// <summary>
    /// Convert the coordinates of points from the monitor coordinate system to the camera coordinate system.
    /// </summary>
    /// <param name="points">The points in the monitor coordinate system</param>
    /// <returns>The points in the camera coordinate system</returns>
    public Point2f[] MonitorToCamera(Point2f[] points)
    {
        return Cv2.PerspectiveTransform(points, _transformationMonitorToCamera);
    }

    /// <summary>
    /// Convert the coordinates of points from the camera coordinate system to the monitor coordinate system.
    /// </summary>
    /// <param name="points">The points in the camera coordinate system</param>
    /// <returns>The points in the monitor coordinate system</returns>
    public Point2f[] CameraToMonitor(Point2f[] points)
    {
        return Cv2.PerspectiveTransform(points, _transformationCameraToMonitor);
    }

    /// <summary>
    /// Convert the coordinates of points from the camera coordinate system to the court coordinate system.
    /// </summary>
    /// <param name="points">The points in the camera coordinate system</param>
    /// <returns>The points in the court coordinate system</returns>
    public Point2f[] CameraToCourt(Point2f[] points)
    {
        return Cv2.PerspectiveTransform(points, _transformationCameraToCourt);
    }

    /// <summary>
    /// Convert the coordinates of points from the court coordinate system to the camera coordinate system.
    /// </summary>
    /// <param name="points">The points in the court coordinate system</param>
    /// <returns>The points in the camera coordinate system</returns>
    public Point2f[] CourtToCamera(Point2f[] points)
    {
        return Cv2.PerspectiveTransform(points, _transformationCourtToCamera);
    }
}