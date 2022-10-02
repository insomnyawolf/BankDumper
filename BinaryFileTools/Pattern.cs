using System.Text;
using System.Text.Json.Serialization;
using System.IO;

namespace BinaryFileTools
{
    public abstract class BasePattern
    {
        public string Name { get; protected set; }

        [JsonConverter(typeof(ByteArrayConverter))]
        public byte[] Bytes { get; protected set; }
        public int Length { get => Bytes.Length; }

        public BasePattern(string Name, byte[] Bytes = null)
        {
            this.Name = Name;

            // Get Bytes from the byte array
            if (Bytes != null)
            {
                this.Bytes = Bytes;
            }
            // Get bytes form the name
            else
            {
                this.Bytes = Encoding.ASCII.GetBytes(Name);
            }
        }

        public virtual Stream GetReader(Stream target)
        {
            // need to modify reader methods
            return target;
        }

        public virtual Stream GetWriter(Stream target)
        {
            // need to modify writer methods
            return target;
        }
    }

    public class PatternSettings : BasePattern
    {
        public PatternFile? PatternFile { get; }

        [JsonConstructor]
        public PatternSettings(string Name, byte[]? Bytes = null, PatternFile? PatternFile = null) : base(Name, Bytes)
        {
            this.Name = Name;

            // Get bytes from the file
            if (PatternFile != null)
            {
                this.PatternFile = PatternFile;
                this.Bytes = PatternFile.GetPattern();
            }
            // Get Bytes from the byte array
            else if (Bytes != null)
            {
                this.Bytes = Bytes;
            }
            // Get bytes form the name
            else
            {
                this.Bytes = Encoding.ASCII.GetBytes(Name);
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
