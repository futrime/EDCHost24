using System;

namespace EdcHost;

/// <summary>
/// A barrier
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
            area.TopLeft.X >= area.BottomRight.X ||
            area.TopLeft.Y >= area.BottomRight.Y
        )
        {
            throw new Exception("The area is invalid.");
        }

        // Check if the size range is valid
        if (
            sizeRange.Min.X >= sizeRange.Max.X ||
            sizeRange.Min.Y >= sizeRange.Max.Y
        )
        {
            throw new Exception("The size range is invalid.");
        }

        // Check if the area is large enough
        if (
            area.TopLeft.X + sizeRange.Max.X > area.BottomRight.X ||
            area.TopLeft.Y + sizeRange.Max.Y > area.BottomRight.Y
        )
        {
            throw new Exception("The area is too small to generate.");
        }

        // The size of the barrier
        // Note that the range for random.Next() is in [a, b) form.
        var size = new Dot(
            Utility.RandomGenerator.Next(sizeRange.Min.X, sizeRange.Max.X + 1),
            Utility.RandomGenerator.Next(sizeRange.Min.Y, sizeRange.Max.Y + 1)
        );

        var topLeftPosition = new Dot(
            Utility.RandomGenerator.Next(area.TopLeft.X, area.BottomRight.X - size.X + 1),
            Utility.RandomGenerator.Next(area.TopLeft.Y, area.BottomRight.Y - size.Y + 1)
        );

        var bottomRightPosition = new Dot(
            topLeftPosition.X + size.X,
            topLeftPosition.Y + size.Y
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
            topLeftPosition.X >= bottomRightPosition.X ||
            topLeftPosition.Y >= bottomRightPosition.Y
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
        if ((position.X >= this._topLeftPosition.X + 3)&&
            (position.Y >= this._topLeftPosition.Y + 3)&&
            (position.X <= this._bottomRightPosition.X - 3)&&
            (position.Y <= this._bottomRightPosition.Y - 3)
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