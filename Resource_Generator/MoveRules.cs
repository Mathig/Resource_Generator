namespace Resource_Generator
{
    /// <summary>
    /// Contains rules for Plate Movement.
    /// </summary>
    internal class MoveRules : GeneralRules
    {
        /// <summary>
        /// How much to scale heights when overlapping.
        /// </summary>
        public double OverlapFactor;

        /// <summary>
        /// Multiplier for how much to move each plate.
        /// </summary>
        public double timeStep;
    }
}