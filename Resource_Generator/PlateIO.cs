using System;
using System.IO;
using System.Xml;

namespace Resource_Generator
{
    /// <summary>
    /// Class for inputting and outputting plate data.
    /// </summary>
    internal static class PlateIO
    {
        /// <summary>
        /// Location of directory.
        /// </summary>
        public static string directory;

        /// <summary>
        /// Opens plate data.
        /// </summary>
        /// <param name="fileLocation">Location of plate data.</param>
        /// <param name="plateData">Plate data.</param>
        /// <returns>True if successful and data is consistent, false otherwise.</returns>
        public static bool OpenPlateData(string fileLocation, out PlateData plateData)
        {
            plateData = new PlateData();
            int plateCount = 0;
            try
            {
                XmlReader reader = XmlReader.Create(directory + "\\" + fileLocation + ".xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Handler")
                    {
                        if (reader.HasAttributes)
                        {
                            plateCount = int.Parse(reader.GetAttribute("Plate_Count"));
                            plateData.Direction = new double[plateCount][];
                            plateData.Speed = new double[plateCount];
                            for (int i = 0; i < plateCount; i++)
                            {
                                plateData.Direction[i] = new double[2];
                            }
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Plate")
                    {
                        if (reader.HasAttributes)
                        {
                            int index = int.Parse(reader.GetAttribute("Plate_Index"));
                            if (index >= plateCount)
                            {
                                Console.WriteLine("Too many plates detected.");
                                return false;
                            }
                            plateData.Speed[index] = double.Parse(reader.GetAttribute("Speed"));
                            plateData.Direction[index][0] = double.Parse(reader.GetAttribute("Direction_One"));
                            plateData.Direction[index][1] = double.Parse(reader.GetAttribute("Direction_Two"));
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Plate Data file not found.");
                return false;
            }
            catch (UriFormatException)
            {
                Console.WriteLine("Plate Data file yielded UriFormatException.");
                return false;
            }
            catch (XmlException e)
            {
                Console.WriteLine("XML Exception: " + e.Message);
                return false;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Plate Data is formatted incorrectly.");
                return false;
            }
            return true;
        }
    }
}