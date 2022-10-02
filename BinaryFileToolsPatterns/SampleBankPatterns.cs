using BinaryFileTools;
using System.IO;

namespace BinaryFileToolsPatterns
{
    public static class Extensions
    {
        public static void AddPatternAKPK(this PatternSearch obj)
        {
            obj.TryAddPattern(new PatternAKPK());
        }

        public static void AddPatternBKHD(this PatternSearch obj)
        {
            obj.TryAddPattern(new PatternBKHD());
        }

        public static void AddPatternFSB5(this PatternSearch obj)
        {
            obj.TryAddPattern(new PatternFSB5());
        }
    }

    public class PatternAKPK : BasePattern
    {
        public PatternAKPK() : base("AKPK") { }
    }

    public class PatternBKHD : BasePattern
    {
        public PatternBKHD() : base("BKHD") { }
    }

    public class PatternFSB5 : BasePattern
    {
        public PatternFSB5() : base("FSB5") { }

        // This does nothing especial, it's just an example about how to implement a custom encoder/decoder
        public override Stream GetReader(Stream target)
        {
            return new DefaultConverterStream(target);
        }

        public override Stream GetWriter(Stream target)
        {
            return new DefaultConverterStream(target);
        }
    }

    // Sample custom converters for special formats
    public class DefaultConverterStream : Stream
    {
        public DefaultConverterStream(Stream Target)
        {
            this.BaseStream = Target;
        }
        public Stream BaseStream { get; }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
    }
}