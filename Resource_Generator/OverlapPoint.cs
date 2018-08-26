using System;
using System.Collections.Generic;

namespace Resource_Generator
{
    /// <summary>
    /// A point that contains extra integers for resolving overlaps.
    /// </summary>
    public class OverlapPoint : IComparable, IPoint
    {
        /// <summary>
        /// Stores the location of this point.
        /// </summary>
        private readonly SimplePoint _position;

        /// <summary>
        /// Contains indexes for which points are continental.
        /// </summary>
        public List<bool> isContinentalIndex;

        /// <summary>
        /// Contains list of indexes for which plates are overlapped.
        /// </summary>
        public List<int> plateIndex;

        /// <summary>
        /// Constructor that sets the position and initial indexes.
        /// </summary>
        /// <param name="iPoint">Input Point.</param>
        public OverlapPoint(PlatePoint iPoint)
        {
            _position = new SimplePoint(iPoint.X, iPoint.Y);
            plateIndex = new List<int>();
            isContinentalIndex = new List<bool>();
            plateIndex.Add(iPoint.PlateNumber);
            isContinentalIndex.Add(iPoint.IsContinental);
        }

        /// <summary>
        /// Constructor that sets the position and initial index.
        /// </summary>
        /// <param name="inPoint">Coordinates of point.</param>
        /// <param name="inPlateIndex">First index for plate index.</param>
        public OverlapPoint(SimplePoint inPoint, int inPlateIndex)
        {
            plateIndex = new List<int>();
            _position = inPoint;
            plateIndex.Add(inPlateIndex);
        }

        /// <summary>
        /// Opens <see cref="SimplePoint.X"/> to access through X.
        /// </summary>
        public int X
        {
            get
            {
                return _position.X;
            }
        }

        /// <summary>
        /// Opens <see cref="SimplePoint.Y"/> to access through Y.
        /// </summary>
        public int Y
        {
            get
            {
                return _position.Y;
            }
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
            if (obj is OverlapPoint otherPoint)
            {
                return _position.CompareTo(otherPoint._position);
            }
            else
            {
                throw new ArgumentException("Object is not an Overlap Point.");
            }
        }

        /// <summary>
        /// Determines the points above and below this point, including wrap-arounds.
        /// </summary>
        /// <param name="abovePoint">The point above this point.</param>
        /// <param name="belowPoint">The point below this point.</param>
        public void FindAboveBelowPoints(out SimplePoint abovePoint, out SimplePoint belowPoint)
        {
            _position.FindAboveBelowPoints(out abovePoint, out belowPoint);
        }

        /// <summary>
        /// Determines the points left and right of this point, including wrap-arounds.
        /// </summary>
        /// <param name="leftPoint">The point to the left of this point.</param>
        /// <param name="rightPoint">The point to the right of this point.</param>
        public void FindLeftRightPoints(out SimplePoint leftPoint, out SimplePoint rightPoint)
        {
            _position.FindLeftRightPoints(out leftPoint, out rightPoint);
        }

        /// <summary>
        /// Tests to see if all points are oceanic. Returns the index of the first continental point if false.
        /// </summary>
        /// <param name="index">Index of first continental point.</param>
        /// <returns>True if only oceanic, otherwise false.</returns>
        public bool IsOceanicOnly(out int index)
        {
            for (int i = 0; i < isContinentalIndex.Count; i++)
            {
                if (isContinentalIndex[i])
                {
                    index = i;
                    return false;
                }
            }
            index = 0;
            return true;
        }
    }
}