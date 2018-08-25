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
        /// File extension for heights bin folder.
        /// </summary>
        private const string heightExtension = "height.bin";

        /// <summary>
        /// File extension for plate number bin folder.
        /// </summary>
        private const string plateNumberExtension = "plateNumber.bin";

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
        /// Opens a data file and outputs a double array of the data, presumed to be heights. Returns
        /// true if successful, false otherwise.
        /// </summary>
        /// <param name="fileName">File Name (not including extension) to open.</param>
        /// <param name="heights">Output of file data.</param>
        /// <param name="size">Size of map, as <see cref="PlateLayer._layerSize"/>.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private static bool OpenHeightData(string fileName, int xSize, int ySize, out double[,] heights)
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
        /// Saves height data as an image file.
        /// </summary>
        /// <param name="fileName">File to store at, not including extension.</param>
        /// <param name="heightData">Height data to store.</param>
        private static void SaveHeightImage(string fileName, double[,] heightData)
        {
            int[,] imageData = ScaleData(heightData);
            SaveImageData(directory + "\\" + fileName + ".png", imageData);
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
        /// Saves double data from 2 dimensional array to .bin file.
        /// </summary>
        /// <param name="fileName">Name of file to store at, not including .bin extension.</param>
        /// <param name="mapData">Data to store, in 2 dimesnional array of doubles.</param>
        private static void SaveMapData(string fileName, double[,] mapData)
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
            if (!OpenHeightData(directory + "//" + fileName + heightExtension, 2 * rules.xHalfSize, rules.ySize, out double[,] heights))
            { return false; }
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    pointData[x, y] = new PlatePoint(x, y, plateNumbers[x, y], heights[x, y]);
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
        public static bool OpenPointImage(string fileName, out PlatePoint[,] data, out GeneralRules rules)
        {
            rules = new GeneralRules();
            try
            {
                using (Image<Rgba32> image = Image.Load(directory + "\\" + fileName + ".png"))
                {
                    rules.xHalfSize = (int)(0.5 * image.Width);
                    rules.ySize = image.Height;
                    data = new PlatePoint[image.Width, image.Height];
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
                                    data[x, y] = new PlatePoint(x, y, c);
                                    foundColor = true;
                                    break;
                                }
                            }
                            if (!foundColor)
                            {
                                colorList.Add(image[x, y]);
                                data[x, y] = new PlatePoint(x, y, colorList.Count - 1);
                            }
                        }
                    }
                    rules.plateCount = colorList.Count;
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
        /// Saves point data to the given file.
        /// </summary>
        /// <param name="fileName">File to open.</param>
        /// <param name="type">Rules for how data will be written.</param>
        /// <param name="pointData">Data to save.</param>
        public static void SavePointData(string fileName, GeneralRules rules, PlatePoint[,] pointData)
        {
            int[,] plateNumbers = new int[2 * rules.xHalfSize, rules.ySize];
            double[,] heights = new double[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    plateNumbers[x, y] = pointData[x, y].PlateNumber;
                    heights[x, y] = pointData[x, y].Height;
                }
            }
            SaveMapData(directory + "//" + fileName + heightExtension, heights);
            CheapBinaryIO.Write(directory + "//" + fileName + plateNumberExtension, plateNumbers, rules.plateCount);
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
            double[,] heights = new double[2 * rules.xHalfSize, rules.ySize];
            for (int x = 0; x < 2 * rules.xHalfSize; x++)
            {
                for (int y = 0; y < rules.ySize; y++)
                {
                    plateNumbers[x, y] = pointData[x, y].PlateNumber;
                    heights[x, y] = pointData[x, y].Height;
                }
            }
            SavePlateImage(fileName + "p", plateNumbers);
            SaveHeightImage(fileName + "h", heights);
        }
    }
}