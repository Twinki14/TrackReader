## Usage
![trackreader_h](.github/images/trackreader_h.png)

- TrackReader reads a list of tracks from a specified tsv or csv file given by `input`
- After compiling a list of tracks, it'll write the tracks in-order to the given `output`, spaced out by an associated start-time 
- Each track needs a start-time in [timecode format](https://www.mediacollege.com/video/editing/timecode/), specified by the `Time` column
- The app will read the list, determine the run-time for each track based on the associated `Time` column and `Framerate` passed when starting the cli, 
  and write to the `output` in order according to the `Number` column and track run-time.
- You can manually control the current track with customizable [global hotkeys](#hotkeys)
- You can customize [the format of the output](#output)

### Notes
- May need to run with heightened permissions, eg as administrator for global hotkeys
- The `output` file **will be overwritten** of any contents at regular intervals

## Config

The `appsettings.json` provides defaults to the app. Any options passed to the cli will override the associated config found in `appsettings.json`.

### Input
- `filename` the name of the file to read from in the local app directory, must be csv or tsv, and abide by the [input format](#input-format)
- `framerate` the framerate (as a double) to use when converting the `Time` time-code, to real-time
```json
"input": {
  "filename": "tracks.tsv",
  "framerate": 24.0
}
```

### Output
- `filename` the name of the file write the currently played track to in the local app directory
- `format` the format of how the track will be written to the `output` file
    - pattern matching based on the `input` column name, eg `{Title}` will be replaced with the data in the current tracks `Title` column
    - see [input format](#input-format) for a list of columns you can use
```json
"output": {
  "filename": "output.txt",
  "format": "{Title} - {Artist}"
}
```

### Hotkeys
- `start` after reading the `input`, the app will wait until this hotkey is pressed to start playing the track-list, this hotkey is optional, 
  and if set to nothing will automatically start playing the track-list as soon as it's ready
- `next` manually starts the next track in the track-list, optional
- `previous` manually goes back a track in the track-list, optional
```json
"hotkeys": {
  "start": "Up",
  "next": "Right",
  "previous": "Left"
}
```
Hotkeys can be set to key combinations or single key presses in appsettings.json
The syntax is the name of the key found in the [windows forms keys enum.](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys)

For example, `"Next": "Alt+Control+N"` would skip to the next track when holding `Alt+Control` key modifiers, and pressing `N`. `"Previous": "P"` would go to the previous track when pressing `P`.

### appsettings.json
An example of `appsettings.json`, this configuration will use `tracks.csv` as the input, and apply a framerate of 24 when calculating track run-time. It'll output to `output.txt`
and every track will output as `Title - Artist`, eg, `Could This Be - Noisia`. The `Up` arrow key will start the track list. `Right` and `Left` arrow keys offers manual control of the current
 track being played.
```json
{
  "input": {
    "filename": "tracks.tsv",
    "framerate": 24.0
  },
  "output": {
    "filename": "output.txt",
    "format": "{Title} - {Artist}"
  },
  "hotkeys": {
    "start": "Up",
    "next": "Right",
    "previous": "Left"
  }
}
```

## Input format
- column data whitespace is trimmed, so don't worry about extra whitespace
- comments are supported, `#`, full-line comments must start with `#`
- each of the columns below are required
- the track list will be sorted by `Track number`
- extra columns that don't conflict will simply be ignored, so you can add any extra columns
- the first line must be the header

### csv
```
Track number,   Time,           Speed,      Artist,                 Title,                          BPM,        Link,                                           Notes
1,              0:00:00:00,     100.00%,    Noisia,                 Could This Be,                  86,         https://www.youtube.com/watch?v=O2l1PzsVlD0,    #
2,              0:03:54:00,     100.00%,    Mutated Forms,          The Last Time,                  87,         https://www.youtube.com/watch?v=NHzAzzXdU5o,    #
3,              0:08:38:00,     100.00%,    Fred V x Graffix,       Its Not Right But Its Ok,       87,         https://www.youtube.com/watch?v=YBvI8sH24Ec,    #
4,              0:13:46:00,     100.00%,    Noisia x Begg,          ShellShock ft. Foreign Beggars, 86,         https://www.youtube.com/watch?v=lt4iHCpIdHE,    #
5,              0:17:12:00,     100.00%,    Fox Stevenson,          All This Time,                  87,         https://www.youtube.com/watch?v=hPokJFyUq1s,    #
6,              0:21:15:00,     101.16%,    Feint ft,               The Journey,                    86 <- 87,   https://www.youtube.com/watch?v=knbkFOvfucQ,    #
7,              0:24:16:00,     100.00%,    Ross D,                 Loving You,                     175,        https://www.youtube.com/watch?v=rmuqKeNXLoU,    #
8,              0:31:02:00,     100.00%,    Colussus,               Under the Weather,              87,         https://www.youtube.com/watch?v=-ka9TH9CH6o,    #
9,              0:38:06:00,     100.00%,    Netsky x Crystal Clear, King of the stars,              174,        https://www.youtube.com/watch?v=WfTRi-_BDik,    #
10,             0:42:06:00,     100.00%,    Fred V x Graffix,       Paradise,                       174,        https://www.youtube.com/watch?v=daq_GV7Zg1Q,    #
11,             0:45:09:00,     99.40%,     Modest Intentions,      Last Summer,                    174 <- 175, https://www.youtube.com/watch?v=QubnhZczWsQ,    #
12,             0:49:53:00,     100.00%,    Fred V x Graffix,       Long Distance,                  174,        https://www.youtube.com/watch?v=2jXnytbyXTs,    #
13,             0:52:41:00,     100.00%,    Well Being,             Storms by Streetlight,          86,         https://www.youtube.com/watch?v=SuxTKmiovUk,    #
14,             0:59:21:00,     100.00%,    Joetf,                  Flashing Lights,                87.5,       Missing,                                        #
```

### tsv
```
Track number    Time            Speed	    Artist                  Title                           Bpm	        Link	                                        Notes
1               0:00:00:00      100.00%	    Noisia                  Could This Be                   86	        https://www.youtube.com/watch?v=O2l1PzsVlD0	#
2               0:03:54:00      100.00%	    Mutated Forms           The Last Time                   87	        https://www.youtube.com/watch?v=NHzAzzXdU5o	#
3               0:08:38:00      100.00%	    Fred V x Graffix        Its Not Right But Its Ok        87	        https://www.youtube.com/watch?v=YBvI8sH24Ec	#
4               0:13:46:00      100.00%	    Noisia x Begg           ShellShock ft. Foreign Beggars  86	        https://www.youtube.com/watch?v=lt4iHCpIdHE	#
5               0:17:12:00      100.00%	    Fox Stevenson           All This Time                   87	        https://www.youtube.com/watch?v=hPokJFyUq1s	#
6               0:21:15:00      101.16%	    Feint ft	            The Journey                     86 <- 87	https://www.youtube.com/watch?v=knbkFOvfucQ	#
7               0:24:16:00      100.00%	    Ross D                  Loving You                      175	        https://www.youtube.com/watch?v=rmuqKeNXLoU	#
8               0:31:02:00      100.00%	    Colussus                Under the Weather               87	        https://www.youtube.com/watch?v=-ka9TH9CH6o	#
9               0:38:06:00      100.00%	    Netsky x Crystal Clear  King of the stars               174	        https://www.youtube.com/watch?v=WfTRi-_BDik	#
10              0:42:06:00      100.00%	    Fred V x Graffix	    Paradise                        174	        https://www.youtube.com/watch?v=daq_GV7Zg1Q	#
11              0:45:09:00      99.40%	    Modest Intentions	    Last Summer                     174 <- 175	https://www.youtube.com/watch?v=QubnhZczWsQ	#
12              0:49:53:00      100.00%	    Fred V x Graffix	    Long Distance                   174	        https://www.youtube.com/watch?v=2jXnytbyXTs	#
13              0:52:41:00      100.00%	    Well Being	            Storms by Streetlight           86	        https://www.youtube.com/watch?v=SuxTKmiovUk	#
14              0:59:21:00      100.00%	    Joetf                   Flashing Lights                 87.5        #	                                        #
```
