using TrackReader.Repositories;

namespace TrackReader.Services
{
    public interface ITrackListPlayer
    {
        public bool Start(string inputFilename, string outputFilename);

        public bool Running();
        public void Next();
        public void Previous();


        public Track CurrentTrack();
        public void PlayTrack(Track track);
        public void WriteTrack(Track track);

        public bool Stop();
    }
}
