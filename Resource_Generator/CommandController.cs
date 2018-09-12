using System;
using System.IO;

namespace Resource_Generator
{
    /// <summary>
    /// Processes commands from console.
    /// </summary>
    internal static class CommandController
    {
        /// <summary>
        /// Describes the criteria for a valid file name, according to our custom definition.
        /// </summary>
        private const string validFileNameCriteria = "Use only alpha-numeric or _.";

        /// <summary>
        /// Directory for file processing.
        /// </summary>
        private static string _directory;

        /// <summary>
        /// Processes commands and handles errors.
        /// </summary>
        private static void CommandProcessing()
        {
            bool closeProgram = false;
            while (!closeProgram)
            {
                string newCommand = Console.ReadLine();
                try
                {
                    closeProgram = !RunCommand(newCommand);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(validFileNameCriteria);
                }
                catch (SystemException e) when (e is FileNotFoundException || e is InvalidDataException)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Copies an image plate file into a data file.
        /// </summary>
        private static void CopyPlates()
        {
            FileName image = InputFileName("Plate image");
            FileName dataFile = InputFileName("Plate data");
            int[,] data = PointIO.OpenPointImage(image.Name);
            PointIO.SavePointPlateData(dataFile.Name, data);
        }

        /// <summary>
        /// Generates altitude map for points.
        /// </summary>
        private static void GenerateAltitudes()
        {
            FileName rulesLocation = InputFileName("Generate Height Rules");
            FileName inDataLocation = InputFileName("Point Data");
            FileName outDataLocation = InputFileName("Height Data");
            AltitudeMapRules rules = RulesInput.LoadAltitudeRules(rulesLocation.Name);
            PlatePoint[,] inPointData = PointIO.OpenPointData(inDataLocation.Name, rules);
            double[,] heightMap = GenerateAltitudeMap.Run(inPointData, rules);
            PointIO.SaveHeightImage(outDataLocation.Name, heightMap);
            PointIO.SaveMapData(outDataLocation.Name + ".bin", heightMap);
        }

        /// <summary>
        /// Erodes plates.
        /// </summary>
        private static void GenerateErosion()
        {
            FileName rulesLocation = InputFileName("Erosion Rule");
            FileName inHeightLocation = InputFileName("Height Data");
            FileName inRainLocation = InputFileName("Rainfall Data");
            FileName outErosionLocation = InputFileName("Erosion Data");
            FileName outIsWaterLocation = InputFileName("Is Water Data");
            ErosionMapRules rules = RulesInput.LoadErosionRules(rulesLocation.Name);
            double[,] heightMap = PointIO.OpenDoubleData(inHeightLocation.Name + ".bin", 2 * rules.xHalfSize, rules.ySize);
            double[,] rainfallMap = new double[2 * rules.xHalfSize, rules.ySize];
            for (int i = 0; i < rules.numberSeasons; i++)
            {
                double[,] rainfallMapTemp = PointIO.OpenDoubleData(inHeightLocation.Name + ".bin", 2 * rules.xHalfSize, rules.ySize);
                for (int x = 0; x < 2 * rules.xHalfSize; x++)
                {
                    for (int y = 0; y < rules.ySize; y++)
                    {
                        rainfallMap[x, y] += rainfallMapTemp[x, y];
                    }
                }
            }
            GenerateErosionMap.Run(heightMap, rainfallMap, rules, out bool[,] isWater, out double[,] erosionMap);
            PointIO.SaveHeightImage(outErosionLocation.Name, erosionMap);
            PointIO.SaveMapData(outErosionLocation.Name + ".bin", erosionMap);
            CheapBinaryIO.WriteBinary(outIsWaterLocation.Name + ".bin", isWater);
        }

        /// <summary>
        /// Generates tectonic plates.
        /// </summary>
        private static void GeneratePlates()
        {
            FileName rulesLocation = InputFileName("Generate Rules");
            FileName outDataLocation = InputFileName("Point Data");
            FileName outImageLocation = InputFileName("Point Image");
            GenerateRules rules = RulesInput.LoadGenerateRules(rulesLocation.Name);
            PlatePoint[,] pointData = GeneratePlateData.Run(rules);
            PointIO.SavePointData(outDataLocation.Name, rules, pointData);
            PointIO.SavePointImage(outImageLocation.Name, rules, pointData);
            GC.Collect();
        }

        /// <summary>
        /// Generates rainfall map.
        /// </summary>
        private static void GenerateRainfall()
        {
            FileName rulesLocation = InputFileName("Rainfall Rules");
            FileName inDataLocation = InputFileName("Height Map Data");
            FileName outDataLocation = InputFileName("Rainfall Data");
            RainfallMapRules rules = RulesInput.LoadRainfallRules(rulesLocation.Name);
            double[,] heightMap = PointIO.OpenDoubleData(inDataLocation.Name + ".bin", 2 * rules.xHalfSize, rules.ySize);
            double[,,] rainfallMap = GenerateRainfallMap.Run(heightMap, rules);
            for (int i = 0; i < rainfallMap.GetLength(2); i++)
            {
                double[,] rainfallMapTemp = new double[rainfallMap.GetLength(0), rainfallMap.GetLength(1)];
                for (int x = 0; x < rainfallMap.GetLength(0); x++)
                {
                    for (int y = 0; y < rainfallMap.GetLength(1); y++)
                    {
                        rainfallMapTemp[x, y] = rainfallMap[x, y, i];
                    }
                }
                PointIO.SaveHeightImage(outDataLocation.Name + i.ToString(), rainfallMapTemp);
                PointIO.SaveMapData(outDataLocation.Name + i.ToString() + ".bin", rainfallMapTemp);
            }
        }

        /// <summary>
        /// Lists options for possible commands.
        /// </summary>
        private static void Help()
        {
            Console.WriteLine("Possible commands include:");
            Console.WriteLine("Erode Plates: Erodes plates.");
            Console.WriteLine("Help: Lists possible commands.");
            Console.WriteLine("Copy Plates: Converts an image file of data to a binary data file.");
            Console.WriteLine("Move Directory: Moves the directory.");
            Console.WriteLine("Move Plates: Moves the Plates.");
            Console.WriteLine("Generate Rainfall Map: Generates Rainfall Map.");
            Console.WriteLine("Generate Plates: Generates the Plates");
            Console.WriteLine("Generate Altitude Map: Generates altitude map.");
            Console.WriteLine("Close: Closes Program.");
        }

        /// <summary>
        /// Asks the user to input a file name.
        /// </summary>
        /// <param name="nameDescriptor">Descriptor for user to reference the file name.</param>
        /// <returns>File name.</returns>
        /// <exception cref="FormatException">Yields exception if the file name is invalid.</exception>
        private static FileName InputFileName(string nameDescriptor)
        {
            Console.WriteLine("Input " + nameDescriptor + " file name.");
            FileName fileName = new FileName(Console.ReadLine());
            fileName.CheckValidity();
            return fileName;
        }

        /// <summary>
        /// Moves the directory.
        /// </summary>
        /// <exception cref="InvalidDataException">Directory was invalid.</exception>
        private static void MoveDirectory()
        {
            Console.WriteLine("Enter the new directory.");
            string tempDirectory = Console.ReadLine();
            if (DirectoryManager.Test(tempDirectory))
            {
                DirectoryManager.CoreDirectory(out string coreDirectory);
                DirectoryManager.GenerateDefaultFiles(tempDirectory);
                DirectoryManager.Save(coreDirectory, tempDirectory);
                _directory = tempDirectory;
                FileName.directory = _directory;
                Console.WriteLine("Directory successfully moved to: " + _directory);
            }
            else
            {
                throw new InvalidDataException("Directory is invalid");
            }
        }

        /// <summary>
        /// Moves tectonic plates.
        /// </summary>
        private static void MovePlates()
        {
            FileName rulesLocation = InputFileName("Move Rules");
            FileName plateDataLocation = InputFileName("Plate Data");
            FileName inPointDataLocation = InputFileName("Source Point Data");
            FileName outPointDataLocation = InputFileName("Destination Point Data");
            FileName plateImageLocation = InputFileName("Image");
            MoveRules rules = RulesInput.LoadMoveRules(rulesLocation.Name);
            PlateData plateData = PlateIO.OpenPlateData(plateDataLocation.Name);
            PlatePoint[,] inPointData = PointIO.OpenPointData(inPointDataLocation.Name, rules);
            PlatePoint[,] outPointData = MovePlatesData.Run(rules, plateData, inPointData);
            PointIO.SavePointData(outPointDataLocation.Name, rules, outPointData);
            PointIO.SavePointImage(plateImageLocation.Name, rules, outPointData);
        }

        /// <summary>
        /// Runs a command based on the string.
        /// </summary>
        /// <param name="command">String of command to run.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private static bool RunCommand(string command)
        {
            switch (command)
            {
                case "Erode Plates":
                    GenerateErosion();
                    break;

                case "Copy Plates":
                    CopyPlates();
                    break;

                case "Generate Plates":
                    GeneratePlates();
                    break;

                case "Generate Rainfall Map":
                    GenerateRainfall();
                    break;

                case "Generate Altitude Map":
                    GenerateAltitudes();
                    break;

                case "Move Plates":
                    MovePlates();
                    break;

                case "Move Directory":
                    MoveDirectory();
                    break;

                case "Help":
                    Help();
                    break;

                case "Close":
                    return false;

                default:
                    Console.WriteLine("Invalid Command. Use Help to list available commands.");
                    break;
            }
            return true;
        }

        /// <summary>
        /// Initial entry point. Sets up directory and... then redirects control to <see cref="CommandProcessing"/>.
        /// </summary>
        public static void Run()
        {
            if (DirectoryManager.Setup(out string directory))
            {
                _directory = directory;
                FileName.directory = directory;
                CommandProcessing();
            }
        }
    }
}