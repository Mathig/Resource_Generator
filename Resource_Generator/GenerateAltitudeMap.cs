using System;

namespace Resource_Generator
{
    /// <summary>
    /// Allows for generating a height map based on birth location, time, and collision history
    /// </summary>
    internal class GenerateAltitudeMap
    {
        /// <summary>
        /// Height map for every point.
        /// </summary>
        private static double[,] heightMap;

        /// <summary>
        /// Rules for how to generate heights for points.
        /// </summary>
        private static AltitudeMapRules rules;

        /// <summary>
        /// Finds the height given an input point.
        /// </summary>
        /// <param name="iPoint">Point to find height for.</param>
        /// <returns>Height of point.</returns>
        private static double GetHeight(PlatePoint iPoint, double randomNumber)
        {
            double height = 0;
            if (!iPoint.IsContinental)
            {
                return height;
            }
            else
            {
                //Some math depending on concentration and density of initial point
                height += 1000 + 100 * randomNumber;
                //Some math depending on collision history
                height += 10 * iPoint.History.ContinentalBuildup * (iPoint.History.ContinentalRecency + 1) / rules.currentTime;
                height += 10 * iPoint.History.OceanicBuildup * (iPoint.History.OceanicRecency + 1) / rules.currentTime;
                return height;
            }
        }

        /// <summary>
        /// Generates a height map given an array of points and rules for how to do so.
        /// </summary>
        /// <param name="pointMap">Points to base height map off of.</param>
        /// <param name="inRules">Rules for generating points.</param>
        /// <returns>Height map to generate.</returns>
        public static double[,] Run(PlatePoint[,] pointMap, AltitudeMapRules inRules)
        {
            rules = inRules;
            heightMap = new double[pointMap.GetLength(0), pointMap.GetLength(1)];
            Random randomNumber = new Random();
            for (int x = 0; x < pointMap.GetLength(0); x++)
            {
                for (int y = 0; y < pointMap.GetLength(1); y++)
                {
                    heightMap[x, y] = GetHeight(pointMap[x, y], randomNumber.NextDouble());
                }
            }
            return heightMap;
        }
    }
}