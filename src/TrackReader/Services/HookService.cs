using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Spectre.Console;

namespace TrackReader.Services
{
    public class HookService : IHookService
    {
        private IKeyboardMouseEvents _globalHook;

        public void InstallHooks()
        {
            if (_globalHook != null) // hooks are already installed
                return;

            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyPress += GlobalHookKeyPress;
        }

        public void ReleaseHooks()
        {
            if (_globalHook == null) // no hooks to release
                return;

            _globalHook.Dispose();
            _globalHook = null;
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            AnsiConsole.MarkupLine("KeyPress: \t{0}", e.KeyChar);
        }

        public void RegisterHotkeys()
        {
            if (_globalHook == null) // no hooks to register our combinations to
                return;

            //_globalHook.OnCombination();
        }

        public void Dispose()
        {
            _globalHook?.Dispose();
        }
    }
}
