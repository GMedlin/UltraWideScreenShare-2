using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Timer = System.Windows.Forms.Timer;

namespace UltraWideScreenShare.WinForms
{
    public partial class MainWindow : Form
    {
        private readonly Timer _dispatcherTimer = new() { Interval = 16 };
        private readonly Timer _savePositionTimer = new() { Interval = 750 };
        private MagnifierController? _magnifierController;
        private TitleBarWindow? _titleBarWindow;
        private bool _showMagnifierScheduled;
        private bool _isTransparent;
        private Color _frameColor = Color.FromArgb(255, 128, 128, 128);
        private const int _logicalBorderWidth = 2;
        private const int _logicalTitleBarHeight = 32;
        private const int _logicalResizeMargin = 8;
        private int _borderWidth = 2;
        private int _titleBarHeight = _logicalTitleBarHeight;
        private int _resizeMargin = _logicalResizeMargin;

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
            _titleBarHeight = (int)Math.Round(_logicalTitleBarHeight * scale);
            _resizeMargin = Math.Max(6, (int)Math.Round(_logicalResizeMargin * scale));
            Padding = new Padding(_borderWidth);

            // Propagate resize margin to the panel for hit-test transparency
            if (magnifierPanel is HitTransparentPanel htp)
            {
                htp.EffectiveResizeMargin = _resizeMargin;
            }
        }

        private void MainWindow_Load(object? sender, EventArgs e)
        {
            RestoreWindowPosition();

            if (!StartMagnifier())
            {
                Close();
                return;
            }

            _dispatcherTimer.Tick += (_, _) =>
            {
                UpdateTransparency();
                _magnifierController?.UpdateMagnifierWindow();

                if (_showMagnifierScheduled)
                {
                    _magnifierController?.ShowMagnifier();
                    _showMagnifierScheduled = false;
                }
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

        private bool StartMagnifier()
        {
            try
            {
                _magnifierController = new MagnifierController(magnifierPanel);

                if (!_magnifierController.Initialize())
                {
                    MessageBox.Show(this,
                        "Failed to initialize screen magnification.",
                        "UltraWideScreenShare",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"magnifier_init_failed: {ex}");
                MessageBox.Show(this,
                    "An error occurred initializing screen magnification.",
                    "UltraWideScreenShare",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void UpdateTransparency()
        {
            var cursor = PointToClient(Cursor.Position);
            // Leave a non-transparent resize band around the edges
            var captureBounds = Rectangle.Inflate(magnifierPanel.Bounds, -_resizeMargin, -_resizeMargin);
            bool insideCapture = captureBounds.Contains(cursor);

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

        private void MainWindow_ResizeBegin(object? sender, EventArgs e)
        {
            _magnifierController?.HideMagnifier();
        }

        private void MainWindow_ResizeEnd(object? sender, EventArgs e)
        {
            _showMagnifierScheduled = true;
        }

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
                case WM_NCHITTEST:
                    // Perform our custom hit-test first
                    this.TryResize(ref m, _resizeMargin);
                    if (m.Result != IntPtr.Zero)
                    {
                        return; // Prevent base from overriding our HT*
                    }
                    break;

                case WM_NCCALCSIZE:
                    return;

                case WM_NCACTIVATE:
                    m.Result = new IntPtr(-1);
                    return;
            }

            base.WndProc(ref m);
        }

        private void MainWindow_Paint(object? sender, PaintEventArgs e)
        {
            if (_borderWidth > 0 && _frameColor.A > 0)
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                    _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                    _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                    _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                    _frameColor, _borderWidth, ButtonBorderStyle.Solid);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowPosition();
            _dispatcherTimer.Stop();
            _dispatcherTimer.Dispose();

            _magnifierController?.Dispose();
            _magnifierController = null;

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
}
