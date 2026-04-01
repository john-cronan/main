namespace JC.CommandLine.IntegrationTests.CommandDispatch
{
    internal static class DriveFormatCommand
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
                    .Bind<DriveFormatCommandLine>();
            if (commandLine.Help)
            {
                Usage();
                return Task.FromResult(0);
            }
            return ExecuteAsync(commandLine);
        }

        private static Task<int> ExecuteAsync(DriveFormatCommandLine commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine.RootDirectory)
                && string.IsNullOrWhiteSpace(commandLine.VolumeName))
            {
                Console.Out.WriteLine("Either RootDirectory or VolumeName must be specified");
                return Task.FromResult(1);
            }
            if (!string.IsNullOrWhiteSpace(commandLine.RootDirectory)
                && !string.IsNullOrWhiteSpace(commandLine.VolumeName))
            {
                Console.Out.WriteLine("Both RootDirectory and VolumnName may not be specified");
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
            if (drive != null && drive.IsReady)
            {
                Console.Out.WriteLine(drive.DriveFormat.ToString());
                return Task.FromResult(0);
            }
            else
            {
                Console.Out.WriteLine("The specified drive was not found or is not ready.");
                return Task.FromResult(2);
            }
        }

        private static void Usage()
        {
            Console.Out.WriteLine("Drive Format [-RootDirectory {value} | -VolumeName {value}]");
        }

        private class DriveFormatCommandLine
        {
            private readonly string _rootDirectory;
            private readonly string _volumeName;
            private readonly bool _help;

            public DriveFormatCommandLine(string rootDirectory, string volumeName, bool help)
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
