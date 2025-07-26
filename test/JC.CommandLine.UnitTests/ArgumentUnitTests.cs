using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class ArgumentUnitTests
    {
        [TestMethod]
        public void Constructor_throws_if_both_Hex_and_Base64_are_specified()
        {
            try
            {
                var testee = new Argument("Key", ArgumentMultiplicity.One, true, ArgumentFlags.AssumeBase64 | ArgumentFlags.AssumeHexadecimal);
                Assert.Fail("Expected: ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("flags", ex.ParamName);
            }
        }

        [TestMethod]
        public void No_exception_when_only_hex_is_specified()
        {
            var testee = new Argument("Key", ArgumentMultiplicity.One, true, ArgumentFlags.AssumeHexadecimal);
        }
    }
}
