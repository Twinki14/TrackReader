using System;

namespace TrackReader.Types
{
    public static partial class FrameRateExtensions
    {
        public static int ToInt(this FrameRate frameRate)
        {
            return frameRate switch
            {
                FrameRate.Fps23_98 => 24,
                FrameRate.Fps24 => 24,
                FrameRate.Fps25 => 25,
                FrameRate.Fps29_97 => 30,
                FrameRate.Fps30 => 30,
                FrameRate.Fps50 => 50,
                FrameRate.Fps59_94 => 60,
                FrameRate.Fps60 => 60,
                _ => throw new ArgumentOutOfRangeException(nameof(frameRate), frameRate, null)
            };
        }

        public static double ToDouble(this FrameRate frameRate)
        {
            return frameRate switch
            {
                FrameRate.Fps23_98 => 23.98,
                FrameRate.Fps24 => 24,
                FrameRate.Fps25 => 25,
                FrameRate.Fps29_97 => 29.97,
                FrameRate.Fps30 => 30,
                FrameRate.Fps50 => 50,
                FrameRate.Fps59_94 => 59.94,
                FrameRate.Fps60 => 60,
                _ => throw new ArgumentOutOfRangeException(nameof(frameRate), frameRate, null)
            };
        }

        public static FrameRate FromDouble(double value)
        {
            return value switch
            {
                23.98 => FrameRate.Fps23_98,
                24 => FrameRate.Fps24,
                25 => FrameRate.Fps25,
                29.97 => FrameRate.Fps29_97,
                30 => FrameRate.Fps30,
                50 => FrameRate.Fps50,
                59.94 => FrameRate.Fps59_94,
                60 => FrameRate.Fps60,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}
