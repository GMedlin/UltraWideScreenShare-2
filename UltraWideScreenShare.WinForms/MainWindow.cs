using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Timer = System.Windows.Forms.Timer;

namespace UltraWideScreenShare.WinForms
{
    public partial class MainWindow : Form
    {
        private readonly Timer _dispatcherTimer = new() { Interval = 16 };
        private readonly Timer _savePositionTimer = new() { Interval = 750 };
        private DesktopDuplicationCaptureController? _captureController;
        private TitleBarWindow? _titleBarWindow;
        private bool _isTransparent;
        private Color _frameColor = Color.FromArgb(255, 255, 221, 0);
        private const int _logicalBorderWidth = 2;
        private const int _logicalTitleBarHeight = 32;
        private int _borderWidth = 2;
        private int _titleBarHeight = _logicalTitleBarHeight;

        public MainWindow()
        {
            InitializeComponent();
            Shown += MainWindow_Shown;
            InitializeScalingDependentMetrics(DeviceDpi);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            _savePositionTimer.Tick += SavePositionTimer_Tick;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.InitializeMainWindowStyle();
        }

        private void InitializeScalingDependentMetrics(int dpi)
        {
            float scale = dpi / 96f;
            _borderWidth = Math.Max(1, (int)Math.Round(_logicalBorderWidth * scale));
            _titleBarHeight = Math.Max(28, (int)Math.Round(_logicalTitleBarHeight * scale));
            Padding = new Padding(_borderWidth);
        }

        private void MainWindow_Load(object? sender, EventArgs e)
        {
            try
            {
                if (!PInvoke.SetWindowDisplayAffinity(new HWND(Handle), WINDOW_DISPLAY_AFFINITY.WDA_EXCLUDEFROMCAPTURE))
                {
                    Trace.WriteLine("set_window_display_affinity_failed");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"set_window_display_affinity_error: {ex}");
            }

            RestoreWindowPosition();

            if (!StartDesktopDuplication())
            {
                Close();
                return;
            }

            _dispatcherTimer.Tick += (_, _) =>
            {
                UpdateTransparency();
                _captureController?.ProcessFrame();
            };
            _dispatcherTimer.Start();
        }

        private void MainWindow_Shown(object? sender, EventArgs e)
        {
            InitializeTitleBarWindow();
        }

        private void InitializeTitleBarWindow()
        {
            if (_titleBarWindow != null)
            {
                return;
            }

            _titleBarWindow = new TitleBarWindow(this);
            _titleBarWindow.MinimizeRequested += (_, _) => WindowState = FormWindowState.Minimized;
            _titleBarWindow.MaximizeRequested += (_, _) => ToggleWindowState();
            _titleBarWindow.CloseRequested += (_, _) => Close();
            _titleBarWindow.ApplyScale(_titleBarHeight);
            _titleBarWindow.UpdateTitle(Text);
            _titleBarWindow.UpdateMaximizeState(WindowState == FormWindowState.Maximized);
            _titleBarWindow.Show(this);
            UpdateTitleBarBounds();
        }

        private bool StartDesktopDuplication()
        {
            var monitorHandle = NativeCaptureUtilities.MonitorFromWindow(Handle);
            if (monitorHandle == IntPtr.Zero)
            {
                MessageBox.Show(this,
                    "Could not determine the monitor for capture.",
                    "UltraWideScreenShare",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            var screen = Screen.FromHandle(Handle);
            try
            {
                _captureController = new DesktopDuplicationCaptureController(magnifierPanel, GetCaptureRegion);
                _captureController.Start(monitorHandle, screen.Bounds);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"desktop_duplication_start_failed: {ex}");
                MessageBox.Show(this,
                    "We couldn't start screen capture. Please make sure screen recording is enabled in Settings > Privacy & security > Screen recording and try again.",
                    "UltraWideScreenShare",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void UpdateTransparency()
        {
            var cursor = PointToClient(Cursor.Position);
            bool insideCapture = magnifierPanel.Bounds.Contains(cursor);

            if (insideCapture && !_isTransparent)
            {
                this.SetTransparency(_isTransparent = true);
                Trace.WriteLine("cursor_enter_capture");
            }
            else if (!insideCapture && _isTransparent)
            {
                this.SetTransparency(_isTransparent = false);
                Trace.WriteLine("cursor_leave_capture");
            }
        }

        private void MainWindow_ResizeBegin(object? sender, EventArgs e) { }

        private void MainWindow_ResizeEnd(object? sender, EventArgs e) { }

        private void ToggleWindowState()
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;

            UpdateTitleBarState();
        }

        private void UpdateTitleBarState()
        {
            if (_titleBarWindow == null)
            {
                return;
            }

            _titleBarWindow.UpdateMaximizeState(WindowState == FormWindowState.Maximized);
            if (WindowState == FormWindowState.Minimized)
            {
                _titleBarWindow.Hide();
            }
            else if (!_titleBarWindow.Visible)
            {
                _titleBarWindow.Show();
            }

            UpdateTitleBarBounds();
        }

        private void UpdateTitleBarBounds()
        {
            if (_titleBarWindow == null || WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _titleBarWindow.ApplyScale(_titleBarHeight);
            _titleBarWindow.UpdateTitle(Text);

            var targetScreen = Screen.FromControl(this).WorkingArea;
            int x = Location.X;
            int y = Location.Y - _titleBarWindow.Height;
            int width = Width;

            if (y < targetScreen.Top)
            {
                y = targetScreen.Top;
            }

            _titleBarWindow.Bounds = new Rectangle(x, y, width, _titleBarWindow.Height);
            _titleBarWindow.TopMost = TopMost;
        }

        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCACTIVATE = 0x0086;
        private const int WM_NCHITTEST = 0x0084;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCCALCSIZE:
                    return;
                case WM_NCACTIVATE:
                    m.Result = new IntPtr(-1);
                    return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST)
            {
                this.TryResize(ref m, _borderWidth);
            }
        }

        private void MainWindow_Paint(object? sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowPosition();
            _dispatcherTimer.Stop();
            _dispatcherTimer.Dispose();
            _captureController?.Dispose();
            _captureController = null;
            _savePositionTimer.Stop();
            _savePositionTimer.Dispose();
            if (_titleBarWindow != null)
            {
                _titleBarWindow.Close();
                _titleBarWindow = null;
            }

            base.OnClosing(e);
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            MaximizedBounds = new Rectangle(Point.Empty, Screen.GetWorkingArea(Location).Size);
            UpdateTitleBarBounds();
            SaveWindowPositionDelayed();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateTitleBarState();
            SaveWindowPositionDelayed();
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            InitializeScalingDependentMetrics(e.DeviceDpiNew);
            Invalidate();
            UpdateTitleBarBounds();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            _titleBarWindow?.UpdateTitle(Text);
        }

        private Rectangle GetCaptureRegion()
        {
            var rect = magnifierPanel.RectangleToScreen(magnifierPanel.ClientRectangle);
            rect.Inflate(_borderWidth, _borderWidth);
            return rect;
        }

        private void SaveWindowPositionDelayed()
        {
            if (WindowState != FormWindowState.Normal)
            {
                return;
            }

            _savePositionTimer.Stop();
            _savePositionTimer.Start();
        }

        private void SavePositionTimer_Tick(object? sender, EventArgs e)
        {
            SaveWindowPosition();
            _savePositionTimer.Stop();
        }

        private void SaveWindowPosition()
        {
            if (WindowState != FormWindowState.Normal)
            {
                var bounds = RestoreBounds;
                var settingsMaximized = Properties.Settings.Default;
                settingsMaximized.WindowLocation = bounds.Location;
                settingsMaximized.WindowSize = bounds.Size;
                settingsMaximized.IsMaximized = WindowState == FormWindowState.Maximized;
                settingsMaximized.FirstRun = false;
                settingsMaximized.Save();
                return;
            }

            var settings = Properties.Settings.Default;
            settings.WindowLocation = Location;
            settings.WindowSize = Size;
            settings.IsMaximized = false;
            settings.FirstRun = false;
            settings.Save();
        }

        private void RestoreWindowPosition()
        {
            var settings = Properties.Settings.Default;

            if (settings.FirstRun)
            {
                CenterWindowOnPrimaryScreen();
                return;
            }

            var savedLocation = settings.WindowLocation;
            var savedSize = settings.WindowSize;

            var savedBounds = new Rectangle(savedLocation, savedSize);
            bool intersectsScreen = Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(savedBounds));

            if (intersectsScreen)
            {
                StartPosition = FormStartPosition.Manual;
                Location = savedLocation;
                Size = savedSize;

                if (settings.IsMaximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    // Ensure there is room for the detached title bar above the main window
                    var wa = Screen.FromPoint(Location).WorkingArea;
                    int requiredTop = wa.Top + _titleBarHeight;
                    if (Location.Y < requiredTop)
                    {
                        Location = new Point(Location.X, requiredTop);
                    }
                }
            }
            else
            {
                CenterWindowOnPrimaryScreen();
            }
        }

        private void CenterWindowOnPrimaryScreen()
        {
            var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            StartPosition = FormStartPosition.Manual;

            // Ensure there is room for the detached title bar above the main window
            int centeredY = screen.Top + (screen.Height - Height) / 2;
            int requiredTop = screen.Top + _titleBarHeight;
            int finalY = Math.Max(centeredY, requiredTop);

            Location = new Point(
                screen.Left + (screen.Width - Width) / 2,
                finalY);
        }
    }

    internal static class NativeCaptureUtilities
    {
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        public static IntPtr MonitorFromWindow(IntPtr hwnd)
        {
            return MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        }
    }
}
