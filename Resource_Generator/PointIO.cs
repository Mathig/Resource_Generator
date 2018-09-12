using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Resource_Generator
{
    /// <summary>
    /// Allows inputting and outputting data for points.
    /// </summary>
    internal static class PointIO
    {
        /// <summary>
        /// File extension for birth time.
        /// </summary>
        private const string birthTimeExtension = "birthTime.bin";

        /// <summary>
        /// File extension for continental collision buildup.
        /// </summary>
        private const string continentalBuildupExtension = "continentalBuildup.bin";

        /// <summary>
        /// File extension for continental collision recency.
        /// </summary>
        private const string continentalRecencyExtension = "continentalBuildupRecency.bin";

        /// <summary>
        /// File extension for heights bin folder.
        /// </summary>
        private const string heightExtension = "height.bin";

        /// <summary>
        /// File extension for continental information.
        /// </summary>
        private const string isContinentalExtension = "isContinental.bin";

        /// <summary>
        /// File extension for continental collision buildup.
        /// </summary>
        private const string oceanicBuildupExtension = "oceanicBuildup.bin";

        /// <summary>
        /// File extension for continental collision recency.
        /// </summary>
        private const string oceanicRecencyExtension = "oceanicBuildupRecency.bin";

        /// <summary>
        /// File extension for plate number bin folder.
        /// </summary>
        private const string plateNumberExtension = "plateNumber.bin";

        /// <summary>
        /// File extension for X values of birthplace.
        /// </summary>
        private const string xBirthPlaceExtension = "xBirthPlace.bin";

        /// <summary>
        /// File extension for Y values of birthplace.
        /// </summary>
        private const string yBirthPlaceExtension = "yBirthPlace.bin";

        /// <summary>
        /// Finds the minimum and maximum value in a set.
        /// </summary>
        /// <param name="data">Set of data to search.</param>
        /// <param name="min">Minimum value of set.</param>
        /// <param name="max">Maximum value of set.</param>
        private static void FindMinMax<T>(T[,] data, out T min, out T max) where T : IComparable<T>
        {
            min = max = data[0, 0];
            foreach (T item in data)
            {
                if (item.CompareTo(min) < 0)
                {
                    min = item;
                }
                if (item.CompareTo(max) > 0)
                {
                    max = item;
                }
            }
        }

        /// <summary>
        /// Returns a color for a given integer.
        /// </summary>
        /// <param name="number">Integer to determine color.</param>
        /// <returns>Color determined.</returns>
        private static Rgba32 HeightMapColor(int number)
        {
            Rgba32 outColor;
            outColor.A = 255;
            if (number < 0)
            {
                outColor.R = (byte)(-1 * number);
                outColor.G = 0;
                outColor.B = 0;
            }
            else
            {
                outColor.R = 0;
                outColor.G = (byte)number;
                outColor.B = (byte)number;
            }
            return outColor;
        }

        /// <summary>
        /// Stores integer data to image file.
        /// </summary>
        /// <param name="fileName">Full file path name.</param>
        /// <param name="imageData">Data to store.</param>
        private static void SaveImageData(string fileName, int[,] imageData)
        {
            using (Image<Rgba32> image = new Image<Rgba32>(imageData.GetLength(0), imageData.GetLength(1)))
            {
                for (int x = 0; x < imageData.GetLength(0); x++)
                {
                    for (int y = 0; y < imageData.GetLength(1); y++)
                    {
                        image[x, y] = HeightMapColor(imageData[x, y]);
                    }
                }
                image.Save(fileName);
            }
        }

        /// <summary>
        /// Saves plate data as an image file.
        /// </summary>
        /// <param name="fileName">File to store at, not including extension.</param>
        /// <param name="heightData">Height data to store.</param>
        private static void SavePlateImage(string fileName, int[,] plateData)
        {
            double[,] largerData = new double[plateData.GetLength(0), plateData.GetLength(1)];
            for (int x = 0; x < plateData.GetLength(0); x++)
            {
                for (int y = 0; y < plateData.GetLength(1); y++)
                {
                    largerData[x, y] = plateData[x, y];
                }
            }
            int[,] imageData = ScaleData(largerData);
            SaveImageData(fileName + ".png", imageData);
        }

        /// <summary>
        /// Scales data to integers from -255 to 255.
        /// </summary>
        /// <param name="data">Data to scale.</param>
        /// <returns>Data returned as integers from -255 to 255.</returns>
        private static int[,] ScaleData(double[,] data)
        {
            FindMinMax(data, out double min, out double max);
            double dif = max - min;
            int[,] output = new int[data.GetLength(0), data.GetLength(1)];
            Parallel.For(0, data.GetLength(0), (x) =>
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    output[x, y] = (int)Math.Round(((data[x, y] - min) * 510 / dif), 0) - 255;
                }
            });
            return output;
        }

        /// <summary>
        /// Opens a data file and outputs a double array of the data.
        /// </summary>
        /// <param name="fileName">Full file name to open.</param>
        /// <param name="xSize">X size of array.</param>
        /// <param name="ySize">Y size of array.</param>
        /// <returns>Output of file data.</returns>
        /// <exception cref="InvalidDataException">File is not formatted correctly.</exception>
        /// <exception cref="FileNotFoundException">File could not be found.</exception>
        public static double[,] OpenDoubleData(string fileName, int xSize, int ySize)
        {
            double[,] data = new double[xSize, ySize];
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            data[x, y] = reader.ReadDouble();
                        }
                    }
                }
                return data;
            }
            catch (Exception e) when (e is NullReferenceException || e is EndOfStreamException)
            {
                throw new InvalidDataException("Double data file is not formatted correctly.", e);
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Data file missing. Can't find double file: " + fileName, e);
            }
        }

        /// <summary>
        /// Opens point data located at given file.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <param name="rules">Rules for how data will be read.</param>
        /// <returns>Data to output.</returns>
        /// <exception cref="InvalidDataException">File is not formatted correctly.</exception>
        /// <exception cref="FileNotFoundException">File could not be found.</exception>
        public static PlatePoint[,] OpenPointData(string fileName, GeneralRules rules)
        {
            PlatePoint[,] pointData = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
            try
            {
                int[,] plateNumbers = CheapBinaryIO.Read(fileName + plateNumberExtension, rules.plateCount, 2 * rules.xHalfSize, rules.ySize);
                bool[,] isContinental = CheapBinaryIO.ReadBinary(fileName + isContinentalExtension, 2 * rules.xHalfSize, rules.ySize);
                int[,] birthTime = CheapBinaryIO.Read(fileName + birthTimeExtension, rules.currentTime, 2 * rules.xHalfSize, rules.ySize);
                int[,] xBirthPlace = CheapBinaryIO.Read(fileName + xBirthPlaceExtension, 2 * rules.xHalfSize, 2 * rules.xHalfSize, rules.ySize);
                int[,] yBirthPlace = CheapBinaryIO.Read(fileName + yBirthPlaceExtension, rules.ySize, 2 * rules.xHalfSize, rules.ySize);
                int[,] continentalBuildup = CheapBinaryIO.Read(fileName + continentalBuildupExtension, rules.maxBuildup, 2 * rules.xHalfSize, rules.ySize);
                int[,] continentalRecency = CheapBinaryIO.Read(fileName + continentalRecencyExtension, rules.currentTime, 2 * rules.xHalfSize, rules.ySize);
                int[,] oceanicBuildup = CheapBinaryIO.Read(fileName + oceanicBuildupExtension, rules.maxBuildup, 2 * rules.xHalfSize, rules.ySize);
                int[,] oceanicRecency = CheapBinaryIO.Read(fileName + oceanicRecencyExtension, rules.currentTime, 2 * rules.xHalfSize, rules.ySize);

                for (int x = 0; x < 2 * rules.xHalfSize; x++)
                {
                    for (int y = 0; y < rules.ySize; y++)
                    {
                        SimplePoint point = new SimplePoint(x, y);
                        SimplePoint birthPoint = new SimplePoint(xBirthPlace[x, y], yBirthPlace[x, y]);
                        BoundaryHistory history = new BoundaryHistory(continentalBuildup[x, y], continentalRecency[x, y], oceanicBuildup[x, y], oceanicRecency[x, y]);
                        pointData[x, y] = new PlatePoint(point, birthPoint, birthTime[x, y], plateNumbers[x, y], history, isContinental[x, y]);
                    }
                }
                return pointData;
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Data file missing. Can't find: " + e.Message, e.InnerException);
            }
            catch (Exception e) when (e is NullReferenceException || e is EndOfStreamException)
            {
                throw new InvalidDataException("Data file not formatted correctly. See: " + e.Message, e.InnerException);
            }
        }

        /// <summary>
        /// Opens an image file and returns the data.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <returns>Data to retrieve.</returns>
        /// <exception cref="InvalidDataException">File is not formatted correctly.</exception>
        /// <exception cref="FileNotFoundException">File could not be found.</exception>
        public static int[,] OpenPointImage(string fileName)
        {
            try
            {
                int[,] data;
                using (Image<Rgba32> image = Image.Load(fileName + ".png"))
                {
                    data = new int[image.Width, image.Height];
                    List<Rgba32> colorList = new List<Rgba32>();
                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            bool foundColor = false;
                            for (int c = 0; c < colorList.Count; c++)
                            {
                                if (colorList[c] == image[x, y])
                                {
                                    data[x, y] = c;
                                    foundColor = true;
                                    break;
                                }
                            }
                            if (!foundColor)
                            {
                                colorList.Add(image[x, y]);
                                data[x, y] = colorList.Count - 1;
                            }
                        }
                    }
                }
                return data;
            }
            catch (Exception e) when (e is NullReferenceException || e is EndOfStreamException)
            {
                throw new InvalidDataException("Image file is not formatted correctly.", e);
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Data file missing. Can't find image file: " + fileName, e);
            }
        }

        /// <summary>
        /// Saves height data as an image file.
        /// </summary>
        /// <param name="fileName">File to store at, not including extension.</param>
        /// <param name="heightData">Height data to store.</param>
        public static void SaveHeightImage(string fileName, double[,] heightData)
        {
            int[,] imageData = ScaleData(heightData);
            SaveImageData(fileName + ".png", imageData);
        }

        /// <summary>
        /// Saves double data from 2 dimensional array to .bin file.
        /// </summary>
        /// <param name="fileName">Name of file to store at, not including .bin extension.</param>
        /// <param name="mapData">Data to store, in 2 dimesnional array of doubles.</param>
        public static void SaveMapData(string fileName, double[,] mapData)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                foreach (double iData in mapData)
                {
                    writer.Write(iData);
                }
            }
        }

        /// <summary>
        /// Saves point data to the given file.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <param name="type">Rules for how data will be written.</param>
        /// <param name="pointData">Data to save.</param>
        public static void SavePointData(string fileName, GeneralRules rules, PlatePoint[,] pointData)
        {
            int[,] plateNumbers = new int[2 * rules.xHalfSize, rules.ySize];
            int[,] xBirthPlace = new int[2 * rules.xHalfSize, rules.ySize];
            int[,] yBirthPlace = new int[2 * rules.xHalfSize, rules.ySize];
            int[,] birthDate = new int[2 * rules.xHalfSize, rules.ySize];
            bool[,] isContinental = new bool[2 * rules.xHalfSize, rules.ySize];
            int[,] continentalBuildup = new int[2 * rules.xHalfSize, rules.ySize];
            int[,] continentalRecency = new int[2 * rules.xHalfSize, rules.ySize];
            int[,] oceanicBuildup = new int[2 * rules.xHalfSize, rules.ySize];
            int[,] oceanicRecency = new int[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    plateNumbers[x, y] = pointData[x, y].PlateNumber;
                    xBirthPlace[x, y] = pointData[x, y]._birthPlace.X;
                    yBirthPlace[x, y] = pointData[x, y]._birthPlace.Y;
                    birthDate[x, y] = pointData[x, y]._birthDate;
                    isContinental[x, y] = pointData[x, y].IsContinental;
                    continentalBuildup[x, y] = pointData[x, y].History.ContinentalBuildup;
                    continentalRecency[x, y] = pointData[x, y].History.ContinentalRecency;
                    oceanicBuildup[x, y] = pointData[x, y].History.OceanicBuildup;
                    oceanicRecency[x, y] = pointData[x, y].History.OceanicRecency;
                }
            }
            CheapBinaryIO.Write(fileName + plateNumberExtension, plateNumbers, rules.plateCount);
            CheapBinaryIO.Write(fileName + xBirthPlaceExtension, xBirthPlace, 2 * rules.xHalfSize);
            CheapBinaryIO.Write(fileName + yBirthPlaceExtension, yBirthPlace, rules.ySize);
            CheapBinaryIO.Write(fileName + birthTimeExtension, birthDate, rules.ySize);
            CheapBinaryIO.WriteBinary(fileName + isContinentalExtension, isContinental);
            CheapBinaryIO.Write(fileName + continentalBuildupExtension, continentalBuildup, rules.maxBuildup);
            CheapBinaryIO.Write(fileName + continentalRecencyExtension, continentalRecency, rules.currentTime);
            CheapBinaryIO.Write(fileName + oceanicBuildupExtension, oceanicBuildup, rules.maxBuildup);
            CheapBinaryIO.Write(fileName + oceanicRecencyExtension, oceanicRecency, rules.currentTime);
        }

        /// <summary>
        /// Saves point data to an image file.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <param name="type">Rules for how image will be written.</param>
        /// <param name="pointData">Data to save.</param>
        public static void SavePointImage(string fileName, GeneralRules rules, PlatePoint[,] pointData)
        {
            int[,] plateNumbers = new int[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    plateNumbers[x, y] = pointData[x, y].PlateNumber;
                }
            }
            SavePlateImage(fileName + "p", plateNumbers);
        }

        /// <summary>
        /// Saves point plate data to the given file.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <param name="pointData">Data to save.</param>
        public static void SavePointPlateData(string fileName, int[,] pointPlateData)
        {
            int plateCount = 0;
            for (int x = 0; x < pointPlateData.GetLength(0); x++)
            {
                for (int y = 0; y < pointPlateData.GetLength(1); y++)
                {
                    if (pointPlateData[x, y] > plateCount)
                    {
                        plateCount = pointPlateData[x, y];
                    }
                }
            }
            CheapBinaryIO.Write(fileName + plateNumberExtension, pointPlateData, plateCount);
        }
    }
}