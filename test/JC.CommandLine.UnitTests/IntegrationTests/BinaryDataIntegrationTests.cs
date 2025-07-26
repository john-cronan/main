using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JC.CommandLine.UnitTests.IntegrationTests
{
    [TestClass]
    public class BinaryDataIntegrationTests
    {
        [TestMethod]
        public void PassingBinaryData()
        {
            var args = new string[]
            {
                Environment.GetCommandLineArgs()[0],
                "-hex", "0x0102030405060708",
                "-hexNoPreamble", "AAABACADAEAF",
                "-base64", "D+3LqYdlQyE=",
                "-base64NoFlags", "D+3LqYdlQyE=",
                "-singleByte", "0x20",
                "-unparsedString", "0x0102030405060708"
            };
            var commandLine =
                new CommandLineParserBuilder()
                    .UseArgumentDelimitter('-')
                    .UseExactNameMatching()
                    .AddArgument("hex", ArgumentMultiplicity.One, true)
                    .AddArgument("hexNoPreamble", ArgumentMultiplicity.One, true, ArgumentFlags.AssumeHexadecimal)
                    .AddArgument("base64", ArgumentMultiplicity.One, true, ArgumentFlags.AssumeBase64)
                    .AddArgument("base64NoFlags", ArgumentMultiplicity.One, true)
                    .AddArgument("singleByte", ArgumentMultiplicity.One, true)
                    .AddArgument("unparsedString", ArgumentMultiplicity.One, true)
                    .CreateParser()
                    .Parse(args)
                    .Bind<BinaryArgumentsTarget>();
            var expectedBytes = Binary.FromHex("0x0102030405060708");
            Assert.IsTrue(commandLine.Hex.SequenceEqual(expectedBytes));
            expectedBytes = Binary.FromHex("aaabacadaeaf");
            Assert.IsTrue(commandLine.HexNoPreamble.SequenceEqual(expectedBytes));
            expectedBytes = Binary.FromHex("0xfedcba987654321");
            Assert.IsTrue(commandLine.Base64.SequenceEqual(expectedBytes));
            Assert.IsTrue(commandLine.Base64NoFlags.SequenceEqual(expectedBytes));
            Assert.AreEqual(commandLine.SingleByte, 32);
            Assert.AreEqual(commandLine.UnparsedString, "0x0102030405060708");
            Assert.IsTrue(commandLine.BeginInitCalled);
            Assert.IsTrue(commandLine.EndInitCalled);
        }
    }
}
