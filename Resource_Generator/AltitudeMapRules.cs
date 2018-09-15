using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Rules for generating altitude map.
    /// </summary>
    [XmlRoot("Altitude_Map_Rules", IsNullable = false)]
    public class AltitudeMapRules : GeneralRules
    {
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
                    throw new InvalidDataException(nameof(AltitudeMapRules) + " file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(nameof(AltitudeMapRules) + " file is missing values.", e);
            }
        }

        /// <summary>
        /// Generates default values for class.
        /// </summary>
        public new void Default()
        {
            base.Default();
        }
    }
}