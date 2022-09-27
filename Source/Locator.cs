using OpenCvSharp;
using System.Collections.Generic;
namespace EdcHost;

/// <summary>
/// A locator the locate the position of a color block
/// </summary>
public class Locator
{
    #region Types

    /// <summary>
    /// The configuration type
    /// </summary>
    public struct ConfigType
    {
        public (int Min, int Max) Hue;
        public (int Min, int Max) Saturation;
        public (int Min, int Max) Value;
        public decimal MinArea;
    }

    #endregion


    #region Public properties

    /// <summary>
    /// The image where the locator locate the target. Write-only.
    /// </summary>
    public Mat Image
    {
        set
        {
            if (value == null || value.Empty())
            {
                return;
            }

            // Not to modify the original image
            var image = value.Clone();

            // Convert the color space to HSV
            Cv2.CvtColor(
                src: image,
                dst: image,
                code: ColorConversionCodes.RGB2HSV
            );

            // Binarization
            Cv2.InRange(
                src: image,
                lowerb: new Scalar(
                    this._config.Hue.Min,
                    this._config.Saturation.Min,
                    this._config.Value.Min
                ),
                upperb: new Scalar(
                    this._config.Hue.Max,
                    this._config.Saturation.Max,
                    this._config.Value.Max
                ),
                dst: image
            );

            // If the show mask option is on
            if (this._showMask)
            {
                // Show the binarized image
                Cv2.ImShow(
                    winName:
                        $"Hue: {this._config.Hue.Min} ~ {this._config.Hue.Max}" +
                        $"Saturation: {this._config.Saturation.Min} ~ {this._config.Saturation.Max}" +
                        $"Value: {this._config.Value.Min} ~ {this._config.Value.Max}",
                    mat: image
                );
            }

            var contourList = Cv2.FindContoursAsArray(
                image: image,
                mode: RetrievalModes.External, // Other modes may also work
                                               // Keep ending points only
                method: ContourApproximationModes.ApproxSimple
            );

            bool isTargetFound = false;

            // If any contour detected
            if (contourList.Length > 0)
            {
                // The moments of the contour, a mathematical
                // concept
                // Refer to https://docs.opencv.org/4.6.0/d8/d23/classcv_1_1Moments.html

                // Find the max length of contour in the contourList
                int maxLength = 0;
                int maxLengthIndex = 0;
                for (int i = 0; i < contourList.Length; i++)
                {
                    int currentLength = contourList[i].Length;
                    if (currentLength > maxLength)
                    {
                        maxLength = currentLength;
                        maxLengthIndex = i;
                    }
                }

                var moments = Cv2.Moments(contourList[maxLengthIndex]);
                // If the area detected is larger than the threshold
                if ((decimal)moments.M00 >= this._config.MinArea)
                {
                    // The barycenter of the ending points
                    this._targetPosition = new Point2f(
                        (float)(moments.M10 / moments.M00),
                        (float)(moments.M01 / moments.M00)
                    );

                    isTargetFound = true;
                }
            }
            if (!isTargetFound)
            {
                this._targetPosition = null;
            }
        }
    }

    /// <summary>
    /// The position of the target. Null if not detected.
    /// </summary>
    public Point2f? TargetPosition => this._targetPosition;

    #endregion

    #region Private fields

    private ConfigType _config;
    private bool _showMask;
    private Point2f? _targetPosition = null;

    #endregion


    #region  Public methods

    /// <summary>
    /// Construct a new locator
    /// </summary>
    /// <param name="config">The configurations</param>
    /// <param name="showMask">
    /// True if to show the mask
    /// </param>
    public Locator(ConfigType config, bool showMask)
    {
        this._config = config;
        this._showMask = showMask;
    }

    #endregion
}