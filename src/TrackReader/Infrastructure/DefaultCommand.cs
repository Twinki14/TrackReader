using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TrackReader.Services;
using TrackReader.Types;

namespace TrackReader.Infrastructure
{
    public class DefaultCommand : Command<DefaultCommand.Settings>
    {
        private readonly IMessageLoopService _messageLoopService;
        private readonly ITrackListPlayer _trackListPlayer;

        private const string DefaultInput = "tracks.tsv";
        private const string DefaultOutput = "output.txt";

        public class Settings : CommandSettings
        {
            [CommandOption("-i|--input")]
            [Description("The tracks file to read as input. [dim]" + DefaultInput + " by default[/]")]
            [DefaultValue(DefaultInput)]
            public string Input { get; init; }

            [CommandOption("-o|--output")]
            [Description("The file to write the curently playing track to. [dim]" + DefaultOutput + " by default[/]")]
            [DefaultValue(DefaultOutput)]
            public string Output { get; init; }

            [CommandOption("-f|--framerate|--fps")]
            [Description("The framerate to use in TimeCode conversions. [dim]24 fps by default[/]")]
            [DefaultValue(24.0)]
            public double Fps { get; init; }
        }

        public DefaultCommand(IMessageLoopService messageLoop, ITrackListPlayer trackListPlayer)
        {
            _messageLoopService = messageLoop;
            _trackListPlayer = trackListPlayer;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.Progress()
                       .AutoRefresh(true)
                       .AutoClear(false)
                       .HideCompleted(false)
                       .Columns(new TaskDescriptionColumn { Alignment = Justify.Left })
                       .Start(ctx =>
                       {
                           _messageLoopService.Startup();
                           _trackListPlayer.Start(settings.Input, settings.Output, new FrameRate().FromDouble(settings.Fps));
                           _trackListPlayer.Render(ctx);
                       });
            return 1;
        }
    }
}
