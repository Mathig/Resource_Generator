using System;

namespace Resource_Generator
{
    /// <summary>
    /// A point that contains index keys and can be sorted.
    /// </summary>
    public readonly struct KeyPoint : IPoint , IEquatable<KeyPoint>
    {
        /// <summary>
        /// Constructor for point.
        /// </summary>
        /// <param name="inX">X position for point.</param>
        /// <param name="inY">Y position for point.</param>
        public KeyPoint(int inX, int inY)
        {
            X = inX;
            Y = inY;
        }

        /// <summary>
        /// X value of point.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y value of point.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Sorts between two points, sorting by Y first, then X.
        /// </summary>
        /// <param name="obj">Object to compare to this one.</param>
        /// <returns>
        /// -1 if input is after this object, 0 if input is identical to this object, and 1 if input
        ///  is before this object.
        /// </returns>
        public int CompareTo(object obj)
        {
            if (obj is IPoint inputPoint)
            {
                return CompareTo(inputPoint);
            }
            else
            {
                throw new ArgumentException("Object is not a KeyPoint.");
            }
        }

        /// <summary>
        /// Sorts between two points, sorting by Y first, then X.
        /// </summary>
        /// <param name="otherPoint">Object to compare to this one.</param>
        /// <returns>
        /// -1 if input is after this object, 0 if input is identical to this object, and 1 if input
        ///  is before this object.
        /// </returns>
        public int CompareTo(IPoint otherPoint)
        {
            var yComparison = Y.CompareTo(otherPoint.Y);
            return yComparison == 0 ? X.CompareTo(otherPoint.X) : yComparison;
        }

        /// <summary>
        /// Interface for comparing whether points are equivalent.
        /// </summary>
        /// <param name="other">Other point to compare with.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public bool Equals(KeyPoint other)
        {
            if (this.X == other.X && this.Y == other.Y)
            {
                return true;
            }
            return false;
        }
    }
}