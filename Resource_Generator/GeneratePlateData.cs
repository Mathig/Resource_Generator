﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resource_Generator
{
    /// <summary>
    /// Class for generating plate data.
    /// </summary>
    internal static class GeneratePlateData
    {
        /// <summary>
        /// List of points associated with a particular "plate".
        /// </summary>
        private static List<KeyPoint>[] platePoints;

        /// <summary>
        /// List of active points, used for processing points.
        /// </summary>
        private static bool[,] pointActives;

        /// <summary>
        /// Contains magnitudes of points, for the purposes of generating new plates.
        /// </summary>
        private static double[,] pointMagnitudes;

        /// <summary>
        /// Map of Base Points for processing.
        /// </summary>
        private static BasePoint[,] pointMap;

        /// <summary>
        /// Rules that define plate generation.
        /// </summary>
        private static GenerateRules rules;

        /// <summary>
        /// List of points associated with a temporary "plate".
        /// </summary>
        private static List<KeyPoint> temporaryPoints;

        /// <summary>
        /// Adds border point to point list.
        /// </summary>
        /// <param name="iPoint">Point to add.</param>
        private static void AddBorderPoint(OverlapPoint iPoint)
        {
            pointActives[iPoint.X, iPoint.Y] = true;
            platePoints[iPoint.plateIndex[0]].Add(new KeyPoint(iPoint.X, iPoint.Y));
        }

        /// <summary>
        /// Checks points near input point and adds border points to point queue.
        /// </summary>
        /// <param name="pointQueue">Queue to add border points to.</param>
        /// <param name="iPoint">Point to check around.</param>
        /// <param name="plateIndex">Index of plate.</param>
        private static void CheckBorderPoints(Queue<OverlapPoint> pointQueue, IPoint iPoint, int plateIndex)
        {
            var newPoints = pointMap[iPoint.X, iPoint.Y].Near.Points;
            foreach (KeyPoint newSimplePoint in newPoints)
            {
                if (!pointActives[newSimplePoint.X, newSimplePoint.Y])
                {
                    var newPoint = new OverlapPoint(newSimplePoint, plateIndex);
                    pointQueue.Enqueue(newPoint);
                }
            }
        }

        /// <summary>
        /// Checks neighboring points in <see cref="pointActives"/> and adds to pointStack.
        /// </summary>
        /// <param name="iPoint">Point to check neighbors for.</param>
        /// <param name="pointStack">Stack to add appropriate points to stack.</param>
        private static void CheckNeighbor(KeyPoint iPoint, Stack<KeyPoint> pointStack)
        {
            var newPoints = pointMap[iPoint.X, iPoint.Y].Near.Points;
            foreach (KeyPoint newPoint in newPoints)
            {
                if (pointActives[newPoint.X, newPoint.Y])
                {
                    pointActives[newPoint.X, newPoint.Y] = false;
                    pointStack.Push(newPoint);
                }
            }
        }

        /// <summary>
        /// Converts plate sorted plate point data into a map array.
        /// </summary>
        /// <returns>Map array of plate point data.</returns>
        private static PlatePoint[,] CompileData()
        {
            var output = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
            Parallel.For(0, (rules.plateCount), (i) =>
            {
                foreach (KeyPoint iPoint in platePoints[i])
                {
                    output[iPoint.X, iPoint.Y] = new PlatePoint(iPoint, i, rules.currentTime);
                }
            });
            return output;
        }

        /// <summary>
        /// Starts up and allocates data arrays.
        /// </summary>
        /// <param name="inRules">Rules to use for constructing data.</param>
        private static void ConstructData(GenerateRules inRules)
        {
            rules = inRules;
            BasePoint.MapSetup(rules.xHalfSize, rules.ySize);
            pointMap = new BasePoint[2 * rules.xHalfSize, rules.ySize];
            pointMagnitudes = new double[2 * rules.xHalfSize, rules.ySize];
            pointActives = new bool[2 * rules.xHalfSize, rules.ySize];
            platePoints = new List<KeyPoint>[rules.plateCount];
            for (int i = 0; i < rules.plateCount; i++)
            {
                platePoints[i] = new List<KeyPoint>();
            }
            Parallel.For(0, (2 * rules.xHalfSize), (x) =>
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    pointMap[x, y] = new BasePoint(x, y);
                }
            });
        }

        /// <summary>
        /// Sorts all point magnitudes and returns the magnitude at the given index.
        /// </summary>
        /// <returns>Height at given index.</returns>
        private static double CutoffMagnitude()
        {
            var output = new double[2 * rules.xHalfSize * rules.ySize];
            var k = 0;
            foreach (double pointMagnitude in pointMagnitudes)
            {
                output[k++] = pointMagnitude;
            }
            Array.Sort(output);
            return output[rules.cutOff];
        }

        /// <summary>
        /// Finally deallocate data.
        /// </summary>
        private static void DeconstructData()
        {
            rules = null;
            platePoints = null;
        }

        /// <summary>
        /// Checks a border point in point queue.
        /// </summary>
        /// <param name="pointQueue">Point queue to withdraw point from.</param>
        /// <param name="borderPoint">Border point to dequeue and check.</param>
        /// <returns>True if still valid border point, otherwise false.</returns>
        private static bool DequeueBorderPoint(Queue<OverlapPoint> pointQueue, out OverlapPoint borderPoint)
        {
            borderPoint = pointQueue.Dequeue();
            if (!pointActives[borderPoint.X, borderPoint.Y])
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Distributes a circle point about a given point.
        /// </summary>
        /// <param name="centerPoint">Center of circle.</param>
        /// <param name="generationStat">Generation statistics to use.</param>
        /// <param name="pointMagnitudes">Array of magnitudes to fill.</param>
        private static void DistributeCircle(in BasePoint centerPoint, in GenerationStat generationStat, double[,] pointMagnitudes)
        {
            centerPoint.Range(generationStat.radius, out int xMin, out int xMax, out int yMin, out int yMax);
            for (int xP = xMin; xP < xMax; xP++)
            {
                var x = xP;
                if (x >= 2 * rules.xHalfSize)
                {
                    x -= 2 * rules.xHalfSize;
                }
                for (int y = yMin; y < yMax; y++)
                {
                    if (centerPoint.Distance(pointMap[x, y]) < generationStat.radiusSquared)
                    {
                        pointMagnitudes[x, y] += generationStat.magnitude;
                    }
                }
            }
        }

#pragma warning disable EPS05 // Use in-modifier for a readonly struct

        /// <summary>
        /// Increases the Height of all points within circles centered at the given list of points.
        /// </summary>
        /// <param name="points">List of points where the circles are centered.</param>
        /// <param name="generationStat">Generation statistics to use.</param>
        private static void DistributeCircles(GenerationStat generationStat, List<BasePoint> points)
#pragma warning restore EPS05 // Use in-modifier for a readonly struct
        {
            Parallel.For(0, (points.Count), (i) =>
            {
                DistributeCircle(points[i], generationStat, pointMagnitudes);
            });
        }

        /// <summary>
        /// Expands each plate one pixel in every direction until no point outside of all plates exists.
        /// </summary>
        private static void ExpandPlates()
        {
            var borderPoints = new Queue<OverlapPoint>();
            for (int i = 0; i < platePoints.Length; i++)
            {
                FindPlateBorders(platePoints[i], borderPoints, i);
            }
            while (borderPoints.Count > 0)
            {
                if (DequeueBorderPoint(borderPoints, out OverlapPoint borderPoint))
                {
                    AddBorderPoint(borderPoint);
                    CheckBorderPoints(borderPoints, borderPoint, borderPoint.plateIndex[0]);
                }
            }
        }

        /// <summary>
        /// Scans for all points in <see cref="pointActives"/> that are set to true and are
        /// contiguously adjacent to the given point and adds them to <see cref="temporaryPoints"/>,
        /// setting the <see cref="pointActives"/> to false in the process.
        /// </summary>
        /// <param name="startingPoint">Starting point to check neighbors.</param>
        private static void FindContiguousPoints(KeyPoint startingPoint)
        {
            var pointStack = new Stack<KeyPoint>();
            pointActives[startingPoint.X, startingPoint.Y] = false;
            pointStack.Push(startingPoint);
            while (pointStack.Count != 0)
            {
                var point = pointStack.Pop();
                temporaryPoints.Add(point);
                CheckNeighbor(point, pointStack);
            }
        }

        /// <summary>
        /// Finds points on plate boundaries.
        /// </summary>
        /// <param name="iPlatePoints">List of plate points.</param>
        /// <param name="pointQueue">Plate Point queue to add to.</param>
        /// <param name="plateIndex">Which plate we are searching.</param>
        private static void FindPlateBorders(List<KeyPoint> iPlatePoints, Queue<OverlapPoint> pointQueue, int plateIndex)
        {
            for (int i = 0; i < iPlatePoints.Count; i++)
            {
                CheckBorderPoints(pointQueue, iPlatePoints[i], plateIndex);
            }
        }

        /// <summary>
        /// Finds valid plate to replace or populate with new data from <see cref="temporaryPoints"/>.
        /// </summary>
        /// <param name="oldPlateList">Old plate, if it exists.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private static bool FindReplaceablePlate(out List<KeyPoint> oldPlateList)
        {
            foreach (List<KeyPoint> pointList in platePoints)
            {
                if (pointList.Count == 0)
                {
                    oldPlateList = pointList;
                    return true;
                }
            }
            foreach (List<KeyPoint> pointList in platePoints)
            {
                if (pointList.Count < temporaryPoints.Count)
                {
                    oldPlateList = pointList;
                    return true;
                }
            }
            oldPlateList = null;
            return false;
        }

        /// <summary>
        /// Sets <see cref="ActivePoints"/> to true for points above magnitude threshold, false otherwise.
        /// </summary>
        /// <param name="magnitudeThreshold">Magnitude of threshold for setting points to active.</param>
        private static void NoiseFilter(double magnitudeThreshold)
        {
            Parallel.For(0, (2 * rules.xHalfSize), (x) =>
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    if (magnitudeThreshold < pointMagnitudes[x, y])
                    {
                        pointActives[x, y] = true;
                    }
                    else
                    {
                        pointActives[x, y] = false;
                    }
                }
            });
        }

        /// <summary>
        /// Generates Noise field.
        /// </summary>
        private static void NoiseGenerator()
        {
            var pointMagnitudes = new double[2 * rules.xHalfSize, rules.ySize];
            var rnd = new Random();
            for (int i = 0; i < rules.magnitude.Length; i++)
            {
                var circleList = new List<BasePoint>();
                foreach (BasePoint iPoint in pointMap)
                {
                    if (iPoint.TestMomentum(rnd.NextDouble(), rules.pointConcentration[i]))
                    {
                        circleList.Add(iPoint);
                    }
                }
                var generationStat = new GenerationStat(rules.radius[i], rules.magnitude[i]);
                DistributeCircles(generationStat, circleList);
            }
        }

        /// <summary>
        /// Uses <see cref="FindContiguousPoints"/> to generate a plate starting at the given point, and
        /// transfers it to <see cref="platePoints"/> if there is an empty plate or the plate is
        /// larger than the smallest existing plate.
        /// </summary>
        /// <param name="inPoint">Starting Point.</param>
        private static void PlateMaker(KeyPoint inPoint)
        {
            temporaryPoints = new List<KeyPoint>();
            if (pointActives[inPoint.X, inPoint.Y])
            {
                FindContiguousPoints(inPoint);
                if (FindReplaceablePlate(out List<KeyPoint> oldPlateList))
                {
                    ReplacePlate(oldPlateList);
                }
            }
            temporaryPoints = null;
            return;
        }

        /// <summary>
        /// Generates the largest <see cref="platePoints"/>, sets <see cref="pointActives"/> to true for every point in one
        /// of those plates, and false otherwise.
        /// </summary>
        private static void PlateMaking()
        {
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    if (pointActives[x, y])
                    {
                        PlateMaker(new KeyPoint(x, y));
                    }
                }
            }
            Parallel.For(0, (rules.plateCount), (i) =>
            {
                foreach (KeyPoint iPoint in platePoints[i])
                {
                    pointActives[iPoint.X, iPoint.Y] = true;
                }
            });
        }

        /// <summary>
        /// Initially deallocate data.
        /// </summary>
        private static void PruneData()
        {
            pointMap = null;
            pointMagnitudes = null;
            pointActives = null;
        }

        /// <summary>
        /// Replaces target plate list with <see cref="temporaryPoints"/>.
        /// </summary>
        /// <param name="targetPlateList">Plate list to replace at.</param>
        private static void ReplacePlate(List<KeyPoint> targetPlateList)
        {
            targetPlateList.Clear();
            foreach (KeyPoint iPoint in temporaryPoints)
            {
                targetPlateList.Add(iPoint);
            }
        }

        /// <summary>
        /// Generates Plates
        /// </summary>
        /// <param name="inRules">Rules to run generation.</param>
        /// <returns>Plate points.</returns>
        public static PlatePoint[,] Run(GenerateRules inRules)
        {
            ConstructData(inRules);
            NoiseGenerator();
            NoiseFilter(CutoffMagnitude());
            PlateMaking();
            ExpandPlates();
            PruneData();
            var output = CompileData();
            DeconstructData();
            return output;
        }

        /// <summary>
        /// Helper class for holding point generation stats.
        /// </summary>
        private readonly struct GenerationStat
        {
            /// <summary>
            /// Weight of point to be added.
            /// </summary>
            public readonly double magnitude;

            /// <summary>
            /// Radius of circle.
            /// </summary>
            public readonly double radius;

            /// <summary>
            /// Square of radius, for computational efficiency.
            /// </summary>
            public readonly double radiusSquared;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="inRadius">Radius to use.</param>
            /// <param name="inMagnitude">Magnitude to use.</param>
            public GenerationStat(double inRadius, double inMagnitude)
            {
                radius = inRadius;
                magnitude = inMagnitude;
                radiusSquared = radius * radius;
            }
        }
    }
}