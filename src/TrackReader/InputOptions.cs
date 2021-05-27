namespace TrackReader
{
    public class InputOptions
    {
        public const string Position = "input";
        public const string DefaultFilename = "tracks.csv";
        public const double DefaultFramerate = 24.0;

        public string Filename { get; set; } = DefaultFilename;
        public double Framerate { get; set; } = DefaultFramerate;
    }
}
