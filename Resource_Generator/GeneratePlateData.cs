using System;
using System.Collections.Generic;
using System.Linq;
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
                if (UCheck(newSimplePoint))
                {
                    var newPoint = new OverlapPoint(newSimplePoint, plateIndex);
                    pointQueue.Enqueue(newPoint);
                }
            }
        }

        /// <summary>
        /// Checks neighboring points in pointTracker and adds to pointStack.
        /// </summary>
        /// <param name="iPoint">Point to check neighbors for.</param>
        /// <param name="pointStack">Stack to add appropriate points to stack.</param>
        /// <param name="pointTracker">Matrix to check points for.</param>
        private static void CheckNeighbor(KeyPoint iPoint, Stack<KeyPoint> pointStack, bool[,] pointTracker)
        {
            var newPoints = pointMap[iPoint.X, iPoint.Y].Near.Points;
            foreach (KeyPoint newPoint in newPoints)
            {
                if (pointTracker[newPoint.X,newPoint.Y])
                {
                    pointTracker[newPoint.X, newPoint.Y] = false;
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
            return output[rules.cutOff] == 0
                ? rules.magnitude[0] * 0.99
                : output[rules.cutOff];
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
            if (UCheck(borderPoint))
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
            for (int xP = xMin; xP <= xMax; xP++)
            {
                var x = xP;
                if (x >= 2 * rules.xHalfSize)
                {
                    x -= 2 * rules.xHalfSize;
                }
                for (int y = yMin; y <= yMax; y++)
                {
                    if (centerPoint.Distance(pointMap[x, y]) < generationStat.radiusSquared)
                    {
                        pointMagnitudes[x, y] += generationStat.magnitude;
                    }
                }
            }
        }

        /// <summary>
        /// Increases the Height of all points within circles centered at the given list of points.
        /// </summary>
        /// <param name="points">List of points where the circles are centered.</param>
        /// <param name="generationStat">Generation statistics to use.</param>
        private static void DistributeCircles(GenerationStat generationStat, List<BasePoint> points)
        {
            if (points.Count < rules.plateCount)
            {
                throw new ArgumentException("Point Concentration set too low. Not enough points were generated.");
            }
            Parallel.For(0, (points.Count), (i) =>
            {
                DistributeCircle(points[i], generationStat, pointMagnitudes);
            });
        }

        /// <summary>
        /// Grows plates using a dendritic growth model.
        /// </summary>
        /// <param name="randomNumber">Random number.</param>
        private static void DendriticGrowth(Random randomNumber)
        {
            DendriticPoint.Setup(rules);
            var DendriticPointArray = new List<DendriticPoint>[rules.plateCount];
            for (int i = 0; i < rules.plateCount; i++)
            {
                DendriticPointArray[i] = SeedDendriticList(i, randomNumber);
            }
            while (true)
            {
                var dendriticPointCount = 0;
                for (int i = 0; i < rules.plateCount; i++)
                {
                    DendriticPointArray[i] = GrowDendriticList(DendriticPointArray[i], randomNumber, platePoints[i]);
                    dendriticPointCount += DendriticPointArray[i].Count;
                }
                if (dendriticPointCount == 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Seeds a list of points for dendritic growth.
        /// </summary>
        /// <param name="plateNumber">Plate number to seed.</param>
        /// <param name="randomNumber">Random number used in generating direction.</param>
        /// <returns>List of plate points for dendritic growth.</returns>
        private static List<DendriticPoint> SeedDendriticList(int plateNumber, Random randomNumber)
        {
            var newList = new List<DendriticPoint>();
            var midPoint = (platePoints[plateNumber].Count - 1) / 2;
            var dendriticShape = new DendriticShape(platePoints[plateNumber][midPoint]);
            foreach (var iPoint in platePoints[plateNumber])
            {
                CheckBorderPoints(newList, iPoint, dendriticShape, randomNumber);
            }
            Reduce(ref newList, randomNumber);
            return newList;
        }
        /// <summary>
        /// Reduces a list of dendritic Points.
        /// </summary>
        /// <param name="dendriticPoints">List of dendritic points to reduce.</param>
        /// <param name="randomNumber">Random number to use reducing points.</param>
        private static void Reduce(ref List<DendriticPoint> dendriticPoints, Random randomNumber)
        {
            var unreducedList = dendriticPoints.Distinct().ToList();
            dendriticPoints = new List<DendriticPoint>();
            foreach (var iPoint in unreducedList)
            {
                if (randomNumber.NextDouble() > rules.InitialDendrites)
                {
                    dendriticPoints.Add(iPoint);
                }
            }
        }

        /// <summary>
        /// Checks points near input point and adds border points to point queue.
        /// </summary>
        /// <param name="pointList">Queue to add border points to.</param>
        /// <param name="iPoint">Point to check around.</param>
        /// <param name="dendriticShape">Shape of Dendrites for that plate.</param>
        /// <param name="randomNumber">Random number for dendritic direction.</param>
        private static void CheckBorderPoints(List<DendriticPoint> pointList, IPoint iPoint, DendriticShape dendriticShape, Random randomNumber)
        {
            var newPoints = pointMap[iPoint.X, iPoint.Y].Near.Points;
            foreach (KeyPoint newSimplePoint in newPoints)
            {
                if (UCheck(newSimplePoint))
                {
                    pointList.Add(new DendriticPoint(dendriticShape, newSimplePoint, randomNumber.NextDouble()));
                }
            }
        }

        /// <summary>
        /// Grows a single dendritic point.
        /// </summary>
        /// <param name="dPoint">Dendritic Point to grow.</param>
        /// <param name="randomNumber">Random number used in generating direction.</param>
        /// <param name="plateList">Plate list to add new points to.</param>
        /// <param name="newList">New list of dendritic points to add to.</param>
        private static void GrowDendriticPoint(DendriticPoint dPoint, Random randomNumber, List<KeyPoint> plateList, List<DendriticPoint> newList)
        {
            var tracedPoints = dPoint.Step(randomNumber, out List<DendriticPoint> newPoints);
            foreach (var iPoint in tracedPoints)
            {
                if (UCheck(iPoint))
                {
                    plateList.Add(iPoint);
                    pointActives[iPoint.X, iPoint.Y] = true;
                }
            }
            foreach (var iPoint in newPoints)
            {
                if (UCheck(iPoint))
                {
                    plateList.Add(iPoint.position);
                    pointActives[iPoint.X, iPoint.Y] = true;
                    newList.Add(iPoint);
                }
            }
            if (UCheck(dPoint))
            {
                plateList.Add(dPoint.position);
                pointActives[dPoint.X, dPoint.Y] = true;
                newList.Add(dPoint);
            }
        }


        /// <summary>
        /// Grows list of dendritic points.
        /// </summary>
        /// <param name="inList">List of dendritic points to grow.</param>
        /// <param name="randomNumber">Random number used in generating direction.</param>
        /// <param name="plateList">Plate list to add new points to.</param>
        /// <returns>New List of dendrites.</returns>
        private static List<DendriticPoint> GrowDendriticList(List<DendriticPoint> inList, Random randomNumber, List<KeyPoint> plateList)
        {
            var newList = new List<DendriticPoint>();
            foreach (var dendriticPoint in inList)
            {
                GrowDendriticPoint(dendriticPoint, randomNumber, plateList, newList);
            }
            return newList;
        }

        /// <summary>
        /// Checks to see if point is already added.
        /// </summary>
        /// <param name="iPoint"></param>
        /// <returns></returns>
        private static bool Check(IPoint iPoint)
        {
            if (iPoint == null)
            {
                return false;
            }
            if (pointActives[iPoint.X, iPoint.Y])
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks to see if point is already added.
        /// </summary>
        /// <param name="iPoint"></param>
        /// <returns></returns>
        private static bool UCheck(IPoint iPoint)
        {
            if (iPoint == null)
            {
                return false;
            }
            if (pointActives[iPoint.X, iPoint.Y])
            {
                return false;
            }
            return true;
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
        /// contiguously adjacent to the given point and adds them to the given list,
        /// setting the pointTracker to false in the process.
        /// </summary>
        /// <param name="startingPoint">Starting point to check neighbors.</param>
        /// <param name="temporaryPoints">List of points to add to.</param>
        /// <param name="pointTracker">Point tracker to set for added points.</param>
        private static void FindContiguousPoints(KeyPoint startingPoint, List<KeyPoint> temporaryPoints, bool[,] pointTracker)
        {
            var pointStack = new Stack<KeyPoint>();
            pointTracker[startingPoint.X, startingPoint.Y] = false;
            pointStack.Push(startingPoint);
            while (pointStack.Count != 0)
            {
                var point = pointStack.Pop();
                temporaryPoints.Add(point);
                CheckNeighbor(point, pointStack, pointTracker);
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
        /// Finds valid plate to replace or populate with given list of points and replaces that plate.
        /// </summary>
        /// <param name="temporaryPoints">List of plate points to add to.</param>
        private static void FindReplaceablePlate(List<KeyPoint> temporaryPoints)
        {
            for (int i = 0; i < platePoints.Length; i++)
            {
                if (platePoints[i].Count == 0)
                {
                    platePoints[i] = temporaryPoints;
                    return;
                }
            }
            for (int i = 0; i < platePoints.Length; i++)
            {
                if (platePoints[i].Count < temporaryPoints.Count)
                {
                    platePoints[i] = temporaryPoints;
                    return;
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
        /// <param name="randomNumber">Random number for noise generator.</param>
        private static void NoiseGenerator(Random randomNumber)
        {
            var pointMagnitudes = new double[2 * rules.xHalfSize, rules.ySize];
            for (int i = 0; i < rules.magnitude.Length; i++)
            {
                var circleList = new List<BasePoint>();
                foreach (BasePoint iPoint in pointMap)
                {
                    if (iPoint.TestMomentum(randomNumber.NextDouble(), rules.pointConcentration[i]))
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
            var temporaryPoints = new List<KeyPoint>();
            FindContiguousPoints(inPoint, temporaryPoints, pointActives);
            FindReplaceablePlate(temporaryPoints);
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
        /// Removes points that are not part of the largest contiguous section for each plate.
        /// </summary>
        private static void CleanNonContiguous()
        {
            Parallel.For(0, rules.plateCount, (i) =>
            {
                var checkedPoints = new bool[2 * rules.xHalfSize, rules.ySize];
                foreach (var iPoint in platePoints[i])
                {
                    checkedPoints[iPoint.X, iPoint.Y] = true;
                }
                var biggestList = new List<KeyPoint>();
                foreach (var iPoint in platePoints[i])
                {
                    if (checkedPoints[iPoint.X, iPoint.Y])
                    {
                        var iList = new List<KeyPoint>();
                        FindContiguousPoints(iPoint, iList, checkedPoints);
                        if (iList.Count > biggestList.Count)
                        {
                            biggestList = iList;
                        }
                    }
                }
                foreach (var iPoint in platePoints[i])
                {
                    pointActives[iPoint.X, iPoint.Y] = false;
                }
                platePoints[i] = biggestList;
                foreach (var iPoint in platePoints[i])
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
            ConstructData(inRules);
            var randomNumber = new Random(rules.seed);
            NoiseGenerator(randomNumber);
            NoiseFilter(CutoffMagnitude());
            PlateMaking();
            DendriticGrowth(randomNumber);
            CleanNonContiguous();
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