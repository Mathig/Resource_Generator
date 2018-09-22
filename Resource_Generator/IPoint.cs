using System;

namespace Resource_Generator
{
    /// <summary>
    /// Interface to allow points to access and sort their contained key point.
    /// </summary>
    public interface IPoint : IComparable
    {
        /// <summary>
        /// X value of point, for array index.
        /// </summary>
        int X { get; }

        /// <summary>
        /// Y value of point, for array index.
        /// </summary>
        int Y { get; }
    }
}