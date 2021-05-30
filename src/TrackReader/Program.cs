using System;
using System.Text;
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
            Console.OutputEncoding = Encoding.Unicode;

            Log.Logger = new LoggerConfiguration()
                         .WriteTo.File("Log.txt", LogEventLevel.Verbose, "[{Timestamp:yyyy-MM-dd:HH:mm:ss.ff} {Level:u4}] {Message:lj}{NewLine}{Exception}",
                                       rollingInterval: RollingInterval.Minute, rollOnFileSizeLimit: true, retainedFileCountLimit: 5, shared: false,
                                       hooks: new HeaderWriter("-----------------------", true))
                         .WriteTo.SpectreConsole("{Level:u3} > {Message:lj}{NewLine}{Exception}", LogEventLevel.Information)
                         .MinimumLevel.Verbose()
                         .CreateLogger();

            var conf = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", true, false)
                                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IHookService, HookService>();
            services.AddSingleton<IMessageLoopService, MessageLoopService>();
            services.AddSingleton<ITrackRepository, TrackRepository>();
            services.AddSingleton<ITrackListPlayer, TrackListPlayer>();

            services.Configure<HotkeyOptions>(o => conf.GetSection(HotkeyOptions.Position).Bind(o));
            services.Configure<InputOptions>(o => conf.GetSection(InputOptions.Position).Bind(o));
            services.Configure<OutputOptions>(o => conf.GetSection(OutputOptions.Position).Bind(o));

            var registrar = new TypeRegistrar(services);
            var app = new CommandApp<DefaultCommand>(registrar);

            app.Configure(config =>
            {
                config.SetApplicationName("TrackReader");

                config.AddExample(new[] {"-i tracks.tsv", "-o output.txt", "-f 24"});
                config.AddExample(new[] {"-i tracks.tsv", "-o output.txt", "-f 59.94"});
                config.AddExample(new[] {"-i tracks.tsv", "-o output.txt", "--framerate=23.97"});
                config.AddExample(new[] {"--input=tracks.tsv", "--output=output.txt", "--fps=23.97"});

                config.ValidateExamples();
            });

            var result = app.Run(args);
            Console.ReadKey();
            Log.CloseAndFlush();
            return result;
        }
    }
}
