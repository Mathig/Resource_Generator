namespace Resource_Generator
{
    /// <summary>
    /// Holds rules for how erosion will work.
    /// </summary>
    internal class ErosionMapRules : GeneralRules
    {
        /// <summary>
        /// How many seasons exist for rain.
        /// </summary>
        public int numberSeasons;

        /// <summary>
        /// How much water before a point becomes a lake.
        /// </summary>
        public double waterThreshold;
    }
}