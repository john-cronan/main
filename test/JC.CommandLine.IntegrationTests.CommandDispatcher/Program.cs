namespace JC.CommandLine.IntegrationTests.CommandDispatch
{
    public static class Program
    {
        public static Task Main()
        {
            var dispatcher = new CommandDispatcher();
            dispatcher.RegisterDefault(DefaultExecuteAsync);
            dispatcher.Register("Drive", DriveCommand.ExecuteAsync);
            dispatcher.Register(new string[] { "Drive", "Format" }, DriveFormatCommand.ExecuteAsync);
            dispatcher.Register(new string[] { "Drive", "Free" }, DriveFreeCommand.ExecuteAsync);
            dispatcher.Register(new string[] { "Drive", "Size" }, DriveSizeCommand.ExecuteAsync);
            dispatcher.Register(new string[] { "Drive", "Type" }, DriveTypeCommand.ExecuteAsync);
            return dispatcher.ExecuteAsync();
        }

        private static Task<int> DefaultExecuteAsync(IEnumerable<string> args)
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
