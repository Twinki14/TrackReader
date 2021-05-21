using System;
using System.Text.RegularExpressions;

namespace TrackReader.Types
{
    public enum FrameRate
    {
        /// <summary>
        ///     SMPTE 23.98frames/sec.
        /// </summary>
        Fps23_98,
        /// <summary>
        ///     SMPTE 24frames/sec.
        /// </summary>
        Fps24,
        /// <summary>
        ///     SMPTE 25frames/sec.
        /// </summary>
        Fps25,
        /// <summary>
        ///     SMPTE 29.97frames/sec.
        /// </summary>
        Fps29_97,
        /// <summary>
        ///     SMPTE 30frames/sec.
        /// </summary>
        Fps30,
        /// <summary>
        ///     SMPTE 50frames/sec.
        /// </summary>
        Fps50,
        /// <summary>
        ///     SMPTE 59.94frames/sec.
        /// </summary>
        Fps59_94,
        /// <summary>
        ///     SMPTE 60frames/sec.
        /// </summary>
        Fps60
    }

    public readonly partial struct TimeCode
    {
        private const int MillisInt = 1000;
        private const string TimeCodePattern = @"^(?<hours>[0-2][0-9]):(?<minutes>[0-5][0-9]):(?<seconds>[0-5][0-9])[:|;|\.](?<frame>[0-9]{2,3})$";

        public TimeSpan TimeSpan { get; }

        public TimeCode(int hours, int minutes, int seconds, int frame = 0, FrameRate frameRate = FrameRate.Fps24)
        {
            var fps = frameRate.ToDouble();
            var frameMs = MillisInt / fps * frame;
            var frameMsInt = (int) Math.Round(frameMs, MidpointRounding.AwayFromZero);

            TimeSpan = new TimeSpan(0, hours, minutes, seconds, frameMsInt);
        }

        public static TimeCode FromString(string input, FrameRate frameRate)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(nameof(input));
            }

            var regex = new Regex(TimeCodePattern);
            var match = regex.Match(input);

            if (!match.Success)
            {
                throw new ArgumentException("Input text was not in valid TimeCode format.", nameof(input));
            }

            var tc = new TimeCode(hours: int.Parse(match.Groups["hours"].Value),
                                  minutes: int.Parse(match.Groups["minutes"].Value),
                                  seconds: int.Parse(match.Groups["seconds"].Value),
                                  frame: int.Parse(match.Groups["frame"].Value),
                                  frameRate: frameRate);
            return tc;
        }
    }
}
