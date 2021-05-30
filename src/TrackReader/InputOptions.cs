namespace TrackReader
{
    public class InputOptions
    {
        private string _filename = DefaultFilename;

        public const string Position = "input";
        public const string DefaultFilename = "tracks.csv";
        public const double DefaultFramerate = 24.0;

        public string Filename
        {
            get => string.IsNullOrEmpty(_filename) ? DefaultFilename : _filename;
            set => _filename = value;
        }

        public double Framerate { get; set; } = DefaultFramerate;
    }
}
