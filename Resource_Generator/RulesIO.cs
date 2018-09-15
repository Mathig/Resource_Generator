using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Resource_Generator
{
    /// <summary>
    /// Inputs rules for program functions.
    /// </summary>
    public static class RulesIO
    {
        /// <summary>
        /// Loads rules for altitude map.
        /// </summary>
        /// <param name="fileName">Altitude map file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        private static AltitudeMapRules LoadAltitudeRules(string fileName)
        {
            var serializer = new XmlSerializer(typeof(AltitudeMapRules));
            AltitudeMapRules rules;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                rules = (AltitudeMapRules)serializer.Deserialize(fileStream);
            }
            rules.CheckRules();
            return rules;
        }

        /// <summary>
        /// Loads rules for erosion.
        /// </summary>
        /// <param name="fileName">Altitude map file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        private static ErosionMapRules LoadErosionRules(string fileName)
        {
            var serializer = new XmlSerializer(typeof(ErosionMapRules));
            ErosionMapRules rules;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                rules = (ErosionMapRules)serializer.Deserialize(fileStream);
            }
            rules.CheckRules();
            return rules;
        }

        /// <summary>
        /// Loads rules for generation.
        /// </summary>
        /// <param name="fileName">Altitude map file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        private static GenerateRules LoadGenerateRules(string fileName)
        {
            var serializer = new XmlSerializer(typeof(GenerateRules));
            GenerateRules rules;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                rules = (GenerateRules)serializer.Deserialize(fileStream);
            }
            rules.CheckRules();
            return rules;
        }

        /// <summary>
        /// Loads rules for plate movement.
        /// </summary>
        /// <param name="fileName">Altitude map file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        private static MoveRules LoadMoveRules(string fileName)
        {
            var serializer = new XmlSerializer(typeof(MoveRules));
            MoveRules rules;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                rules = (MoveRules)serializer.Deserialize(fileStream);
            }
            rules.CheckRules();
            return rules;
        }

        /// <summary>
        /// Loads rules for rainfall.
        /// </summary>
        /// <param name="fileName">Altitude map file name.</param>
        /// <returns>Rules to generate.</returns>
        /// <exception cref="InvalidDataException">Some data values are missing, or an xml or uri exception were called.</exception>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        private static RainfallMapRules LoadRainfallRules(string fileName)
        {
            var serializer = new XmlSerializer(typeof(RainfallMapRules));
            RainfallMapRules rules;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                rules = (RainfallMapRules)serializer.Deserialize(fileStream);
            }
            rules.CheckRules();
            return rules;
        }

        /// <summary>
        /// Loads rules from the given file, not including extension.
        /// </summary>
        /// <param name="fileName">File name to load.</param>
        /// <param name="type">Type of rules to load, as string.</param>
        /// <returns>Rules.</returns>
        public static GeneralRules Load(string fileName, string type)
        {
            try
            {
                switch (type)
                {
                    case nameof(AltitudeMapRules):
                        return LoadAltitudeRules(fileName + ".xml");

                    case nameof(ErosionMapRules):
                        return LoadErosionRules(fileName + ".xml");

                    case nameof(GenerateRules):
                        return LoadGenerateRules(fileName + ".xml");

                    case nameof(MoveRules):
                        return LoadMoveRules(fileName + ".xml");

                    case nameof(RainfallMapRules):
                        return LoadRainfallRules(fileName + ".xml");

                    default:
                        return null;
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(type + " file missing. Can't find: " + fileName, e);
            }
            catch (UriFormatException e)
            {
                throw new InvalidDataException(type + " file yielded UriFormatException: " + e.Message, e);
            }
            catch (XmlException e)
            {
                throw new InvalidDataException(type + " file yielded XML Exception: " + e.Message, e);
            }
            catch (NullReferenceException e)
            {
                throw new InvalidDataException(type + " file is missing values.", e);
            }
        }

        /// <summary>
        /// Saves rules to given file location.
        /// </summary>
        /// <param name="fileName">File to save to, not including extension.</param>
        /// <param name="rules">Rules to save.</param>
        public static void Save(string fileName, AltitudeMapRules rules)
        {
            using (StreamWriter file = new StreamWriter(fileName + ".xml", false))
            {
                var ruleType = rules.GetType();
                var serializer = new XmlSerializer(typeof(AltitudeMapRules));
                serializer.Serialize(file, rules);
            }
        }

        /// <summary>
        /// Saves rules to given file location.
        /// </summary>
        /// <param name="fileName">File to save to, not including extension.</param>
        /// <param name="rules">Rules to save.</param>
        public static void Save(string fileName, ErosionMapRules rules)
        {
            using (StreamWriter file = new StreamWriter(fileName + ".xml", false))
            {
                var ruleType = rules.GetType();
                var serializer = new XmlSerializer(typeof(ErosionMapRules));
                serializer.Serialize(file, rules);
            }
        }

        /// <summary>
        /// Saves rules to given file location.
        /// </summary>
        /// <param name="fileName">File to save to, not including extension.</param>
        /// <param name="rules">Rules to save.</param>
        public static void Save(string fileName, GenerateRules rules)
        {
            using (StreamWriter file = new StreamWriter(fileName + ".xml", false))
            {
                var ruleType = rules.GetType();
                var serializer = new XmlSerializer(typeof(GenerateRules));
                serializer.Serialize(file, rules);
            }
        }

        /// <summary>
        /// Saves rules to given file location.
        /// </summary>
        /// <param name="fileName">File to save to, not including extension.</param>
        /// <param name="rules">Rules to save.</param>
        public static void Save(string fileName, MoveRules rules)
        {
            using (StreamWriter file = new StreamWriter(fileName + ".xml", false))
            {
                var ruleType = rules.GetType();
                var serializer = new XmlSerializer(typeof(MoveRules));
                serializer.Serialize(file, rules);
            }
        }

        /// <summary>
        /// Saves rules to given file location.
        /// </summary>
        /// <param name="fileName">File to save to, not including extension.</param>
        /// <param name="rules">Rules to save.</param>
        public static void Save(string fileName, RainfallMapRules rules)
        {
            using (StreamWriter file = new StreamWriter(fileName + ".xml", false))
            {
                var ruleType = rules.GetType();
                var serializer = new XmlSerializer(typeof(RainfallMapRules));
                serializer.Serialize(file, rules);
            }
        }
    }
}