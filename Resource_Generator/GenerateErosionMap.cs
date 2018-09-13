using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Resource_Generator
{
    /// <summary>
    /// Class for performing erosion.
    /// </summary>
    internal static class GenerateErosionMap
    {
        /// <summary>
        /// Height of each point.
        /// </summary>
        private static double[,] heightMap;

        /// <summary>
        /// List of points which are part of a lake.
        /// </summary>
        private static List<SimplePoint> lakePoints;

        /// <summary>
        /// Points used for calculations.
        /// </summary>
        private static BasePoint[,] points;

        /// <summary>
        /// How much water rained on that point.
        /// </summary>
        private static double[,] rainWater;

        /// <summary>
        /// Rules for operation.
        /// </summary>
        private static ErosionMapRules rules;

        /// <summary>
        /// How much water is in each point of running water.
        /// </summary>
        private static double[,] waterArea;

        /// <summary>
        /// How much water is in each point.
        /// </summary>
        private static double[,] waterContained;

        /// <summary>
        /// How much water flows through each point.
        /// </summary>
        private static double[,] waterFlow;

        /// <summary>
        /// Adjusts height for given point to enable lake formation.
        /// </summary>
        /// <param name="iPoint">Point to adjust.</param>
        private static void AdjustPointAltitude(SimplePoint iPoint)
        {
            if (!FindLowestNeighbor(iPoint, out SimplePoint outPoint))
            {
                if (waterContained[iPoint.X, iPoint.Y] > rules.waterThreshold * points[iPoint.X, iPoint.Y]._sinPhi)
                {
                    heightMap[iPoint.X, iPoint.Y] = heightMap[outPoint.X, outPoint.Y] + 1;
                    lakePoints.Add(iPoint);
                }
            }
        }

        /// <summary>
        /// Calculates erosion for map.
        /// </summary>
        /// <returns>Map of erosion.</returns>
        private static double[,] CalculateErosionMap()
        {
            var output = new double[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    output[x, y] = CalculateErosionPoint(x, y);
                }
            }
            return output;
        }

        /// <summary>
        /// Calculates erosion for a given point.
        /// </summary>
        /// <param name="x">X coordinate of point.</param>
        /// <param name="y">Y coordinate of point.</param>
        /// <returns>Erosion value for point.</returns>
        private static double CalculateErosionPoint(int x, int y)
        {
            return waterFlow[x, y];
        }

        /// <summary>
        /// Placeholder for calculating water area.
        /// </summary>
        private static void CalculateWaterArea()
        {
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    waterArea[x, y] = waterFlow[x, y];//Some calculation belongs here.
                }
            }
        }

        /// <summary>
        /// Calculates amount of water flow from scratch.
        /// </summary>
        /// <param name="iPoint">Point to consider.</param>
        /// <returns>Amount of water flow.</returns>
        private static double CalculateWaterFlow(SimplePoint iPoint)
        {
            var water = rainWater[iPoint.X, iPoint.Y];
            var nearPoints = iPoint.FindNeighborPoints();
            for (int i = 0; i < nearPoints.Length; i++)
            {
                if (FindLowestNeighbor(nearPoints[i], out SimplePoint testPoint))
                {
                    if (iPoint.CompareTo(testPoint) == 0)
                    {
                        water += waterFlow[testPoint.X, testPoint.Y];
                    }
                }
            }
            return water;
        }

        /// <summary>
        /// Checks which points classify as water.
        /// </summary>
        /// <returns>Matrix of points which classify as water.</returns>
        private static bool[,] CheckWaterMap()
        {
            var output = new bool[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    output[x, y] = CheckWaterPoint(x, y);
                }
            }
            return output;
        }

        /// <summary>
        /// Checks whether a point classifies as water or not.
        /// </summary>
        /// <param name="x">X coordinate of point.</param>
        /// <param name="y">Y coordinate of point.</param>
        /// <returns>True if classifies as water, otherwise false.</returns>
        private static bool CheckWaterPoint(int x, int y)
        {
            if (heightMap[x, y] == 0)
            {
                return true;
            }
            if (waterArea[x, y] > points[x, y]._sinPhi)
            {
                return true;
            }
            if (waterContained[x, y] > points[x, y]._sinPhi * rules.waterThreshold)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Constructs data arrays.
        /// </summary>
        private static void ConstructData()
        {
            BasePoint.MapSetup(rules.xHalfSize, rules.ySize);
            points = new BasePoint[2 * rules.xHalfSize, rules.ySize];
            waterFlow = new double[2 * rules.xHalfSize, rules.ySize];
            waterArea = new double[2 * rules.xHalfSize, rules.ySize];
            lakePoints = new List<SimplePoint>();
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    points[x, y] = new BasePoint(x, y);
                }
            }
        }

        /// <summary>
        /// Attempts to find the nearest neighbor with the lowest point. Returns false if center point is lowest point, otherwise true.
        /// </summary>
        /// <param name="inPoint">Input point to search around.</param>
        /// <param name="outPoint">Point of lowest altitude.</param>
        /// <returns>True if center point is not lowest, otherwise false.</returns>
        private static bool FindLowestNeighbor(SimplePoint inPoint, out SimplePoint outPoint)
        {
            outPoint = inPoint;
            var notCenter = false;
            var lowestHeight = heightMap[inPoint.X, inPoint.Y];
            var neighborPoints = inPoint.FindNeighborPoints();
            for (int i = 0; i < neighborPoints.Length; i++)
            {
                if (heightMap[neighborPoints[i].X, neighborPoints[i].Y] < lowestHeight)
                {
                    lowestHeight = heightMap[neighborPoints[i].X, neighborPoints[i].Y];
                    outPoint = neighborPoints[i];
                    notCenter = true;
                }
            }
            return notCenter;
        }

        /// <summary>
        /// Calculates the number of sections to use.
        /// </summary>
        /// <param name="firstSpacing">Spacing for first section.</param>
        /// <param name="sectionSpacing">Standard spacing between sections.</param>
        /// <returns>The number of sections to use.</returns>
        private static int SectionCount(out int sectionSpacing, out int firstSpacing)
        {
            int sectionCount;
            var processorCount = Environment.ProcessorCount;
            sectionCount = rules.xHalfSize > 20 * processorCount ? 4 * processorCount : 1;
            sectionSpacing = (int)Math.Floor(2 * (double)rules.xHalfSize / sectionCount);
            firstSpacing = 2 * rules.xHalfSize - sectionCount * sectionSpacing;
            return sectionCount;
        }

        /// <summary>
        /// Flows water for the rest of the map.
        /// </summary>
        /// <param name="sectionCount">Spacing between sections for multi-threading.</param>
        /// <param name="firstSpacing">Spacing of first section for multi-threading.</param>
        /// <param name="sectionSpacing">Spacing for non-first section for multi-threading.</param>
        private static void WaterFullFlow(int sectionSpacing, int firstSpacing, int sectionCount)
        {
            var listPoints = new List<HeightPoint>();
            for (int i = 0; i < sectionCount; i++)
            {
                int start;
                int end;
                if (i == 0)
                {
                    start = 0;
                    end = firstSpacing - 1;
                }
                else
                {
                    start = firstSpacing + (i - 1) * sectionSpacing;
                    end = firstSpacing + i * sectionSpacing - 1;
                }
                for (int y = 1; y < rules.ySize - 1; y++)
                {
                    if (heightMap[start, y] != 0)
                    {
                        listPoints.Add(new HeightPoint(start, y, heightMap[start, y]));
                    }
                    if (heightMap[end, y] != 0)
                    {
                        listPoints.Add(new HeightPoint(end, y, heightMap[end, y]));
                    }
                }
            }
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                if (heightMap[x, 0] != 0)
                {
                    listPoints.Add(new HeightPoint(x, 0, heightMap[x, 0]));
                }
                if (heightMap[x, rules.ySize - 1] != 0)
                {
                    listPoints.Add(new HeightPoint(x, rules.ySize - 1, heightMap[x, rules.ySize - 1]));
                }
            }
            listPoints.Sort();
            listPoints.Reverse();
            var explodingPoints = new Queue<SimplePoint>();
            for (int i = 0; i < listPoints.Count; i++)
            {
                var newPoint = WaterPointFlow(listPoints[i].X, listPoints[i].Y);
                if (newPoint.CompareTo(listPoints[i]) == 0)
                {
                    explodingPoints.Enqueue(newPoint);
                }
            }
            while (explodingPoints.Count != 0)
            {
                var queuePoint = explodingPoints.Dequeue();
                AdjustPointAltitude(queuePoint);
                var newPoint = WaterPointFlow(queuePoint.X, queuePoint.Y);
                explodingPoints.Enqueue(newPoint);
            }
        }

        /// <summary>
        /// Flows water for single point.
        /// </summary>
        /// <param name="x">X coordinate of point.</param>
        /// <param name="y">Y coordinate of point.</param>
        /// <returns>Destination for water flow.</returns>
        private static SimplePoint WaterPointFlow(int x, int y)
        {
            var point = new SimplePoint(x, y);
            if (FindLowestNeighbor(point, out SimplePoint destinationPoint))
            {
                waterContained[destinationPoint.X, destinationPoint.Y] += waterContained[point.X, point.Y];
                if (waterFlow[point.X, point.Y] == 0)
                {
                    waterFlow[point.X, point.Y] += waterContained[point.X, point.Y];
                }
                else
                {
                    waterFlow[point.X, point.Y] = CalculateWaterFlow(point);
                }
                waterContained[point.X, point.Y] = 0;
                return destinationPoint;
            }
            else
            {
                return point;
            }
        }

        /// <summary>
        /// Flows water in the map for the section defined by the start and end point.
        /// </summary>
        /// <param name="start">Start point for section.</param>
        /// <param name="end">End point for section.</param>
        private static void WaterSectionFlow(int start, int end)
        {
            var listPoints = new List<HeightPoint>();
            for (int x = start + 1; x < end - 1; x++)
            {
                for (int y = 1; y < rules.ySize - 1; y++)
                {
                    if (heightMap[x, y] != 0)
                    {
                        listPoints.Add(new HeightPoint(x, y, heightMap[x, y]));
                    }
                }
            }
            listPoints.Sort();
            listPoints.Reverse();
            var explodingPoints = new Queue<SimplePoint>();
            for (int i = 0; i < listPoints.Count; i++)
            {
                var newPoint = WaterPointFlow(listPoints[i].X, listPoints[i].Y);
                if (newPoint.CompareTo(listPoints[i]) == 0)
                {
                    explodingPoints.Enqueue(newPoint);
                }
            }
            while (explodingPoints.Count != 0)
            {
                var queuePoint = explodingPoints.Dequeue();
                AdjustPointAltitude(queuePoint);
                var newPoint = WaterPointFlow(queuePoint.X, queuePoint.Y);
                if (newPoint.X > start && newPoint.X < end - 1 && newPoint.Y > 0 && newPoint.Y < rules.ySize - 1)
                {
                    explodingPoints.Enqueue(newPoint);
                }
            }
        }

        /// <summary>
        /// Runs the erosion program.
        /// </summary>
        /// <param name="inHeightMap">Height map.</param>
        /// <param name="rainfallMap">Rainfall map.</param>
        /// <param name="inRules">Rules for erosion.</param>
        /// <param name="isWater">Whether a point is above a certain threshold for being water.</param>
        /// <param name="erosionMap">How much erosion each point experiences.</param>
        public static void Run(double[,] inHeightMap, double[,] rainfallMap, ErosionMapRules inRules, out bool[,] isWater, out double[,] erosionMap)
        {
            rules = inRules;
            ConstructData();
            waterContained = rainfallMap;
            heightMap = inHeightMap;
            rainWater = rainfallMap;
            var sectionCount = SectionCount(out int sectionSpacing, out int firstSpacing);
            Parallel.For(0, (sectionCount), (i) =>
            {
                if (i == 0)
                {
                    WaterSectionFlow(0, firstSpacing);
                }
                else
                {
                    WaterSectionFlow(firstSpacing + (i - 1) * sectionSpacing, firstSpacing + i * sectionSpacing);
                }
            });
            WaterFullFlow(sectionSpacing, firstSpacing, sectionCount);
            CalculateWaterArea();
            isWater = CheckWaterMap();
            erosionMap = CalculateErosionMap();
        }
    }
}