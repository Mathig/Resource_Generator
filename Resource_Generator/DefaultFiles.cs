namespace Resource_Generator
{
    internal static class DefaultFiles
    {
        /// <summary>
        /// Generates default setting files in directory.
        /// </summary>
        /// <param name="directory">Directory to use for creating new files.</param>
        public static void Create(string directory)
        {
            var altitudeMapRules = new AltitudeMapRules();
            altitudeMapRules.Default();
            RulesIO.Save(directory + "\\GenerateAltitudeRules", altitudeMapRules);

            var erosionMapRules = new ErosionMapRules();
            erosionMapRules.Default();
            RulesIO.Save(directory + "\\GenerateErosionRules", erosionMapRules);

            var generateRules = new GenerateRules();
            generateRules.Default();
            RulesIO.Save(directory + "\\GenerationRules", generateRules);

            var moveRules = new MoveRules();
            moveRules.Default();
            RulesIO.Save(directory + "\\MoveRules", moveRules);

            var rainfallMapRules = new RainfallMapRules();
            rainfallMapRules.Default();
            RulesIO.Save(directory + "\\RainfallMapRules", rainfallMapRules);

            var plateData = new PlateData();
            plateData.Default();
            PlateIO.Save(directory + "\\PlateData", plateData);
        }
    }
}