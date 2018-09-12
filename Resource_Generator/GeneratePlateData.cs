using System;
using System.Collections.Concurrent;
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
        private static List<SimplePoint>[] platePoints;

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
        private static List<SimplePoint> temporaryPoints;

        /// <summary>
        /// Adds border point to point queue.
        /// </summary>
        /// <param name="pointQueue">Point queue to add to.</param>
        /// <param name="iPoint">Point to add.</param>
        private static void AddBorderPoint(ConcurrentQueue<OverlapPoint> pointQueue, OverlapPoint iPoint)
        {
            pointActives[iPoint.X, iPoint.Y] = true;
            platePoints[iPoint.plateIndex[0]].Add(new SimplePoint(iPoint.X, iPoint.Y));
        }

        /// <summary>
        /// Checks points near input point and adds border points to point queue.
        /// </summary>
        /// <param name="pointQueue">Queue to add border points to.</param>
        /// <param name="iPoint">Point to check around.</param>
        /// <param name="plateIndex">Index of plate.</param>
        private static void CheckBorderPoints(ConcurrentQueue<OverlapPoint> pointQueue, IPoint iPoint, int plateIndex)
        {
            SimplePoint[] newPoints = iPoint.FindNeighborPoints();
            foreach (SimplePoint newSimplePoint in newPoints)
            {
                if (!pointActives[newSimplePoint.X, newSimplePoint.Y])
                {
                    OverlapPoint newPoint = new OverlapPoint(newSimplePoint, plateIndex);
                    pointQueue.Enqueue(newPoint);
                }
            }
        }

        /// <summary>
        /// Checks neighboring points in <see cref="pointActives"/> and adds to pointStack.
        /// </summary>
        /// <param name="iPoint">Point to check neighbors for.</param>
        /// <param name="pointStack">Stack to add appropriate points to stack.</param>
        private static void CheckNeighbor(SimplePoint iPoint, Stack<SimplePoint> pointStack)
        {
            SimplePoint[] newPoints = iPoint.FindNeighborPoints();
            foreach (SimplePoint newPoint in newPoints)
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
            PlatePoint[,] output = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
            Parallel.For(0, (rules.plateCount), (i) =>
            {
                foreach (SimplePoint iPoint in platePoints[i])
                {
                    output[iPoint.X, iPoint.Y] = new PlatePoint(iPoint, i, rules.currentTime);
                }
            });
            return output;
        }

        /// <summary>
        /// Starts up and allocates data arrays.
        /// </summary>
        private static void ConstructData(GenerateRules inRules)
        {
            rules = inRules;
            BasePoint.MapSetup(rules.xHalfSize, rules.ySize);
            pointMap = new BasePoint[2 * rules.xHalfSize, rules.ySize];
            pointMagnitudes = new double[2 * rules.xHalfSize, rules.ySize];
            pointActives = new bool[2 * rules.xHalfSize, rules.ySize];
            platePoints = new List<SimplePoint>[rules.plateCount];
            for (int i = 0; i < rules.plateCount; i++)
            {
                platePoints[i] = new List<SimplePoint>();
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
        /// <param name="index">Index to return height value.</param>
        /// <returns>Height at given index.</returns>
        private static double CutoffMagnitude()
        {
            double[] output = new double[2 * rules.xHalfSize * rules.ySize];
            int k = 0;
            foreach (double pointMagnitude in pointMagnitudes)
            {
                output[k++] = pointMagnitude;
            }
            Array.Sort(output);
            return output[rules.cutOff];
        }

        /// <summary>
        /// Deallocates data arrays.
        /// </summary>
        private static void DeconstructData()
        {
            rules = null;
            pointMap = null;
            pointMagnitudes = null;
            pointActives = null;
            platePoints = null;
            GC.Collect();
        }

        /// <summary>
        /// Checks a border point in point queue.
        /// </summary>
        /// <param name="pointQueue">Point queue to withdraw point from.</param>
        /// <param name="borderPoint">Border point to dequeue and check.</param>
        /// <returns>True if still valid border point, otherwise false.</returns>
        private static bool DequeueBorderPoint(ConcurrentQueue<OverlapPoint> pointQueue, out OverlapPoint borderPoint)
        {
            if (pointQueue.TryDequeue(out borderPoint))
            {
                if (!pointActives[borderPoint.X, borderPoint.Y])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Distributes a circle point about a given point.
        /// </summary>
        /// <param name="centerPoint">Center of circle.</param>
        /// <param name="radius">Radius of circle.</param>
        /// <param name="magnitude">Weight of point to be added.</param>
        /// <param name="radiusSquared">Square of the radius, for computational efficiency.</param>
        private static void DistributeCircle(BasePoint centerPoint, double radius, double magnitude, double radiusSquared)
        {
            centerPoint.Range(radius, out int xMin, out int xMax, out int yMin, out int yMax);
            for (int xP = xMin; xP < xMax; xP++)
            {
                int x = xP;
                if (x >= 2 * rules.xHalfSize)
                {
                    x -= 2 * rules.xHalfSize;
                }
                for (int y = yMin; y < yMax; y++)
                {
                    if (centerPoint.Distance(pointMap[x, y]) < radiusSquared)
                    {
                        pointMagnitudes[x, y] += magnitude;
                    }
                }
            }
        }

        /// <summary>
        /// Increases the Height of all points within circles centered at the given list of points.
        /// </summary>
        /// <param name="radius">Radius of circles.</param>
        /// <param name="magnitude">Magnitude of height to be added per point per circle.</param>
        /// <param name="points">List of points where the circles are centered.</param>
        private static void DistributeCircles(double radius, double magnitude, List<BasePoint> points)
        {
            double radiusSquared = radius * radius;
            Parallel.For(0, (points.Count), (i) =>
            {
                DistributeCircle(points[i], radius, magnitude, radiusSquared);
            });
        }

        /// <summary>
        /// Expands each plate one pixel in every direction until no point outside of all plates exists.
        /// </summary>
        private static void ExpandPlates()
        {
            ConcurrentQueue<OverlapPoint> borderPoints = new ConcurrentQueue<OverlapPoint>();
            Parallel.For(0, platePoints.Length, (i) =>
            {
                FindPlateBorders(platePoints[i], borderPoints);
            });
            while (borderPoints.Count != 0)
            {
                if (DequeueBorderPoint(borderPoints, out OverlapPoint borderPoint))
                {
                    AddBorderPoint(borderPoints, borderPoint);
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
        private static void FindContiguousPoints(SimplePoint startingPoint)
        {
            Stack<SimplePoint> pointStack = new Stack<SimplePoint>();
            pointActives[startingPoint.X, startingPoint.Y] = false;
            pointStack.Push(startingPoint);
            while (pointStack.Count != 0)
            {
                SimplePoint point = pointStack.Pop();
                temporaryPoints.Add(point);
                CheckNeighbor(point, pointStack);
            }
        }

        /// <summary>
        /// Finds points on plate boundaries.
        /// </summary>
        /// <param name="iPlatePoints">List of plate points.</param>
        /// <param name="pointQueue">Plate Point queue to add to.</param>
        private static void FindPlateBorders(List<SimplePoint> iPlatePoints, ConcurrentQueue<OverlapPoint> pointQueue)
        {
            for (int i = 0; i < iPlatePoints.Count; i++)
            {
                CheckBorderPoints(pointQueue, iPlatePoints[i], i);
            }
        }

        /// <summary>
        /// Finds valid plate to replace or populate with new data from <see cref="temporaryPoints"/>.
        /// </summary>
        /// <param name="oldPlateList">Old plate, if it exists.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private static bool FindReplaceablePlate(out List<SimplePoint> oldPlateList)
        {
            foreach (List<SimplePoint> pointList in platePoints)
            {
                if (pointList.Count == 0)
                {
                    oldPlateList = pointList;
                    return true;
                }
            }
            foreach (List<SimplePoint> pointList in platePoints)
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
            Random rnd = new Random();
            for (int i = 0; i < rules.magnitude.Length; i++)
            {
                List<BasePoint> circleList = new List<BasePoint>();
                foreach (BasePoint iPoint in pointMap)
                {
                    if (iPoint.TestMomentum(rnd.NextDouble(), rules.pointConcentration[i]))
                    {
                        circleList.Add(iPoint);
                    }
                }
                DistributeCircles(rules.radius[i], rules.magnitude[i], circleList);
            }
        }

        /// <summary>
        /// Uses <see cref="FindContiguousPoints"/> to generate a plate starting at the given point, and
        /// transfers it to <see cref="platePoints"/> if there is an empty plate or the plate is
        /// larger than the smallest existing plate.
        /// </summary>
        /// <param name="inPoint">Starting Point.</param>
        private static void PlateMaker(SimplePoint inPoint)
        {
            temporaryPoints = new List<SimplePoint>();
            if (pointActives[inPoint.X, inPoint.Y])
            {
                FindContiguousPoints(inPoint);
                if (FindReplaceablePlate(out List<SimplePoint> oldPlateList))
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
                        PlateMaker(new SimplePoint(x, y));
                    }
                }
            }
            Parallel.For(0, (rules.plateCount), (i) =>
            {
                foreach (SimplePoint iPoint in platePoints[i])
                {
                    pointActives[iPoint.X, iPoint.Y] = true;
                }
            });
        }

        /// <summary>
        /// Replaces target plate list with <see cref="temporaryPoints"/>.
        /// </summary>
        /// <param name="targetPlateList">Plate list to replace at.</param>
        private static void ReplacePlate(List<SimplePoint> targetPlateList)
        {
            targetPlateList.Clear();
            foreach (SimplePoint iPoint in temporaryPoints)
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
            PlatePoint[,] output = CompileData();
            DeconstructData();
            return output;
        }
    }
}