using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Threading;

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

    public class Pattern
    {
        public string Name { get; }
        [JsonConverter(typeof(ByteArrayConverter))]
        public byte[] Bytes { get; }
        public int Length { get => Bytes.Length; }
        public PatternFile? PatternFile { get; }

        [JsonConstructor]
        public Pattern(string Name, byte[]? Bytes = null, PatternFile? PatternFile = null)
        {
            this.Name = Name;

            if (PatternFile != null)
            {
                this.PatternFile = PatternFile;
                this.Bytes = PatternFile.GetPattern();
            }
            else if (Bytes == null)
            {
                this.Bytes = Encoding.ASCII.GetBytes(Name);
            }
            else
            {
                this.Bytes = Bytes;
            }
        }
    }

    public class PatternFile
    {
        public string Path { get; }
        public int Offset { get; }
        public int Length { get; }

        [JsonConstructor]
        public PatternFile(string Path, int Offset, int Length)
        {
            this.Path = Path;
            this.Offset = Offset;
            this.Length = Length;
        }

        internal byte[] GetPattern()
        {
            using var file = File.Open(Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            var pattern = new byte[Length];

            var bytesRead = file.Read(pattern, Offset, pattern.Length);

            if (bytesRead < Length)
            {
                throw new InvalidDataException();
            }

            return pattern;
        }
    }

    public class PatternMatch
    {
        public Pattern Pattern { get; internal set; }
        public long PositionStart { get => PositionStartWithoutPattern - Pattern.Length; }
        public long PositionEnd { get; internal set; }

        public long Length
        {
            get
            {
                var length = PositionEnd - PositionStart;

                // +1 because 0 is a valid adress
                if (PositionStart == 0)
                {
                    return length + 1;
                }
                return length;
            }
        }
        public long PositionStartWithoutPattern { get; internal set; }
        public long LengthWithoutPattern { get => Length - Pattern.Length; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class PatternMatches
    {
        // I'm using this to prevent multithreading issues
        private EventWaitHandle EventWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
        internal Stream Stream { get; }
        public List<PatternMatch> Matches { get; } = new List<PatternMatch>();

        internal PatternMatches(Stream stream)
        {
            Stream = stream;
        }

        public void ExtractWithPatternTo(PatternMatch PatternMatch, Stream target)
        {
            EventWaitHandle.WaitOne();

            Stream.Position = PatternMatch.PositionStart;

            Stream.CopyTo(target, (int)PatternMatch.Length);

            EventWaitHandle.Set();
        }

        public void ExtractWithoutPatternTo(PatternMatch PatternMatch, Stream target)
        {
            EventWaitHandle.WaitOne();

            Stream.Position = PatternMatch.PositionStartWithoutPattern;

            Stream.CopyTo(target, (int)PatternMatch.LengthWithoutPattern);

            EventWaitHandle.Set();
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(Matches);
        }
    }
}
