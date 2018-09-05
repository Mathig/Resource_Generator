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
        /// Location of file directory.
        /// </summary>
        public static string directory;

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
                        Rgba32 plateColor;
                        plateColor.A = 255;
                        if (imageData[x, y] < 0)
                        {
                            plateColor.R = (byte)(-1 * imageData[x, y]);
                            plateColor.G = 0;
                            plateColor.B = 0;
                        }
                        else
                        {
                            plateColor.R = 0;
                            plateColor.G = (byte)imageData[x, y];
                            plateColor.B = (byte)imageData[x, y];
                        }
                        image[x, y] = plateColor;
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
            SaveImageData(directory + "\\" + fileName + ".png", imageData);
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
        /// Opens a data file and outputs a double array of the data, presumed to be heights. Returns
        /// true if successful, false otherwise.
        /// </summary>
        /// <param name="fileName">File Name (not including extension) to open.</param>
        /// <param name="heights">Output of file data.</param>
        /// <param name="size">Size of map, as <see cref="PlateLayer._layerSize"/>.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        public static bool OpenHeightData(string fileName, int xSize, int ySize, out double[,] heights)
        {
            heights = new double[xSize, ySize];
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            heights[x, y] = reader.ReadDouble();
                        }
                    }
                }
                return true;
            }
            catch (Exception e) when (e is FileNotFoundException || e is NullReferenceException || e is EndOfStreamException)
            {
                return false;
            }
        }

        /// <summary>
        /// Opens point data located at given file.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <param name="rules">Rules for how data will be read.</param>
        /// <param name="pointData">Data to output.</param>
        /// <returns>True if successful and data is good, false otherwise.</returns>
        public static bool OpenPointData(string fileName, GeneralRules rules, out PlatePoint[,] pointData)
        {
            pointData = new PlatePoint[2 * rules.xHalfSize, rules.ySize];
            if (!CheapBinaryIO.Read(directory + "//" + fileName + plateNumberExtension, rules.plateCount, 2 * rules.xHalfSize, rules.ySize, out int[,] plateNumbers))
            { return false; }
            if (!CheapBinaryIO.ReadBinary(directory + "//" + fileName + isContinentalExtension, 2 * rules.xHalfSize, rules.ySize, out bool[,] isContinental))
            { return false; }
            if (!CheapBinaryIO.Read(directory + "//" + fileName + birthTimeExtension, rules.currentTime, 2 * rules.xHalfSize, rules.ySize, out int[,] birthTime))
            { return false; }
            if (!CheapBinaryIO.Read(directory + "//" + fileName + xBirthPlaceExtension, 2 * rules.xHalfSize, 2 * rules.xHalfSize, rules.ySize, out int[,] xBirthPlace))
            { return false; }
            if (!CheapBinaryIO.Read(directory + "//" + fileName + yBirthPlaceExtension, rules.ySize, 2 * rules.xHalfSize, rules.ySize, out int[,] yBirthPlace))
            { return false; }
            if (!CheapBinaryIO.Read(directory + "//" + fileName + continentalBuildupExtension, rules.maxBuildup, 2 * rules.xHalfSize, rules.ySize, out int[,] continentalBuildup))
            { return false; }
            if (!CheapBinaryIO.Read(directory + "//" + fileName + continentalRecencyExtension, rules.currentTime, 2 * rules.xHalfSize, rules.ySize, out int[,] continentalRecency))
            { return false; }
            if (!CheapBinaryIO.Read(directory + "//" + fileName + oceanicBuildupExtension, rules.maxBuildup, 2 * rules.xHalfSize, rules.ySize, out int[,] oceanicBuildup))
            { return false; }
            if (!CheapBinaryIO.Read(directory + "//" + fileName + oceanicRecencyExtension, rules.currentTime, 2 * rules.xHalfSize, rules.ySize, out int[,] oceanicRecency))
            { return false; }

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
            return true;
        }

        /// <summary>
        /// Opens an image file and returns rules for handling the data and the data.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <param name="data">Data to retrieve.</param>
        /// <param name="rules">Rules for how the data is processed.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool OpenPointImage(string fileName, out int[,] data)
        {
            try
            {
                using (Image<Rgba32> image = Image.Load(directory + "\\" + fileName + ".png"))
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
                return true;
            }
            catch (Exception e) when (e is FileNotFoundException || e is NullReferenceException || e is EndOfStreamException)
            {
                data = null;
                return false;
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
            SaveImageData(directory + "\\" + fileName + ".png", imageData);
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
            CheapBinaryIO.Write(directory + "//" + fileName + plateNumberExtension, plateNumbers, rules.plateCount);
            CheapBinaryIO.Write(directory + "//" + fileName + xBirthPlaceExtension, xBirthPlace, 2 * rules.xHalfSize);
            CheapBinaryIO.Write(directory + "//" + fileName + yBirthPlaceExtension, yBirthPlace, rules.ySize);
            CheapBinaryIO.WriteBinary(directory + "//" + fileName + isContinentalExtension, isContinental);
            CheapBinaryIO.Write(directory + "//" + fileName + continentalBuildupExtension, continentalBuildup, rules.maxBuildup);
            CheapBinaryIO.Write(directory + "//" + fileName + continentalRecencyExtension, continentalRecency, rules.currentTime);
            CheapBinaryIO.Write(directory + "//" + fileName + oceanicBuildupExtension, oceanicBuildup, rules.maxBuildup);
            CheapBinaryIO.Write(directory + "//" + fileName + oceanicRecencyExtension, oceanicRecency, rules.currentTime);
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
            CheapBinaryIO.Write(directory + "//" + fileName + plateNumberExtension, pointPlateData, plateCount);
        }
    }
}