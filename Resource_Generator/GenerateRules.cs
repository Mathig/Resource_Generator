using System;
using System.IO;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Contains rules for Plate Generation.
    /// </summary>
    [XmlRootAttribute(IsNullable = false)]
    public class GenerateRules : GeneralRules
    {
        /// <summary>
        /// Parameter determining how many points will initially become plates.
        /// </summary>
        public int cutOff;

        /// <summary>
        /// Parameters determining weight of each set of circles.
        /// </summary>
        public double[] magnitude;

        /// <summary>
        /// Parameters determining the probability of any point being the center of a circle, for
        /// each set of circles.
        /// </summary>
        public double[] pointConcentration;

        /// <summary>
        /// Parameters determining the size of each set of circles.
        /// </summary>
        public double[] radius;

        /// <summary>
        /// Returns name of class in string form.
        /// </summary>
        /// <returns>Name of class.</returns>
        public const string ClassName = "GenerateRules";


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
                for (int i = 0; i < this.magnitude.Length; i++)
                {
                    if (this.radius[i] == 0 || this.pointConcentration[i] == 0 || this.magnitude[i] == 0)
                    {
                        throw new InvalidDataException(ClassName + " file has invalid values.");
                    }
                }
                if (this.cutOff == 0 || this.cutOff > 2 * this.xHalfSize * this.ySize)
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