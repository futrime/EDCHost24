using System;

namespace EdcHost;

/// <summary>
/// The barrier on the court
/// </summary>
public class Barrier
{
    /// <summary>
    /// The position of the top left corner
    /// </summary>
    public Dot TopLeftPosition => this._topLeftPosition;
    /// <summary>
    /// The position of the bottom right corner
    /// </summary>
    public Dot BottomRightPosition => this._bottomRightPosition;

    private Dot _topLeftPosition;
    private Dot _bottomRightPosition;


    /// <summary>
    /// Generate a random barrier.
    /// </summary>
    /// <param name="area">
    /// The area to generate
    /// </param>
    /// <param name="size">
    /// The size range of the barrier
    /// </param>
    /// <returns>
    /// The generated barrier
    /// </returns>
    public static Barrier GenerateRandomBarrier(
        (Dot TopLeft, Dot BottomRight) area,
        (Dot Min, Dot Max) sizeRange
    )
    {
        // Check if the area is valid
        if (
            area.TopLeft.x >= area.BottomRight.x ||
            area.TopLeft.y >= area.BottomRight.y
        )
        {
            throw new Exception("The area is invalid.");
        }

        // Check if the size range is valid
        if (
            sizeRange.Min.x >= sizeRange.Max.x ||
            sizeRange.Min.y >= sizeRange.Max.y
        )
        {
            throw new Exception("The size range is invalid.");
        }

        // Check if the area is large enough
        if (
            area.TopLeft.x + sizeRange.Max.x > area.BottomRight.x ||
            area.TopLeft.y + sizeRange.Max.y > area.BottomRight.y
        )
        {
            throw new Exception("The area is too small to generate.");
        }

        var random = new Random((int)DateTime.Now.Ticks);

        // The size of the barrier
        // Note that the range for random.Next() is in [a, b) form.
        var size = new Dot(
            random.Next(sizeRange.Min.x, sizeRange.Max.x + 1),
            random.Next(sizeRange.Min.y, sizeRange.Max.y + 1)
        );

        var topLeftPosition = new Dot(
            random.Next(area.TopLeft.x, area.BottomRight.x - size.x + 1),
            random.Next(area.TopLeft.y, area.BottomRight.y - size.y + 1)
        );

        var bottomRightPosition = new Dot(
            topLeftPosition.x + size.x,
            topLeftPosition.y + size.y
        );

        return new Barrier(topLeftPosition, bottomRightPosition);
    }

    /// <summary>
    /// Construct a barrier.
    /// </summary>
    /// <param name="topLeftPosition">
    /// The position of the top left corner
    /// </param>
    /// <param name="bottomRightPosition">
    /// The position of the bottom right corner
    /// </param>
    public Barrier(Dot topLeftPosition, Dot bottomRightPosition)
    {
        if (
            topLeftPosition.x >= bottomRightPosition.x ||
            topLeftPosition.y >= bottomRightPosition.y
        ) // If the area of the barrier is not positive
        {
            throw new Exception("The corners are invalid.");
        }

        this._topLeftPosition = topLeftPosition;
        this._bottomRightPosition = bottomRightPosition;
    }

    /// <summary>
    /// Check if a position is in the barrier.
    /// </summary>
    /// <param name="position">The position</param>
    /// <returns>
    /// True if the position is in the barrier; otherwise false
    /// </returns>
    public bool IsIn(Dot position)
    {
        if (position.x >= this._topLeftPosition.x &&
            position.y >= this._topLeftPosition.y &&
            position.x <= this._bottomRightPosition.x &&
            position.y <= this._bottomRightPosition.y
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}