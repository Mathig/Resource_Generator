using System;
using System.IO;
using System.Threading.Tasks;

namespace Resource_Generator
{
    /// <summary>
    /// Cheaply reads and writes to binary files.
    /// </summary>
    internal static class CheapBinaryIO
    {
        /// <summary>
        /// Compresses integer data to binary form.
        /// </summary>
        /// <param name="inData">Data to compress.</param>
        /// <returns>Compressed data.</returns>
        private static bool[,] CompressToBinary(int[,] inData)
        {
            bool[,] output = new bool[inData.GetLength(0), inData.GetLength(1)];
            Parallel.For(0, inData.GetLength(0), (x) =>
            {
                for (int y = 0; y < inData.GetLength(1); y++)
                {
                    if (inData[x, y] == 1)
                    {
                        output[x, y] = true;
                    }
                }
            });
            return output;
        }

        /// <summary>
        /// Compresses integer data to byte form.
        /// </summary>
        /// <param name="inData">Data to compress.</param>
        /// <returns>Compressed data.</returns>
        private static byte[,] CompressToByte(int[,] inData)
        {
            byte[,] output = new byte[inData.GetLength(0), inData.GetLength(1)];
            Parallel.For(0, inData.GetLength(0), (x) =>
            {
                for (int y = 0; y < inData.GetLength(1); y++)
                {
                    output[x, y] = (byte)inData[x, y];
                }
            });
            return output;
        }

        /// <summary>
        /// Compresses integer data to ushort form.
        /// </summary>
        /// <param name="inData">Data to compress.</param>
        /// <returns>Compressed data.</returns>
        private static ushort[,] CompressToUshort(int[,] inData)
        {
            ushort[,] output = new ushort[inData.GetLength(0), inData.GetLength(1)];
            Parallel.For(0, inData.GetLength(0), (x) =>
            {
                for (int y = 0; y < inData.GetLength(1); y++)
                {
                    output[x, y] = (ushort)inData[x, y];
                }
            });
            return output;
        }

        /// <summary>
        /// Reads byte data from file.
        /// </summary>
        /// <param name="fileName">File to read from.</param>
        /// <param name="xSize">Width of data array.</param>
        /// <param name="ySize">Height of data array.</param>
        /// <param name="data">Data to read.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private static bool ReadByte(string fileName, int xSize, int ySize, out byte[,] data)
        {
            data = new byte[xSize, ySize];
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            data[x, y] = reader.ReadByte();
                        }
                    }
                }
                return true;
            }
            catch (Exception e) when (e is FileNotFoundException || e is NullReferenceException || e is EndOfStreamException)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads int data from file.
        /// </summary>
        /// <param name="fileName">File to read from.</param>
        /// <param name="xSize">Width of data array.</param>
        /// <param name="ySize">Height of data array.</param>
        /// <param name="data">Data to read.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private static bool ReadInt(string fileName, int xSize, int ySize, out int[,] data)
        {
            data = new int[xSize, ySize];
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            data[x, y] = reader.ReadInt32();
                        }
                    }
                }
                return true;
            }
            catch (Exception e) when (e is FileNotFoundException || e is NullReferenceException || e is EndOfStreamException)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads ushort data from file.
        /// </summary>
        /// <param name="fileName">File to read from.</param>
        /// <param name="xSize">Width of data array.</param>
        /// <param name="ySize">Height of data array.</param>
        /// <param name="data">Data to read.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private static bool ReadUshort(string fileName, int xSize, int ySize, out ushort[,] data)
        {
            data = new ushort[xSize, ySize];
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            data[x, y] = reader.ReadUInt16();
                        }
                    }
                }
                return true;
            }
            catch (Exception e) when (e is FileNotFoundException || e is NullReferenceException || e is EndOfStreamException)
            {
                return false;
            }
        }

        /// <summary>
        /// Reduces a max value to the appropriate bitsize int.
        /// </summary>
        /// <param name="rawValue">Max value to reduce.</param>
        /// <returns>Bit size int. 1 for bool, 8 for byte, 16 for ushort and otherwise 32.</returns>
        private static int Simplify(int rawValue)
        {
            if (rawValue < 3)
            {
                return 1;
            }
            if (rawValue < 257)
            {
                return 8;
            }
            if (rawValue < 65537)
            {
                return 16;
            }
            return 32;
        }

        /// <summary>
        /// Uncompresses data from binary to integer form.
        /// </summary>
        /// <param name="inData">Data to uncompress.</param>
        /// <returns>Uncompressed data.</returns>
        private static int[,] UncompressBinary(bool[,] inData)
        {
            int[,] output = new int[inData.GetLength(0), inData.GetLength(1)];
            Parallel.For(0, inData.GetLength(0), (x) =>
            {
                for (int y = 0; y < inData.GetLength(1); y++)
                {
                    if (inData[x, y])
                    {
                        output[x, y] = 1;
                    }
                }
            });
            return output;
        }

        /// <summary>
        /// Uncompresses data from byte to integer form.
        /// </summary>
        /// <param name="inData">Data to uncompress.</param>
        /// <returns>Uncompressed data.</returns>
        private static int[,] UncompressByte(byte[,] inData)
        {
            int[,] output = new int[inData.GetLength(0), inData.GetLength(1)];
            Parallel.For(0, inData.GetLength(0), (x) =>
            {
                for (int y = 0; y < inData.GetLength(1); y++)
                {
                    output[x, y] = inData[x, y];
                }
            });
            return output;
        }

        /// <summary>
        /// Uncompresses data from ushort to integer form.
        /// </summary>
        /// <param name="inData">Data to uncompress.</param>
        /// <returns>Uncompressed data.</returns>
        private static int[,] UncompressUshort(ushort[,] inData)
        {
            int[,] output = new int[inData.GetLength(0), inData.GetLength(1)];
            Parallel.For(0, inData.GetLength(0), (x) =>
            {
                for (int y = 0; y < inData.GetLength(1); y++)
                {
                    output[x, y] = inData[x, y];
                }
            });
            return output;
        }

        /// <summary>
        /// Writes data to file, as byte.
        /// </summary>
        /// <param name="fileName">File to write to.</param>
        /// <param name="data">Data to store.</param>
        private static void WriteByte(string fileName, byte[,] data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                foreach (byte iData in data)
                {
                    writer.Write(iData);
                }
            }
        }

        /// <summary>
        /// Writes data to file, as int.
        /// </summary>
        /// <param name="fileName">File to write to.</param>
        /// <param name="data">Data to store.</param>
        private static void WriteInt(string fileName, int[,] data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                foreach (int iData in data)
                {
                    writer.Write(iData);
                }
            }
        }

        /// <summary>
        /// Writes data to file, as ushort.
        /// </summary>
        /// <param name="fileName">File to write to.</param>
        /// <param name="data">Data to store.</param>
        private static void WriteUshort(string fileName, ushort[,] data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                foreach (ushort iData in data)
                {
                    writer.Write(iData);
                }
            }
        }

        /// <summary>
        /// Writes from a file using a compressed binary format, based on the max value of the data.
        /// </summary>
        /// <param name="filePath">File to read from.</param>
        /// <param name="maxValue">Max value of the data.</param>
        /// <param name="data">Data to retrieve.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool Read(string filePath, int maxValue, int xSize, int ySize, out int[,] data)
        {
            bool output = false;
            data = null;
            int bitSize = Simplify(maxValue);
            switch (bitSize)
            {
                case 1:
                    output = ReadBinary(filePath, xSize, ySize, out bool[,] rawBoolData);
                    data = UncompressBinary(rawBoolData);
                    break;

                case 8:
                    output = ReadByte(filePath, xSize, ySize, out byte[,] rawByteData);
                    data = UncompressByte(rawByteData);
                    break;

                case 16:
                    output = ReadUshort(filePath, xSize, ySize, out ushort[,] rawUshortData);
                    data = UncompressUshort(rawUshortData);
                    break;

                case 32:
                    output = ReadInt(filePath, xSize, ySize, out data);
                    break;
            }
            return output;
        }

        /// <summary>
        /// Reads binary data from file.
        /// </summary>
        /// <param name="fileName">File to read from.</param>
        /// <param name="xSize">Width of data array.</param>
        /// <param name="ySize">Height of data array.</param>
        /// <param name="data">Data to read.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool ReadBinary(string fileName, int xSize, int ySize, out bool[,] data)
        {
            data = new bool[xSize, ySize];
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            data[x, y] = reader.ReadBoolean();
                        }
                    }
                }
                return true;
            }
            catch (Exception e) when (e is FileNotFoundException || e is NullReferenceException || e is EndOfStreamException)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes to a file using a compressed binary format, based on the max value of the data.
        /// </summary>
        /// <param name="filePath">Full file path to save to.</param>
        /// <param name="data">Data to store.</param>
        /// <param name="maxValue">Max value of data.</param>
        public static void Write(string filePath, int[,] data, int maxValue)
        {
            int bitSize = Simplify(maxValue);
            switch (bitSize)
            {
                case 1:
                    WriteBinary(filePath, CompressToBinary(data));
                    break;

                case 8:
                    WriteByte(filePath, CompressToByte(data));
                    break;

                case 16:
                    WriteUshort(filePath, CompressToUshort(data));
                    break;

                case 32:
                    WriteInt(filePath, data);
                    break;
            }
        }

        /// <summary>
        /// Writes data to file, as binary.
        /// </summary>
        /// <param name="fileName">File to write to.</param>
        /// <param name="data">Data to store.</param>
        public static void WriteBinary(string fileName, bool[,] data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                foreach (bool iData in data)
                {
                    writer.Write(iData);
                }
            }
        }
    }
}