using Microsoft.Extensions.DependencyInjection;
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
            var services = new ServiceCollection();

            services.AddSingleton<IHookService, HookService>();
            services.AddSingleton<IMessageLoopService, MessageLoopService>();
            services.AddSingleton<ITrackRepository, TrackRepository>();
            services.AddSingleton<ITrackListPlayer, TrackListPlayer>();

            var registrar = new TypeRegistrar(services);

            var app = new CommandApp<DefaultCommand>(registrar);
            app.Configure(config =>
            {
                config.SetApplicationName("TrackReader");
                config.ValidateExamples();

                config.AddExample(new[] {"tracks.csv", "output.txt"});
            });

            return app.Run(args);
        }
    }
}
