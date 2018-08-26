using System;
using System.IO;
using System.Xml;

namespace Resource_Generator
{
    /// <summary>
    /// Inputs rules for program functions.
    /// </summary>
    internal static class RulesInput
    {
        /// <summary>
        /// Directory for rule files.
        /// </summary>
        public static string directory;

        /// <summary>
        /// Checks the generation rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        private static bool CheckGenerateRules(GenerateRules rules)
        {
            try
            {
                if (rules.xHalfSize == 0 || rules.ySize == 0 || rules.plateCount == 0)
                {
                    Console.WriteLine("Generation Rules file is formatted incorrectly.");
                    return false;
                }
                for (int i = 0; i < rules.magnitude.Length; i++)
                {
                    if (rules.radius[i] == 0 || rules.pointConcentration[i] == 0 || rules.magnitude[i] == 0)
                    {
                        Console.WriteLine("Generation Rules file is formatted incorrectly.");
                        return false;
                    }
                }
                if (rules.cutOff == 0)
                {
                    Console.WriteLine("Generation Rules file is formatted incorrectly.");
                    return false;
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Generation Rules file is formatted incorrectly.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks the move rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        private static bool CheckMoveRules(MoveRules rules)
        {
            try
            {
                if (rules.xHalfSize == 0 || rules.ySize == 0 || rules.plateCount == 0)
                {
                    Console.WriteLine("Generation Rules file is formatted incorrectly.");
                    return false;
                }
                if (rules.timeStep == 0)
                {
                    Console.WriteLine("Generation Rules file is formatted incorrectly.");
                    return false;
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Move Rules file is formatted incorrectly.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Loads Plate Generation rules from the input file.
        /// </summary>
        /// <param name="fileName">Rules file location.</param>
        /// <param name="rules">Rule structure for generation rules.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool LoadGenerateRules(string fileName, out GenerateRules rules)
        {
            rules = new GenerateRules();
            int generationLength = 0;
            try
            {
                XmlReader reader = XmlReader.Create(directory + "\\" + fileName + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Handler")
                    {
                        if (reader.HasAttributes)
                        {
                            generationLength = int.Parse(reader.GetAttribute("Generation_Length"));
                            rules.magnitude = new double[generationLength];
                            rules.pointConcentration = new double[generationLength];
                            rules.radius = new double[generationLength];
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "General")
                    {
                        if (reader.HasAttributes)
                        {
                            rules.plateCount = int.Parse(reader.GetAttribute("Plate_Count"));
                            rules.xHalfSize = int.Parse(reader.GetAttribute("X_Half_Size"));
                            rules.ySize = int.Parse(reader.GetAttribute("Y_Size"));
                            rules.currentTime = int.Parse(reader.GetAttribute("Current_Time"));
                            rules.maxBuildup = int.Parse(reader.GetAttribute("Max_Buildup"));
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Magnitude")
                    {
                        if (reader.HasAttributes)
                        {
                            int index = int.Parse(reader.GetAttribute("index"));
                            if (index >= generationLength)
                            {
                                Console.WriteLine("Too many generation criteria detected.");
                                return false;
                            }
                            rules.magnitude[index] = double.Parse(reader.GetAttribute("value"));
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Point_Concentration")
                    {
                        if (reader.HasAttributes)
                        {
                            int index = int.Parse(reader.GetAttribute("index"));
                            if (index >= generationLength)
                            {
                                Console.WriteLine("Too many generation criteria detected.");
                                return false;
                            }
                            rules.pointConcentration[index] = double.Parse(reader.GetAttribute("value"));
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Radius")
                    {
                        if (reader.HasAttributes)
                        {
                            int index = int.Parse(reader.GetAttribute("index"));
                            if (index >= generationLength)
                            {
                                Console.WriteLine("Too many generation criteria detected.");
                                return false;
                            }
                            rules.radius[index] = double.Parse(reader.GetAttribute("value"));
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Cutoff")
                    {
                        if (reader.HasAttributes)
                        {
                            rules.cutOff = int.Parse(reader.GetAttribute("value"));
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Generation Rules file not found.");
                return false;
            }
            catch (UriFormatException)
            {
                Console.WriteLine("Generation Rules file yielded UriFormatException.");
                return false;
            }
            catch (XmlException e)
            {
                Console.WriteLine("XML Exception: " + e.Message);
                return false;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Generation Rules file is formatted incorrectly.");
                return false;
            }
            return CheckGenerateRules(rules);
        }

        /// <summary>
        /// Loads Plate Movement rules from the input file.
        /// </summary>
        /// <param name="fileName">Rules file location.</param>
        /// <param name="rules">Rule structure for generation rules.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool LoadMoveRules(string fileName, out MoveRules rules)
        {
            rules = new MoveRules();
            try
            {
                XmlReader reader = XmlReader.Create(directory + "\\" + fileName + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "General")
                    {
                        if (reader.HasAttributes)
                        {
                            rules.plateCount = int.Parse(reader.GetAttribute("Plate_Count"));
                            rules.xHalfSize = int.Parse(reader.GetAttribute("X_Half_Size"));
                            rules.ySize = int.Parse(reader.GetAttribute("Y_Size"));
                            rules.currentTime = int.Parse(reader.GetAttribute("Current_Time"));
                            rules.maxBuildup = int.Parse(reader.GetAttribute("Max_Buildup"));
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "MoveRules")
                    {
                        if (reader.HasAttributes)
                        {
                            rules.OverlapFactor = double.Parse(reader.GetAttribute("Overlap_Factor"));
                            rules.timeStep = double.Parse(reader.GetAttribute("Time_Step"));
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Move Rules file not found.");
                return false;
            }
            catch (UriFormatException)
            {
                Console.WriteLine("Move Rules file yielded UriFormatException.");
                return false;
            }
            catch (XmlException e)
            {
                Console.WriteLine("XML Exception: " + e.Message);
                return false;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Move Rules file is formatted incorrectly.");
                return false;
            }
            return CheckMoveRules(rules);
        }
    }
}