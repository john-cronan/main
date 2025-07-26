using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JC.CommandLine.IntegrationTests.SimpleParse
{
    public class Program
    {
        //
        //  Required command line args for this program:
        //
        //  Import /Files "File A.csv" "File B.xml" "File C.json" /Ids 1024 19004 2090
        //
        public static void Main()
        {
            var args =
                new CommandLineParserBuilder()
                    .AddArgument("Files", ArgumentMultiplicity.OneOrMore, true)
                    .AddArgument("Ids", ArgumentMultiplicity.OneOrMore, true)
                    .AddSwitch("Strict")
                    .CreateParser()
                    .Parse()
                    .Bind<CommandLine>();

            Assert.IsNotNull(args);
            Assert.IsNotNull(args.LeadingUnnamedValues);
            Assert.AreEqual("Import", args.LeadingUnnamedValues.ElementAt(0));
            Assert.IsNotNull(args.Files);
            Assert.AreEqual(3, args.Files.Count());
            Assert.AreEqual("File A.csv", args.Files.ElementAt(0));
            Assert.AreEqual("File B.xml", args.Files.ElementAt(1));
            Assert.AreEqual("File C.json", args.Files.ElementAt(2));
            Assert.IsNotNull(args.Ids);
            Assert.AreEqual(3, args.Ids.Count());
            Assert.AreEqual(22118, args.Ids.Sum());
            Assert.IsFalse(args.Strict);
        }
    }
}
