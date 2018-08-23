using System;

namespace Resource_Generator
{
    /// <summary>
    /// Struct for handling file names.
    /// </summary>
    internal struct FileName
    {
        /// <summary>
        /// Name of file
        /// </summary>
        public readonly string name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inName">See <see cref="name"/>.</param>
        public FileName(string inName)
        {
            name = inName;
        }

        /// <summary>
        /// Tests if the string fulfills strict requirements for a file name.
        /// </summary>
        /// <param name="inString">String to test.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            foreach (char iChar in name)
            {
                if (!(Char.IsLetterOrDigit(iChar) || iChar.Equals('_')))
                {
                    return false;
                }
            }
            return true;
        }
    }
}