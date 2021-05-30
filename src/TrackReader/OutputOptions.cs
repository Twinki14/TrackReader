namespace TrackReader
{
    public class OutputOptions
    {
        private string _filename = DefaultFilename;
        private string _format = DefaultFormat;

        public const string Position = "output";
        public const string DefaultFilename = "output.txt";
        public const string DefaultFormat = "{Title} - {Artist}";

        public string Filename
        {
            get => string.IsNullOrEmpty(_filename) ? DefaultFilename : _filename;
            set => _filename = value;
        }

        public string Format
        {
            get => string.IsNullOrEmpty(_format) ? DefaultFormat : _format;
            set => _format = value;
        }
    }
}
