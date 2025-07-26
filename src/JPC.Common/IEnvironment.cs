namespace JPC.Common
{
    public enum OperatingSystem
    {
        Windows,
        Linux,
        OSX,
        Unix
    }

    public interface IEnvironment
    {
        string CommandLine { get; }
        void Exit(int exitCode);
        string ExpandEnvironmentVariables(string stringWithVariables);
        string[] GetCommandLineArgs();
        string MachineName { get; }
        string NewLine { get; }
        string UserName { get; }
        OperatingSystem? OperatingSystem { get; }
    }
}
