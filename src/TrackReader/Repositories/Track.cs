﻿using CsvHelper.Configuration.Attributes;
using TrackReader.Types;

namespace TrackReader.Repositories
{
    public class Track
    {
        [Name("#N")]
        public int Number { get; set; }

        [Name("Time")]
        [TypeConverter(typeof(ToTimeCodeConverter))]
        public TimeCode Time { get; set; }

        [Name("Speed")]
        public string Speed { get; set; }

        [Name("Artist")]
        public string Artist { get; set; }

        [Name("Title")]
        public string Title { get; set; }

        [Name("BPM")]
        public string Bpm { get; set; }

        [Name("Link")]
        public string Link { get; set; }

        [Name("Notes")]
        public string Notes { get; set; }
    }
}
