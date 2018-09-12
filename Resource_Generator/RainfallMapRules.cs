using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Class for holding data for rainfall generation.
    /// </summary>
    [XmlRootAttribute(IsNullable = false)]
    public class RainfallMapRules : GeneralRules
    {
        /// <summary>
        /// Weight of altitude for pressure fields.
        /// </summary>
        public double altitudeWeight;

        /// <summary>
        /// Amount of axis tilt of planet.
        /// </summary>
        public double axisTilt;

        /// <summary>
        /// Weight of land's deviance for pressure fields.
        /// </summary>
        public double landWeight;

        /// <summary>
        /// Number of seasons to work with.
        /// </summary>
        public int numberSeasons;

        /// <summary>
        /// Weight of ocean for pressure fields.
        /// </summary>
        public double oceanWeight;

        /// <summary>
        /// Returns name of class in string form.
        /// </summary>
        public const string ClassName = "RainfallMapRules";


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


            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(ClassName + " file is missing values.", e);
            }
        }
    }
}