namespace JC.CommandLine.IntegrationTests.CommandDispatch
{
    internal static class DriveFreeCommand
    {
        public static Task<int> ExecuteAsync(IEnumerable<string> arguments)
        {
            var commandLine =
                new CommandLineParserBuilder()
                    .UseConstructorBinding()
                    .AddArgument("RootDirectory", ArgumentMultiplicity.One, false)
                    .AddArgument("VolumeName", ArgumentMultiplicity.One, false)
                    .AddSwitch("Available")
                    .AddSwitch("Total")
                    .AddHelpSwitch()
                    .CreateParser()
                    .Parse()
                    .Bind<DriveFreeCommandLine>();
            if (commandLine.Help)
            {
                Usage();
                return Task.FromResult(0);
            }
            else
            {
                return ExecuteAsync(commandLine);
            }
        }

        private static Task<int> ExecuteAsync(DriveFreeCommandLine commandLine)
        {
            var drives =
                from d in DriveInfo.GetDrives()
                where (!string.IsNullOrWhiteSpace(commandLine.RootDirectory)
                        && d.Name.Equals(commandLine.RootDirectory, StringComparison.InvariantCultureIgnoreCase))
                || (d.IsReady
                    && !string.IsNullOrWhiteSpace(commandLine.VolumeName)
                    && d.VolumeLabel.Equals(commandLine.VolumeName, StringComparison.InvariantCultureIgnoreCase))
                select d;
            var drive = drives.FirstOrDefault();
            if (drive == null || !drive.IsReady)
            {
                Console.Out.WriteLine("The specified drive was not found or is not ready.");
                return Task.FromResult(2);
            }
            if (commandLine.Available == commandLine.Total)
            {
                //
                //  i.e. both are true or both are false.
                Console.Out.WriteLine($"Available: {drive.AvailableFreeSpace.ToString("#,##0")}");
                Console.Out.WriteLine($"Total: {drive.TotalFreeSpace.ToString("#,##0")}");
            }
            else if (commandLine.Available)
            {
                Console.Out.WriteLine(drive.AvailableFreeSpace.ToString("#,##0"));
            }
            else if (commandLine.Total)
            {
                Console.Out.WriteLine(drive.TotalFreeSpace.ToString("#,##0"));
            }
            return Task.FromResult(0);
        }

        private static void Usage()
        {
            Console.Out.WriteLine("Drive Free [-RootDirectory {value} | -VolumeName {value}] "
                + "[-Available] [-Total]");
        }



        private class DriveFreeCommandLine
        {
            private readonly string _rootDirectory;
            private readonly string _volumeName;
            private readonly bool _help;
            private readonly bool _available;
            private readonly bool _total;

            public DriveFreeCommandLine(string rootDirectory, string volumeName, bool help,
                bool available, bool total)
            {
                _rootDirectory = rootDirectory;
                _volumeName = volumeName;
                _help = help;
                _available = available;
                _total = total;
            }

            public string RootDirectory => _rootDirectory;
            public string VolumeName => _volumeName;
            public bool Help => _help;
            public bool Available => _available;
            public bool Total => _total;
        }
    }
}
