namespace Resource_Generator
{
    /// <summary>
    /// Class for holding data for rainfall generation.
    /// </summary>
    internal class RainfallMapRules : GeneralRules
    {
        /// <summary>
        /// Weight of altitude for pressure fields.
        /// </summary>
        public double altitudeWeight;

        /// <summary>
        /// Amount of axis tilt of planet.
        /// </summary>
        public double axisTilt;

        /// <summary>
        /// Weight of land's deviance for pressure fields.
        /// </summary>
        public double landWeight;

        /// <summary>
        /// Number of seasons to work with.
        /// </summary>
        public int numberSeasons;

        /// <summary>
        /// Weight of ocean for pressure fields.
        /// </summary>
        public double oceanWeight;
    }
}