using System;
using System.Runtime.InteropServices;

namespace JPC.Common.Internal
{
    internal class EnvironmentWrapper : IEnvironment
    {
        private readonly OperatingSystem? _operatingSystem;

        public EnvironmentWrapper()
        {
            _operatingSystem = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _operatingSystem = OperatingSystem.Windows;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _operatingSystem = OperatingSystem.Linux;                
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _operatingSystem = OperatingSystem.OSX;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                _operatingSystem = OperatingSystem.Unix;
            }
        }

        string IEnvironment.CommandLine => Environment.CommandLine;

        string IEnvironment.MachineName => Environment.MachineName;

        string IEnvironment.NewLine => Environment.NewLine;

        string IEnvironment.UserName => Environment.UserName;

        OperatingSystem? IEnvironment.OperatingSystem => _operatingSystem;

        void IEnvironment.Exit(int exitCode) => Environment.Exit(exitCode);

        string IEnvironment.ExpandEnvironmentVariables(string stringWithVariables)
        {
            return Environment.ExpandEnvironmentVariables(stringWithVariables);
        }

        string[] IEnvironment.GetCommandLineArgs() => Environment.GetCommandLineArgs();
    }
}
