using System.Collections.Generic;

namespace TrackReader.Repositories
{
    public interface ITrackRepository
    {
        public void ReadFrom(string inputFile);
        public Track GetTrack(int number);
        public IEnumerable<Track> GetTracks();
    }
}
