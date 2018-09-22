using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resource_Generator
{
    /// <summary>
    /// Used for dendritic growth.
    /// </summary>
    public class DendriticPoint :IPoint, IEquatable<DendriticPoint>
    {
        /// <summary>
        /// Multiplier for step movement.
        /// </summary>
        private const int _stepMultiplier = 5;

        /// <summary>
        /// Stores the location of this point.
        /// </summary>
        public KeyPoint position;

        /// <summary>
        /// Contains direction and information for dendritic growth.
        /// </summary>
        private readonly DendriticShape dendriticShape;

        /// <summary>
        /// Contains direction for dendritic point growth.
        /// </summary>
        private readonly double[] dendriticAngle;

        /// <summary>
        /// Constructor for new Dendritic Point.
        /// </summary>
        /// <param name="inDendriticShape"></param>
        /// <param name="inPosition"></param>
        /// <param name="randomNumber">Random Number used for generating direction.</param>
        public DendriticPoint(DendriticShape inDendriticShape, KeyPoint inPosition, double randomNumber)
        {
            dendriticShape = inDendriticShape;
            position = inPosition;
            var directionIndex = (int)(DendriticShape.DirectionCount * randomNumber);
            if (directionIndex >= DendriticShape.DirectionCount)
            {
                directionIndex = DendriticShape.DirectionCount - 1;
            }
            dendriticAngle = dendriticShape.Direction[directionIndex];
        }
        /// <summary>
        /// Step size to take for step function.
        /// </summary>
        private static double _stepSize;

        /// <summary>
        /// Maximum number of steps to take for step function.
        /// </summary>
        private static int _stepCount;

        /// <summary>
        /// Sets up Dendritic Point class's static variables using <see cref="GenerateRules"/>.
        /// </summary>
        /// <param name="rules">Rules to set up with.</param>
        public static void Setup(GenerateRules rules)
        {
            _stepCount = rules.xHalfSize > rules.ySize
                ? _stepMultiplier * 2 * rules.xHalfSize
                : _stepMultiplier * 2 * rules.ySize;
            _stepSize = _stepMultiplier * Math.PI / _stepCount;
            _pointCount = rules.DendritePointCount;
            _stepThreshold = rules.DendriticStepThreshold;
            DendriticShape.MapSetup(rules.xHalfSize, rules.ySize, rules.DendriteDirectionCount);
        }

        /// <summary>
        /// Points to touch for step function.
        /// </summary>
        private static int _pointCount;

        /// <summary>
        /// Multiplier for step count.
        /// </summary>
        private static double _stepThreshold;

        /// <summary>
        /// Allows access of X through position.
        /// </summary>
        public int X => (position).X;
        /// <summary>
        /// Allows access of Y through position.
        /// </summary>
        public int Y => (position).Y;

        /// <summary>
        /// Removes duplicate entries from list of clean points, including previous Dendritic Point.
        /// </summary>
        /// <param name="tracedPoints">List of points to clean.</param>
        private void CleanPointList(List<KeyPoint> tracedPoints)
        {
            tracedPoints = tracedPoints.Distinct().ToList();
            for (int i = 0; i < tracedPoints.Count; i++)
            {
                if (tracedPoints[i].Equals(position))
                {
                    tracedPoints.RemoveAt(i);
                    break;
                }
            }
            if (tracedPoints.Count == 0) //Step should always move at least a little.
            {
                throw new ArgumentException("StepSize is too small.");
            }
        }

        /// <summary>
        /// Traces out points to generate a list of traced points.
        /// </summary>
        /// <returns>List of traced out points.</returns>
        private List<KeyPoint> TracePoints()
        {
            var tracedPoints = new List<KeyPoint>();
            var basePoint = new BasePoint(position);
            var index = 0;
            tracedPoints.Add(position);
            for (int i = 0; i < _stepCount; i++)
            {
                var thisAngle = new double[3]
                {
                    dendriticAngle[0],
                    dendriticAngle[1],
                    _stepSize * (i+1),
                };
                var newPoint = basePoint.Transform(thisAngle);
                if (!newPoint.Equals(tracedPoints[index]))
                {
                    tracedPoints.Add(newPoint);
                    index++;
                }
                if (index > _pointCount)
                {
                    break;
                }
            }
            CleanPointList(tracedPoints);
            return tracedPoints;
        }
        /// <summary>
        /// Spawns new Dendritic point if random number exceeds <see cref="_stepThreshold"/>.
        /// </summary>
        /// <param name="tracedPoints">List of traced points to add from.</param>
        /// <param name="randomNumber">Random Number.</param>
        /// <returns>New Dendritic points.</returns>
        private List<DendriticPoint> SpawnNewPoints(List<KeyPoint> tracedPoints, Random randomNumber)
        {
            if (tracedPoints.Count == 0) //We need at least 1 remaining point to spawn a new point.
            {
                return null;
            }
            var output = new List<DendriticPoint>();
            foreach (var iPoint in tracedPoints)
            {
                if (randomNumber.NextDouble() > _stepThreshold)
                {
                    output.Add(new DendriticPoint(dendriticShape, iPoint, randomNumber.NextDouble()));
                }
            }
            foreach (var iPoint in output)
            {
                foreach (var jPoint in tracedPoints)
                {
                    if (jPoint.Equals(iPoint.position))
                    {
                        tracedPoints.Remove(jPoint);
                        break;
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Steps a Dendritic point creating keypoints and new dendritic points in the process.
        /// </summary>
        /// <param name="randomNumber">Random number to used in selecting new points.</param>
        /// <param name="newPoints">New Dendritic point to create.</param>
        /// <returns>List of points traced over, excluding new dendritic point.</returns>
        public List<KeyPoint> Step(Random randomNumber, out List<DendriticPoint> newPoints)
        {
            if (_pointCount < 1) //We need to take at least one step.
            {
                throw new ArgumentException("StepCount is too small.");
            }
            var tracedPoints = TracePoints();
            position = tracedPoints[tracedPoints.Count - 1];
            tracedPoints.RemoveAt(tracedPoints.Count - 1);
            newPoints = SpawnNewPoints(tracedPoints, randomNumber);
            return tracedPoints;
        }

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
            return (position).CompareTo(obj);
        }
        /// <summary>
        /// Compares equality through the position.
        /// </summary>
        /// <param name="other">Other point to compare with.</param>
        /// <returns>True of points are equal, otherwise false.</returns>
        public bool Equals(DendriticPoint other)
        {
            return this.position.Equals(other.position);
        }
    }
    /// <summary>
    /// Used to indicate direction in dendritic growth.
    /// </summary>
    public class DendriticShape
    {
        /// <summary>
        /// Array of directions
        /// </summary>
        public readonly double[][] Direction;

        /// <summary>
        /// Gets the number of directions in this point.
        /// </summary>
        public static int DirectionCount;

        /// <summary>
        /// Angular height of point in radians.
        /// </summary>
        private static double _dPhi;

        /// <summary>
        /// Angular width of point in radians.
        /// </summary>
        private static double _dTheta;

        /// <summary>
        /// Constant for converting between array index (YCoord) and phi.
        /// </summary>
        private static double _phiShift;

        /// <summary>
        /// Sets static variables for Base Point and Simple Point.
        /// </summary>
        /// <param name="inHalfXSize">Half size of map in X direction.</param>
        /// <param name="inYSize">Size of map in Y direction.</param>
        /// <param name="inDirectionCount">How many directions are in this point.</param>
        public static void MapSetup(int inHalfXSize, int inYSize, int inDirectionCount)
        {
            DirectionCount = inDirectionCount;
            _dTheta = Math.PI / inHalfXSize;
            _dPhi = Math.PI / inYSize;
            _phiShift = 0.5 * _dPhi - (Math.PI / 2);
        }

        public DendriticShape(KeyPoint centerPoint)
        {
            var basePoint = new BasePoint(centerPoint);
            var angle = new double[3]
            {
                0,
                0,
                0.5 * Math.PI,
            };
            basePoint = new BasePoint(basePoint.Transform(angle));
            Direction = new double[DirectionCount][];
            angle[0] = centerPoint.X * _dTheta;
            angle[1] = (centerPoint.Y * _dPhi) + _phiShift;
            for (int i = 0; i < DirectionCount; i++)
            {
                angle[2] = i * 2 * Math.PI / DirectionCount;
                var sidePoint = basePoint.Transform(angle);
                Direction[i] = new double[2]
                {
                    sidePoint.X * _dTheta,
                    (sidePoint.Y * _dPhi) + _phiShift,
                };
            }
        }
    }
}
