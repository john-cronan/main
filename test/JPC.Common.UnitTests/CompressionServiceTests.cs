using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JPC.Common;
using JPC.Common.Internal;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class CompressionServiceTests
    {
        private string _largeTextFile;
        private byte[] _largeTextFileCompressed;

        [TestInitialize]
        public void TestInitialize()
        {
            var resourceBytes = ReadResourceBytes("FileList.txt");
            _largeTextFile = Encoding.UTF8.GetString(resourceBytes);
            _largeTextFileCompressed = ReadResourceBytes("FileList.gz");
        }

        private byte[] ReadResourceBytes(string parialName)
        {
            var resourceName =
                (from asm in new Assembly[] { GetType().Assembly }
                 from resName in asm.GetManifestResourceNames()
                 where resName.Contains(parialName, StringComparison.InvariantCulture)
                 select resName).Single();
            using var resourceStream = GetType().Assembly.GetManifestResourceStream(resourceName);
            var bytes = new byte[resourceStream.Length];
            resourceStream.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        [TestMethod]
        public async Task Compressed_data_is_smaller_than_source()
        {
            var inputData = Encoding.UTF8.GetBytes(_largeTextFile);
            ICompressionService testee = new CompressionService();
            var compressed = await testee.CompressAsync(inputData);
            var compressionRatio = inputData.Length / compressed.Length;
            Assert.IsTrue(compressionRatio >= 2);
        }

        [TestMethod]
        public async Task Expanded_data_is_larger_than_source()
        {
            var inputData = _largeTextFileCompressed;
            ICompressionService testee = new CompressionService();
            var expanded = await testee.ExpandAsync(inputData);
            var compressionRatio = expanded.Length / inputData.Length;
            Assert.IsTrue(compressionRatio >= 2);
        }

        [TestMethod]
        public async Task Compressed_data_Expands_to_same_value()
        {
            var originalData = Encoding.UTF8.GetBytes(_largeTextFile);
            ICompressionService testee = new CompressionService();
            var compressedData = await testee.CompressAsync(originalData);
            var expandedData = await testee.ExpandAsync(compressedData);
            var expandedText = Encoding.UTF8.GetString(expandedData);
            
            Assert.AreEqual(originalData.Length, expandedData.Length);
            Assert.IsTrue(originalData.SequenceEqual(expandedData));
            Assert.AreEqual(_largeTextFile, expandedText);
        }
    }
}
