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
        /// Checks the altitude rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        private static void CheckRules(AltitudeMapRules rules)
        {
            try
            {
                if (rules.xHalfSize < 1 || rules.ySize < 1 || rules.plateCount < 1)
                {
                    throw new InvalidDataException("Altitude Rules file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Altitude Rules file is missing values.", e);
            }
        }

        /// <summary>
        /// Checks the Erosion rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        private static void CheckRules(ErosionMapRules rules)
        {
            try
            {
                if (rules.xHalfSize < 1 || rules.ySize < 1 || rules.plateCount < 1)
                {
                    throw new InvalidDataException("Erosion Rules file has invalid values.");
                }
                if (rules.numberSeasons < 1)
                {
                    throw new InvalidDataException("Erosion Rules file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Erosion Rules file is missing values.", e);
            }
        }

        /// <summary>
        /// Checks the generation rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        private static void CheckRules(GenerateRules rules)
        {
            try
            {
                if (rules.xHalfSize < 1 || rules.ySize < 1 || rules.plateCount < 1)
                {
                    throw new InvalidDataException("Generation Rules file has invalid values.");
                }
                for (int i = 0; i < rules.magnitude.Length; i++)
                {
                    if (rules.radius[i] == 0 || rules.pointConcentration[i] == 0 || rules.magnitude[i] == 0)
                    {
                        throw new InvalidDataException("Generation Rules file has invalid values.");
                    }
                }
                if (rules.cutOff == 0 || rules.cutOff > 2 * rules.xHalfSize * rules.ySize)
                {
                    throw new InvalidDataException("Generation Rules file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Generation Rules file is missing values.", e);
            }
        }

        /// <summary>
        /// Checks the move rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        private static void CheckRules(MoveRules rules)
        {
            try
            {
                if (rules.xHalfSize < 1 || rules.ySize < 1 || rules.plateCount < 1)
                {
                    throw new InvalidDataException("Move Rules file has invalid values.");
                }
                if (rules.timeStep == 0)
                {
                    throw new InvalidDataException("Move Rules file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Move Rules file is missing values.", e);
            }
        }

        /// <summary>
        /// Checks the rainfall rules to confirm they are valid.
        /// </summary>
        /// <param name="rules">Rules to check.</param>
        /// <exception cref="InvalidDataException">Some data values are lower than required or missing.</exception>
        private static void CheckRules(RainfallMapRules rules)
        {
            try
            {
                if (rules.xHalfSize < 1 || rules.ySize < 1 || rules.plateCount < 1)
                {
                    throw new InvalidDataException("Altitude Rules file has invalid values.");
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Altitude Rules file is missing values.", e);
            }
        }

        /// <summary>
        /// Loads general rules from the XML reader.
        /// </summary>
        /// <param name="rules">Rules to load to.</param>
        /// <param name="reader">Reader set to load the General Rules.</param>
        private static void LoadGeneralRules(GeneralRules rules, XmlReader reader)
        {
            rules.plateCount = int.Parse(reader.GetAttribute("Plate_Count"));
            rules.xHalfSize = int.Parse(reader.GetAttribute("X_Half_Size"));
            rules.ySize = int.Parse(reader.GetAttribute("Y_Size"));
            rules.currentTime = int.Parse(reader.GetAttribute("Current_Time"));
            rules.maxBuildup = int.Parse(reader.GetAttribute("Max_Buildup"));
        }

        /// <summary>
        /// Loads rules for altitude map.
        /// </summary>
        /// <param name="fileName">Altitude map file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        public static AltitudeMapRules LoadAltitudeRules(string fileName)
        {
            AltitudeMapRules rules = new AltitudeMapRules();
            try
            {
                XmlReader reader = XmlReader.Create(fileName + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.HasAttributes)
                    {
                        switch (reader.Name)
                        {
                            case "General":
                                LoadGeneralRules(rules, reader);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Altitude Rules file missing. Can't find: " + fileName, e);
            }
            catch (UriFormatException e)
            {
                throw new InvalidDataException("Altitude Rules file yielded UriFormatException: " + e.Message, e);
            }
            catch (XmlException e)
            {
                throw new InvalidDataException("Altitude Rules file yielded XML Exception: " + e.Message, e);
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Altitude Rules file is missing values.", e);
            }
            CheckRules(rules);
            return rules;
        }

        /// <summary>
        /// Loads rules for Erosion map.
        /// </summary>
        /// <param name="fileName">Erosion map rules file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        public static ErosionMapRules LoadErosionRules(string fileName)
        {
            ErosionMapRules rules = new ErosionMapRules();
            try
            {
                XmlReader reader = XmlReader.Create(fileName + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.HasAttributes)
                    {
                        switch (reader.Name)
                        {
                            case "General":
                                LoadGeneralRules(rules, reader);
                                break;

                            case "Erosion":
                                rules.waterThreshold = double.Parse(reader.GetAttribute("Water_Threshold"));
                                rules.numberSeasons = int.Parse(reader.GetAttribute("Number_of_Seasons"));
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Erosion Rules file missing. Can't find: " + fileName, e);
            }
            catch (UriFormatException e)
            {
                throw new InvalidDataException("Erosion Rules file yielded UriFormatException: " + e.Message, e);
            }
            catch (XmlException e)
            {
                throw new InvalidDataException("Erosion Rules file yielded XML Exception: " + e.Message, e);
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Erosion Rules file is missing values.", e);
            }
            CheckRules(rules);
            return rules;
        }

        /// <summary>
        /// Loads Plate Generation rules from the input file.
        /// </summary>
        /// <param name="fileName">Rules file location.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        public static GenerateRules LoadGenerateRules(string fileName)
        {
            GenerateRules rules = new GenerateRules();
            int generationLength = 0;
            try
            {
                XmlReader reader = XmlReader.Create(fileName + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.HasAttributes)
                    {
                        int index;
                        switch (reader.Name)
                        {
                            case "General":
                                LoadGeneralRules(rules, reader);
                                break;

                            case "Handler":
                                generationLength = int.Parse(reader.GetAttribute("Generation_Length"));
                                rules.magnitude = new double[generationLength];
                                rules.pointConcentration = new double[generationLength];
                                rules.radius = new double[generationLength];
                                break;

                            case "Magnitude":
                                index = int.Parse(reader.GetAttribute("index"));
                                if (index >= generationLength)
                                {
                                    throw new InvalidDataException("Generation Rules file has too many values.");
                                }
                                rules.magnitude[index] = double.Parse(reader.GetAttribute("value"));
                                break;

                            case "Point_Concentration":
                                index = int.Parse(reader.GetAttribute("index"));
                                if (index >= generationLength)
                                {
                                    throw new InvalidDataException("Generation Rules file has too many values.");
                                }
                                rules.pointConcentration[index] = double.Parse(reader.GetAttribute("value"));
                                break;

                            case "Radius":
                                index = int.Parse(reader.GetAttribute("index"));
                                if (index >= generationLength)
                                {
                                    throw new InvalidDataException("Generation Rules file has too many values.");
                                }
                                rules.radius[index] = double.Parse(reader.GetAttribute("value"));
                                break;

                            case "Cutoff":
                                rules.cutOff = int.Parse(reader.GetAttribute("value"));
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Generation Rules file missing. Can't find: " + fileName, e);
            }
            catch (UriFormatException e)
            {
                throw new InvalidDataException("Generation Rules file yielded UriFormatException: " + e.Message, e);
            }
            catch (XmlException e)
            {
                throw new InvalidDataException("Generation Rules file yielded XML Exception: " + e.Message, e);
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Generation Rules file is missing values.", e);
            }
            CheckRules(rules);
            return rules;
        }

        /// <summary>
        /// Loads Plate Movement rules from the input file.
        /// </summary>
        /// <param name="fileName">Rules file location.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        public static MoveRules LoadMoveRules(string fileName)
        {
            MoveRules rules = new MoveRules();
            try
            {
                XmlReader reader = XmlReader.Create(fileName + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.HasAttributes)
                    {
                        switch (reader.Name)
                        {
                            case "General":
                                LoadGeneralRules(rules, reader);
                                break;

                            case "MoveRules":
                                rules.OverlapFactor = double.Parse(reader.GetAttribute("Overlap_Factor"));
                                rules.timeStep = int.Parse(reader.GetAttribute("Time_Step"));
                                rules.numberSteps = int.Parse(reader.GetAttribute("Number_Steps"));
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Move Rules file missing. Can't find: " + fileName, e);
            }
            catch (UriFormatException e)
            {
                throw new InvalidDataException("Move Rules file yielded UriFormatException: " + e.Message, e);
            }
            catch (XmlException e)
            {
                throw new InvalidDataException("Move Rules file yielded XML Exception: " + e.Message, e);
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Move Rules file is missing values.", e);
            }
            CheckRules(rules);
            return rules;
        }

        /// <summary>
        /// Loads rules for rainfall map.
        /// </summary>
        /// <param name="fileName">Altitude map file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        public static RainfallMapRules LoadRainfallRules(string fileName)
        {
            RainfallMapRules rules = new RainfallMapRules();
            try
            {
                XmlReader reader = XmlReader.Create(fileName + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.HasAttributes)
                    {
                        switch (reader.Name)
                        {
                            case "General":
                                LoadGeneralRules(rules, reader);
                                break;

                            case "Rainfall":
                                rules.axisTilt = double.Parse(reader.GetAttribute("Axis_Tilt"));
                                rules.oceanWeight = double.Parse(reader.GetAttribute("Ocean_Weight"));
                                rules.landWeight = double.Parse(reader.GetAttribute("Land_Weight"));
                                rules.altitudeWeight = double.Parse(reader.GetAttribute("Altitude_Weight"));
                                rules.numberSeasons = int.Parse(reader.GetAttribute("Number_of_Seasons"));
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Rainfall Rules file missing. Can't find: " + fileName, e);
            }
            catch (UriFormatException e)
            {
                throw new InvalidDataException("Rainfall Rules file yielded UriFormatException: " + e.Message, e);
            }
            catch (XmlException e)
            {
                throw new InvalidDataException("Rainfall Rules file yielded XML Exception: " + e.Message, e);
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Rainfall Rules file is missing values.", e);
            }
            CheckRules(rules);
            return rules;
        }
    }
}