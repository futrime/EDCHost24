namespace EdcHost;

/// <summary>
/// A collection of configurations
/// </summary>
public class ConfigTypeLegacy
{    
    /// <summary>
    /// The object detection configurations
    /// </summary>
    public class LocatorConfigTypeLegacy
    {
        public int MinHueVehicleA;
        public int MinHueVehicleB;
        public int MaxHueVehicleA;
        public int MaxHueVehicleB;
        public int MinSaturationVehicleA;
        public int MinSaturationVehicleB;
        public int valueLower;
        /// <summary>
        /// The mininum area of the color block to be detected as a vehicle.
        /// </summary>
        public int MinArea;
    }


    public bool ShowMask = false;
    public int CalibrationClickCount = 4;
    public LocatorConfigTypeLegacy LocatorConfig = new LocatorConfigTypeLegacy();

    public OpenCvSharp.Size MonitorFrameSize = new OpenCvSharp.Size();
    public OpenCvSharp.Size CameraFrameSize = new OpenCvSharp.Size();
    public OpenCvSharp.Size CourtSize = new OpenCvSharp.Size(254, 254);
}