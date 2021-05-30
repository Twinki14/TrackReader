using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Serilog;
using TrackReader.Types;

namespace TrackReader.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        private readonly CsvConfiguration _csvConfiguration;
        private IEnumerable<Track> _tracks = Array.Empty<Track>();

        public TrackRepository()
        {
            _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                AllowComments = true,
                DetectDelimiter = true,
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };
        }

        public bool ReadFrom(string inputFile, FrameRate frameRate)
        {
            if (string.IsNullOrEmpty(inputFile))
            {
                throw new ArgumentException("Input is null or empty, we need an input file to process! Check appsettings.json for valid configuration");
            }

            Log.Information("Attempting to read input file {@File} for tracks", inputFile);
            try
            {
                using (var reader = new StreamReader(inputFile))
                using (var csv = new CsvReader(reader, _csvConfiguration))
                {
                    ToTimeCodeConverter.FrameRate = frameRate; // hacky fix TODO
                    _tracks = csv.GetRecords<Track>().ToList().OrderBy(track => track.Number);
                    Log.Information("Read {@Count} tracks from input file", _tracks.Count());
                    return true;
                }
            }
            catch (FileNotFoundException e)
            {
                Log.Debug(e, "Input file not found");
                throw new FileNotFoundException($"Input file '{inputFile}' not found", e);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Unhandled exception");
                throw;
            }
        }

        public Track GetTrack(int index) => _tracks.ElementAt(index);
        public IEnumerable<Track> GetTracks() => _tracks;
    }
}
