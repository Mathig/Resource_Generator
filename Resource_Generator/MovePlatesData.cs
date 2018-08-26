using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resource_Generator
{    /// <summary>
     /// Class for moving plates
     /// </summary>
    internal static class MovePlatesData
    {
        /// <summary>
        /// List of new continental collision data.
        /// </summary>
        private static int[,] newContinentalCollision;

        /// <summary>
        /// List of new oceanic collision data.
        /// </summary>
        private static int[,] newOceanicCollision;

        /// <summary>
        /// Contains all the plates on the map, within their associated plate.
        /// </summary>
        private static Plate[] plates;

        /// <summary>
        /// List of active points, used for processing points.
        /// </summary>
        private static bool[,] pointActives;

        /// <summary>
        /// Contains history of each point.
        /// </summary>
        private static PlatePoint[,] pointHistories;

        /// <summary>
        /// Rules that define plate movement.
        /// </summary>
        private static MoveRules rules;

        /// <summary>
        /// Calculates the average height of adjacent points in <see cref="pointHeights"/>.
        /// </summary>
        /// <param name="originalPoint">Original point before transformation.</param>
        /// <param name="reversedPoint">Point after transformation.</param>
        /// <param name="inPlate">Index of plate to scan for with <see cref="pointPastPlates"/>.</param>
        /// <param name="angle">Angle of motion.</param>
        /// <returns>Average of nearby points from <see cref="PastHeights"/>.</returns>
        private static PlatePoint AdjacentAverage(int inPlate, BasePoint originalPoint, SimplePoint reversedPoint, double[] angle)
        {
            originalPoint.GridTransform(angle, out double xCoord, out double yCoord);
            SimplePoint[] potentialNeighbors = new SimplePoint[8];
            reversedPoint.FindAboveBelowPoints(out potentialNeighbors[0], out potentialNeighbors[1]);
            reversedPoint.FindLeftRightPoints(out potentialNeighbors[2], out potentialNeighbors[3]);
            potentialNeighbors[0].FindLeftRightPoints(out potentialNeighbors[4], out potentialNeighbors[5]);
            potentialNeighbors[1].FindLeftRightPoints(out potentialNeighbors[6], out potentialNeighbors[7]);
            List<PlatePoint> pointList = new List<PlatePoint>();
            foreach (SimplePoint iPoint in potentialNeighbors)
            {
                if (pointHistories[iPoint.X, iPoint.Y].PlateNumber == inPlate)
                {
                    pointList.Add(pointHistories[iPoint.X, iPoint.Y]);
                }
            }
            SimplePoint newPosition = new SimplePoint(originalPoint.X, originalPoint.Y);
            return PlatePoint.ResolveSamePlateNeighbors(pointList, xCoord, yCoord, newPosition, inPlate);
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
                    output[iPoint.X, iPoint.Y] = iPoint;
                    if (newContinentalCollision[iPoint.X, iPoint.Y] != 0)
                    {
                        iPoint.History.ContinentalBuildup += newContinentalCollision[iPoint.X, iPoint.Y];
                        iPoint.History.ContinentalRecency = rules.currentTime;
                    }
                    if (newOceanicCollision[iPoint.X, iPoint.Y] != 0)
                    {
                        iPoint.History.OceanicBuildup += newOceanicCollision[iPoint.X, iPoint.Y];
                        iPoint.History.OceanicRecency = rules.currentTime;
                    }
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
            pointHistories = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
            newContinentalCollision = new int[2 * rules.xHalfSize, rules.ySize];
            newOceanicCollision = new int[2 * rules.xHalfSize, rules.ySize];
            plates = new Plate[rules.plateCount];
            for (int i = 0; i < rules.plateCount; i++)
            {
                plates[i] = new Plate();
            }
        }

        /// <summary>
        /// Modifies a point by a continental convergent event.
        /// </summary>
        /// <param name="triggerPoint">The point which called the event.</param>
        /// <param name="changedPoint">The point to be affected.</param>
        private static void ContinentalConvergent(BasePoint triggerPoint, BasePoint changedPoint)
        {
            newContinentalCollision[changedPoint.X, changedPoint.Y] += 1;
        }

        /// <summary>
        /// Processes a continental point which is at a convergent boundary.
        /// </summary>
        /// <param name="iPoint">Point in question.</param>
        /// <param name="index">Index of point.</param>
        private static void ContinentalConvergent(OverlapPoint overlapPoint, int index)
        {
            BasePoint triggerPoint = new BasePoint(overlapPoint.X, overlapPoint.Y);
            BasePoint borderPoint = FindBorderPoint(triggerPoint, overlapPoint.plateIndex[index]);
            List<BasePoint> affectedPoints = FindAffectedPoints(borderPoint, overlapPoint.plateIndex[index]);
            foreach (BasePoint iPoint in affectedPoints)
            {
                ContinentalConvergent(triggerPoint, iPoint);
            }
            RemovePoint(triggerPoint, overlapPoint.plateIndex[index]);
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
                    plates[borderPoint.plateIndex[0]].PlatePoints.Add(new PlatePoint(borderPoint, rules.currentTime));
                    SimplePoint[] newPointsP = new SimplePoint[4];
                    borderPoint.FindLeftRightPoints(out newPointsP[0], out newPointsP[1]);
                    borderPoint.FindAboveBelowPoints(out newPointsP[2], out newPointsP[3]);
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
        /// Finds all the points affected by the convergent boundary.
        /// </summary>
        /// <param name="triggerPoint">Border point to start search from.</param>
        /// <param name="plateIndex">Plate to search on.</param>
        /// <returns>Affected points.</returns>
        private static List<BasePoint> FindAffectedPoints(BasePoint triggerPoint, int plateIndex)
        {
            List<BasePoint> list = new List<BasePoint>();
            list.Add(triggerPoint);
            return list;
        }

        /// <summary>
        /// Finds the closest border point to the triggered point.
        /// </summary>
        /// <param name="triggerPoint">Triggered point to start search from.</param>
        /// <param name="plateIndex">Plate to search on.</param>
        /// <returns>Border point.</returns>
        private static BasePoint FindBorderPoint(BasePoint triggerPoint, int plateIndex)
        {
            return triggerPoint;
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
                        rawOutput.Add(new OverlapPoint(plates[i].PlatePoints[j]));
                    }
                }
            }
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
                    plates[i].PlatePoints[j].Transform(angle);
                    pointActives[plates[i].PlatePoints[j].X, plates[i].PlatePoints[j].Y] = true;
                }
            });
        }

        /// <summary>
        /// Processes an oceanic point which is at a convergent boundary.
        /// </summary>
        /// <param name="iPoint">Point in question.</param>
        /// <param name="index">Index of point.</param>
        private static void OceanicConvergent(OverlapPoint overlapPoint, int index)
        {
            BasePoint triggerPoint = new BasePoint(overlapPoint.X, overlapPoint.Y);
            BasePoint borderPoint = FindBorderPoint(triggerPoint, overlapPoint.plateIndex[index]);
            List<BasePoint> affectedPoints = FindAffectedPoints(borderPoint, overlapPoint.plateIndex[index]);
            foreach (BasePoint iPoint in affectedPoints)
            {
                OceanicConvergent(triggerPoint, iPoint);
            }
            RemovePoint(triggerPoint, overlapPoint.plateIndex[index]);
        }

        /// <summary>
        /// Modifies a point by an oceanic convergent event.
        /// </summary>
        /// <param name="triggerPoint">The point which called the event.</param>
        /// <param name="changedPoint">The point to be affected.</param>
        private static void OceanicConvergent(BasePoint triggerPoint, BasePoint changedPoint)
        {
            newOceanicCollision[changedPoint.X, changedPoint.Y] += 1;
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
        /// Inputs data to set up <see cref="pointHistories"/>.
        /// </summary>
        /// <param name="points">Plate points to input.</param>
        private static void PointInput(PlatePoint[,] points)
        {
            Parallel.For(0, rules.plateCount, (i) =>
            {
                foreach (PlatePoint iPoint in plates[i].PlatePoints)
                {
                    pointHistories[iPoint.X, iPoint.Y] = new PlatePoint(iPoint);
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
            rawList.Sort();
            List<OverlapPoint> output = new List<OverlapPoint>();
            int index = 0;
            for (int i = 1; i < rawList.Count; i++)
            {
                if (rawList[i - 1].CompareTo(rawList[i]) == 0)
                {
                    rawList[index].plateIndex.Add(rawList[i].plateIndex[0]);
                    rawList[index].isContinentalIndex.Add(rawList[i].isContinentalIndex[0]);
                }
                else
                {
                    output.Add(rawList[index]);
                    index = i;
                }
            }
            output.Add(rawList[index]);
            return output;
        }

        /// <summary>
        /// Removes the given point from the Plates matrix given the plate index.
        /// </summary>
        /// <param name="doomedPoint">Point to remove.</param>
        /// <param name="plateIndex">Plate index of the point.</param>
        private static void RemovePoint(BasePoint doomedPoint, int plateIndex)
        {
            PlatePoint tempPoint = new PlatePoint(new SimplePoint(doomedPoint.X, doomedPoint.Y));
            for (int i = 0; i < plates[plateIndex].PlatePoints.Count; i++)
            {
                if (tempPoint.CompareTo(plates[plateIndex].PlatePoints[i]) == 0)
                {
                    plates[plateIndex].PlatePoints.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Resolves non-trivial point overlaps.
        /// </summary>
        /// <param name="input">Point overlaps to resolve.</param>
        private static void ResolveOtherOverlap(List<OverlapPoint> input)
        {
            foreach (OverlapPoint iPoint in input)
            {
                if (iPoint.IsOceanicOnly(out int firstContinental))
                {
                    for (int i = 1; i < iPoint.plateIndex.Count; i++)
                    {
                        OceanicConvergent(iPoint, i);
                    }
                }
                else
                {
                    for (int i = 0; i < iPoint.plateIndex.Count; i++)
                    {
                        if (firstContinental != i)
                        {
                            if (!iPoint.isContinentalIndex[i])
                            {
                                OceanicConvergent(iPoint, i);
                            }
                            else
                            {
                                ContinentalConvergent(iPoint, i);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds all points with two or more entries in the same plate and resolves the overlap.
        /// </summary>
        private static void ResolveSameOverlap()
        {
            Parallel.For(0, rules.plateCount, (i) =>
            {
                plates[i].PlatePoints.Sort();
                int indexA = 0;
                int indexB = 1;
                while (indexB < plates[i].PlatePoints.Count)
                {
                    if (plates[i].PlatePoints[indexA].CompareTo(plates[i].PlatePoints[indexB]) == 0)
                    {
                        List<PlatePoint> samePoints = new List<PlatePoint>();
                        samePoints.Add(plates[i].PlatePoints[indexB]);
                        for (int j = indexB + 1; j < plates[i].PlatePoints.Count; j++)
                        {
                            if (plates[i].PlatePoints[indexA].CompareTo(plates[i].PlatePoints[j]) == 0)
                            {
                                samePoints.Add(plates[i].PlatePoints[j]);
                            }
                            else
                            {
                                break;
                            }
                        }
                        plates[i].PlatePoints[indexA].ResolveSamePlateOverlap(samePoints);
                        for (int j = 1; j < samePoints.Count; j++)
                        {
                            plates[i].PlatePoints.RemoveAt(samePoints.Count - j);
                        }
                    }
                    else
                    {
                        indexA = indexB;
                        indexB++;
                    }
                }
            });
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
                if (pointHistories[reversedPoint.X, reversedPoint.Y].PlateNumber == i)
                {
                    plates[i].PlatePoints.Add(AdjacentAverage(i, input, reversedPoint, angle));
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
            ResolveSameOverlap();
            ResolveOtherOverlap(FindRawOverlap());
            return CompileData();
        }
    }
}