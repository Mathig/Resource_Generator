using System;
using System.IO;

namespace Resource_Generator
{
    /// <summary>
    /// Used to check and create directories, as well as handle involved errors.
    /// </summary>
    internal class DirectoryManager
    {
        /// <summary>
        /// Example format for directory redirect.
        /// </summary>
        private const string formatExample = "\"D:\\\\Documents\"";

        /// <summary>
        /// File name for testing if directories exist.
        /// </summary>
        private const string pingFileName = "\\pingFile";

        /// <summary>
        /// Default program folder name.
        /// </summary>
        private const string programFolder = "\\Resource_Generator";

        /// <summary>
        /// Filename for directory redirect.
        /// </summary>
        private const string redirectExtension = "\\DirectoryRedirect.txt";

        /// <summary>
        /// Attempts to read the core directory for an existing directory.
        /// </summary>
        /// <param name="coreDirectory">Core directory location.</param>
        /// <param name="directory">Contained directory if successful, root directory otherwise.</param>
        /// <returns>True if successful, false if file does not exist.</returns>
        private static bool FindStored(string coreDirectory, out string directory)
        {
            try
            {
                using (StreamReader reader = new StreamReader(File.Open(coreDirectory + redirectExtension, FileMode.Open)))
                {
                    directory = reader.ReadLine();
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Saved Directory not found.");
                directory = "";
                return false;
            }
        }

        /// <summary>
        /// Suggests the user create the directory manually, with instructions.
        /// </summary>
        /// <param name="coreDirectory">Root directory where redirect is located at.</param>
        private static void SuggestManualCreation(string coreDirectory)
        {
            Console.WriteLine("Manually enter the desired directory at: \"" + coreDirectory + "\".");
            Console.WriteLine("File should be named \"" + redirectExtension + "\", with new directory path on first line inside.");
            Console.WriteLine("Format should be " + formatExample + ".");
        }

        /// <summary>
        /// Tries to create a directory in the default location.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        private static bool TryToCreateDefaultDirectory(string coreDirectory, out string directory)
        {
            directory = "";
            try
            {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            catch (PlatformNotSupportedException)
            {
                Console.WriteLine("Platform not supported for accessing MyDocuments.");
                SuggestManualCreation(coreDirectory);
                return false;
            }
            directory += programFolder;
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid directory exception.");
                SuggestManualCreation(coreDirectory);
                return false;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException)
            {
                Console.WriteLine("Failed to create directory at: \"" + directory + "\".");
                SuggestManualCreation(coreDirectory);
                return false;
            }
            if (Test(directory))
            {
                GenerateDefaultFiles(directory);
                Save(coreDirectory, directory);
                Console.WriteLine("Directory created at: \"" + directory + "\".");
                return true;
            }
            Console.WriteLine("Directory created at: \"" + directory + "\", but program failed to access it.");
            SuggestManualCreation(coreDirectory);
            return false;
        }

        /// <summary>
        /// Returns the Core Directory, if accessible.
        /// </summary>
        /// <param name="directory">Core Directory.</param>
        /// <returns>True if directory is accessible, false if inaccessible.</returns>
        public static bool CoreDirectory(out string directory)
        {
            directory = Directory.GetCurrentDirectory();
            return Test(directory);
        }

        /// <summary>
        /// Generates default setting files in directory.
        /// </summary>
        /// <param name="directory">Directory to use for creating new files.</param>
        public static void GenerateDefaultFiles(string directory)
        {
            using (StreamWriter file = new StreamWriter(directory + "\\GenerationRules.xml", false))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                file.WriteLine("<root>");
                file.WriteLine("<Handler Generation_Length = \"10\"/>");
                file.WriteLine("<General Plate_Count = \"10\" X_Half_Size = \"1000\" Y_Size = \"1000\" Current_Time = \"0\" Max_Buildup = \"0\"/>");
                for (int i = 0; i < 10; i++)
                {
                    file.WriteLine("<Magnitude index = \"" + i.ToString() + "\" value = \"" + (16 - i).ToString() + "\"/>");
                    file.WriteLine("<Point_Concentration index = \"" + i.ToString() + "\" value = \"0.999\"/>");
                    file.WriteLine("<Radius index = \"" + i.ToString() + "\" value = \"" + (Math.Round(Math.Sin(Math.PI * (12 - i) / 120), 2)).ToString() + "\"/>");
                }
                file.WriteLine("<Cutoff value = \"" + (1000 * 1000 * 3 / 2).ToString() + "\"/>");
                file.WriteLine("</root>");
            }
            using (StreamWriter file = new StreamWriter(directory + "\\MoveRules.xml", false))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                file.WriteLine("<root>");
                file.WriteLine("<General Plate_Count = \"10\" X_Half_Size = \"1000\" Y_Size = \"1000\" Current_Time = \"0\" Max_Buildup = \"0\"/>");
                file.WriteLine("<MoveRules Overlap_Factor = \"0.6\" Time_Step = \"1\"/>");
                file.WriteLine("</root>");
            }
            using (StreamWriter file = new StreamWriter(directory + "\\GenerateAltitudeRules.xml", false))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                file.WriteLine("<root>");
                file.WriteLine("<General Plate_Count = \"10\" X_Half_Size = \"1000\" Y_Size = \"1000\" Current_Time = \"0\" Max_Buildup = \"0\"/>");
                file.WriteLine("</root>");
            }
            using (StreamWriter file = new StreamWriter(directory + "\\GenerateRainfallRules.xml", false))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                file.WriteLine("<root>");
                file.WriteLine("<General Plate_Count = \"10\" X_Half_Size = \"1000\" Y_Size = \"1000\" Current_Time = \"0\" Max_Buildup = \"0\"/>");
                file.WriteLine("<Rainfall Axis_Tilt = \"10\" Number_of_Seasons = \"4\" Ocean_Weight = \"0.125\" Altitude_Weight = \"0.001\" Land_Weight = \"5\"/>");
                file.WriteLine("</root>");
            }
            using (StreamWriter file = new StreamWriter(directory + "\\GenerateErosionRules.xml", false))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                file.WriteLine("<root>");
                file.WriteLine("<General Plate_Count = \"10\" X_Half_Size = \"1000\" Y_Size = \"1000\" Current_Time = \"0\" Max_Buildup = \"0\"/>");
                file.WriteLine("<Erosion Number_of_Seasons = \"4\" Water_Threshold = \"0.1\"/>");
                file.WriteLine("</root>");
            }
            using (StreamWriter file = new StreamWriter(directory + "\\PlateData.xml", false))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                file.WriteLine("<root>");
                file.WriteLine("<Handler Plate_Count = \"10\"/>");
                for (int i = 0; i < 10; i++)
                {
                    double angleOne = i * Math.PI / 4.5;
                    angleOne = Math.Round(angleOne, 3);
                    file.WriteLine("<Plate Plate_Index = \"" + i.ToString() + "\" Speed = \"0.02\" DirectionOne = \"" + (angleOne).ToString() + "\" DirectionTwo = \"" + (angleOne).ToString() + "\"/>");
                }
                file.WriteLine("</root>");
            }
        }

        /// <summary>
        /// Saves new directory location in the root directory.
        /// </summary>
        /// <param name="coreDirectory">Location of root directory.</param>
        /// <param name="directory">New directory to store.</param>
        public static void Save(string coreDirectory, string directory)
        {
            using (StreamWriter writer = new StreamWriter(File.Open(coreDirectory + redirectExtension, FileMode.Create)))
            {
                writer.WriteLine(directory);
            }
        }

        /// <summary>
        /// Checks if working directory exists, and sets up a new one if not.
        /// </summary>
        /// <param name="directory">Directory location.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool Setup(out string directory)
        {
            directory = "";
            string coreDirectory = "";
            try
            {
                CoreDirectory(out coreDirectory);
            }
            catch (Exception e) when (e is ArgumentException || e is IOException || e is UnauthorizedAccessException)
            {
                Console.WriteLine("Failed to access core directory.");
                return false;
            }
            if (FindStored(coreDirectory, out directory))
            {
                if (Test(directory))
                {
                    Console.WriteLine("Directory found: \"" + directory + "\".");
                    return true;
                }
                else
                {
                    Console.WriteLine("Directory found at: \"" + directory + "\" is invalid. Creating new directory.");
                }
            }
            return TryToCreateDefaultDirectory(coreDirectory, out directory);
        }

        /// <summary>
        /// Tests that the given directory is writable.
        /// </summary>
        /// <param name="directory">Directory path to check.</param>
        /// <returns>True if writable, false if not writable.</returns>
        public static bool Test(string directory)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(directory + pingFileName, FileMode.Create)))
                {
                    writer.Write(true);
                }
                File.Delete(directory + pingFileName);
                return true;
            }
            catch (Exception e) when (e is ArgumentException || e is IOException || e is UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}