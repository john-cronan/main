using System.Diagnostics;

namespace JC.CommandLine.IntegrationTests.ArgsFiles
{
    public static class Program
    {
        public static void Main()
        {
            var commandLine = 
                new CommandLineParserBuilder()
                    .UseConstructorBinding()
                    .AllowUnnamedValues()
                    .AllowArgsFiles('@')
                    .UseStemNameMatching()
                    .AddArgument("Files", ArgumentMultiplicity.ZeroOrMore, false)
                    .AddSwitch("Strict")
                    .AddArgument("Batch-Size", ArgumentMultiplicity.One, false)
                    .AddArgument("Max-Parallelism", ArgumentMultiplicity.One, false)
                    .CreateParser()
                    .Parse()
                    .Bind<CommandLine>();
            Debug.Assert(commandLine != null);
            Debug.Assert(commandLine.Files != null);
            Debug.Assert(commandLine.Files.Count() == 4);
            Debug.Assert(commandLine.Strict);
            Debug.Assert(commandLine.BatchSize == 1000);
            Debug.Assert(commandLine.MaxParallelism == null);
        }
    }
}
