using System;
using System.IO;

namespace Resource_Generator
{
    /// <summary>
    /// Struct for handling file names.
    /// </summary>
    internal readonly struct FileName
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
        /// Constructor that checks to make sure the name is valid.
        /// </summary>
        /// <param name="inName">See <see cref="_name"/>.</param>
        public FileName(string inName)
        {
            _name = inName;
            CheckValidity();
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
        /// Tests if the string requirements for a file name.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        /// <exception cref="FormatException">Yields exception if the file name is invalid.</exception>
        public void CheckValidity()
        {
            foreach (char iChar in _name)
            {
                if (!Char.IsLetterOrDigit(iChar))
                {
                    if (iChar == '.')
                    {
                        throw new FormatException("File names can not include extensions or periods.");
                    }
                    foreach (char jChar in Path.GetInvalidFileNameChars())
                    {
                        if (iChar == jChar)
                        {
                            throw new FormatException("File name contains invalid character: " + jChar);
                        }
                    }
                }
            }
        }
    }
}