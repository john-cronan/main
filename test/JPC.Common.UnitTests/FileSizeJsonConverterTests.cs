using JPC.Common.JsonConverters;
using System.Text.Json;

namespace JPC.Common.UnitTests
{
    [TestClass]
    public class FileSizeJsonConverterTests
    {
        [TestMethod]
        public void Deserializes_size()
        {
            var json = @"{
                ""MaxSize"": ""127MB""    
            }";
            var options = new JsonSerializerOptions() {  PropertyNameCaseInsensitive = true };
            options.Converters.Add(new FileSizeJsonConverter());
            var instance = JsonSerializer.Deserialize<ClassWithFileSizeProperty>(json, options );
            Assert.AreEqual(FileSize.Parse("127MB"), instance.MaxSize);
        }

        [TestMethod]
        public void Serializes_size()
        {
            var expected = new ClassWithFileSizeProperty { MaxSize = FileSize.From(14, FileSizeUnits.GB) };
            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new FileSizeJsonConverter());
            var json = JsonSerializer.Serialize(expected, options);
            var actual = JsonSerializer.Deserialize<ClassWithFileSizeProperty>(json, options);
            Assert.AreEqual(expected.MaxSize, actual.MaxSize);
        }


        private class ClassWithFileSizeProperty
        {
            public FileSize MaxSize { get; set; }
        }
    }
}
