using System;

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
        /// Processes commands.
        /// </summary>
        private static void CommandProcessing()
        {
            bool closeProgram = false;
            while (!closeProgram)
            {
                string newCommand = Console.ReadLine();
                switch (newCommand)
                {
                    case "Copy Plates":
                        closeProgram = !CopyPlates();
                        break;

                    case "Generate Plates":
                        closeProgram = !GeneratePlates();
                        break;

                    case "Move Plates":
                        closeProgram = !MovePlates();
                        break;

                    case "Move Directory":
                        closeProgram = MoveDirectory();
                        break;

                    case "Help":
                        Help();
                        break;

                    case "Close":
                        closeProgram = true;
                        break;

                    default:
                        Console.WriteLine("Invalid Command. Use Help to list available commands.");
                        break;
                }
            }
        }

        /// <summary>
        /// Copies an image plate file into a data file.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        private static bool CopyPlates()
        {
            Console.WriteLine("Input Plate image file name.");
            FileName image = new FileName(Console.ReadLine());
            if (!image.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            Console.WriteLine("Input Plate data file name.");
            FileName dataFile = new FileName(Console.ReadLine());
            if (!dataFile.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            if (!PointIO.OpenPointImage(image.name, out PlatePoint[,] data, out GeneralRules rules))
            {
                Console.WriteLine("Image file is corrupted.");
                return false;
            }
            PointIO.SavePointData(dataFile.name, rules, data);
            return true;
        }

        /// <summary>
        /// Generates tectonic plates.
        /// </summary>
        /// <returns>Returns true if successful, false if crashed.</returns>
        private static bool GeneratePlates()
        {
            Console.WriteLine("Input Generate Rules File Name.");
            FileName rulesLocation = new FileName(Console.ReadLine());
            if (!rulesLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            Console.WriteLine("Input Data File Name.");
            FileName outDataLocation = new FileName(Console.ReadLine());
            if (!outDataLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            Console.WriteLine("Input Image File Name.");
            FileName outImageLocation = new FileName(Console.ReadLine());
            if (!outImageLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            if (!RulesInput.LoadGenerateRules(rulesLocation.name, out GenerateRules rules))
            {
                Console.WriteLine("Rules are invalid. See default rules.");
                return false;
            }
            PlatePoint[,] pointData = GeneratePlateData.Run(rules);
            PointIO.SavePointData(outDataLocation.name, rules, pointData);
            PointIO.SavePointImage(outImageLocation.name, rules, pointData);
            return true;
        }

        /// <summary>
        /// Lists options for possible commands.
        /// </summary>
        private static void Help()
        {
            Console.WriteLine("Possible commands include:");
            Console.WriteLine("Help: Lists possible commands.");
            Console.WriteLine("Copy Plates: Converts an image file of data to a binary data file.");
            Console.WriteLine("Move Directory: Moves the directory.");
            Console.WriteLine("Move Plates: Moves the Plates.");
            Console.WriteLine("Generate Plates: Generates the Plates");
            Console.WriteLine("Close: Closes Program.");
        }

        /// <summary>
        /// Moves the directory.
        /// </summary>
        private static bool MoveDirectory()
        {
            Console.WriteLine("Enter the new directory.");
            string tempDirectory = Console.ReadLine();
            if (DirectoryManager.Test(tempDirectory))
            {
                DirectoryManager.CoreDirectory(out string coreDirectory);
                DirectoryManager.GenerateDefaultFiles(tempDirectory);
                DirectoryManager.Save(coreDirectory, tempDirectory);
                _directory = tempDirectory;
                Console.WriteLine("Directory successfully moved to: " + _directory);
                return true;
            }
            else
            {
                Console.WriteLine("Directory is invalid.");
                return false;
            }
        }

        /// <summary>
        /// Moves tectonic plates.
        /// </summary>
        /// <returns>Returns true if successful, false if crashed.</returns>
        private static bool MovePlates()
        {
            Console.WriteLine("Input Move Rules File Name.");
            FileName rulesLocation = new FileName(Console.ReadLine());
            if (!rulesLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            Console.WriteLine("Input Plate Data File Name.");
            FileName plateDataLocation = new FileName(Console.ReadLine());
            if (!plateDataLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            Console.WriteLine("Input Source Point Data File Name.");
            FileName inPointDataLocation = new FileName(Console.ReadLine());
            if (!inPointDataLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            Console.WriteLine("Input Destination Point Data File Name.");
            FileName outPointDataLocation = new FileName(Console.ReadLine());
            if (!outPointDataLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            Console.WriteLine("Input Image File Name.");
            FileName plateImageLocation = new FileName(Console.ReadLine());
            if (!plateImageLocation.IsValid())
            {
                Console.WriteLine("File name is invalid. " + validFileNameCriteria);
                return false;
            }
            if (!RulesInput.LoadMoveRules(rulesLocation.name, out MoveRules rules))
            {
                Console.WriteLine("Rules are invalid. See default rules.");
                return false;
            }
            if (!PlateIO.OpenPlateData(plateDataLocation.name, out PlateData plateData))
            {
                Console.WriteLine("Plate Data is corrupted.");
                return false;
            }
            if (!PointIO.OpenPointData(inPointDataLocation.name, rules, out PlatePoint[,] inPointData))
            {
                Console.WriteLine("Point Data is corrupted.");
                return false;
            }
            PlatePoint[,] outPointData = MovePlatesData.Run(rules, plateData, inPointData);
            PointIO.SavePointData(outPointDataLocation.name, rules, outPointData);
            PointIO.SavePointImage(plateImageLocation.name, rules, outPointData);
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
                RulesInput.directory = _directory;
                PlateIO.directory = _directory;
                PointIO.directory = _directory;
                CommandProcessing();
            }
        }
    }
}