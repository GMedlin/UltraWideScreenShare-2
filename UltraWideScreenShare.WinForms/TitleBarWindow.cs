using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace UltraWideScreenShare.WinForms
{
    public partial class TitleBarWindow : Form
    {
        private readonly MainWindow _parentWindow;
        private int _currentHeight;

        public event EventHandler? MinimizeRequested;
        public event EventHandler? MaximizeRequested;
        public event EventHandler? CloseRequested;

        public TitleBarWindow(MainWindow parentWindow)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
            InitializeComponent();
            InitializeTitleBarAppearance();
        }

        private void InitializeTitleBarAppearance()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            ControlBox = false;
            DoubleBuffered = true;
            TopMost = _parentWindow.TopMost;

            ConfigureButton(minimizeButton, Properties.Resources.minimize, "Minimize");
            ConfigureButton(maximizeButton, Properties.Resources.maximize, "Maximize");
            ConfigureButton(closeButton, Properties.Resources.dismiss, "Close", isCloseButton: true);
        }

        public void ApplyScale(int targetHeight)
        {
            // Use system metrics for proper scaling
            int dpi = DeviceDpi;
            var metrics = TitleBarMetrics.GetForDpi(dpi);
            ApplyScaleWithMetrics(targetHeight, metrics.ButtonWidth, metrics.ButtonHeight);
        }

        public void ApplyScaleWithMetrics(int targetHeight, int buttonWidth, int buttonHeight)
        {
            if (targetHeight <= 0 || targetHeight == _currentHeight)
            {
                return;
            }

            _currentHeight = targetHeight;
            ClientSize = new Size(ClientSize.Width, targetHeight);

            foreach (var button in new[] { minimizeButton, maximizeButton, closeButton })
            {
                button.Size = new Size(buttonWidth, buttonHeight);
                button.Margin = new Padding(0);
                button.TabStop = false;
            }

            // Explicitly set buttonStrip size and position
            buttonStrip.Height = targetHeight;
            buttonStrip.Width = buttonWidth * 3; // Fit exactly 3 buttons; no extra gaps since margins/padding are 0
            buttonStrip.Location = new Point(buttonStrip.Location.X, 0);

            rootPanel.Padding = new Padding(12, 0, 0, 0);
            buttonStrip.Padding = new Padding(0);
            titleLabel.Padding = new Padding(4, 0, 0, 0);

            rootPanel.PerformLayout();
            buttonStrip.PerformLayout();
        }

        public void UpdateTitle(string title)
        {
            titleLabel.Text = title;
        }

        public void UpdateMaximizeState(bool isMaximized)
        {
            maximizeButton.Image = isMaximized ? Properties.Resources.restore : Properties.Resources.maximize;
            maximizeButton.AccessibleName = isMaximized ? "Restore" : "Maximize";
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Calculate caption metrics from Windows system for current DPI
            int dpi = DeviceDpi;
            var metrics = TitleBarMetrics.GetForDpi(dpi);
            int targetHeight = metrics.CaptionHeight;

            // Use system caption font for proper alignment
            this.Font = SystemFonts.CaptionFont;
            titleLabel.Font = SystemFonts.CaptionFont;

            // Disable all auto-sizing to prevent height inflation
            this.AutoSize = false;
            this.MinimumSize = new Size(0, targetHeight);
            this.MaximumSize = new Size(int.MaxValue, targetHeight);
            this.Padding = new Padding(0);

            rootPanel.AutoSize = false;
            rootPanel.Padding = new Padding(12, 0, 0, 0);
            rootPanel.Margin = new Padding(0);

            buttonStrip.AutoSize = false;
            buttonStrip.WrapContents = false;
            buttonStrip.AutoScroll = false;
            buttonStrip.Padding = new Padding(0);
            buttonStrip.Margin = new Padding(0);

            // Ensure exact system-metric height
            ApplyScaleWithMetrics(targetHeight, metrics.ButtonWidth, metrics.ButtonHeight);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                PInvoke.SetWindowDisplayAffinity(new HWND(Handle), WINDOW_DISPLAY_AFFINITY.WDA_EXCLUDEFROMCAPTURE);
            }
            catch
            {
                // Ignore if not supported
            }
        }

        private void ConfigureButton(Button button, Image icon, string accessibleName, bool isCloseButton = false)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = isCloseButton
                ? Color.FromArgb(232, 17, 35)
                : Color.FromArgb(240, 242, 245);
            button.FlatAppearance.MouseDownBackColor = isCloseButton
                ? Color.FromArgb(198, 0, 11)
                : Color.FromArgb(222, 224, 227);
            button.BackColor = Color.Transparent;
            button.Image = icon;
            button.AccessibleName = accessibleName;
            button.TabStop = false;
            button.UseMnemonic = false;
            button.Text = string.Empty;
        }

        private void minimizeButton_Click(object? sender, EventArgs e)
        {
            MinimizeRequested?.Invoke(this, EventArgs.Empty);
        }

        private void maximizeButton_Click(object? sender, EventArgs e)
        {
            MaximizeRequested?.Invoke(this, EventArgs.Empty);
        }

        private void closeButton_Click(object? sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Root_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _parentWindow.Drag();
            }
        }

        private void Root_DoubleClick(object? sender, EventArgs e)
        {
            MaximizeRequested?.Invoke(this, EventArgs.Empty);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DPICHANGED = 0x02E0;
            if (m.Msg == WM_DPICHANGED)
            {
                // LOWORD = X dpi, HIWORD = Y dpi (typically equal)
                int newDpi = (int)(m.WParam.ToInt64() & 0xFFFF);
                var metrics = TitleBarMetrics.GetForDpi(newDpi);

                // Update fonts for new DPI
                this.Font = SystemFonts.CaptionFont;
                titleLabel.Font = SystemFonts.CaptionFont;

                // Update height constraints
                this.MinimumSize = new Size(0, metrics.CaptionHeight);
                this.MaximumSize = new Size(int.MaxValue, metrics.CaptionHeight);

                ApplyScaleWithMetrics(metrics.CaptionHeight, metrics.ButtonWidth, metrics.ButtonHeight);
                return;
            }
            base.WndProc(ref m);
        }
    }

    internal static class TitleBarMetrics
    {
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CYCAPTION = 4;  // Height of caption area
        private const int SM_CXSIZE = 30;    // Width of caption button
        private const int SM_CYSIZE = 31;    // Height of caption button

        public static (int CaptionHeight, int ButtonWidth, int ButtonHeight) GetForDpi(int dpi)
        {
            // Get base system metrics at 96 DPI
            int baseCaptionHeight = GetSystemMetrics(SM_CYCAPTION);
            int baseButtonHeight = GetSystemMetrics(SM_CYSIZE);

            // Scale for current DPI
            float scale = dpi / 96.0f;
            int captionHeight = (int)Math.Round(baseCaptionHeight * scale);
            int buttonHeight = (int)Math.Round(baseButtonHeight * scale);

            // Windows standard: title bar buttons are typically 1.4x the caption height
            // This provides consistent sizing across all DPI levels
            int buttonWidth = (int)Math.Round(captionHeight * 1.4f);

            return (captionHeight, buttonWidth, buttonHeight);
        }
    }
}
