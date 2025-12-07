using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JPC.Common.JsonConverters
{
    public class FileSizeJsonConverter : JsonConverter<FileSize>
    {
        public override FileSize Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var valueAsString = Encoding.UTF8.GetString(reader.ValueSpan.ToArray());
            return FileSize.Parse(valueAsString);
        }

        public override void Write(Utf8JsonWriter writer, FileSize value, JsonSerializerOptions options)
        {
            var valueAsString = value.ToString();
            writer.WriteStringValue(valueAsString);
        }
    }
}
