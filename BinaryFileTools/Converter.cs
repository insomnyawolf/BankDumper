using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace BinaryFileTools
{
    // You can't easily read/write byte[] without a custom converter like thisone
    internal class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var charArray = JsonSerializer.Deserialize<uint[]>(ref reader);

            if (charArray == null)
            {
                return null;
            }

            // DOES NOT WORK, DATA BEHAVE WEIRD IF YOU DO THIS, SERIOUSLY
            // IT TOOK 3 HOURS TO FIGURE IT OUT

            //    return Unsafe.As<byte[]>(charArray);

            var bytes = new byte[charArray.Length];

            for (int index = 0; index < bytes.Length; index++)
            {
                bytes[index] = (byte)charArray[index];
            }

            return bytes;
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
}