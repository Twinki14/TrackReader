namespace TrackReader
{
    public class OutputOptions
    {
        public const string Position = "output";
        public const string DefaultFilename = "output.txt";

        public string Filename { get; set; } = DefaultFilename;
        public string Format { get; set; } = "{Title} - {Artist}";
    }
}
