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
        /// Opens plate data.
        /// </summary>
        /// <param name="fileName">Location of plate data, not including extension.</param>
        /// <returns>Plate data.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        public static PlateData OpenPlateData(string fileName)
        {
            PlateData plateData = new PlateData();
            int plateCount = 0;
            try
            {
                XmlReader reader = XmlReader.Create(fileName + ".xml");
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
                                throw new InvalidDataException("Plate Data file has too many plates.");
                            }
                            plateData.Speed[index] = double.Parse(reader.GetAttribute("Speed"));
                            plateData.Direction[index][0] = double.Parse(reader.GetAttribute("Direction_One"));
                            plateData.Direction[index][1] = double.Parse(reader.GetAttribute("Direction_Two"));
                        }
                    }
                }
                return plateData;
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Plate Data file missing. Can't find: " + fileName, e);
            }
            catch (UriFormatException e)
            {
                throw new InvalidDataException("Plate Data file yielded UriFormatException: " + e.Message, e);
            }
            catch (XmlException e)
            {
                throw new InvalidDataException("Plate Data file yielded XML Exception: " + e.Message, e);
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException("Plate Data file is missing values.", e);
            }
        }
    }
}