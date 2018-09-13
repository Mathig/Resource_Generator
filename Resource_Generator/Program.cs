using System;

namespace Resource_Generator
{
    internal class Program
    {
        /// <summary>
        /// Entry program loop. Redirects control to CommandController, and allows user to close
        /// program at will on crash.
        /// </summary>
        /// <param name="args">Unused.</param>
        private static void Main(string[] args)
        {
            CommandController.Run();
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }
    }
}