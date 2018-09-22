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
        /// Concentration of initial dendrites to add.
        /// </summary>
        [XmlElement("Initial_Dendrites")]
        public double InitialDendrites;

        /// <summary>
        /// How many directions each plate can have.
        /// </summary>
        [XmlElement("Dendritic_Direction_Count")]
        public int DendriteDirectionCount;

        /// <summary>
        /// How many steps are used at a time for dendrite growth.
        /// </summary>
        [XmlElement("Dendritic_Point_Count")]
        public int DendritePointCount;

        /// <summary>
        /// How often should new Dendrites be formed.
        /// </summary>
        [XmlElement("Dendritic_Step_Threshold")]
        public double DendriticStepThreshold;

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
                if (InitialDendrites > 1 || InitialDendrites < 0 || DendriticStepThreshold > 1 || DendriticStepThreshold < 0)
                {
                    throw new InvalidDataException(nameof(GeneralRules) + " file has invalid values.");
                }
                if (DendriteDirectionCount < 1 || DendritePointCount < 1)
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
            InitialDendrites = 0.9;
            DendriteDirectionCount = 6;
            DendritePointCount = 3;
            DendriticStepThreshold = 0.9;
            for (int i = 0; i < 10; i++)
            {
                magnitude[i] = 16 - i;
                pointConcentration[i] = 0.999;
                radius[i] = 0.13 - (double)i/100;
            }
        }
    }
}