using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankDumperLib
{
    // You can't easily read/write byte[] without a custom converter like thisone
    internal class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var charArray = JsonSerializer.Deserialize<sbyte[]>(ref reader);

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

    public class MagicNumber
    {
        public string Text { get; }
        [JsonConverter(typeof(ByteArrayConverter))]
        public byte[] Bytes { get; }

        [JsonConstructor]
        public MagicNumber(string Text, byte[]? Bytes = null)
        {
            this.Text = Text;

            if (Bytes == null)
            {
                this.Bytes = Encoding.ASCII.GetBytes(Text);
            }
            else
            {
                this.Bytes = Bytes;
            }
        }
    }

    public class MagicNumberFound
    {
        public MagicNumber Pattern { get; internal set; }
        public long Position { get; internal set; }
    }
}
