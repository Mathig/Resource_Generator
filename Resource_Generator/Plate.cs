using System.Collections.Generic;

namespace Resource_Generator
{
    /// <summary>
    /// Contains data and functions for plates.
    /// </summary>
    public class Plate
    {
        /// <summary>
        /// Two dimensional vector for rotation direction, in radians.
        /// </summary>
        public double[] Direction;

        /// <summary>
        /// Collection of points within this plate.
        /// </summary>
        public List<PlatePoint> PlatePoints;

        /// <summary>
        /// Magnitude of rotation per time, in radians per time unit.
        /// </summary>
        public double Speed;

        /// <summary>
        /// Allocates space for <see cref="Direction"/>
        /// </summary>
        public Plate()
        {
            Direction = new double[2];
            PlatePoints = new List<PlatePoint>();
        }
    }
}