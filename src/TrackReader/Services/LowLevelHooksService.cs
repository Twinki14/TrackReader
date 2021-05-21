using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrackReader.Services
{
    public class LowLevelHooksService : IDisposable
    {
        private readonly Hook _hook;

        public LowLevelHooksService()
        {
            _hook = new Hook();
            _hook.OnKeyPressed += OnKeyPress;
            _hook.OnKeyPressed += OnKeyRelease;
            _hook.HookKeyboard();
        }

        public static void OnKeyPress(object source, KeyEventArgs keyEvent)
        {

        }

        public void OnKeyRelease(object source, KeyEventArgs keyEvent)
        {

        }

        private class Hook
        {
            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private const int WM_SYSKEYDOWN = 0x0104;
            private const int WM_KEYUP = 0x101;
            private const int WM_SYSKEYUP = 0x105;

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);

            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            public event EventHandler<KeyEventArgs> OnKeyPressed;
            public event EventHandler<KeyEventArgs> OnKeyReleased;

            private readonly LowLevelKeyboardProc _proc;
            private IntPtr _hookID = IntPtr.Zero;

            public Hook()
            {
                _proc = HookCallback;
            }

            public void HookKeyboard()
            {
                _hookID = SetHook(_proc);
            }

            public void ReleaseKeyboard()
            {
                UnhookWindowsHookEx(_hookID);
            }

            private static IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule?.ModuleName), 0);
                }
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN || wParam == (IntPtr) WM_SYSKEYDOWN)
                {
                    var vkCode = Marshal.ReadInt32(lParam);
                    var args = new KeyEventArgs((Keys) vkCode);
                    OnKeyPressed?.Invoke(this, args);
                } else if (nCode >= 0 && wParam == (IntPtr) WM_KEYUP || wParam == (IntPtr) WM_SYSKEYUP)
                {
                    var vkCode = Marshal.ReadInt32(lParam);
                    var args = new KeyEventArgs((Keys) vkCode);
                    OnKeyReleased?.Invoke(this, args);
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
        }
    }
}
