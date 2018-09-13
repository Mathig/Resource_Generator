namespace Resource_Generator
{
    public readonly struct AdjacentPoints
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
        /// Constructor for set of adjacent points.
        /// </summary>
        /// <param name="inPoint">Point to find adjacent points for.</param>
        public AdjacentPoints(in KeyPoint inPoint)
        {
            Points = new KeyPoint[4]
            {
                FindAbovePoint(inPoint),
                FindBelowPoint(inPoint),
                FindLeftPoint(inPoint),
                FindRightPoint(inPoint)
            };
        }

        /// <summary>
        /// Constructor for set of adjacent points.
        /// </summary>
        /// <param name="inX">X coordinate of center point.</param>
        /// <param name="inY">Y coordinate of center point.</param>
        public AdjacentPoints(int inX, int inY)
        {
            var inPoint = new KeyPoint(inX, inY);
            Points = new KeyPoint[4]
            {
                FindAbovePoint(inPoint),
                FindBelowPoint(inPoint),
                FindLeftPoint(inPoint),
                FindRightPoint(inPoint)
            };
        }

        /// <summary>
        /// Returns the above point.
        /// </summary>
        public KeyPoint Above => Points[0];

        /// <summary>
        /// Returns the below point.
        /// </summary>
        public KeyPoint Below => Points[1];

        /// <summary>
        /// Returns the left point.
        /// </summary>
        public KeyPoint Left => Points[2];

        /// <summary>
        /// Contains list of nearest points.
        /// </summary>
        public KeyPoint[] Points { get; }

        /// <summary>
        /// Returns the right point.
        /// </summary>
        public KeyPoint Right => Points[3];

        /// <summary>
        /// Finds the point above the input point.
        /// </summary>
        /// <param name="inPoint">Input point.</param>
        /// <returns>Point above the input point.</returns>
        private static KeyPoint FindAbovePoint(KeyPoint inPoint)
        {
            return inPoint.Y == 0
                ? new KeyPoint(XWrap(inPoint.X), inPoint.Y)
                : new KeyPoint(inPoint.X, inPoint.Y - 1);
        }

        /// <summary>
        /// Finds the point above the input point.
        /// </summary>
        /// <param name="inPoint">Input point.</param>
        /// <returns>Point below the input point.</returns>
        private static KeyPoint FindBelowPoint(KeyPoint inPoint)
        {
            return inPoint.Y == _mapYSize - 1
                ? new KeyPoint(XWrap(inPoint.X), inPoint.Y)
                : new KeyPoint(inPoint.X, inPoint.Y + 1);
        }

        /// <summary>
        /// Finds the point left of the input point.
        /// </summary>
        /// <param name="inPoint">Input point.</param>
        /// <returns>Point left of the input point.</returns>
        private static KeyPoint FindLeftPoint(KeyPoint inPoint)
        {
            return new KeyPoint(inPoint.X == 0
                ? 2 * _halfMapXSize - 1
                : inPoint.X - 1
                , inPoint.Y);
        }

        /// <summary>
        /// Finds the point left of the input point.
        /// </summary>
        /// <param name="inPoint">Input point.</param>
        /// <returns>Point left of the input point.</returns>
        private static KeyPoint FindRightPoint(KeyPoint inPoint)
        {
            return new KeyPoint(inPoint.X == 2 * _halfMapXSize - 1
                ? 0
                : inPoint.X + 1
                , inPoint.Y);
        }

        /// <summary>
        /// Finds the x value for wraping around the vertical boundary conditions, ie the top or
        /// bottom poles.
        /// </summary>
        /// <param name="X">X coordinate to check for wrapping.</param>
        /// <returns>The x value for a point that wraps around the boundary conditions.</returns>
        private static int XWrap(int X)
        {
            return X >= _halfMapXSize ? (X - _halfMapXSize) : (X + _halfMapXSize);
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
    }
}