using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Holds rules for how erosion will work.
    /// </summary>
    [XmlRootAttribute(IsNullable = false)]
    public class ErosionMapRules : GeneralRules
    {
        /// <summary>
        /// How many seasons exist for rain.
        /// </summary>
        public int numberSeasons;

        /// <summary>
        /// How much water before a point becomes a lake.
        /// </summary>
        public double waterThreshold;

        /// <summary>
        /// Returns name of class in string form.
        /// </summary>
        public const string ClassName = "ErosionMapRules";


        /// <summary>
        /// Checks the rules to confirm they are valid.
        /// </summary>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        public void CheckRules()
        {
            try
            {
                if (this.xHalfSize < 1 || this.ySize < 1 || this.plateCount < 1)
                {
                    throw new InvalidDataException(ClassName + " file has invalid values.");
                }
                if (this.numberSeasons < 1)
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