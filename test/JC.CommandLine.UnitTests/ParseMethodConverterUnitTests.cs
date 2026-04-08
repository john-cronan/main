using JC.CommandLine.TargetTypeConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public  class ParseMethodConverterUnitTests
    {
        private Mock<ITargetTypeConverterInstances> _mockOtherConverters;
        private Mock<IFilesystem> _mockFilesystem;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockFilesystem = new Mock<IFilesystem>();
            _mockOtherConverters = new Mock<ITargetTypeConverterInstances>();
        }

        [TestMethod]
        public void Parses_int()
        {
            var testee = new ParseMethodConverter(_mockOtherConverters.Object,
                _mockFilesystem.Object);
            var result = testee.TryConvert("13", new TargetType(typeof(int)), 
                ArgumentFlags.None);
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(result.Result.Single(), 13);
        }

        [TestMethod]
        public void Parses_type_without_TryParse_method()
        {
            var testee = new ParseMethodConverter(_mockOtherConverters.Object,
                _mockFilesystem.Object);
            var result = testee.TryConvert("13", new TargetType(typeof(TypeWithParseMethod)),
                ArgumentFlags.None);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual((result.Result.Single() as TypeWithParseMethod).Value, 13);
        }

        private class TypeWithParseMethod
        {
            public static TypeWithParseMethod Parse(string s)
                => new TypeWithParseMethod(int.Parse(s));


            private readonly int _value;

            private TypeWithParseMethod(int value)
            {
                _value = value;    
            }

            public int Value => _value;
        }

    }
}
