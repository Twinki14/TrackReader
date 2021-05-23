using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File.Header;
using Serilog.Sinks.SpectreConsole;
using Spectre.Console.Cli;
using TrackReader.Infrastructure;
using TrackReader.Repositories;
using TrackReader.Services;

namespace TrackReader
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                         .WriteTo.File("Log.txt", LogEventLevel.Verbose, "[{Timestamp:yyyy-MM-dd:HH:mm:ss.ff} {Level:u4}] {Message:lj}{NewLine}{Exception}",
                                       rollingInterval: RollingInterval.Minute, rollOnFileSizeLimit: true, retainedFileCountLimit: 5, shared: false,
                                       hooks: new HeaderWriter("-----------------------", true))
                         .WriteTo.SpectreConsole("{Level:u3} > {Message:lj}{NewLine}{Exception}", LogEventLevel.Verbose)
                         .MinimumLevel.Verbose()
                         .CreateLogger();

            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IHookService, HookService>();
            services.AddSingleton<IMessageLoopService, MessageLoopService>();
            services.AddSingleton<ITrackRepository, TrackRepository>();
            services.AddSingleton<ITrackListPlayer, TrackListPlayer>();
            services.Configure<HotkeyOptions>(options => configuration.GetSection(HotkeyOptions.Position)
                                                                      .Bind(options));

            var registrar = new TypeRegistrar(services);
            var app = new CommandApp<DefaultCommand>(registrar);

            app.Configure(config =>
            {
                config.SetApplicationName("TrackReader");
                config.ValidateExamples();

                config.AddExample(new[] {"-i tracks.tsv", "-o output.txt", "-f 24"});
                config.AddExample(new[] {"-i tracks.tsv", "-o output.txt", "-f 59.94"});
                config.AddExample(new[] {"-i tracks.tsv", "-o output.txt", "--framerate=23.97"});
                config.AddExample(new[] {"--input=tracks.tsv", "--output=output.txt", "--fps=23.97"});
            });

            var result = app.Run(args);
            Log.CloseAndFlush();
            return result;
        }
    }
}
