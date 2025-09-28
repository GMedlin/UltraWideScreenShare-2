using System;
using System.Drawing;
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
            if (targetHeight <= 0 || targetHeight == _currentHeight)
            {
                return;
            }

            _currentHeight = targetHeight;
            ClientSize = new Size(ClientSize.Width, targetHeight);

            int buttonWidth = 46;
            int buttonHeight = targetHeight;

            foreach (var button in new[] { minimizeButton, maximizeButton, closeButton })
            {
                button.Size = new Size(buttonWidth, buttonHeight);
                button.Margin = new Padding(0);
            }

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
    }
}
