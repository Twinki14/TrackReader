using System;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace TrackReader.Services
{
    public interface IMessageLoopService
    {
        bool Start(ProgressTask task);
        bool Shutdown();
        void MessageLoop();
    }
}
