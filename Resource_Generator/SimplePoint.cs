using System;

namespace Resource_Generator
{
    /// <summary>
    /// A simple point structure for X and Y (int) coordinates. MapSize should be called before use.
    /// </summary>
    public struct SimplePoint : IComparable, IPoint
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
        /// Internal X value of point, for array index.
        /// </summary>
        private readonly int _x;

        /// <summary>
        /// Internal Y value of point, for array index.
        /// </summary>
        private readonly int _y;

        /// <summary>
        /// Constructor for X and Y coordinates.
        /// </summary>
        /// <param name="xIn">X Coordinate of point, see <see cref="Xi"/></param>
        /// <param name="yIn">Y Coordinate of point, see <see cref="Yi"/></param>
        public SimplePoint(int xIn, int yIn)
        {
            _x = xIn;
            _y = yIn;
        }

        /// <summary>
        /// Opens <see cref="_x"/> to access through X.
        /// </summary>
        public int X
        {
            get
            {
                return _x;
            }
        }

        /// <summary>
        /// Opens <see cref="_y"/> to access through Y.
        /// </summary>
        public int Y
        {
            get
            {
                return _y;
            }
        }

        /// <summary>
        /// Sets the map size for the class.
        /// </summary>
        /// <param name="inXSize">Will become <see cref="_halfMapXSize"/>.</param>
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
                if (this._y > otherPoint._y)
                {
                    return 1;
                }
                if (this._y < otherPoint._y)
                {
                    return -1;
                }
                if (this._x > otherPoint._x)
                {
                    return 1;
                }
                if (this._x < otherPoint._x)
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
        /// Determines the points above and below this point, including wrap-arounds.
        /// </summary>
        /// <param name="abovePoint">The point above this point.</param>
        /// <param name="belowPoint">The point below this point.</param>
        public void FindAboveBelowPoints(out SimplePoint abovePoint, out SimplePoint belowPoint)
        {
            if (_y == 0)
            {
                if (_x >= _halfMapXSize)
                {
                    abovePoint = new SimplePoint(_x - _halfMapXSize, _y);
                }
                else
                {
                    abovePoint = new SimplePoint(_x + _halfMapXSize, _y);
                }
                belowPoint = new SimplePoint(_x, _y + 1);
            }
            else if (_y == _mapYSize - 1)
            {
                abovePoint = new SimplePoint(_x, _y - 1);
                if (_x >= _halfMapXSize)
                {
                    belowPoint = new SimplePoint(_x - _halfMapXSize, _y);
                }
                else
                {
                    belowPoint = new SimplePoint(_x + _halfMapXSize, _y);
                }
            }
            else
            {
                abovePoint = new SimplePoint(_x, _y - 1);
                belowPoint = new SimplePoint(_x, _y + 1);
            }
        }

        /// <summary>
        /// Determines the points left and right of this point, including wrap-arounds.
        /// </summary>
        /// <param name="leftPoint">The point to the left of this point.</param>
        /// <param name="rightPoint">The point to the right of this point.</param>
        public void FindLeftRightPoints(out SimplePoint leftPoint, out SimplePoint rightPoint)
        {
            if (_x == 0)
            {
                leftPoint = new SimplePoint(2 * _halfMapXSize - 1, _y);
                rightPoint = new SimplePoint(_x + 1, _y);
            }
            else if (_x == 2 * _halfMapXSize - 1)
            {
                leftPoint = new SimplePoint(_x - 1, _y);
                rightPoint = new SimplePoint(0, _y);
            }
            else
            {
                leftPoint = new SimplePoint(_x - 1, _y);
                rightPoint = new SimplePoint(_x + 1, _y);
            }
        }
    }
}