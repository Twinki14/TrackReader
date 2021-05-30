using System;
using System.Collections.Generic;
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

            var combinations = new Dictionary<Combination, Action>();

            try
            {
                if (!string.IsNullOrWhiteSpace(_hotkeyOptions.Start))
                    combinations.Add(Combination.FromString(_hotkeyOptions.Start), _player.Start);

                if (!string.IsNullOrWhiteSpace(_hotkeyOptions.Next))
                    combinations.Add(Combination.FromString(_hotkeyOptions.Next), _player.Next);

                if (!string.IsNullOrWhiteSpace(_hotkeyOptions.Previous))
                    combinations.Add(Combination.FromString(_hotkeyOptions.Previous), _player.Previous);
            }
            catch (ArgumentException e)
            {
                Log.Debug(e, "One of our key combinations is invalid");
                throw new ArgumentException("One of our key combinations is invalid", e);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Unhandled exception");
                throw;
            }

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

        public void ReleaseHooks()
        {
            if (_globalHook == null) // no hooks to release
                return;

            _globalHook.Dispose();
            _globalHook = null;

            Log.Information("Released LL keyboard hooks");
        }

        public void Dispose()
        {
            _globalHook?.Dispose();
        }
    }
}
