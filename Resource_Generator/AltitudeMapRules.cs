using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Rules for generating altitude map.
    /// </summary>
    [XmlRootAttribute(IsNullable = false)]
    public class AltitudeMapRules : GeneralRules
    {
        /// <summary>
        /// Returns name of class in string form.
        /// </summary>
        public const string ClassName = "AltitudeMapRules";

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