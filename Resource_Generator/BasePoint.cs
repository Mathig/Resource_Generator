﻿using System;

namespace Resource_Generator
{
    /// <summary>
    /// Contains SimplePoint plus extra variables for calculating distances, sizes, and rotations.
    /// </summary>
    public readonly struct BasePoint : IPoint
    {
        /// <summary>
        /// Angular height of point in radians.
        /// </summary>
        private static double _dPhi;

        /// <summary>
        /// Angular width of point in radians.
        /// </summary>
        private static double _dTheta;

        /// <summary>
        /// Half the number of points per width of map.
        /// </summary>
        private static int _halfMapXSize;

        /// <summary>
        /// Number of points per height of map.
        /// </summary>
        private static int _mapYSize;

        /// <summary>
        /// Constant for converting between array index (YCoord) and phi.
        /// </summary>
        private static double _phiShift;

        /// <summary>
        /// Cosine of angular position of point. Phi ranges from -pi/2 to pi/2.
        /// </summary>
        private readonly double _cosPhi;

        /// <summary>
        /// Location of point.
        /// </summary>
        private readonly KeyPoint _position;

        /// <summary>
        /// Points near this point.
        /// </summary>
        public AdjacentPoints Near { get; }

        /// <summary>
        /// Angular position of point, ranges from 0 to 2 pi.
        /// </summary>
        private readonly double _theta;

        /// <summary>
        /// Sine of angular position of point. Phi ranges from -pi/2 to pi/2.
        /// </summary>
        public readonly double _sinPhi;

        /// <summary>
        /// Constructor for the Base Point. Takes in X and Y coordinates.
        /// </summary>
        /// <param name="inX">X coordinate for point.</param>
        /// <param name="inY">Y coordinate for point.</param>
        public BasePoint(int inX, int inY)
        {
            _position = new KeyPoint(inX, inY);
            Near = new AdjacentPoints(_position);
            _theta = _position.X * _dTheta;
            _cosPhi = Math.Cos((_position.Y * _dPhi) + _phiShift);
            _sinPhi = Math.Sin((_position.Y * _dPhi) + _phiShift);
        }

        /// <summary>
        /// Clone Constructor for the Base Point.
        /// </summary>
        /// <param name="inPoint">Input Point.</param>
        public BasePoint(BasePoint inPoint)
        {
            _position = inPoint._position;
            Near = inPoint.Near;
            _theta = inPoint._theta;
            _cosPhi = inPoint._cosPhi;
            _sinPhi = inPoint._sinPhi;
        }

        /// <summary>
        /// Constructor for the Base Point. Takes in a Simple Point structure.
        /// </summary>
        /// <param name="inPoint">Input Point.</param>
        public BasePoint(KeyPoint inPoint)
        {
            _position = inPoint;
            Near = new AdjacentPoints(_position);
            _theta = _position.X * _dTheta;
            _cosPhi = Math.Cos((_position.Y * _dPhi) + _phiShift);
            _sinPhi = Math.Sin((_position.Y * _dPhi) + _phiShift);
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
        /// Rotates a three dimensional point about z axis.
        /// </summary>
        /// <param name="xIn">Effective X Axis for rotation.</param>
        /// <param name="yIn">Effective Y Axis for rotation.</param>
        /// <param name="zIn">Effective Z Axis for rotation.</param>
        /// <param name="cosAngle">Cosine of angle to rotate by.</param>
        /// <param name="sinAngle">Sine of angle to rotate by.</param>
        /// <returns>Position of rotated point.</returns>
        private static double[] AxisRotation(double xIn, double yIn, double zIn, double cosAngle, double sinAngle)
        {
            double[] rotatedPoint =
            {
                xIn * cosAngle - yIn * sinAngle,
                xIn * sinAngle + yIn * cosAngle,
                zIn
            };
            return rotatedPoint;
        }

        /// <summary>
        /// Rotates a point about 3 axis in 3D Cartesian Coordinates.
        /// </summary>
        /// <param name="pointIn">Point to be rotated.</param>
        /// <param name="angle">Amount to rotate about axis.</param>
        /// <returns>Point rotated about 3 axis.</returns>
        private static double[] CartesianRotation(double[] pointIn, double[] angle)
        {
            var cosAngleX = Math.Cos(angle[0]);
            var sinAngleX = Math.Sin(angle[0]);
            var cosAngleY = Math.Cos(angle[1]);
            var sinAngleY = Math.Sin(angle[1]);
            var pointOut = pointIn;//x y z
            pointOut = new double[] { pointOut[1], pointOut[2], pointOut[0] };//y z x
            pointOut = AxisRotation(pointOut[0], pointOut[1], pointOut[2], cosAngleX, sinAngleX);//Rotate about x
            pointOut = new double[] { pointOut[1], pointOut[2], pointOut[0] };//z x y
            pointOut = AxisRotation(pointOut[0], pointOut[1], pointOut[2], cosAngleY, sinAngleY);
            pointOut = new double[] { pointOut[1], pointOut[2], pointOut[0] };//x y z
            pointOut = AxisRotation(pointOut[0], pointOut[1], pointOut[2], Math.Cos(angle[2]), Math.Sin(angle[2]));
            pointOut = new double[] { pointOut[2], pointOut[0], pointOut[1] };//z x y
            pointOut = AxisRotation(pointOut[0], pointOut[1], pointOut[2], cosAngleY, -1 * sinAngleY);
            pointOut = new double[] { pointOut[2], pointOut[0], pointOut[1] };//y z x
            pointOut = AxisRotation(pointOut[0], pointOut[1], pointOut[2], cosAngleX, -1 * sinAngleX);
            pointOut = new double[] { pointOut[2], pointOut[0], pointOut[1] };//x y z
            return pointOut;
        }

        /// <summary>
        /// Rotates a point about 3 axis in 3D Spherical Coordinates.
        /// </summary>
        /// <param name="thetaIn">Input theta angle.</param>
        /// <param name="cosPhiIn">Cosine of input phi angle.</param>
        /// <param name="sinPhiIn">Sine of input phi angle.</param>
        /// <param name="angle">Angle vector to be rotated.</param>
        /// <returns>Theta and phi of rotated angle. Theta (0,2Pi], Phi (-Pi/2,Pi/2].</returns>
        private static double[] SphericalRotation(double thetaIn, double cosPhiIn, double sinPhiIn, double[] angle)
        {
            var pointCartesian = new double[3];

            pointCartesian[0] = Math.Cos(thetaIn) * cosPhiIn;
            pointCartesian[1] = Math.Sin(thetaIn) * cosPhiIn;
            pointCartesian[2] = sinPhiIn;

            pointCartesian = CartesianRotation(pointCartesian, angle);

            var pointSpherical = new double[2];

            pointSpherical[0] = (Math.Atan2(-1 * pointCartesian[1], -1 * pointCartesian[0]) + Math.PI);
            pointSpherical[1] = Math.Asin(pointCartesian[2]);
            return pointSpherical;
        }

        /// <summary>
        /// Sets static variables for Base Point and Simple Point.
        /// </summary>
        /// <param name="inHalfXSize">Will become <see cref="_halfMapXSize"/>.</param>
        /// <param name="inYSize">Will become <see cref="_mapYSize"/>.</param>
        public static void MapSetup(int inHalfXSize, int inYSize)
        {
            AdjacentPoints.MapSize(inHalfXSize, inYSize);
            _halfMapXSize = inHalfXSize;
            _mapYSize = inYSize;
            _dTheta = Math.PI / inHalfXSize;
            _dPhi = Math.PI / inYSize;
            _phiShift = 0.5 * _dPhi - (Math.PI / 2);
        }

        /// <summary>
        /// Compares two BasePoints using the SimplePoint structure.
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
            if (obj is BasePoint otherPoint)
            {
                return _position.CompareTo(otherPoint._position);
            }
            else
            {
                throw new ArgumentException("Object is not a BasePoint.");
            }
        }

        /// <summary>
        /// Calculates distance between this point and the input point. Uses approximation for cosine for small angles.
        /// </summary>
        /// <param name="iPoint">Second point used for calculating distance.</param>
        /// <returns>Distance between the two points.</returns>
        public double Distance(in BasePoint iPoint)
        {
            var xDif = Math.Abs(iPoint._position.X - _position.X);
            if (xDif > _halfMapXSize)
            {
                xDif = 2 * _halfMapXSize - xDif;
            }
            var cosDif = xDif < _halfMapXSize / 6
                ? 1 - (0.5 * xDif * xDif * _dTheta * _dTheta)
                : Math.Cos(xDif * _dTheta);
            return Math.Abs(2 * (1 - _sinPhi * iPoint._sinPhi - _cosPhi * iPoint._cosPhi * cosDif));
        }

        /// <summary>
        /// Calculates distance between this point and the input coordinates. Uses approximation for cosine for small angles.
        /// </summary>
        /// <param name="xCoord">X coordinate of input point.</param>
        /// <param name="yCoord">Y coordinate of input point.</param>
        /// <returns>Distance between the two points.</returns>
        public double Distance(double xCoord, double yCoord)
        {
            var xDif = Math.Abs(xCoord - X);
            if (xDif > _halfMapXSize)
            {
                xDif = 2 * _halfMapXSize - xDif;
            }
            var cosDif = xDif < _halfMapXSize / 6
                ? 1 - (0.5 * xDif * xDif * _dTheta * _dTheta)
                : Math.Cos(xDif * _dTheta);
            var otherCosPhi = Math.Cos((yCoord * _dPhi) + _phiShift);
            var otherSinPhi = Math.Sin((yCoord * _dPhi) + _phiShift);

            return Math.Abs(2 * (1 - _sinPhi * otherSinPhi - _cosPhi * otherCosPhi * cosDif));
        }

        /// <summary>
        /// Calculates new position of point for a given rotation.
        /// </summary>
        /// <param name="angle">Three dimensional angle for rotation, given in radians.</param>
        /// <param name="xCoord">X coordinate of new position.</param>
        /// <param name="yCoord">Y coordinate of new position.</param>
        /// <returns>New position given as a grid of x and y coordinates.</returns>
        public void GridTransform(double[] angle, out double xCoord, out double yCoord)
        {
            var t1 = X;
            var t2 = Y;
            var doubleOutput = SphericalRotation(_theta, _cosPhi, _sinPhi, angle);

            xCoord = doubleOutput[0] / _dTheta;
            yCoord = (doubleOutput[1] - _phiShift) / _dPhi;
            return;
        }

        /// <summary>
        /// Calculates boundaries for rectangular approximation of points in range of this point.
        /// </summary>
        /// <param name="range">Distance from center point.</param>
        /// <param name="xMin">Minimum X boundary, adding 2*<see cref="_mapSize"/> if range wraps around border.</param>
        /// <param name="xMax">Maximum X boundary, adding 2*<see cref="_mapSize"/> if range wraps around border.</param>
        /// <param name="yMin">Minimum Y boundary.</param>
        /// <param name="yMax">Maximum Y boundary.</param>
        public void Range(double range, out int xMin, out int xMax, out int yMin, out int yMax)
        {
            var zMin = _sinPhi - range;
            if (zMin < -1)
            {
                zMin = -1;
            }
            var zMax = _sinPhi + range;
            if (zMax > 1)
            {
                zMax = 1;
            }
            var rDif = range / _cosPhi;
            if (rDif > 1)
            {
                rDif = 1;
            }
            xMin = X - (int)(rDif * _halfMapXSize);
            xMax = X + (int)(rDif * _halfMapXSize);
            if (xMin <= 0)
            {
                xMin += 2 * _halfMapXSize;
                xMax += 2 * _halfMapXSize;
            }
            yMin = (int)Math.Round((Math.Asin(zMin) - _phiShift) / _dPhi);
            if (yMin < 0)
            {
                yMin = 0;
            }
            yMax = (int)Math.Round((Math.Asin(zMax) - _phiShift) / _dPhi);
            if (yMax >= _mapYSize)
            {
                yMax = _mapYSize - 1;
            }
        }

        /// <summary>
        /// Tests the momentum and returns true if the momentum exceeds the area corrected threshold.
        /// </summary>
        /// <param name="momentum">Momentum to test.</param>
        /// <param name="threshold">Uncorrected threshold to test for.</param>
        /// <returns>True if test succeeds, false otherwise.</returns>
        public bool TestMomentum(double momentum, double threshold)
        {
            if (momentum > Math.Pow(threshold, _cosPhi))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates new position of point for a given rotation.
        /// </summary>
        /// <param name="angle">Three dimensional angle for rotation, given in radians.</param>
        /// <returns>New position given as a Simple Point.</returns>
        public KeyPoint Transform(double[] angle)
        {
            GridTransform(angle, out double xDCoord, out double yDCoord);

            var xCoord = (int)(Math.Round(xDCoord));
            if (xCoord == 2 * _halfMapXSize)
            {
                xCoord = 0;
            }
            var yCoord = (int)(Math.Round(yDCoord));
            if (yCoord == _mapYSize)
            {
                yCoord = 0;
            }
            return new KeyPoint(xCoord, yCoord);
        }
    }
}