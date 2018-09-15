using System;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Class for containing plate data.
    /// </summary>
    [XmlRoot("Plate_Data", IsNullable = false)]
    public class PlateData
    {
        /// <summary>
        /// Contains data on plates direction.
        /// Sorted by plate number first, then direction type.
        /// Ie: Direction[i][0] or Direction[i][1].
        /// </summary>
        public double[][] Direction;

        /// <summary>
        /// Contains data on plates speed.
        /// </summary>
        public double[] Speed;

        /// <summary>
        /// Loads default values.
        /// </summary>
        public void Default()
        {
            Direction = new double[10][];
            Speed = new double[10];

            for (int i = 0; i < 10; i++)
            {
                Direction[i] = new double[2];
                var angleOne = i * Math.PI / 4.5;
                angleOne = Math.Round(angleOne, 3);
                Speed[i] = 0.02;
                Direction[i][0] = angleOne;
                Direction[i][1] = angleOne;
            }
        }
    }
}