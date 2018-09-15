using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Contains rules for Plate Generation.
    /// </summary>
    [XmlRoot("Generate_Rules", IsNullable = false)]
    public class GenerateRules : GeneralRules
    {
        /// <summary>
        /// Parameter determining how many points will initially become plates.
        /// </summary>
        [XmlElement("Cut_Off")]
        public int cutOff;

        /// <summary>
        /// Parameters determining weight of each set of circles.
        /// </summary>
        [XmlArray("Magnitude")]
        public double[] magnitude;

        /// <summary>
        /// Parameters determining the probability of any point being the center of a circle, for
        /// each set of circles.
        /// </summary>
        [XmlArray("Point_Concentration")]
        public double[] pointConcentration;

        /// <summary>
        /// Parameters determining the size of each set of circles.
        /// </summary>
        [XmlArray("Radius")]
        public double[] radius;

        /// <summary>
        /// Checks the rules to confirm they are valid.
        /// </summary>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        public void CheckRules()
        {
            try
            {
                if (xHalfSize < 1 || ySize < 1 || plateCount < 1)
                {
                    throw new InvalidDataException(nameof(GeneralRules) + " file has invalid values.");
                }
                for (int i = 0; i < magnitude.Length; i++)
                {
                    if (radius[i] == 0 || pointConcentration[i] == 0 || magnitude[i] == 0)
                    {
                        throw new InvalidDataException(nameof(GeneralRules) + " file has invalid values.");
                    }
                }
                if (cutOff == 0 || cutOff > 2 * xHalfSize * ySize)
                {
                    throw new InvalidDataException(nameof(GeneralRules) + " file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(nameof(GeneralRules) + " file is missing values.", e);
            }
        }

        /// <summary>
        /// Generates default values for class.
        /// </summary>
        public new void Default()
        {
            base.Default();
            cutOff = 1000 * 1000 * 3 / 2;
            magnitude = new double[10];
            pointConcentration = new double[10];
            radius = new double[10];
            for (int i = 0; i < 10; i++)
            {
                magnitude[i] = 16 - i;
                pointConcentration[i] = 0.999;
                radius[i] = Math.Round(Math.Sin(Math.PI * (12 - i) / 120), 2);
            }
        }
    }
}