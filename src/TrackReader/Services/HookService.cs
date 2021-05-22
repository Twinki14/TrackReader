using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Microsoft.Extensions.Options;
using Serilog;

namespace TrackReader.Services
{
    public class HookService : IHookService
    {
        private readonly ITrackListPlayer _player;
        private readonly HotkeyOptions _hotkeyOptions;
        private IKeyboardMouseEvents _globalHook;

        public HookService(ITrackListPlayer player, IOptions<HotkeyOptions> options)
        {
            _player = player;
            _hotkeyOptions = options.Value;
        }

        public void InstallHooks()
        {
            if (_globalHook != null) // hooks are already installed
                return;

            var next = Combination.FromString(_hotkeyOptions.Next);
            var previous = Combination.FromString(_hotkeyOptions.Previous);
            var combinations = new Dictionary<Combination, Action>
            {
                { next, _player.Next },
                { previous, _player.Previous }
            };

            foreach (var (key, value) in combinations)
            {
                Log.Information("Added combination {@Key} that triggers {@Method}",
                                key.ToString(),
                                value.Method.Name);
            }

            _globalHook = Hook.GlobalEvents();
            _globalHook.OnCombination(combinations);

            Log.Information("Installed low-level keyboard hooks");
        }

        private void GlobalHookOnKeyDown(object sender, KeyEventArgs e)
        {
            Log.Information("KeyPress: \t{@Key}", e);
        }

        public void ReleaseHooks()
        {
            if (_globalHook == null) // no hooks to release
                return;

            _globalHook.Dispose();
            _globalHook = null;

            Log.Information("Released LL keyboard hooks");
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Log.Information("KeyPress: \t{@Key}", e.KeyChar);
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
