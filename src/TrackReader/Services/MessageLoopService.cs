using System;
using System.Runtime.InteropServices;
using System.Threading;
using Serilog;

namespace TrackReader.Services
{
    public class MessageLoopService : IMessageLoopService, IDisposable
    {
        private const int WM_INPUT = 0x00FF;

        private struct Msg
        {
            public IntPtr HWnd;
            public uint Message;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
        }

        [DllImport("user32.dll")]
        private static extern int GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin,uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref Msg lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref Msg lpMsg);

        private Thread _thread;
        private readonly IHookService _hookService;
        private readonly CancellationTokenSource _stoppingCts = new();

        public MessageLoopService(IHookService hookService)
        {
            _hookService = hookService;
        }

        public bool Startup()
        {
            if (!Shutdown())
            {
                return false;
            }

            _thread = new Thread(MessageLoop)
            {
                IsBackground = true // stops the thread whenever the process is done executing
            };

            _thread.Start();
            Log.Information("Started message queue thread");
            return _thread.IsAlive;
        }

        public void MessageLoop()
        {
            Log.Information("Installing hooks");
            _hookService.InstallHooks();
            while (GetMessage(out var message, IntPtr.Zero, WM_INPUT, WM_INPUT) > 0 && !_stoppingCts.Token.IsCancellationRequested)
            {
                TranslateMessage(ref message);
                DispatchMessage(ref message);
            }
            Log.Information("Releasing hooks");
            _hookService.ReleaseHooks();
        }

        public bool Shutdown()
        {
            if (_thread is not {IsAlive: true})
            {
                return true;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {

            }

            _thread.Join();
            Log.Information("Joining message queue thread");
            return !_thread.IsAlive;
        }

        public void Dispose()
        {
            _stoppingCts?.Dispose();
            _hookService?.Dispose();
        }
    }
}
