using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Holds rules for how erosion will work.
    /// </summary>
    [XmlRoot("Erosion_Map_Rules", IsNullable = false)]
    public class ErosionMapRules : GeneralRules
    {
        /// <summary>
        /// How many seasons exist for rain.
        /// </summary>
        [XmlElement("Number_of_Seasons")]
        public int numberSeasons;

        /// <summary>
        /// How much water before a point becomes a lake.
        /// </summary>
        [XmlElement("Water_Threshold")]
        public double waterThreshold;

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
                    throw new InvalidDataException(nameof(ErosionMapRules) + " file has invalid values.");
                }
                if (numberSeasons < 1)
                {
                    throw new InvalidDataException(nameof(ErosionMapRules) + " file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(nameof(ErosionMapRules) + " file is missing values.", e);
            }
        }

        /// <summary>
        /// Generates default values for class.
        /// </summary>
        public new void Default()
        {
            base.Default();
            numberSeasons = 4;
            waterThreshold = 0.1;
        }
    }
}