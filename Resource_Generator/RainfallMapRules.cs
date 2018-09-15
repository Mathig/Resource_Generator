using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Class for holding data for rainfall generation.
    /// </summary>
    [XmlRoot("Rainfall_Map_Rules",IsNullable = false)]
    public class RainfallMapRules : GeneralRules
    {
        /// <summary>
        /// Weight of altitude for pressure fields.
        /// </summary>
        [XmlElement("Altitude_Weight")]
        public double altitudeWeight;

        /// <summary>
        /// Amount of axis tilt of planet.
        /// </summary>
        [XmlElement("Axis_Tilt")]
        public double axisTilt;

        /// <summary>
        /// Weight of land's deviance for pressure fields.
        /// </summary>
        [XmlElement("Land_Weight")]
        public double landWeight;

        /// <summary>
        /// Number of seasons to work with.
        /// </summary>
        [XmlElement("Number_of_Seasons")]
        public int numberSeasons;

        /// <summary>
        /// Weight of ocean for pressure fields.
        /// </summary>
        [XmlElement("Ocean_Weight")]
        public double oceanWeight;

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
                    throw new InvalidDataException(nameof(RainfallMapRules) + " file has invalid values.");
                }


            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(nameof(RainfallMapRules) + " file is missing values.", e);
            }
        }

        /// <summary>
        /// Generates default values for class.
        /// </summary>
        public new void Default()
        {
            base.Default();
            altitudeWeight = 0.001;
            axisTilt = 10;
            landWeight = 5;
            numberSeasons = 4;
            oceanWeight = 0.125;
        }
    }
}