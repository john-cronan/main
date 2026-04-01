namespace JC.CommandLine.IntegrationTests.CommandDispatch
{
    internal static class DriveTypeCommand
    {
        public static Task<int> ExecuteAsync(IEnumerable<string> arguments)
        {
            var commandLine =
                new CommandLineParserBuilder()
                    .UseConstructorBinding()
                    .AddArgument("RootDirectory", ArgumentMultiplicity.One, false)
                    .AddArgument("VolumeName", ArgumentMultiplicity.One, false)
                    .AddHelpSwitch()
                    .CreateParser()
                    .Parse()
                    .Bind<DriveTypeCommandLine>();
            if (commandLine.Help)
            {
                Usage();
                return Task.FromResult(0);
            }
            var drives =
                from d in DriveInfo.GetDrives()
                where (!string.IsNullOrWhiteSpace(commandLine.RootDirectory)
                        && d.Name.Equals(commandLine.RootDirectory, StringComparison.InvariantCultureIgnoreCase))
                || (d.IsReady
                    && !string.IsNullOrWhiteSpace(commandLine.VolumeName)
                    && d.VolumeLabel.Equals(commandLine.VolumeName, StringComparison.InvariantCultureIgnoreCase))
                select d;
            var drive = drives.FirstOrDefault();
            if (drive == null)
            {
                Console.Out.WriteLine("The specified drive was not found or is not ready.");
                return Task.FromResult(2);
            }
            Console.Out.WriteLine(drive.DriveType.ToString());
            return Task.FromResult(0);
        }

        private static void Usage()
        {
            Console.Out.WriteLine("Drive Type [-RootDirectory {value} | -VolumeName {value}]");
        }


        private class DriveTypeCommandLine
        {
            private readonly string _rootDirectory;
            private readonly string _volumeName;
            private readonly bool _help;

            public DriveTypeCommandLine(string rootDirectory, string volumeName, bool help)
            {
                _rootDirectory = rootDirectory;
                _volumeName = volumeName;
                _help = help;
            }

            public string RootDirectory => _rootDirectory;
            public string VolumeName => _volumeName;
            public bool Help => _help;
        }
    }
}
