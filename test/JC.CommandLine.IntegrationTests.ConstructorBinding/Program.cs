using System.Diagnostics;

namespace JC.CommandLine.IntegrationTests.ConstructorBinding
{
    public static class Program
    {
        public static void Main()
        {
            var parameters =
                new CommandLineParserBuilder()
                    .UseConstructorBinding()
                    .AllowUnnamedValues()
                    .AddArgument("batch-size", ArgumentMultiplicity.One, false)
                    .AddSwitch("verbose")
                    .CreateParser()
                    .Parse()
                    .Bind<CommandLine>();
            Debug.Assert(parameters.Command.Equals("Import", StringComparison.InvariantCultureIgnoreCase));
            Debug.Assert(parameters.BatchSize == 1000);
            Debug.Assert(parameters.Verbose);
            Debug.Assert(parameters.Files.Count() == 3);
        }
    }
}
