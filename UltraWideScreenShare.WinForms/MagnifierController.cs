using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Magnification;
using Windows.Win32.UI.WindowsAndMessaging;

namespace UltraWideScreenShare.WinForms
{
    internal sealed class MagnifierController : IDisposable
    {
        private readonly HWND _hostWindowHandle;
        private HWND _magnifierWindowHandle;
        private Windows.Win32.UnhookWindowsHookExSafeHandle? _hook;
        private bool _initialized;

        public MagnifierController(Control hostControl)
        {
            if (hostControl == null)
                throw new ArgumentNullException(nameof(hostControl));

            _hostWindowHandle = new HWND(hostControl.Handle);
        }

        public unsafe bool Initialize()
        {
            if (_initialized)
            {
                return true;
            }

            // Initialize magnification system (once per application)
            if (!PInvoke.MagInitialize())
            {
                Debug.WriteLine("MagInitialize failed");
                return false;
            }

            // Set layered window attributes - REQUIRED for Teams compatibility
            PInvoke.SetLayeredWindowAttributes(
                _hostWindowHandle,
                new COLORREF(0),
                255,
                LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);

            // Set up message filter hook - REQUIRED for Teams compatibility
            // Thread ID of 0 installs the hook on the current thread
            _hook = PInvoke.SetWindowsHookEx(
                WINDOWS_HOOK_ID.WH_MSGFILTER,
                FilterMessage,
                null,
                0);

            // Create magnifier window as child of host window
            _magnifierWindowHandle = PInvoke.CreateWindowEx(
                0,
                "Magnifier",
                "MagnifierWindow",
                Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE.WS_CHILD |
                Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE.WS_VISIBLE,
                0, 0, 0, 0,
                _hostWindowHandle,
                null,
                null,
                null);

            if (_magnifierWindowHandle.IsNull)
            {
                Debug.WriteLine("CreateWindowEx (Magnifier) failed");
                return false;
            }

            _initialized = true;
            return true;
        }

        private LRESULT FilterMessage(int code, WPARAM wParam, LPARAM lParam)
        {
            return new LRESULT();
        }

        public void UpdateMagnifierWindow()
        {
            if (_magnifierWindowHandle.IsNull)
            {
                return;
            }

            var magnificationArea = GetMagnificationAreaRECT();

            // Resize magnifier window to match area
            PInvoke.SetWindowPos(
                _magnifierWindowHandle,
                HWND.Null,
                0, 0,
                magnificationArea.Width,
                magnificationArea.Height,
                0);

            // Set the source rectangle for magnification
            PInvoke.MagSetWindowSource(_magnifierWindowHandle, magnificationArea);

            // Force redraw
            PInvoke.InvalidateRect(_magnifierWindowHandle, (RECT?)null, new BOOL(1));
        }

        public void ShowMagnifier()
        {
            if (_magnifierWindowHandle.IsNull)
            {
                return;
            }

            PInvoke.ShowWindow(_magnifierWindowHandle, SHOW_WINDOW_CMD.SW_RESTORE);
            PInvoke.InvalidateRect(_magnifierWindowHandle, (RECT?)null, new BOOL(1));
        }

        public void HideMagnifier()
        {
            if (_magnifierWindowHandle.IsNull)
            {
                return;
            }

            PInvoke.ShowWindow(_magnifierWindowHandle, SHOW_WINDOW_CMD.SW_HIDE);
            PInvoke.InvalidateRect(_magnifierWindowHandle, (RECT?)null, new BOOL(1));
        }

        private unsafe RECT GetMagnificationAreaRECT()
        {
            PInvoke.GetWindowRect(_hostWindowHandle, out RECT windowRect);
            PInvoke.GetClientRect(_hostWindowHandle, out RECT clientRect);

            return new RECT(
                new Point((int)windowRect.left, (int)windowRect.top),
                new Size((int)clientRect.Width, (int)clientRect.Height));
        }

        public void Dispose()
        {
            if (!_magnifierWindowHandle.IsNull)
            {
                PInvoke.DestroyWindow(_magnifierWindowHandle);
                _magnifierWindowHandle = default;
            }

            if (_hook != null)
            {
                _hook.Dispose();
                _hook = null;
            }

            if (_initialized)
            {
                PInvoke.MagUninitialize();
                _initialized = false;
            }
        }
    }
}
