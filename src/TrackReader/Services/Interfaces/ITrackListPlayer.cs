using System;
using Spectre.Console;
using TrackReader.Repositories;
using TrackReader.Types;

namespace TrackReader.Services
{
    public interface ITrackListPlayer
    {
        public bool Start(string inputFilename, string outputFilename, FrameRate frameRate);

        public void Next();
        public void Previous();


        public Track CurrentTrack();
        public void PlayTrack(Track track, TimeSpan duration);
        public void WriteTrack(Track track);

        public bool Stop();

        public void Render(ProgressContext ctx);
    }
}
