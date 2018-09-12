using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Contains rules for Plate Movement.
    /// </summary>
    [XmlRootAttribute(IsNullable = false)]
    public class MoveRules : GeneralRules
    {
        /// <summary>
        /// How many time steps to take.
        /// </summary>
        public int numberSteps;

        /// <summary>
        /// How much to scale heights when overlapping.
        /// </summary>
        public double OverlapFactor;

        /// <summary>
        /// Multiplier for how much to move each plate.
        /// </summary>
        public int timeStep;

        /// <summary>
        /// Returns name of class in string form.
        /// </summary>
        /// <returns>Name of class.</returns>
        public const string ClassName = "MoveRules";


        /// <summary>
        /// Checks the rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        public void CheckRules()
        {
            try
            {
                if (this.xHalfSize < 1 || this.ySize < 1 || this.plateCount < 1)
                {
                    throw new InvalidDataException(ClassName + " file has invalid values.");
                }
                if (this.timeStep == 0)
                {
                    throw new InvalidDataException(ClassName + " file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(ClassName + " file is missing values.", e);
            }
        }
    }
}