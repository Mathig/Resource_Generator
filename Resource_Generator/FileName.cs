using System;

namespace Resource_Generator
{
    /// <summary>
    /// Struct for handling file names.
    /// </summary>
    internal struct FileName
    {
        /// <summary>
        /// Name of file.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Directory for files.
        /// </summary>
        public static string directory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inName">See <see cref="_name"/>.</param>
        public FileName(string inName)
        {
            _name = inName;
        }

        /// <summary>
        /// Public file name which includes the directory.
        /// </summary>
        public string Name
        {
            get
            {
                return (directory + "\\" + _name);
            }
        }

        /// <summary>
        /// Tests if the string fulfills strict requirements for a file name.
        /// </summary>
        /// <param name="inString">String to test.</param>
        /// <returns>True if valid, false otherwise.</returns>
        /// <exception cref="FormatException">Yields exception if the file name is invalid.</exception>
        public void CheckValidity()
        {
            foreach (char iChar in _name)
            {
                if (!(Char.IsLetterOrDigit(iChar) || iChar.Equals('_')))
                {
                    throw new FormatException("File name is invalid. ");
                }
            }
        }
    }
}