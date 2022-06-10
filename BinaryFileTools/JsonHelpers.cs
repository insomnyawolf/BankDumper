using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinaryFileTools
{
    // You can't easily read/write byte[] without a custom converter like thisone
    internal class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var charArray = JsonSerializer.Deserialize<uint[]>(ref reader);

            if (charArray == null)
            {
                return null;
            }

            return Unsafe.As<byte[]>(charArray);
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            for (int i = 0; i < value.Length; i++)
            {
                writer.WriteNumberValue(value[i]);
            }

            writer.WriteEndArray();
        }
    }

    public abstract class JsonSerializable
    {
        internal static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }

        public string ToString(object value)
        {
            return JsonSerializer.Serialize(value, JsonSerializerOptions);
        }
    }
}
