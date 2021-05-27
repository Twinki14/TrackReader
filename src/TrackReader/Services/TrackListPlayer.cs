﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Humanizer;
using Microsoft.Extensions.Options;
using Serilog;
using SmartFormat;
using SmartFormat.Core.Formatting;
using Spectre.Console;
using TrackReader.Repositories;
using TrackReader.Types;

namespace TrackReader.Services
{
    public class TrackListPlayer : ITrackListPlayer, IDisposable
    {
        private Timer _timer;

        private readonly object _lockObj = new();

        private int _minTrackIndex;
        private int _maxTrackIndex;

        private int _currentTrackIndex;

        private string _inputFilename;
        private string _outputFilename;
        private string _outputFormat;
        private FrameRate _frameRate;

        private readonly HotkeyOptions _options;
        private readonly ITrackRepository _repository;

        public TrackListPlayer(ITrackRepository repository, IOptions<HotkeyOptions> options)
        {
            _repository = repository;
            _options = options.Value;
        }

        public bool Start(string inputFilename, string outputFilename, string outputFormat, FrameRate frameRate)
        {
            if (!Stop())
            {
                return false;
            }

            _minTrackIndex = 0;
            _maxTrackIndex = 0;

            _currentTrackIndex = 0;

            _inputFilename = inputFilename;
            _outputFilename = outputFilename;
            _outputFormat = outputFormat;
            _frameRate = frameRate;

            try
            {
                _repository.ReadFrom(_inputFilename, _frameRate);

                var tracks = _repository.GetTracks().ToList();

                _minTrackIndex = 0;
                _maxTrackIndex = tracks.Count - 1;

                _currentTrackIndex = _minTrackIndex;

                var nextTrackIndex = _currentTrackIndex + 1;
                if (nextTrackIndex > _maxTrackIndex)
                    nextTrackIndex = _currentTrackIndex - 1;

                var currentTrack = CurrentTrack();
                var nextTrack = _repository.GetTrack(nextTrackIndex);
                var duration = Math.Abs(currentTrack.Time.TimeSpan.TotalMilliseconds - nextTrack.Time.TimeSpan.TotalMilliseconds);

                PlayTrack(currentTrack, TimeSpan.FromMilliseconds(duration));

                return true;
            }
            catch (InvalidOperationException e)
            {
                Log.Error(e, "No tracks were read in TrackListPlayer from our csv input!");
            }
            catch (Exception e)
            {
                Log.Error(e, "Unhandled exception thrown when starting TrackListPlayer");
            }

            return false;
        }

        public void Next()
        {
            lock (_lockObj)
            {
                if (_currentTrackIndex + 1 > _maxTrackIndex)
                    return;

                _currentTrackIndex++;

                var nextTrackIndex = _currentTrackIndex + 1;
                if (nextTrackIndex > _maxTrackIndex)
                    nextTrackIndex = _currentTrackIndex - 1;

                var currentTrack = CurrentTrack();
                var nextTrack = _repository.GetTrack(nextTrackIndex);
                var duration = Math.Abs(currentTrack.Time.TimeSpan.TotalMilliseconds - nextTrack.Time.TimeSpan.TotalMilliseconds);
                PlayTrack(currentTrack, TimeSpan.FromMilliseconds(duration));
            }
        }

        public void Previous()
        {
            lock (_lockObj)
            {
                if (_currentTrackIndex - 1 < _minTrackIndex)
                    return;

                _currentTrackIndex--;

                var nextTrackIndex = _currentTrackIndex + 1;
                if (nextTrackIndex > _maxTrackIndex)
                    nextTrackIndex = _currentTrackIndex - 1;

                var currentTrack = CurrentTrack();
                var nextTrack = _repository.GetTrack(nextTrackIndex);
                var duration = Math.Abs(currentTrack.Time.TimeSpan.TotalMilliseconds - nextTrack.Time.TimeSpan.TotalMilliseconds);
                PlayTrack(currentTrack, TimeSpan.FromMilliseconds(duration));
            }
        }

        public Track CurrentTrack() => _repository.GetTrack(_currentTrackIndex);

        public void PlayTrack(Track track, TimeSpan duration)
        {
            Log.Information("Starting Track {@TrackNumber} {@Track} for {@Duration}", track.Number, track.Title, duration.Humanize(5));

            _timer?.Dispose();
            _timer = new Timer(state => Next(), null, (int) duration.TotalMilliseconds, Timeout.Infinite);
            WriteTrack(track);
        }

        public void WriteTrack(Track track)
        {
            lock (_lockObj)
            {
                var output = track.Title;
                try
                {
                    output = Smart.Format(_outputFormat, track);
                }
                catch (FormattingException e)
                {
                    Log.Error(e, "Caught exception using custom format");
                    Log.Warning("Output format is invalid, see the log for more details");
                }
                Log.Debug("output > {@Output}", output);
                File.WriteAllText(_outputFilename, output);
            }
        }

        public bool Stop()
        {
            if (_timer == null)
            {
                return true;
            }

            _timer?.Dispose();
            _timer = null;
            return true;
        }

        public void Render(ProgressContext ctx)
        {
            var comboBuilder = new StringBuilder();

            comboBuilder.Append("[dim grey]");
            comboBuilder.AppendFormat("{0} to skip to next", _options.Next);
            comboBuilder.AppendFormat(" - {0} to go to previous track", _options.Previous);
            comboBuilder.Append("[/]");

            var playingTask = ctx.AddTask("Playing track...");
            var combinationTask = ctx.AddTask(comboBuilder.ToString());
            var quitTask = ctx.AddTask("[dim grey]CTRL+C to quit[/]");

            int lastTrackNumber;
            lock (_lockObj)
            {
                var tracks = _repository.GetTracks().ToList();
                lastTrackNumber = tracks.Any() ? tracks.Last().Number : 0;
            }

            var run = true;
            Console.CancelKeyPress += (sender, args) =>
            {
                run = false;
                args.Cancel = true;
            };

            while (run)
            {
                Track current;
                lock (_lockObj)
                {
                    current = CurrentTrack();
                }

                var builder = new StringBuilder();
                builder.AppendFormat("Playing track [lime]{0}[/] / [grey]{1}[/] > [yellow]{2}[/] - [aqua]{3}[/] - [red bold]{4} bpm[/] - [blue bold]{5} speed[/]",
                                     current.Number,
                                     lastTrackNumber,
                                     current.Title,
                                     current.Artist,
                                     current.Bpm,
                                     current.Speed);

                playingTask.Description = builder.ToString();
                ctx.Refresh();
                Thread.Sleep(15);
            }

            lock (_lockObj)
            {
                File.WriteAllText(_outputFilename, string.Empty);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
