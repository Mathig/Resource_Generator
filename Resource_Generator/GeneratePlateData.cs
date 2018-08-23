using System;
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
        private static List<PlatePoint>[] platePoints;

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
        private static List<PlatePoint> temporaryPoints;

        /// <summary>
        /// Scans for all points in <see cref="pointActives"/> that are set to true and are
        /// contiguously adjacent to the given point and adds them to <see cref="temporaryPoints"/>,
        /// setting the <see cref="pointActives"/> to false in the process.
        /// </summary>
        /// <param name="startingPoint">Starting point to check neighbors.</param>
        private static void CheckNeighbor(SimplePoint startingPoint)
        {
            Stack<SimplePoint> pointStack = new Stack<SimplePoint>();
            pointActives[startingPoint.X, startingPoint.Y] = false;
            pointStack.Push(startingPoint);
            while (pointStack.Count != 0)
            {
                SimplePoint point = pointStack.Pop();
                temporaryPoints.Add(new PlatePoint(point));
                SimplePoint[] newPoints = new SimplePoint[4];
                point.FindLeftRightPoints(out newPoints[0], out newPoints[1]);
                point.FindAboveBelowPoints(out newPoints[2], out newPoints[3]);
                for (int i = 0; i < 4; i++)
                {
                    if (pointActives[newPoints[i].X, newPoints[i].Y])
                    {
                        pointActives[newPoints[i].X, newPoints[i].Y] = false;
                        pointStack.Push(newPoints[i]);
                    }
                }
            }
        }

        private static PlatePoint[,] CompileData()
        {
            PlatePoint[,] output = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
            Parallel.For(0, (2 * rules.xHalfSize), (x) =>
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    output[x, y] = new PlatePoint(x, y);
                }
            });
            Parallel.For(0, (rules.plateCount), (i) =>
            {
                for (int j = 0; j < platePoints[i].Count; j++)
                {
                    output[platePoints[i][j].X, platePoints[i][j].Y].PlateNumber = i;
                }
            });
            return output;
        }

        /// <summary>
        /// Starts up and allocates data arrays.
        /// </summary>
        private static void ConstructData()
        {
            BasePoint.MapSetup(rules.xHalfSize, rules.ySize);
            pointMap = new BasePoint[2 * rules.xHalfSize, rules.ySize];
            pointMagnitudes = new double[2 * rules.xHalfSize, rules.ySize];
            pointActives = new bool[2 * rules.xHalfSize, rules.ySize];
            platePoints = new List<PlatePoint>[rules.plateCount];
            for (int i = 0; i < rules.plateCount; i++)
            {
                platePoints[i] = new List<PlatePoint>();
            }
            temporaryPoints = new List<PlatePoint>();
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
                output[k] = pointMagnitude;
                k++;
            }
            Array.Sort(output);
            return output[rules.cutOff];
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
                points[i].Range(radius, out int xMin, out int xMax, out int yMin, out int yMax);
                for (int xP = xMin; xP < xMax; xP++)
                {
                    int x = xP;
                    if (x >= 2 * rules.xHalfSize)
                    {
                        x -= 2 * rules.xHalfSize;
                    }
                    for (int y = yMin; y < yMax; y++)
                    {
                        if (points[i].Distance(pointMap[x, y]) < radiusSquared)
                        {
                            pointMagnitudes[x, y] += magnitude;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Expands each plate one pixel in every direction until no point outside of all plates exists.
        /// </summary>
        private static void ExpandPlates()
        {
            Queue<OverlapPoint> borderPoints = new Queue<OverlapPoint>();
            for (int i = 0; i < rules.plateCount; i++)
            {
                for (int j = 0; j < platePoints[i].Count; j++)
                {
                    SimplePoint[] newPoints = new SimplePoint[4];
                    platePoints[i][j].FindLeftRightPoints(out newPoints[0], out newPoints[1]);
                    platePoints[i][j].FindAboveBelowPoints(out newPoints[2], out newPoints[3]);
                    for (int k = 0; k < 4; k++)
                    {
                        if (!pointActives[newPoints[k].X, newPoints[k].Y])
                        {
                            borderPoints.Enqueue(new OverlapPoint(newPoints[k], i));
                        }
                    }
                }
            }
            while (borderPoints.Count != 0)
            {
                OverlapPoint borderPoint = borderPoints.Dequeue();
                if (!pointActives[borderPoint.X, borderPoint.Y])
                {
                    pointActives[borderPoint.X, borderPoint.Y] = true;
                    SimplePoint oldPoint = new SimplePoint(borderPoint.X, borderPoint.Y);
                    platePoints[borderPoint.plateIndex[0]].Add(new PlatePoint(oldPoint));
                    SimplePoint[] newPointsP = new SimplePoint[4];
                    oldPoint.FindLeftRightPoints(out newPointsP[0], out newPointsP[1]);
                    oldPoint.FindAboveBelowPoints(out newPointsP[2], out newPointsP[3]);
                    for (int k = 0; k < 4; k++)
                    {
                        if (!pointActives[newPointsP[k].X, newPointsP[k].Y])
                        {
                            OverlapPoint newPoint = new OverlapPoint(newPointsP[k], borderPoint.plateIndex[0]);
                            borderPoints.Enqueue(newPoint);
                        }
                    }
                }
            }
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
        /// Uses <see cref="CheckNeighbor"/> to generate a plate starting at the given point, and
        /// transfers it to <see cref="platePoints"/> if there is an empty plate or the plate is
        /// larger than the smallest existing plate.
        /// </summary>
        /// <param name="inPoint">Starting Point.</param>
        private static void PlateMaker(SimplePoint inPoint)
        {
            if (temporaryPoints != null)
            {
                temporaryPoints.Clear();
            }
            if (pointActives[inPoint.X, inPoint.Y])
            {
                CheckNeighbor(inPoint);
                foreach (List<PlatePoint> pointList in platePoints)
                {
                    if (pointList.Count == 0)
                    {
                        pointList.Clear();
                        foreach (PlatePoint iPoint in temporaryPoints)
                        {
                            pointList.Add(iPoint);
                        }
                        return;
                    }
                }
                foreach (List<PlatePoint> pointList in platePoints)
                {
                    if (pointList.Count < temporaryPoints.Count)
                    {
                        pointList.Clear();
                        foreach (PlatePoint iPoint in temporaryPoints)
                        {
                            pointList.Add(iPoint);
                        }
                        return;
                    }
                }
            }
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
                foreach (PlatePoint iPoint in platePoints[i])
                {
                    pointActives[iPoint.X, iPoint.Y] = true;
                }
            });
        }

        /// <summary>
        /// Generates Plates
        /// </summary>
        /// <param name="inRules">Rules to run generation.</param>
        /// <returns>Plate points.</returns>
        public static PlatePoint[,] Run(GenerateRules inRules)
        {
            rules = inRules;
            ConstructData();
            NoiseGenerator();
            NoiseFilter(CutoffMagnitude());
            PlateMaking();
            ExpandPlates();
            PlatePoint[,] output = CompileData();
            return output;
        }
    }
}