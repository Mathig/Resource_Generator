using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Contains General Rules for all functions.
    /// </summary>
    [XmlRootAttribute(IsNullable = false)]
    public class GeneralRules
    {
        /// <summary>
        /// Current Time.
        /// </summary>
        public int currentTime;

        /// <summary>
        /// Max convergent buildup.
        /// </summary>
        public int maxBuildup;

        /// <summary>
        /// Number of plates.
        /// </summary>
        public int plateCount;

        /// <summary>
        /// Width of map.
        /// </summary>
        public int xHalfSize;

        /// <summary>
        /// Height of map.
        /// </summary>
        public int ySize;
    }
}