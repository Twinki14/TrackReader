using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TrackReader.Services;

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
            [CommandArgument(0, "[Input]")]
            [Description("The tracks file to read as input. [dim]" + DefaultInput + " by default[/]")]
            [DefaultValue(DefaultInput)]
            public string Input { get; init; }

            [CommandArgument(1, "[Output]")]
            [Description("The file to write the curently playing track to. [dim]" + DefaultOutput + " by default[/]")]
            [DefaultValue(DefaultOutput)]
            public string Output { get; init; }
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
                           _trackListPlayer.Start(settings.Input, settings.Output);
                           _trackListPlayer.Render(ctx);
                       });
            return 1;
        }
    }
}
