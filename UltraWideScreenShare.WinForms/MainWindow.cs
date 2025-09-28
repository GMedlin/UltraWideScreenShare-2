using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        private Point _tittleBarLocation = Point.Empty;
        private DesktopDuplicationCaptureController? _captureController;
        private bool _isTransparent;
        private Color _frameColor = Color.FromArgb(255, 255, 221, 0);
        private const int _logicalBorderWidth = 6;
        private int _borderWidth = 6;

        public MainWindow()
        {
            InitializeComponent();
            TitleBar.BringToFront();
            InitializePaddingsForBorders();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.InitializeMainWindowStyle();
        }

        private void InitializePaddingsForBorders()
        {
            float scale = DeviceDpi / 96f;
            _borderWidth = (int)Math.Round(_logicalBorderWidth * scale);
            Padding = new Padding(_borderWidth, _borderWidth, _borderWidth, _borderWidth);
            TitleBar.Width += (_borderWidth * 2);
            TitleBar.Height += _borderWidth;
            TitleBar.Padding = new Padding(_borderWidth, 0, _borderWidth, _borderWidth);
        }

        protected override void OnMove(EventArgs e)
        {
            MaximizedBounds = new Rectangle(Point.Empty, Screen.GetWorkingArea(Location).Size);
            base.OnMove(e);
        }

        private void MainWindow_Load(object sender, EventArgs e)
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
            bool insideCapture = magnifierPanel.Bounds.Contains(cursor) && !TitleBar.Bounds.Contains(cursor);

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

        private void MainWindow_ResizeBegin(object sender, EventArgs e) { }

        private void MainWindow_ResizeEnd(object sender, EventArgs e) { }

        private void TittleButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Drag();
                SetupMaximizeButton();
            }
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

        private void MainWindow_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid);
        }

        private void TitleBar_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, TitleBar.ClientRectangle,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, 0, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid,
                _frameColor, _borderWidth, ButtonBorderStyle.Solid);
        }

        private void closeButton_Click(object sender, EventArgs e) => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            _dispatcherTimer.Stop();
            _dispatcherTimer.Dispose();
            _captureController?.Dispose();
            base.OnClosing(e);
        }

        private void minimizeButton_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void maximizeButton_Click(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
            SetupMaximizeButton();
        }

        private void SetupMaximizeButton()
        {
            maximizeButton.Image = WindowState == FormWindowState.Maximized
                ? Properties.Resources.restore
                : Properties.Resources.maximize;
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            float scale = e.DeviceDpiNew / 96f;
            _borderWidth = (int)Math.Round(_logicalBorderWidth * scale);
            Padding = new Padding(_borderWidth, _borderWidth, _borderWidth, _borderWidth);
            TitleBar.Padding = new Padding(_borderWidth, 0, _borderWidth, _borderWidth);
            Invalidate();
        }

        private void DragButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _tittleBarLocation = e.Location;
            }
        }

        private void DragButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TitleBar.Left = Math.Clamp(e.X + TitleBar.Left - _tittleBarLocation.X,
                    0, Width - TitleBar.Width);
            }
        }

        private Rectangle GetCaptureRegion()
        {
            return magnifierPanel.RectangleToScreen(magnifierPanel.ClientRectangle);
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
