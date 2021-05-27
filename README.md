## Usage
![trackreader_h](.github/images/trackreader_h.png)

- TrackReader reads a list of tracks from a specified tsv or csv file given by `input`
- After compiling a list of tracks, it'll write the tracks in-order to the given `output`, spaced out by an associated start-time 
- Each track needs a start-time in [timecode format](https://www.mediacollege.com/video/editing/timecode/), specified by the `Time` column
- The app will read the list, determine the run-time for each track based on the associated `Time` column and `Framerate` passed when starting the cli, 
  and write to the `output` in order according to the `Number` column and track run-time.
- You can manually control the current track with customizable [global hotkeys](#hotkeys)
- You can customize [the format of the output](#output)

## Demo

## Config

The `appsettings.json` provides defaults to the app. Any options passed to the cli will override the associated config found in `appsettings.json`.

### Input
- `filename` the name of the file to read from in the local app directory, must be csv or tsv, and abide by TODO
- `framerate` the framerate (as a double) to use when converting the `Time` time-code, to real-time
```json
"input": {
  "filename": "tracks.tsv",
  "framerate": 24.0
}
```

### Output
- `filename` the name of the file write the currently played track to in the local app directory
- `format` the format to use TODO
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

## Example
### tsv

### csv
