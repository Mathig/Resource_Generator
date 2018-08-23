namespace Resource_Generator
{
    /// <summary>
    /// Contains rules for Plate Generation.
    /// </summary>
    internal class GenerateRules : GeneralRules
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
    }
}