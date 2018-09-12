using System;
using System.Collections.Generic;

namespace Resource_Generator
{
    /// <summary>
    /// Contains data for points to be used to represent tectonic plates.
    /// </summary>
    public class PlatePoint : IComparable, IPoint
    {
        /// <summary>
        /// The position of this point.
        /// </summary>
        private BasePoint point;

        /// <summary>
        /// Indicates when this point was initially generated.
        /// </summary>
        public int _birthDate;

        /// <summary>
        /// Indicates where this point was initially generated.
        /// </summary>
        public SimplePoint _birthPlace;

        /// <summary>
        /// Contains the boundary history of this point.
        /// </summary>
        public BoundaryHistory History;

        /// <summary>
        /// Indicates whether the point is continental crust or oceanic crust.
        /// </summary>
        public bool IsContinental;

        /// <summary>
        /// Indicates which plate this point is part of.
        /// </summary>
        public int PlateNumber;

        /// <summary>
        /// Constructor for plate generation for saved data.
        /// </summary>
        /// <param name="inPosition">Current position for point.</param>
        /// <param name="inBirthPlace">Birthplace for point.</param>
        /// <param name="inBirthDate">Birthdate for point.</param>
        /// <param name="inPlate">Which plate point is part of.</param>
        /// <param name="inHistory">Boundary history for point.</param>
        /// <param name="inIsContinental">Whether or not the point is continental or oceanic.</param>
        public PlatePoint(SimplePoint inPosition, SimplePoint inBirthPlace, int inBirthDate, int inPlate, BoundaryHistory inHistory, bool inIsContinental)
        {
            point = new BasePoint(inPosition);
            _birthPlace = inBirthPlace;
            _birthDate = inBirthDate;
            PlateNumber = inPlate;
            History = inHistory;
            IsContinental = inIsContinental;
        }

        /// <summary>
        /// Constructor for plate expansion.
        /// </summary>
        /// <param name="inPoint">Input point.</param>
        public PlatePoint(OverlapPoint inPoint, int inBirthDate = 0)
        {
            _birthPlace = new SimplePoint(inPoint.X, inPoint.Y);
            point = new BasePoint(_birthPlace);
            _birthDate = inBirthDate;
            PlateNumber = inPoint.plateIndex[0];
            IsContinental = false;
            History = new BoundaryHistory();
        }

        /// <summary>
        /// Constructor for Plate Point.
        /// </summary>
        /// <param name="point">Point to copy.</param>
        public PlatePoint(PlatePoint inPoint)
        {
            point = new BasePoint(inPoint.point);
            PlateNumber = inPoint.PlateNumber;
            _birthDate = inPoint._birthDate;
            _birthPlace = inPoint._birthPlace;
            History = inPoint.History;
            IsContinental = inPoint.IsContinental;
        }

        /// <summary>
        /// Constructor for Plate Point.
        /// </summary>
        /// <param name="inPoint"> Input coordinates for new point.</param>
        /// <param name="inPlateNumber">Number for which plate this point is part of.</param>
        /// <param name="time">Time for point creation.</param>
        public PlatePoint(SimplePoint inPoint, int inPlateNumber = 0, int time = 0)
        {
            point = new BasePoint(inPoint);
            _birthDate = time;
            PlateNumber = inPlateNumber;
            _birthPlace = inPoint;
            History = new BoundaryHistory();
            IsContinental = false;
        }

        /// <summary>
        /// Opens <see cref="BasePoint.X"/> to access through X.
        /// </summary>
        public int X
        {
            get
            {
                return point.X;
            }
        }

        /// <summary>
        /// Opens <see cref="BasePoint.Y"/> to access through Y.
        /// </summary>
        public int Y
        {
            get
            {
                return point.Y;
            }
        }

        /// <summary>
        /// Finds all points within range of the given point.
        /// </summary>
        /// <param name="leftPoint">Left point to check from.</param>
        /// <param name="rightPoint">Right point to check to.</param>
        /// <param name="centerPoint">Center point to avoid.</param>
        /// <returns>List of points in range excluding the center point.</returns>
        private static List<SimplePoint> FindAllPoints(BasePoint leftPoint, BasePoint rightPoint, SimplePoint centerPoint)
        {
            List<SimplePoint> listPoints = new List<SimplePoint>();
            List<int> validXs = new List<int>();
            SimplePoint nextPoint = new SimplePoint(leftPoint.X, leftPoint.Y);
            while (nextPoint.X != rightPoint.X)
            {
                nextPoint.FindLeftRightPoints(out _, out nextPoint);
                validXs.Add(nextPoint.X);
            }
            if (validXs.Count < 2)
            {
                return listPoints;
            }
            int newY = centerPoint.Y;
            for (int x = 0; x < validXs.Count - 1; x++)
            {
                if (validXs[x] != centerPoint.X)
                {
                    listPoints.Add(new SimplePoint(validXs[x], newY));
                }
            }
            return listPoints;
        }

        /// <summary>
        /// Resolves multiple points of the same plate.
        /// </summary>
        /// <param name="points">Other points to resolve.</param>
        public static PlatePoint ResolveSamePlateNeighbors(List<PlatePoint> inPoints, double xCoord, double yCoord, SimplePoint newPosition, int newPlate)
        {
            bool isPureOceanic = true;
            List<PlatePoint> continentalList = new List<PlatePoint>();
            foreach (PlatePoint iPoint in inPoints)
            {
                if (iPoint.IsContinental)
                {
                    isPureOceanic = false;
                    continentalList.Add(iPoint);
                }
            }
            double weightTotal = 0;
            double xBirth = 0;
            double yBirth = 0;
            double birthDate = 0;
            double[] historyVars = new double[4];
            if (isPureOceanic)
            {
                foreach (PlatePoint iPoint in inPoints)
                {
                    double weight = iPoint.point.Distance(xCoord, yCoord);
                    weightTotal += 1 / weight;
                    xBirth += iPoint._birthPlace.X / weight;
                    yBirth += iPoint._birthPlace.Y / weight;
                    historyVars[0] += iPoint.History.ContinentalBuildup / weight;
                    historyVars[1] += iPoint.History.ContinentalRecency / weight;
                    historyVars[2] += iPoint.History.OceanicBuildup / weight;
                    historyVars[3] += iPoint.History.OceanicRecency / weight;
                    birthDate += iPoint._birthDate / weight;
                }
            }
            else
            {
                foreach (PlatePoint iPoint in continentalList)
                {
                    double weight = iPoint.point.Distance(xCoord, yCoord);
                    weightTotal += 1 / weight;
                    xBirth += iPoint._birthPlace.X / weight;
                    yBirth += iPoint._birthPlace.Y / weight;
                    historyVars[0] += iPoint.History.ContinentalBuildup / weight;
                    historyVars[1] += iPoint.History.ContinentalRecency / weight;
                    historyVars[2] += iPoint.History.OceanicBuildup / weight;
                    historyVars[3] += iPoint.History.OceanicRecency / weight;
                    birthDate += iPoint._birthDate / weight;
                }
            }
            xBirth = xBirth / weightTotal;
            yBirth = yBirth / weightTotal;
            historyVars[0] = historyVars[0] / weightTotal;
            historyVars[1] = historyVars[1] / weightTotal;
            historyVars[2] = historyVars[2] / weightTotal;
            historyVars[3] = historyVars[3] / weightTotal;
            SimplePoint newBirthPoint = new SimplePoint((int)Math.Round(xBirth), (int)Math.Round(yBirth));
            BoundaryHistory newHistory = new BoundaryHistory((int)Math.Round(historyVars[0]), (int)Math.Round(historyVars[1]), (int)Math.Round(historyVars[2]), (int)Math.Round(historyVars[3]));
            int newBirthDate = (int)Math.Round(birthDate / weightTotal);
            return new PlatePoint(newPosition, newBirthPoint, newBirthDate, newPlate, newHistory, !isPureOceanic);
        }

        /// <summary>
        /// Compares two PlatePoints using the SimplePoint structure.
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
            if (obj is PlatePoint otherPoint)
            {
                return point.CompareTo(otherPoint.point);
            }
            else
            {
                throw new ArgumentException("Object is not a PlatePoint.");
            }
        }

        /// <summary>
        /// Calculates distance between this point and the input point. Uses approximation for cosine for small angles.
        /// </summary>
        /// <param name="iPoint">Second point used for calculating distance.</param>
        /// <returns>Distance between the two points.</returns>
        public double Distance(PlatePoint inPlatePoint)
        {
            return point.Distance(inPlatePoint.point);
        }

        /// <summary>
        /// Returns neighboring points in an array ordered as above, below, left, then right.
        /// </summary>
        /// <returns>Neighbor points.</returns>
        public SimplePoint[] FindNeighborPoints()
        {
            return point.FindNeighborPoints();
        }

        /// <summary>
        /// Determines the points left and right of this point, including wrap-arounds.
        /// </summary>
        /// <param name="leftPoint">The point to the left of this point.</param>
        /// <param name="rightPoint">The point to the right of this point.</param>
        public void FindLeftRightPoints(out BasePoint leftPoint, out BasePoint rightPoint)
        {
            point.FindLeftRightPoints(out SimplePoint sleftPoint, out SimplePoint srightPoint);
            leftPoint = new BasePoint(sleftPoint);
            rightPoint = new BasePoint(srightPoint);
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
            point.Range(range, out xMin, out xMax, out yMin, out yMax);
        }

        /// <summary>
        /// Resolves multiple points of the same plate.
        /// </summary>
        /// <param name="points">Other points to resolve.</param>
        public void ResolveSamePlateOverlap(List<PlatePoint> points)
        {
            int count = 0;
            int xBirth = 0;
            int yBirth = 0;
            if (IsContinental)
            {
                count++;
                xBirth += _birthPlace.X;
                yBirth += _birthPlace.Y;
            }
            foreach (PlatePoint iPoint in points)
            {
                if (iPoint.IsContinental)
                {
                    IsContinental = true;
                    count++;
                    _birthDate += iPoint._birthDate;
                    xBirth += iPoint._birthPlace.X;
                    yBirth += iPoint._birthPlace.Y;
                    History += iPoint.History;
                }
            }
            if (count > 1)
            {
                _birthDate = _birthDate / count;
                xBirth = (int)(Math.Round(xBirth / (double)count));
                yBirth = (int)(Math.Round(yBirth / (double)count));
                History = History / count;
            }
            if (count != 0)
            {
                _birthPlace = new SimplePoint(xBirth, yBirth);
            }
        }

        /// <summary>
        /// Moves point by a given rotation, returning bonus points if necessary.
        /// </summary>
        /// <param name="angle">Three dimensional angle for rotation, given in radians.</param>
        /// <param name="bonusPoints">Additional points to add.</param>
        /// <returns>True if there are bonus points, otherwise false.</returns>
        public bool Transform(double[] angle, out List<SimplePoint> bonusPoints, int ySize)
        {
            bonusPoints = new List<SimplePoint>();
            SimplePoint tempPoint = point.Transform(angle);
            if (tempPoint.Y == Y)
            {
                point = new BasePoint(tempPoint);
                return false;
            }
            int yVar = Math.Abs(ySize / 2 - tempPoint.Y) - Math.Abs(ySize / 2 - Y);
            if (yVar < 0)
            {
                point = new BasePoint(tempPoint);
                return false;
            }
            FindLeftRightPoints(out BasePoint leftPoint, out BasePoint rightPoint);
            leftPoint = new BasePoint(leftPoint.Transform(angle));
            rightPoint = new BasePoint(rightPoint.Transform(angle));
            leftPoint.FindLeftRightPoints(out _, out SimplePoint leftValue);
            rightPoint.FindLeftRightPoints(out SimplePoint rightValue, out _);
            if (tempPoint.CompareTo(leftValue) == 0 && tempPoint.CompareTo(rightValue) == 0)
            {
                point = new BasePoint(tempPoint);
                return false;
            }
            bonusPoints = FindAllPoints(leftPoint, rightPoint, tempPoint);
            if (bonusPoints.Count > 0)
            {
                point = new BasePoint(tempPoint);
                return true;
            }
            point = new BasePoint(tempPoint);
            return false;
        }
    }
}