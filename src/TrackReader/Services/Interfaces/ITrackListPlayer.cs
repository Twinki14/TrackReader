using Spectre.Console;
using TrackReader.Repositories;

namespace TrackReader.Services
{
    public interface ITrackListPlayer
    {
        public bool Start(string inputFilename, string outputFilename);

        public void Next();
        public void Previous();


        public Track CurrentTrack();
        public void PlayTrack(Track track);
        public void WriteTrack(Track track);

        public bool Stop();

        public void Render(ProgressContext ctx);
    }
}
