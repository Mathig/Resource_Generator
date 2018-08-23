using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resource_Generator
{    /// <summary>
     /// Class for moving plates
     /// </summary>
    internal static class MovePlatesData
    {
        /// <summary>
        /// Contains all the plates on the map, within their associated plate.
        /// </summary>
        private static Plate[] plates;

        /// <summary>
        /// List of active points, used for processing points.
        /// </summary>
        private static bool[,] pointActives;

        /// <summary>
        /// Contains height of points.
        /// </summary>
        private static double[,] pointHeights;

        /// <summary>
        /// Stores which plate a point was previously part of.
        /// </summary>
        private static int[,] pointPastPlates;

        /// <summary>
        /// Rules that define plate movement.
        /// </summary>
        private static MoveRules rules;

        /// <summary>
        /// Adds new point to matrix based on potential rules.
        /// </summary>
        /// <param name="plateIndex">Index of plate that point belongs to.</param>
        /// <param name="point">Point location.</param>
        private static void AddNewPoint(int plateIndex, SimplePoint point)
        {
            plates[plateIndex].PlatePoints.Add(new PlatePoint(point, plateIndex));
        }

        /// <summary>
        /// Calculates the average height of adjacent points in <see cref="pointHeights"/>.
        /// </summary>
        /// <param name="originalPoint">Original point before transformation.</param>
        /// <param name="reversedPoint">Point after transformation.</param>
        /// <param name="inPlate">Index of plate to scan for with <see cref="pointPastPlates"/>.</param>
        /// <param name="angle">Angle of motion.</param>
        /// <returns>Average of nearby points from <see cref="PastHeights"/>.</returns>
        private static double AdjacentHeightAverage(int inPlate, BasePoint originalPoint, SimplePoint reversedPoint, double[] angle)
        {
            originalPoint.GridTransform(angle, out double xCoord, out double yCoord);
            SimplePoint[] potentialNeighbors = new SimplePoint[8];
            reversedPoint.FindAboveBelowPoints(out potentialNeighbors[0], out potentialNeighbors[1]);
            reversedPoint.FindLeftRightPoints(out potentialNeighbors[2], out potentialNeighbors[3]);
            potentialNeighbors[0].FindLeftRightPoints(out potentialNeighbors[4], out potentialNeighbors[5]);
            potentialNeighbors[1].FindLeftRightPoints(out potentialNeighbors[6], out potentialNeighbors[7]);
            double average = 0;
            double weight = 0;
            foreach (SimplePoint iPoint in potentialNeighbors)
            {
                if (pointPastPlates[iPoint.X, iPoint.Y] == inPlate && pointHeights[iPoint.X, iPoint.Y] != 0)
                {
                    BasePoint tempPoint = new BasePoint(iPoint);
                    double iWeight = 1 / (tempPoint.Distance(xCoord, yCoord));
                    average += iWeight * pointHeights[iPoint.X, iPoint.Y];
                    weight += iWeight;
                }
            }
            return (average / weight);
        }

        /// <summary>
        /// Compiles data from Plates into an array of points for output.
        /// </summary>
        /// <returns>Array of points for output.</returns>
        private static PlatePoint[,] CompileData()
        {
            PlatePoint[,] output = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
            Parallel.For(0, (rules.plateCount), (i) =>
            {
                foreach (PlatePoint iPoint in plates[i].PlatePoints)
                {
                    output[iPoint.X, iPoint.Y] = new PlatePoint(iPoint);
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
            pointActives = new bool[2 * rules.xHalfSize, rules.ySize];
            pointHeights = new double[2 * rules.xHalfSize, rules.ySize];
            pointPastPlates = new int[2 * rules.xHalfSize, rules.ySize];
            plates = new Plate[rules.plateCount];
            for (int i = 0; i < rules.plateCount; i++)
            {
                plates[i] = new Plate();
            }
        }

        /// <summary>
        /// Expands each plate one pixel in every direction until no point outside of all plates exists.
        /// </summary>
        private static void ExpandPlates()
        {
            Queue<OverlapPoint> borderPoints = new Queue<OverlapPoint>();
            for (int i = 0; i < rules.plateCount; i++)
            {
                for (int j = 0; j < plates[i].PlatePoints.Count; j++)
                {
                    SimplePoint[] newPoints = new SimplePoint[4];
                    plates[i].PlatePoints[j].FindLeftRightPoints(out newPoints[0], out newPoints[1]);
                    plates[i].PlatePoints[j].FindAboveBelowPoints(out newPoints[2], out newPoints[3]);
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
                    AddNewPoint(borderPoint.plateIndex[0], oldPoint);
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
        /// Finds all points with two or more entries in multiple plates and outputs them in a list of <see cref="OverlapPoint"/>.
        /// </summary>
        /// <returns>List of <see cref="OverlapPoint"/>.</returns>
        private static List<OverlapPoint> FindRawOverlap()
        {
            List<OverlapPoint> rawOutput = new List<OverlapPoint>();
            bool[,] ActivePointsTwo = new bool[2 * rules.xHalfSize, rules.ySize];
            Parallel.For(0, plates.Length, (i) =>
            {
                foreach (PlatePoint iPoint in plates[i].PlatePoints)
                {
                    if (pointActives[iPoint.X, iPoint.Y])
                    {
                        pointActives[iPoint.X, iPoint.Y] = false;
                    }
                    else
                    {
                        ActivePointsTwo[iPoint.X, iPoint.Y] = true;
                    }
                }
            });
            for (int i = 0; i < rules.plateCount; i++)
            {
                for (int j = 0; j < plates[i].PlatePoints.Count; j++)
                {
                    if (ActivePointsTwo[plates[i].PlatePoints[j].X, plates[i].PlatePoints[j].Y])
                    {
                        rawOutput.Add(new OverlapPoint(plates[i].PlatePoints[j].X, plates[i].PlatePoints[j].Y, i, j));
                    }
                }
            }
            rawOutput.Sort();
            return RefineOverlap(rawOutput);
        }

        /// <summary>
        /// Moves all points in each plate using forward movement.
        /// </summary>
        private static void InitialMove()
        {
            Parallel.For(0, rules.plateCount, (i) =>
            {
                double[] angle = new double[3] { plates[i].Direction[0], plates[i].Direction[1], rules.timeStep * plates[i].Speed };
                for (int j = 0; j < plates[i].PlatePoints.Count; j++)
                {
                    plates[i].PlatePoints[j] = new PlatePoint(plates[i].PlatePoints[j].Transform(angle), i, plates[i].PlatePoints[j].Height);
                }
            });
            Parallel.For(0, rules.plateCount, (i) =>
            {
                foreach (PlatePoint iPoint in plates[i].PlatePoints)
                {
                    pointActives[iPoint.X, iPoint.Y] = true;
                }
            });
        }

        /// <summary>
        /// Inputs data to set up plates.
        /// </summary>
        /// <param name="plateData">Plate data to input.</param>
        /// <param name="points">Plate points to input.</param>
        private static void PlateInput(PlateData plateData, PlatePoint[,] points)
        {
            Parallel.For(0, rules.plateCount, (i) =>
            {
                plates[i].Speed = plateData.Speed[i];
                plates[i].Direction = plateData.Direction[i];
            });
            foreach (PlatePoint iPoint in points)
            {
                plates[iPoint.PlateNumber].PlatePoints.Add(new PlatePoint(iPoint));
            }
        }

        /// <summary>
        /// Inputs data to set up <see cref="pointHeights"/>, and <see cref="pointPastPlates"/>.
        /// </summary>
        /// <param name="points">Plate points to input.</param>
        private static void PointInput(PlatePoint[,] points)
        {
            Parallel.For(0, rules.plateCount, (i) =>
            {
                foreach (PlatePoint iPoint in plates[i].PlatePoints)
                {
                    pointHeights[iPoint.X, iPoint.Y] = iPoint.Height;
                    pointPastPlates[iPoint.X, iPoint.Y] = iPoint.PlateNumber;
                }
            });
        }

        /// <summary>
        /// Cleans list to contain compacted data of overlapped points.
        /// </summary>
        /// <param name="rawList">Raw list of points to comb.</param>
        /// <returns>List of <see cref="OverlapPoint"/>.</returns>
        private static List<OverlapPoint> RefineOverlap(List<OverlapPoint> rawList)
        {
            List<OverlapPoint> output = new List<OverlapPoint>();
            int index = 0;
            for (int i = 1; i < rawList.Count; i++)
            {
                if (rawList[i - 1].CompareTo(rawList[i]) == 0)
                {
                    rawList[index].plateIndex.Add(rawList[i].plateIndex[0]);
                    rawList[index].pointIndex.Add(rawList[i].pointIndex[0]);
                }
                else
                {
                    output.Add(rawList[index]);
                    index = i;
                }
            }
            return output;
        }

        /// <summary>
        /// Resolves non-trivial point overlaps.
        /// </summary>
        /// <param name="input">Non-trivial point overlaps to resolve.</param>
        private static void ResolveNontrivialOverlap(List<OverlapPoint> input)
        {
            foreach (OverlapPoint iPoint in input)
            {
                for (int i = 0; i < iPoint.plateIndex.Count; i++)
                {
                    plates[iPoint.plateIndex[0]].PlatePoints[iPoint.pointIndex[0]].Height += rules.OverlapFactor *
                        plates[iPoint.plateIndex[i]].PlatePoints[iPoint.pointIndex[i]].Height;
                    if (i != 0)
                    {
                        plates[iPoint.plateIndex[i]].PlatePoints.RemoveAt(iPoint.pointIndex[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Resolves trivial overlap points, and condences non-trivial overlap points.
        /// </summary>
        /// <param name="rawList">Raw list of overlap points.</param>
        /// <returns>Non-trivial list of overlap points.</returns>
        private static List<OverlapPoint> ResolveTrivialOverlap(List<OverlapPoint> input)
        {
            List<OverlapPoint> rawOutput = new List<OverlapPoint>();
            foreach (OverlapPoint iPoint in input)
            {
                int index = 0;
                bool previouslyNontrivial = false;
                for (int i = 1; i < iPoint.plateIndex.Count; i++)
                {
                    if (plates[iPoint.plateIndex[i]].PlatePoints[iPoint.pointIndex[i]].Height != 0)
                    {
                        if (plates[iPoint.plateIndex[index]].PlatePoints[iPoint.pointIndex[index]].Height != 0)
                        {
                            if (!previouslyNontrivial)
                            {
                                previouslyNontrivial = true;
                                rawOutput.Add(new OverlapPoint(iPoint.X, iPoint.Y, iPoint.plateIndex[index], iPoint.pointIndex[index]));
                            }
                            rawOutput.Add(new OverlapPoint(iPoint.X, iPoint.Y, iPoint.plateIndex[i], iPoint.pointIndex[i]));
                        }
                        else
                        {
                            plates[iPoint.plateIndex[index]].PlatePoints.RemoveAt(iPoint.pointIndex[index]);
                            index = i;
                        }
                    }
                    else
                    {
                        plates[iPoint.plateIndex[i]].PlatePoints.RemoveAt(iPoint.pointIndex[i]);
                    }
                }
            }
            return RefineOverlap(rawOutput);
        }

        /// <summary>
        /// Adds points to plates by tracing them backwards.
        /// </summary>
        /// <param name="input">Point to add.</param>
        /// <param name="timeStep">Time factor for moving plates.</param>
        private static void ReverseAdd(BasePoint input)
        {
            for (int i = 0; i < rules.plateCount; i++)
            {
                double[] angle = new double[3]
                {
                    plates[i].Direction[0], plates[i].Direction[1], -1 * plates[i].Speed * rules.timeStep
                };
                SimplePoint reversedPoint = input.Transform(angle);
                if (pointPastPlates[reversedPoint.X, reversedPoint.Y] == i)
                {
                    double average = AdjacentHeightAverage(i, input, reversedPoint, angle);
                    plates[i].PlatePoints.Add(new PlatePoint(input, average));
                    pointActives[input.X, input.Y] = true;
                }
            }
        }

        /// <summary>
        /// Moves all remaining points in each plate using reverse movement.
        /// </summary>
        private static void SecondaryMove()
        {
            Parallel.For(0, 2 * rules.xHalfSize, (x) =>
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    if (!pointActives[x, y])
                    {
                        ReverseAdd(new BasePoint(x, y));
                    }
                }
            });
        }

        /// <summary>
        /// Moves Plates.
        /// </summary>
        /// <param name="inRules">Rules to move plates.</param>
        /// <param name="plateData">Initial plate data.</param>
        /// <param name="points">Initial plate point data.</param>
        /// <returns>Plate points.</returns>
        public static PlatePoint[,] Run(MoveRules inRules, PlateData plateData, PlatePoint[,] inPoints)
        {
            rules = inRules;
            BasePoint.MapSetup(rules.xHalfSize, rules.ySize);
            ConstructData();
            PlateInput(plateData, inPoints);
            PointInput(inPoints);
            InitialMove();
            SecondaryMove();
            ExpandPlates();
            List<OverlapPoint> rawOverlapList = FindRawOverlap();
            List<OverlapPoint> overlapList = ResolveTrivialOverlap(rawOverlapList);
            ResolveNontrivialOverlap(overlapList);
            return CompileData();
        }
    }
}