using System;

namespace TrackReader.Services
{
    public interface IHookService : IDisposable
    {
        void InstallHooks();
        void ReleaseHooks();
    }
}
