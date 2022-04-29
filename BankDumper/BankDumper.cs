using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankDumperLib
{
    public class MagicNumber
    {
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

        public string Text { get; }
        [JsonConverter(typeof(ByteArrayConverter))]
        public byte[] Bytes { get; }

        [JsonConstructor]
        public MagicNumber(string Text, byte[] Bytes = null)
        {
            this.Text = Text;

            if (Bytes == null)
            {
                this.Bytes =Encoding.ASCII.GetBytes(Text);
            }
            else
            {
                this.Bytes = Bytes;
            }
        }

    }

    public class PatternFind
    {
        public MagicNumber Pattern { get; internal set; }
        public long Position { get; internal set; }
    }

    // Take care for pattern at position 0

    public static class BankDumper
    {
        public static void LoadDefaultMagicNumbers()
        {
            MagicNumbers.LoadDefaultMagicNumbers();
        }

        public static void LoadDefaultMagicNumbers(this List<MagicNumber> numbers)
        {
            numbers.Add(new MagicNumber("FSB5"));
            numbers.Add(new MagicNumber("BKHD"));
            numbers.Add(new MagicNumber("AKPK"));
            UpdateCache();
        }

        public static bool TryAddMagicNumber(MagicNumber number)
        {
            var result = TryFindPattern(number.Bytes);
            if (result != null)
            {
                // Prevents adding the same number twice
                return false;
            }

            MagicNumbers.Add(number);
            UpdateCache();
            // Added!
            return true;
        }

        public static bool TryRemoveMagicNumber(string name)
        {
            for (int i = 0; i < MagicNumbers.Count; i++)
            {
                if (MagicNumbers[i].Text == name)
                {
                    MagicNumbers.RemoveAt(i);
                    UpdateCache();
                    //Removed Sucessfully
                    return true;
                }

            }

            // Not Found
            return false;
        }

        private static readonly List<MagicNumber> MagicNumbers = new List<MagicNumber>();

        private static int LargestPattern = 0;

        private static void UpdateCache()
        {
            // Initialize data that will be helpful later
            for (int MagicNumberIndex = 0; MagicNumberIndex < MagicNumbers.Count; MagicNumberIndex++)
            {
                var current = MagicNumbers[MagicNumberIndex];

                if (LargestPattern < current.Bytes.Length)
                {
                    LargestPattern = current.Bytes.Length;
                }
            }
        }

        /// <summary>
        /// Calls EndsWithPattern on eacha available pattern on MagicNumbers list
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Pattern matched or null</returns>
        private static MagicNumber? TryFindPattern(byte[] value)
        {
            for (int MagicNumberIndex = 0; MagicNumberIndex < MagicNumbers.Count; MagicNumberIndex++)
            {
                var current = MagicNumbers[MagicNumberIndex];
                if (EndsWithPattern(value, current.Bytes))
                {
                    Console.WriteLine($"Detected => {current.Text}");
                    return current;
                }
            }
            return null;
        }

        private static bool EndsWithPattern(byte[] value, byte[] pattern)
        {
#warning maybe optimize this
            if (pattern.Length > value.Length)
            {
                return false;
            }

            for (int i = pattern.Length - 1; i > -1; i--)
            {
                if (value[i] != pattern[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static PatternFind? Extract(Stream input, Stream output)
        {
            var searchBuffer = new byte[LargestPattern];
            var searchBufferLastPosition = searchBuffer.Length - 1;

            // Input Loop
            int currentByte;
#warning Reading it like that will kill the performance, needs to be updated
            // -1 means that there are no more bytes available
            // Wait why does readbyte returns int???
            while ((currentByte = input.ReadByte()) != -1)
            {
                searchBuffer[searchBufferLastPosition] = (byte)currentByte;

                var pattern = TryFindPattern(searchBuffer);
                if (pattern != null)
                {
                    // Pattern dettected

                    // Save useful Data
                    var data = new PatternFind()
                    {
                        Pattern = pattern,
                        Position = input.Position - pattern.Bytes.Length,
                    };

                    // That should copy from the current position to the end of the stream
                    input.CopyTo(output);
                    return data;
                }

                // Shift Bytes
                for (int i = 0; i < searchBufferLastPosition; i++)
                {
                    searchBuffer[i] = searchBuffer[i + 1];
                }
            }

            Console.WriteLine($"File didn't match any pattern.");
            return null;
        }

        public static PatternFind? ExtractMultiple(Stream input, Stream output)
        {
            var found = Extract(input, output);

            if (found != null)
            {
#warning i don't know if that's supported on all kinds of streams
                input.SetLength(found.Position);
                // Shorten the stream so it only contains certain file
            }

            return found;
        }
    }
}
