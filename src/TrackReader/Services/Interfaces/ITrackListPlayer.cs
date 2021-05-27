﻿using System;
using Spectre.Console;
using TrackReader.Types;

namespace TrackReader.Services
{
    public interface ITrackListPlayer
    {
        public bool Setup(string inputFilename, string outputFilename, string outputFormat, FrameRate frameRate);

        public void Start();

        public void Next();
        public void Previous();

        public Track CurrentTrack();
        public void PlayTrack(Track track, TimeSpan duration);
        public void WriteTrack(Track track);

        public bool Stop();

        public void Render(ProgressContext ctx);
    }
}
