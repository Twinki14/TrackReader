using System.ComponentModel;
using System.Threading;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using TrackReader.Repositories;
using TrackReader.Services;
using TrackReader.Types;

namespace TrackReader.Infrastructure
{
    public class DefaultCommand : Command<DefaultCommand.Settings>
    {
        private readonly IMessageLoopService _messageLoopService;
        private readonly ITrackListPlayer _trackListPlayer;

        private readonly ITrackRepository _repository;
        private readonly InputOptions _inputOptions;
        private readonly OutputOptions _outputOptions;

        public class Settings : CommandSettings
        {
            [CommandOption("-i|--input")]
            [Description("The tracks file to read as input. [dim]" + InputOptions.DefaultFilename + " by default[/]")]
            public string Input { get; set; }

            [CommandOption("-o|--output")]
            [Description("The file to write the curently playing track to. [dim]" + OutputOptions.DefaultFilename + " by default[/]")]
            public string Output { get; set; }

            [CommandOption("-f|--framerate|--fps")]
            [Description("The framerate to use in TimeCode conversions. [dim]24 fps by default[/]")]
            [DefaultValue(null)]
            public double? Framerate { get; set; }
        }

        public DefaultCommand(IMessageLoopService messageLoop, ITrackListPlayer trackListPlayer,
                              IOptions<InputOptions> inputOptions, IOptions<OutputOptions> outputOptions, ITrackRepository repository)
        {
            _messageLoopService = messageLoop;
            _trackListPlayer = trackListPlayer;
            _repository = repository;
            _inputOptions = inputOptions.Value;
            _outputOptions = outputOptions.Value;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Input))
                settings.Input = _inputOptions.Filename;

            settings.Framerate ??= _inputOptions.Framerate;

            if (string.IsNullOrWhiteSpace(settings.Output))
                settings.Output = _outputOptions.Filename;


            AnsiConsole.Progress()
                       .AutoRefresh(false)
                       .AutoClear(false)
                       .HideCompleted(true)
                       .Columns(new TaskDescriptionColumn { Alignment = Justify.Left })
                       .Start(ctx =>
                       {
                           var loopTask = ctx.AddTask("Starting message loop");

                           if (!_messageLoopService.Start(loopTask))
                               return;

                           while (!loopTask.IsFinished)
                               Thread.Sleep(20);

                           var playerTask = ctx.AddTask("Starting track list player");
                           if (_trackListPlayer.Setup(playerTask,
                                                      settings.Input,
                                                      settings.Output,
                                                      _outputOptions.Format,
                                                      FrameRateExtensions.FromDouble((double) settings.Framerate)))
                           {
                               _trackListPlayer.Render(ctx);
                           }
                       });
            return 1;
        }
    }
}
