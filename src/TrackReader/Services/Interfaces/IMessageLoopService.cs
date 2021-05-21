using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrackReader.Services
{
    public interface IMessageLoopService
    {
        bool Startup();
        bool Shutdown();
        void MessageLoop();
    }
}
