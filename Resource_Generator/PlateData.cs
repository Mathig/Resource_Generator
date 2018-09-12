using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Class for containing plate data.
    /// </summary>
    [XmlRootAttribute(IsNullable = false)]
    public class PlateData
    {
        /// <summary>
        /// Contains data on plates direction.
        /// </summary>
        public double[][] Direction;

        /// <summary>
        /// Contains data on plates speed.
        /// </summary>
        public double[] Speed;
    }
}