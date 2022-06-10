namespace BinaryFileTools
{
    public class PatternMatch : JsonSerializable
    {
        // Prevent instancing that class out of this library
        internal PatternMatch() { }

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

        public long PatternEnd { get => PositionStartWithoutPattern - 1; }
        public long PositionStartWithoutPattern { get; internal set; }
        public long LengthWithoutPattern { get => Length - Pattern.Length; }
    }
}
