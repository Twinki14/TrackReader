using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using Spectre.Console;
using TrackReader.Repositories;

namespace TrackReader.Services
{
    public class TrackListPlayer : ITrackListPlayer, IDisposable
    {
        private Timer _timer;

        private readonly object _lockObj = new();
        private readonly ISynchronizeInvoke _timerObj;

        private int _minTrackIndex;
        private int _maxTrackIndex;

        private int _currentTrackIndex;

        private string _inputFilename;
        private string _outputFilename;

        private readonly ITrackRepository _repository;

        public TrackListPlayer(ITrackRepository repository)
        {
            _repository = repository;
        }

        public bool Start(string inputFilename, string outputFilename)
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

            try
            {
                _repository.ReadFrom(_inputFilename);

                var tracks = _repository.GetTracks().ToList();

                _minTrackIndex = 0;
                _maxTrackIndex = tracks.Count - 1;

                _currentTrackIndex = _minTrackIndex;

                PlayTrack(CurrentTrack());

                return true;
            }
            catch (InvalidOperationException e)
            {
                Log.Information(e, "No tracks were read in TrackListPlayer from our csv input!");
                AnsiConsole.WriteException(e);
            }
            catch (Exception e)
            {
                Log.Information(e, "Unhandled exception thrown when starting TrackListPlayer");
                AnsiConsole.WriteException(e);
            }

            return false;
        }

        public bool Running()
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            lock (_lockObj)
            {
                if (_currentTrackIndex + 1 > _maxTrackIndex)
                    return;

                _currentTrackIndex++;
                var currentTrack = CurrentTrack();
                PlayTrack(currentTrack);
            }
        }

        public void Previous()
        {
            lock (_lockObj)
            {
                if (_currentTrackIndex - 1 < _minTrackIndex)
                    return;

                _currentTrackIndex--;
                var currentTrack = CurrentTrack();
                PlayTrack(currentTrack);
            }
        }

        public Track CurrentTrack() => _repository.GetTrack(_currentTrackIndex);

        public void PlayTrack(Track track)
        {
            var period = track.Time.TimeSpan;

            //Log.Information("Starting Track  [{@TrackNumber}] {@Track} for {@Period}", track.Number, track.Title, period);
            AnsiConsole.MarkupLine("Starting Track  {0} {1} {2}", track.Number, track.Title, period);

            _timer?.Dispose();
            _timer = new Timer(state => Next(), null, (int) TimeSpan.FromSeconds(5).TotalMilliseconds, Timeout.Infinite);
            WriteTrack(track);
        }

        public void WriteTrack(Track track)
        {
            lock (_lockObj)
            {
                File.WriteAllText(_outputFilename, track.Title);
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

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
