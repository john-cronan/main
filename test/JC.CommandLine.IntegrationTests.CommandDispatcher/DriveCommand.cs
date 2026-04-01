namespace JC.CommandLine.IntegrationTests.CommandDispatch
{
    internal static class DriveCommand
    {
        public static Task<int> ExecuteAsync(IEnumerable<string> arguments)
        {
            Console.Out.WriteLine("Available commands:");
            Console.Out.WriteLine($"\tDrive Format");
            Console.Out.WriteLine($"\tDrive Free");
            Console.Out.WriteLine($"\tDrive Size");
            Console.Out.WriteLine($"\tDrive Type");
            return Task.FromResult(0);
        }
    }
}
