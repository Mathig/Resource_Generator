using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Contains rules for Plate Movement.
    /// </summary>
    [XmlRoot("Move_Rules", IsNullable = false)]
    public class MoveRules : GeneralRules
    {
        /// <summary>
        /// How many time steps to take.
        /// </summary>
        [XmlElement("Number_of_Steps")]
        public int numberSteps;

        /// <summary>
        /// How much to scale heights when overlapping.
        /// </summary>
        [XmlElement("Overlap_Factor")]
        public double OverlapFactor;

        /// <summary>
        /// Multiplier for how much to move each plate.
        /// </summary>
        [XmlElement("Time_Step")]
        public int timeStep;

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
                    throw new InvalidDataException(nameof(MoveRules) + " file has invalid values.");
                }
                if (timeStep == 0)
                {
                    throw new InvalidDataException(nameof(MoveRules) + " file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(nameof(MoveRules) + " file is missing values.", e);
            }
        }

        /// <summary>
        /// Generates default values for class.
        /// </summary>
        public new void Default()
        {
            base.Default();
            numberSteps = 10;
            OverlapFactor = 0.6;
            timeStep = 1;
        }
    }
}