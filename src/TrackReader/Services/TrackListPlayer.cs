using System;
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

        private bool _started;
        private ProgressTask _setupTask;

        private int _minTrackIndex;
        private int _maxTrackIndex;

        private int _currentTrackIndex;

        private string _inputFilename;
        private string _outputFilename;
        private string _outputFormat;
        private FrameRate _frameRate;

        private readonly HotkeyOptions _hotkeyOptions;
        private readonly ITrackRepository _repository;

        public TrackListPlayer(ITrackRepository repository, IOptions<HotkeyOptions> options)
        {
            _repository = repository;
            _hotkeyOptions = options.Value;
        }

        public bool Setup(ProgressTask task, string inputFilename, string outputFilename, string outputFormat, FrameRate frameRate)
        {
            if (!Stop())
            {
                return false;
            }

            _started = false;
            _setupTask = task;
            _minTrackIndex = 0;
            _maxTrackIndex = 0;

            _currentTrackIndex = 0;

            _inputFilename = inputFilename;
            _outputFilename = outputFilename;
            _outputFormat = outputFormat;
            _frameRate = frameRate;

            Log.Information("Using {@Input} as our input file", _inputFilename);
            Log.Information("Using {@Framerate} as our framerate", _frameRate);
            Log.Information("Using {@Output} as our output file", _outputFilename);

            if (!_repository.ReadFrom(_inputFilename, _frameRate))
            {
                Log.Debug("Track list player failed reading the tracks list!");
                throw new InvalidOperationException("Track list player failed reading the tracks list!");
            }

            var tracks = _repository.GetTracks().ToList();

            _minTrackIndex = 0;
            _maxTrackIndex = tracks.Count - 1;

            _currentTrackIndex = _minTrackIndex;

            ClearFile(_outputFilename);

            Log.Information("Track list player ready");
            return true;
        }

        public void Start()
        {
            if (_started)
            {
                _setupTask.StopTask();
                return;
            }

            lock (_lockObj)
            {
                var nextTrackIndex = _currentTrackIndex + 1;
                if (nextTrackIndex > _maxTrackIndex)
                    nextTrackIndex = _currentTrackIndex - 1;

                var currentTrack = CurrentTrack();
                var nextTrack = _repository.GetTrack(nextTrackIndex);
                var duration = Math.Abs(currentTrack.Time.TimeSpan.TotalMilliseconds - nextTrack.Time.TimeSpan.TotalMilliseconds);

                PlayTrack(currentTrack, TimeSpan.FromMilliseconds(duration));

                _started = true;
                _setupTask.StopTask();
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
            _setupTask.StopTask();
            return true;
        }

        public void Next()
        {
            lock (_lockObj)
            {
                if (!_started)
                {
                    return;
                }

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
                if (!_started)
                {
                    return;
                }

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
                try
                {
                    var output = Smart.Format(_outputFormat, track);
                    Log.Debug("output > {@Output}", output);
                    File.WriteAllText(_outputFilename, output);
                }
                catch (FormattingException e)
                {
                    Log.Debug(e, "Exception using custom format - {@OutputFormat}", _outputFormat);
                    throw new FormatException("Custom format exception", e);
                }
                catch (Exception e)
                {
                    Log.Debug(e, "Unhandled exception");
                    throw new Exception("Unhandled exception", e);
                }
            }
        }

        public void ClearFile(string filename)
        {
            lock (_lockObj)
            {
                Log.Information("Clearing file {@Filename} of any content", filename);
                File.WriteAllText(_outputFilename, string.Empty);
            }
        }

        public void Render(ProgressContext ctx)
        {
            var cancelTask = ctx.AddTask("[dim grey]CTRL+C to quit[/]");
            var playingTask = ctx.AddTask("[dim][/]", false);

            int lastTrackNumber;
            lock (_lockObj)
            {
                var tracks = _repository.GetTracks().ToList();
                lastTrackNumber = tracks.Any() ? tracks.Last().Number : 0;
            }

            Console.CancelKeyPress += (sender, args) =>
            {
                playingTask.StopTask();
                args.Cancel = true;
            };

            while (!playingTask.IsFinished)
            {
                if (!_started)
                {
                    // No hotkey, so start manually
                    if (string.IsNullOrWhiteSpace(_hotkeyOptions.Start))
                    {
                        Start();
                        continue;
                    }

                    _setupTask.Description = $"[lime]Press {_hotkeyOptions.Start} to start the track list[/]";
                } else
                {
                    var current = CurrentTrack();
                    playingTask.Description = $"Playing track [lime]{current.Number}[/] / [grey]{lastTrackNumber}[/] > [yellow]{current.Title}[/] - [aqua]{current.Artist}[/] - [red bold]{current.Bpm} bpm[/] - [blue bold]{current.Speed} speed[/]";

                    if (!playingTask.IsStarted)
                    {
                        playingTask.StartTask();
                        cancelTask.StopTask();

                        var hasNext = !string.IsNullOrWhiteSpace(_hotkeyOptions.Next);
                        var hasPrevious = !string.IsNullOrWhiteSpace(_hotkeyOptions.Previous);

                        if (hasNext || hasPrevious)
                        {
                            var comboBuilder = new StringBuilder();

                            comboBuilder.Append("[dim grey]");

                            if (hasNext)
                                comboBuilder.AppendFormat("{0} to skip to next", _hotkeyOptions.Next);

                            if (hasNext && hasPrevious)
                                comboBuilder.AppendFormat(" - ");

                            if (hasPrevious)
                                comboBuilder.AppendFormat("{0} to go to previous track", _hotkeyOptions.Previous);

                            comboBuilder.Append("[/]");
                            ctx.AddTask(comboBuilder.ToString());
                        }
                        cancelTask = ctx.AddTask("[dim grey]CTRL+C to quit[/]");
                    }
                }

                ctx.Refresh();
                Thread.Sleep(15);
            }

            ClearFile(_outputFilename);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
