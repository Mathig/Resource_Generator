namespace Resource_Generator
{
    /// <summary>
    /// Interface to allow points to access the X and Y position and find left, right, above, and below points.
    /// </summary>
    internal interface IPoint
    {
        /// <summary>
        /// X value of point, for array index.
        /// </summary>
        int X { get; }

        /// <summary>
        /// Y value of point, for array index.
        /// </summary>
        int Y { get; }

        /// <summary>
        /// Returns neighboring points in an array ordered as above, below, left, then right.
        /// </summary>
        /// <returns>Neighbor points.</returns>
        SimplePoint[] FindNeighborPoints();
    }
}