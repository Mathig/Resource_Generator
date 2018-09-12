using System;
using System.Threading.Tasks;

namespace Resource_Generator
{
    /// <summary>
    /// Generates rainfall map.
    /// </summary>
    internal class GenerateRainfallMap
    {
        /// <summary>
        /// Points used for calculating nearest neighbors.
        /// </summary>
        private static BasePoint[,] point;

        /// <summary>
        /// Rules for rainfall generation.
        /// </summary>
        private static RainfallMapRules rules;

        /// <summary>
        /// Finds the points lying on the Intertropical Convergence Zone, based on the planet's tilt.
        /// </summary>
        /// <param name="heightMap">Height map of planet.</param>
        /// <param name="pitchEffect">Effect pitch has on points.</param>
        /// <returns>Y altitude of points lying on the zone.</returns>
        private static int[] BaseITC(double[,] heightMap, double pitchEffect)
        {
            int[] initialITC = new int[2 * rules.xHalfSize];
            double threshold = pitchEffect * 0.5 * rules.ySize;
            int midPoint = (int)Math.Round(0.5 * rules.ySize);
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                double counter = 0;
                if (pitchEffect > 0)
                {
                    for (int y = midPoint; y < rules.ySize; y++)
                    {
                        if (heightMap[x, y] == 0)
                        {
                            counter += rules.oceanWeight;
                        }
                        else
                        {
                            counter += rules.altitudeWeight * heightMap[x, y];
                        }
                        if (counter > threshold)
                        {
                            initialITC[x] = y;
                            break;
                        }
                    }
                }
                else
                {
                    for (int y = midPoint; y > 0; y--)
                    {
                        if (heightMap[x, y] == 0)
                        {
                            counter += rules.oceanWeight;
                        }
                        else
                        {
                            counter += rules.altitudeWeight * heightMap[x, y];
                        }
                        if (counter > threshold)
                        {
                            initialITC[x] = y;
                            break;
                        }
                    }
                }
            }
            return initialITC;
        }

        /// <summary>
        /// Calculates a base pressure given the ITC zone.
        /// </summary>
        /// <param name="ITCZone">ITC zone to generate base pressure field with.</param>
        /// <returns>Base pressure field.</returns>
        private static double[,] BasePressure(int[] ITCZone, double[,] heightMap, double yearFactor)
        {
            double[,] output = new double[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                int ITCZonepoint = ITCZone[x];
                int polarOne = (int)Math.Round((double)ITCZonepoint / 3);
                int polarTwo = ITCZonepoint + (int)Math.Round(2 * (double)(rules.ySize - ITCZonepoint) / 3);
                int subTropicalOne = (int)Math.Round(2 * (double)ITCZonepoint / 3);
                int subTropicalTwo = ITCZonepoint + (int)Math.Round((double)(rules.ySize - ITCZonepoint) / 3);
                for (int y = 0; y < ITCZonepoint; y++)
                {
                    if (heightMap[x, y] != 0)
                    {
                        output[x, y] = rules.landWeight * yearFactor;
                    }
                }
                for (int y = ITCZonepoint; y < rules.ySize; y++)
                {
                    if (heightMap[x, y] != 0)
                    {
                        output[x, y] = -1 * rules.landWeight * yearFactor;
                    }
                }
                output[x, polarOne] += -20 * rules.ySize;
                output[x, subTropicalOne] += 10 * rules.ySize;
                output[x, subTropicalTwo] += 10 * rules.ySize;
                output[x, polarOne] += -10 * rules.ySize;
                output[x, polarTwo] += -10 * rules.ySize;
            }
            return output;
        }

        /// <summary>
        /// Constructs variables and allocates space for persistent variables.
        /// </summary>
        private static void ConstructData()
        {
            BasePoint.MapSetup(rules.xHalfSize, rules.ySize);
            point = new BasePoint[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    point[x, y] = new BasePoint(x, y);
                }
            }
        }

        /// <summary>
        /// Generates a map showing how much each point moves to neighboring points, based on a gradient and corriolis effect.
        /// </summary>
        /// <param name="pressureMap">Pressure map to move.</param>
        /// <returns>Wind map, displayed as x,y,i where i indicates magnitude in left, right, top, below directions.</returns>
        private static double[,,] PressureGradient(double[,] pressureMap)
        {
            double[,,] output = new double[2 * rules.xHalfSize, rules.ySize, 4];
            int midPoint = (int)Math.Round(0.5 * rules.ySize);
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    SimplePoint[] nearPoints = point[x, y].FindNeighborPoints();
                    double leftHeight = pressureMap[nearPoints[2].X, nearPoints[2].Y];
                    double rightHeight = pressureMap[nearPoints[3].X, nearPoints[3].Y];
                    double aboveHeight = pressureMap[nearPoints[0].X, nearPoints[0].Y];
                    double belowHeight = pressureMap[nearPoints[1].X, nearPoints[1].Y];
                    double xDerivative = rightHeight - leftHeight;
                    double yDerivative = aboveHeight - belowHeight;
                    double rDerivative = Math.Sqrt(xDerivative * xDerivative + yDerivative * yDerivative);
                    double aDerivative = Math.Atan2(yDerivative, xDerivative);
                    if (y < midPoint)
                    {
                        aDerivative += Math.PI * 0.2;
                    }
                    else
                    {
                        aDerivative -= Math.PI * 0.2;
                    }
                    xDerivative = rDerivative * Math.Cos(aDerivative);
                    yDerivative = rDerivative * Math.Sin(aDerivative);
                    if (xDerivative > 0)
                    {
                        output[x, y, 0] = xDerivative * point[x, midPoint]._sinPhi / point[x, y]._sinPhi;
                        output[x, y, 1] = 0;
                    }
                    else
                    {
                        output[x, y, 0] = 0;
                        output[x, y, 1] = xDerivative * point[x, midPoint]._sinPhi / point[x, y]._sinPhi;
                    }
                    if (yDerivative < 0)
                    {
                        output[x, y, 2] = yDerivative;
                        output[x, y, 3] = 0;
                    }
                    else
                    {
                        output[x, y, 2] = 0;
                        output[x, y, 3] = yDerivative;
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Calculates amount of rainfall for a given instance of time.
        /// </summary>
        /// <param name="windMap">Map of wind directions.</param>
        /// <param name="heightMap">Altitude map.</param>
        /// <returns>Rainfall map.</returns>
        private static double[,] RainFlow(double[,,] windMap, double[,] heightMap)
        {
            double[,] tempRainfallOne = new double[2 * rules.xHalfSize, rules.ySize];
            double[,] tempRainfallTwo = new double[2 * rules.xHalfSize, rules.ySize];
            double[,] output = new double[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    if (heightMap[x, y] == 0)
                    {
                        tempRainfallOne[x, y] = 10;
                    }
                }
            }
            for (int i = 0; i < 10; i++)
            {
                //double rainfallReduction = (double)(i + 1) * (i + 1) / (100);
                double rainfallReduction = 1;
                for (int x = 0; x < 2 * rules.xHalfSize; x++)
                {
                    for (int y = 0; y < rules.ySize; y++)
                    {
                        SimplePoint[] nearPoints = point[x, y].FindNeighborPoints();
                        for (int j = 0; j < 4; j++)
                        {
                            tempRainfallTwo[nearPoints[j].X, nearPoints[j].Y] = windMap[x, y, 0] * tempRainfallOne[x, y];
                        }
                        tempRainfallTwo[x, y] = tempRainfallOne[x, y] * (1 - windMap[x, y, 0] - windMap[x, y, 1] - windMap[x, y, 2] - windMap[x, y, 3]);
                        output[x, y] += rainfallReduction * tempRainfallOne[x, y] * heightMap[x, y];
                        tempRainfallTwo[x, y] = (1 - rainfallReduction) * tempRainfallTwo[x, y] * heightMap[x, y] / 3000;
                    }
                }
                tempRainfallOne = tempRainfallTwo;
            }
            return output;
        }

        /// <summary>
        /// Calculates a base pressure given the ITC zone.
        /// </summary>
        /// <param name="ITCZone">ITC zone to generate base pressure field with.</param>
        /// <returns>Base pressure field.</returns>
        private static double[,] SmoothPressure(double[,] rawPressure)
        {
            double[,] output = new double[2 * rules.xHalfSize, rules.ySize];
            double[,] tempInput = rawPressure;
            for (int i = 0; i < 10; i++)
            {
                for (int x = 0; x < 2 * rules.xHalfSize; x++)
                {
                    for (int y = 0; y < rules.ySize; y++)
                    {
                        output[x, y] = 0.6 * tempInput[x, y];
                        SimplePoint[] nearPoints = point[x, y].FindNeighborPoints();
                        for (int j = 0; j < 4; j++)
                        {
                            output[x, y] = 0.1 * tempInput[nearPoints[j].X, nearPoints[j].Y];
                        }
                    }
                }
                tempInput = output;
            }
            return output;
        }

        /// <summary>
        /// Generates rainfall map given a height map.
        /// </summary>
        /// <param name="heightMap">Height map to base rainfall off of.</param>
        /// <param name="rules">Rules for how generation is to take place.</param>
        /// <returns>Rainfall map</returns>
        public static double[,,] Run(double[,] heightMap, RainfallMapRules inRules)
        {
            rules = inRules;
            ConstructData();
            double[,,] output = new double[2 * rules.xHalfSize, rules.ySize, rules.numberSeasons];
            Parallel.For(0, (rules.numberSeasons), (i) =>
            {
                double yearEffect = Math.PI * (2 * (i / rules.numberSeasons) - 1);
                double pitchEffect = Math.Sin(yearEffect) * rules.axisTilt;
                int[] ITCZone = BaseITC(heightMap, pitchEffect);
                double[,] pressureMap = BasePressure(ITCZone, heightMap, yearEffect);
                pressureMap = SmoothPressure(pressureMap);
                double[,,] windMap = PressureGradient(pressureMap);
                double[,] smallRainMap = RainFlow(windMap, heightMap);
                for (int x = 0; x < 2 * rules.xHalfSize; x++)
                {
                    for (int y = 0; y < rules.ySize; y++)
                    {
                        output[x, y, i] = smallRainMap[x, y];
                    }
                }
            });
            return output;
        }
    }
}