using System;

namespace Resource_Generator
{
    /// <summary>
    /// A simple point structure for X and Y (int) coordinates. MapSize should be called before use.
    /// </summary>
    public readonly struct SimplePoint : IComparable, IPoint
    {
        /// <summary>
        /// Half the number of points per width of map.
        /// </summary>
        private static int _halfMapXSize;

        /// <summary>
        /// Number of points per height of map.
        /// </summary>
        private static int _mapYSize;

        /// <summary>
        /// X value of point, for array index.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y value of point, for array index.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Constructor for X and Y coordinates.
        /// </summary>
        /// <param name="xIn">X Coordinate of point, see <see cref="Xi"/></param>
        /// <param name="yIn">Y Coordinate of point, see <see cref="Yi"/></param>
        public SimplePoint(int xIn, int yIn)
        {
            X = xIn;
            Y = yIn;
        }

        /// <summary>
        /// Sets the map size for the class.
        /// </summary>
        /// <param name="inHalfXSize">Will become <see cref="_halfMapXSize"/>.</param>
        /// <param name="inYSize">Will become <see cref="_mapYSize"/>.</param>
        public static void MapSize(int inHalfXSize, int inYSize)
        {
            _halfMapXSize = inHalfXSize;
            _mapYSize = inYSize;
        }

        /// <summary>
        /// Compares two points for sorting, sorted by Y first, then X.
        /// </summary>
        /// <param name="obj">Second point to compare.</param>
        /// <returns>-1 if input is after this object, 0 if the positions are equal,
        /// and 1 if input is before this object.</returns>
        public int CompareTo(Object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is SimplePoint otherPoint)
            {
                if (Y > otherPoint.Y)
                {
                    return 1;
                }
                if (Y < otherPoint.Y)
                {
                    return -1;
                }
                if (X > otherPoint.X)
                {
                    return 1;
                }
                if (X < otherPoint.X)
                {
                    return -1;
                }
                return 0;
            }
            else
            {
                throw new ArgumentException("Object is not a SimplePoint.");
            }
        }
        /// <summary>
        /// Returns neighboring points in an array ordered as above, below, left, then right.
        /// </summary>
        /// <returns>Array of neighboring points.</returns>
        public SimplePoint[] FindNeighborPoints()
        {
            var output = new SimplePoint[4];
            this.FindAboveBelowPoints(out output[0], out output[1]);
            this.FindLeftRightPoints(out output[2], out output[3]);
            return output;
        }

        /// <summary>
        /// Finds the x value for wraping around the vertical boundary conditions, ie the top or
        /// bottom poles.
        /// </summary>
        /// <returns>The x value for a point that wraps around the boundary conditions.</returns>
        private int PoleBoundaryXWrap()
        {
            return X >= _halfMapXSize ? (X - _halfMapXSize) : (X + _halfMapXSize);
        }

        /// <summary>
        /// Determines the points above and below this point, including wrap-arounds.
        /// </summary>
        /// <param name="abovePoint">The point above this point.</param>
        /// <param name="belowPoint">The point below this point.</param>
        public void FindAboveBelowPoints(out SimplePoint abovePoint, out SimplePoint belowPoint)
        {
            if (Y == 0)
            {
                abovePoint = new SimplePoint(PoleBoundaryXWrap(), Y);
                belowPoint = new SimplePoint(X, Y + 1);
            }
            else if (Y == _mapYSize - 1)
            {
                abovePoint = new SimplePoint(X, Y - 1);
                belowPoint = new SimplePoint(PoleBoundaryXWrap(), Y);
            }
            else
            {
                abovePoint = new SimplePoint(X, Y - 1);
                belowPoint = new SimplePoint(X, Y + 1);
            }
        }

        /// <summary>
        /// Determines the points left and right of this point, including wrap-arounds.
        /// </summary>
        /// <param name="leftPoint">The point to the left of this point.</param>
        /// <param name="rightPoint">The point to the right of this point.</param>
        public void FindLeftRightPoints(out SimplePoint leftPoint, out SimplePoint rightPoint)
        {
            if (X == 0)
            {
                leftPoint = new SimplePoint(2 * _halfMapXSize - 1, Y);
                rightPoint = new SimplePoint(X + 1, Y);
            }
            else if (X == 2 * _halfMapXSize - 1)
            {
                leftPoint = new SimplePoint(X - 1, Y);
                rightPoint = new SimplePoint(0, Y);
            }
            else
            {
                leftPoint = new SimplePoint(X - 1, Y);
                rightPoint = new SimplePoint(X + 1, Y);
            }
        }
    }
}