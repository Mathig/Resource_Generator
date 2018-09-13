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
        /// Compiles data from Plates into an array of points for output.
        /// </summary>
        /// <returns>Array of points for output.</returns>
        private static PlatePoint[,] CompileData()
        {
            var output = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
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
        private static void ContinentalConvergent(in BasePoint triggerPoint, in BasePoint changedPoint)
        {
            newContinentalCollision[changedPoint.X, changedPoint.Y] += 1;
        }

        /// <summary>
        /// Processes a continental point which is at a convergent boundary.
        /// </summary>
        /// <param name="overlapPoint">Point in question.</param>
        /// <param name="plateIndex">Index of plate for point.</param>
        private static void ContinentalConvergent(in BasePoint overlapPoint, int plateIndex)
        {
            var borderPoint = FindBorderPoint(overlapPoint, plateIndex);
            var affectedPoints = FindAffectedPoints(borderPoint, plateIndex);
            foreach (BasePoint iPoint in affectedPoints)
            {
                ContinentalConvergent(overlapPoint, iPoint);
            }
        }

        /// <summary>
        /// Performs plate convergence for a given point.
        /// </summary>
        /// <param name="points">Points to perform convergence for.</param>
        /// <returns>PlatePoint to keep.</returns>
        private static PlatePoint Convergent(List<PlatePoint> points)
        {
            var basePoint = points[0];
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].IsContinental)
                {
                    basePoint = points[i];
                    break;
                }
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (basePoint.PlateNumber != points[i].PlateNumber)
                {
                    Convergent(points[i]);
                }
            }
            return basePoint;
        }

        /// <summary>
        /// Performs plate convergence for a given point, which is the point that subducts.
        /// </summary>
        /// <param name="platePoint">Point which is being subducted.</param>
        private static void Convergent(PlatePoint platePoint)
        {
            var position = new BasePoint(platePoint.X, platePoint.Y);
            if (platePoint.IsContinental)
            {
                ContinentalConvergent(position, platePoint.PlateNumber);
            }
            else
            {
                OceanicConvergent(position, platePoint.PlateNumber);
            }
        }

        /// <summary>
        /// Expands each plate one pixel in every direction until no point outside of all plates exists.
        /// </summary>
        private static void ExpandPlates()
        {
            var borderPoints = new Queue<OverlapPoint>();
            for (int i = 0; i < rules.plateCount; i++)
            {
                for (int j = 0; j < plates[i].PlatePoints.Count; j++)
                {
                    var newPoints = plates[i].PlatePoints[j].FindNeighborPoints();
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
                var borderPoint = borderPoints.Dequeue();
                if (!pointActives[borderPoint.X, borderPoint.Y])
                {
                    pointActives[borderPoint.X, borderPoint.Y] = true;
                    plates[borderPoint.plateIndex[0]].PlatePoints.Add(new PlatePoint(borderPoint, rules.currentTime));
                    var newPointsP = borderPoint.FindNeighborPoints();
                    for (int k = 0; k < 4; k++)
                    {
                        if (!pointActives[newPointsP[k].X, newPointsP[k].Y])
                        {
                            var newPoint = new OverlapPoint(newPointsP[k], borderPoint.plateIndex[0]);
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
        private static List<BasePoint> FindAffectedPoints(in BasePoint triggerPoint, int plateIndex)
        {
            var list = new List<BasePoint>
            {
                triggerPoint
            };
            return list;
        }

        /// <summary>
        /// Finds the closest border point to the triggered point.
        /// </summary>
        /// <param name="triggerPoint">Triggered point to start search from.</param>
        /// <param name="plateIndex">Plate to search on.</param>
        /// <returns>Border point.</returns>
        private static BasePoint FindBorderPoint(in BasePoint triggerPoint, int plateIndex)
        {
            return triggerPoint;
        }

        /// <summary>
        /// Finds points at the same plate that are neighbors of the target point.
        /// </summary>
        /// <param name="inPlate">Target plate.</param>
        /// <param name="targetPoint">Target point to look around.</param>
        /// <returns>List of neighboring points around target point.</returns>
        private static List<PlatePoint> FindNeighborPlatePoints(int inPlate, IPoint targetPoint)
        {
            var potentialNeighbors = targetPoint.FindNeighborPoints();
            var pointList = new List<PlatePoint>();
            foreach (SimplePoint iPoint in potentialNeighbors)
            {
                if (pointHistories[iPoint.X, iPoint.Y].PlateNumber == inPlate)
                {
                    pointList.Add(pointHistories[iPoint.X, iPoint.Y]);
                }
            }
            return pointList;
        }

        /// <summary>
        /// Finds points that match the target point using the plate point indexes given.
        /// </summary>
        /// <param name="targetPoint">Target point to look for.</param>
        /// <param name="platePointIndexes">Indexes to check.</param>
        /// <returns>List of matching points.</returns>
        private static List<PlatePoint> FindPoints(PlatePoint targetPoint, int[] platePointIndexes)
        {
            var foundPoints = new List<PlatePoint>();
            for (int i = 0; i < rules.plateCount; i++)
            {
                if (platePointIndexes[i] < plates[i].PlatePoints.Count)
                {
                    var platePoint = plates[i].PlatePoints[platePointIndexes[i]];
                    if (targetPoint.CompareTo(platePoint) == 0)
                    {
                        foundPoints.Add(platePoint);
                    }
                }
            }
            return foundPoints;
        }

        /// <summary>
        /// Moves all points in each plate using forward movement.
        /// </summary>
        private static void InitialMove()
        {
            Parallel.For(0, rules.plateCount, (i) =>
            {
                var angle = new double[3] { plates[i].Direction[0], plates[i].Direction[1], rules.timeStep * plates[i].Speed };
                var previousPointCount = plates[i].PlatePoints.Count;
                for (int j = 0; j < previousPointCount; j++)
                {
                    var point = plates[i].PlatePoints[j];
                    if (point.Transform(angle, out List<SimplePoint> bonusPoints, rules.ySize))
                    {
                        var Rangle = new double[3] { angle[0], angle[1], -1 * angle[2] };
                        foreach (SimplePoint bonusPoint in bonusPoints)
                        {
                            plates[i].PlatePoints.Add(InterlacePlatePoint(i, bonusPoint, Rangle));
                        }
                    }
                    pointActives[point.X, point.Y] = true;
                }
            });
        }

        /// <summary>
        /// Creates a plate point based on interlacing from the original point.
        /// </summary>
        /// <param name="newPoint">Point after transformation.</param>
        /// <param name="inPlate">Index of plate to scan for with <see cref="pointPastPlates"/>.</param>
        /// <param name="angle">Angle to reverse trace with.</param>
        /// <returns>Average of nearby points from <see cref="PastHeights"/>.</returns>
        private static PlatePoint InterlacePlatePoint(int inPlate, SimplePoint newPoint, double[] angle)
        {
            var oldPoint = new BasePoint((new BasePoint(newPoint)).Transform(angle));
            oldPoint.GridTransform(angle, out double xCoord, out double yCoord);
            var pointList = FindNeighborPlatePoints(inPlate, oldPoint);
            return PlatePoint.ResolveSamePlateNeighbors(pointList, xCoord, yCoord, newPoint, inPlate);
        }

        /// <summary>
        /// Processes an oceanic point which is at a convergent boundary.
        /// </summary>
        /// <param name="overlapPoint">Point in question.</param>
        /// <param name="plateIndex">Index of plate of point.</param>
        private static void OceanicConvergent(in BasePoint overlapPoint, int plateIndex)
        {
            var borderPoint = FindBorderPoint(overlapPoint, plateIndex);
            var affectedPoints = FindAffectedPoints(borderPoint, plateIndex);
            foreach (BasePoint iPoint in affectedPoints)
            {
                OceanicConvergent(overlapPoint, iPoint);
            }
        }

        /// <summary>
        /// Modifies a point by an oceanic convergent event.
        /// </summary>
        /// <param name="triggerPoint">The point which called the event.</param>
        /// <param name="changedPoint">The point to be affected.</param>
        private static void OceanicConvergent(in BasePoint triggerPoint, in BasePoint changedPoint)
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
                    pointHistories[iPoint.X, iPoint.Y] = iPoint;
                }
            });
        }

        /// <summary>
        /// Resolves non-trivial point overlaps.
        /// </summary>
        private static void ResolveOtherOverlap()
        {
            var index = new int[rules.plateCount];
            var newLists = new List<PlatePoint>[rules.plateCount];
            for (int i = 0; i < rules.plateCount; i++)
            {
                plates[i].PlatePoints.Sort();
                newLists[i] = new List<PlatePoint>();
            }
            for (int y = 0; y < rules.ySize; y++)
            {
                for (int x = 0; x < 2 * rules.xHalfSize; x++)
                {
                    var targetPoint = new PlatePoint(new SimplePoint(x, y));
                    var points = FindPoints(targetPoint, index);
                    if (points.Count == 1)
                    {
                        newLists[points[0].PlateNumber].Add(points[0]);
                        index[points[0].PlateNumber]++;
                    }
                    else if (points.Count > 1)
                    {
                        var basePoint = Convergent(points);
                        newLists[basePoint.PlateNumber].Add(basePoint);
                        for (int i = 0; i < points.Count; i++)
                        {
                            index[points[i].PlateNumber]++;
                        }
                    }
                }
            }
            for (int i = 0; i < rules.plateCount; i++)
            {
                plates[i].PlatePoints = newLists[i];
            }
        }

        /// <summary>
        /// Finds all points with two or more entries in the same plate and resolves the overlap.
        /// </summary>
        private static void ResolveSameOverlap()
        {
            Parallel.ForEach(plates, (plate) =>
            {
                plate.PlatePoints.Sort();
                var tempList = new List<PlatePoint>();
                var index = 0;
                for (int i = 1; i < plate.PlatePoints.Count; i++)
                {
                    if (index < i)
                    {
                        if (plate.PlatePoints[index].CompareTo(plate.PlatePoints[i]) == 0)
                        {
                            var samePoints = new List<PlatePoint>();
                            for (int j = index; j < plate.PlatePoints.Count; j++)
                            {
                                if (plate.PlatePoints[index].CompareTo(plate.PlatePoints[j]) == 0)
                                {
                                    samePoints.Add(plate.PlatePoints[j]);
                                }
                                else
                                {
                                    plate.PlatePoints[index].ResolveSamePlateOverlap(samePoints);
                                    tempList.Add(plate.PlatePoints[index]);
                                    index = j;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            tempList.Add(plate.PlatePoints[index]);
                            index = i;
                        }
                    }
                }
                if (plate.PlatePoints[plate.PlatePoints.Count - 2].CompareTo(plate.PlatePoints[plate.PlatePoints.Count - 1]) != 0)
                {
                    tempList.Add(plate.PlatePoints[plate.PlatePoints.Count - 1]);
                }
                plate.PlatePoints = tempList;
            });
        }

        /// <summary>
        /// Adds points to plates by tracing them backwards.
        /// </summary>
        /// <param name="input">Point to add.</param>
        private static void ReverseAdd(in BasePoint input)
        {
            for (int i = 0; i < rules.plateCount; i++)
            {
                var angle = new double[3]
                {
                    plates[i].Direction[0], plates[i].Direction[1], -1 * plates[i].Speed * rules.timeStep
                };
                var reversedPoint = input.Transform(angle);
                if (pointHistories[reversedPoint.X, reversedPoint.Y].PlateNumber == i)
                {
                    plates[i].PlatePoints.Add(InterlacePlatePoint(i, new SimplePoint(input.X, input.Y), angle));
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
        /// <param name="inPoints">Initial plate point data.</param>
        /// <returns>Plate points.</returns>
        public static PlatePoint[,] Run(MoveRules inRules, PlateData plateData, PlatePoint[,] inPoints)
        {
            rules = inRules;
            BasePoint.MapSetup(rules.xHalfSize, rules.ySize);
            for (int i = 0; i < rules.numberSteps; i++)
            {
                ConstructData();
                PlateInput(plateData, inPoints);
                PointInput(inPoints);
                InitialMove();
                SecondaryMove();
                ResolveSameOverlap();
                ResolveOtherOverlap();
                pointActives = new bool[2 * rules.xHalfSize, rules.ySize];
                Parallel.ForEach(plates, (plate) =>
                {
                    foreach (PlatePoint iPoint in plate.PlatePoints)
                    {
                        pointActives[iPoint.X, iPoint.Y] = true;
                    }
                });
                ExpandPlates();
                inRules.currentTime += inRules.timeStep;
                inPoints = CompileData();
            }
            Parallel.For(0, 2 * rules.xHalfSize, (x) =>
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    if (inPoints[x, y]._birthDate == 0)
                    {
                        inPoints[x, y].IsContinental = true;
                    }
                }
            });
            return CompileData();
        }
    }
}