using System;

namespace Resource_Generator
{
    /// <summary>
    /// Allows comparison and sorting based on a height value.
    /// </summary>
    internal class HeightPoint : IComparable, IPoint
    {
        /// <summary>
        /// Location of point.
        /// </summary>
        private readonly KeyPoint _position;

        /// <summary>
        /// Height of point.
        /// </summary>
        public double height;

        /// <summary>
        /// Constructor that intakes position as coordinates as well as the height.
        /// </summary>
        /// <param name="inX">X coordinate of point.</param>
        /// <param name="inY">Y coordinate of point.</param>
        /// <param name="inHeight">Height of point.</param>
        public HeightPoint(int inX, int inY, double inHeight = 0)
        {
            _position = new KeyPoint(inX, inY);
            height = inHeight;
        }

        /// <summary>
        /// Opens <see cref="KeyPoint.X"/> to access through X.
        /// </summary>
        public int X => _position.X;

        /// <summary>
        /// Opens <see cref="KeyPoint.Y"/> to access through Y.
        /// </summary>
        public int Y => _position.Y;

        /// <summary>
        /// Compares two HeightPoints based on the height.
        /// </summary>
        /// <param name="obj">Points to be compared.</param>
        /// <returns>-1 if input is after this object, 0 if the positions are equal,
        /// and 1 if input is before this object.</returns>
        public int CompareTo(Object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is HeightPoint otherPoint)
            {
                return height.CompareTo(otherPoint.height);
            }
            else
            {
                throw new ArgumentException("Object is not a HeightPoint.");
            }
        }
    }
}