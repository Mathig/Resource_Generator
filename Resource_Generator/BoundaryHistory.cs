using System;

namespace Resource_Generator
{
    public class BoundaryHistory
    {
        /// <summary>
        /// How much a point has been near continental vs continental convergent boundaries.
        /// </summary>
        public int ContinentalBuildup;

        /// <summary>
        /// How recently a point has been near continental vs continental convergent boundaries.
        /// </summary>
        public int ContinentalRecency;

        /// <summary>
        /// How much a point has been near continental vs oceanic convergent boundaries.
        /// </summary>
        public int OceanicBuildup;

        /// <summary>
        /// How recently this point has been near continental vs oceanic convergent boundaries.
        /// </summary>
        public int OceanicRecency;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BoundaryHistory()
        {
            ContinentalBuildup = 0;
            ContinentalRecency = 0;
            OceanicBuildup = 0;
            OceanicRecency = 0;
        }

        /// <summary>
        /// Constructor for data loading.
        /// </summary>
        /// <param name="inContinentalBuildup">See <see cref="ContinentalBuildup"/>.</param>
        /// <param name="inContinentalRecency">See <see cref="ContinentalRecency"/>.</param>
        /// <param name="inOceanicBuildup">See <see cref="OceanicBuildup"/>.</param>
        /// <param name="inOceanicRecency">See <see cref="OceanicRecency"/>.</param>
        public BoundaryHistory(int inContinentalBuildup, int inContinentalRecency, int inOceanicBuildup, int inOceanicRecency)
        {
            ContinentalBuildup = inContinentalBuildup;
            ContinentalRecency = inContinentalRecency;
            OceanicBuildup = inOceanicBuildup;
            OceanicRecency = inOceanicRecency;
        }

        /// <summary>
        /// Addition Operator for averaging.
        /// </summary>
        /// <param name="oneVariable">First Variable.</param>
        /// <param name="count">Number of units averaging.</param>
        /// <returns>Averaged value</returns>
        public static BoundaryHistory operator /(BoundaryHistory oneVariable, int count)
        {
            oneVariable.ContinentalBuildup = (int)Math.Round((double)oneVariable.ContinentalBuildup / count);
            oneVariable.ContinentalRecency = (int)Math.Round((double)oneVariable.ContinentalRecency / count);
            oneVariable.OceanicBuildup = (int)Math.Round((double)oneVariable.OceanicBuildup / count);
            oneVariable.OceanicRecency = (int)Math.Round((double)oneVariable.OceanicRecency / count);
            return oneVariable;
        }

        /// <summary>
        /// Addition Operator for averaging.
        /// </summary>
        /// <param name="oneVariable">First Variable.</param>
        /// <param name="otherVariable">Second Variable.</param>
        /// <returns>Sum of variables.</returns>
        public static BoundaryHistory operator +(BoundaryHistory oneVariable, BoundaryHistory otherVariable)
        {
            oneVariable.ContinentalBuildup += otherVariable.ContinentalBuildup;
            oneVariable.ContinentalRecency += otherVariable.ContinentalRecency;
            oneVariable.OceanicBuildup += otherVariable.OceanicBuildup;
            oneVariable.OceanicRecency += otherVariable.OceanicRecency;
            return oneVariable;
        }
    }
}