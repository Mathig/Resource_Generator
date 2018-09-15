using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Contains General Rules for all functions.
    /// </summary>
    [XmlRoot(IsNullable = false)]
    public class GeneralRules
    {
        /// <summary>
        /// Current Time.
        /// </summary>
        [XmlElement("Current_Time")]
        public int currentTime;

        /// <summary>
        /// Max convergent buildup.
        /// </summary>
        [XmlElement("Maximum_Buildup")]
        public int maxBuildup;

        /// <summary>
        /// Number of plates.
        /// </summary>
        [XmlElement("Plate_Count")]
        public int plateCount;

        /// <summary>
        /// Width of map.
        /// </summary>
        [XmlElement("X_Half_Size")]
        public int xHalfSize;

        /// <summary>
        /// Height of map.
        /// </summary>
        [XmlElement("Y_Size")]
        public int ySize;

        /// <summary>
        /// Generates default values for class.
        /// </summary>
        public void Default()
        {
            currentTime = 0;
            maxBuildup = 0;
            plateCount = 10;
            xHalfSize = 1000;
            ySize = 1000;
        }
    }
}