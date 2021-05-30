using System.Collections.Generic;
using TrackReader.Types;

namespace TrackReader.Repositories
{
    public interface ITrackRepository
    {
        public bool ReadFrom(string inputFile, FrameRate frameRate = FrameRate.Fps24);
        public Track GetTrack(int index);
        public IEnumerable<Track> GetTracks();
    }
}
