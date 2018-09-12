using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Class for inputting and outputting plate data.
    /// </summary>
    public static class PlateIO
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
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlateData));
                PlateData plateData;
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    plateData = (PlateData)serializer.Deserialize(fileStream);
                }
                //plateData.CheckData();
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