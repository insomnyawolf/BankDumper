using System.Text;
using System.Text.Json.Serialization;
using System.IO;

namespace BinaryFileTools
{
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

            if (Bytes == null)
            {
                this.Bytes = Encoding.ASCII.GetBytes(Name);
            }
            else if (PatternFile != null)
            {
                this.PatternFile = PatternFile;
                this.Bytes = PatternFile.GetPattern();
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
}
