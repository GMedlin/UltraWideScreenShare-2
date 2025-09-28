using System.Drawing;
using System.Windows.Forms;

namespace UltraWideScreenShare.WinForms
{
    partial class TitleBarWindow
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            rootPanel = new Panel();
            titleLabel = new Label();
            buttonStrip = new FlowLayoutPanel();
            minimizeButton = new Button();
            maximizeButton = new Button();
            closeButton = new Button();
            rootPanel.SuspendLayout();
            buttonStrip.SuspendLayout();
            SuspendLayout();

            //
            // rootPanel
            //
            rootPanel.BackColor = Color.White;
            rootPanel.Dock = DockStyle.Fill;
            rootPanel.Location = new Point(0, 0);
            rootPanel.Margin = new Padding(0);
            rootPanel.Name = "rootPanel";
            rootPanel.Padding = new Padding(12, 0, 12, 0);
            rootPanel.Size = new Size(600, 44);
            rootPanel.TabIndex = 0;
            rootPanel.MouseDown += Root_MouseDown;
            rootPanel.DoubleClick += Root_DoubleClick;

            //
            // titleLabel
            //
            titleLabel.AutoEllipsis = true;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            titleLabel.ForeColor = Color.Black;
            titleLabel.Location = new Point(12, 0);
            titleLabel.Margin = new Padding(0);
            titleLabel.Name = "titleLabel";
            titleLabel.Padding = new Padding(4, 0, 0, 0);
            titleLabel.Size = new Size(444, 44);
            titleLabel.TabIndex = 1;
            titleLabel.Text = "Ultra Wide Screen Share";
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            titleLabel.MouseDown += Root_MouseDown;
            titleLabel.DoubleClick += Root_DoubleClick;

            //
            // buttonStrip
            //
            buttonStrip.AutoSize = false;
            buttonStrip.Dock = DockStyle.Right;
            buttonStrip.FlowDirection = FlowDirection.RightToLeft;
            buttonStrip.Location = new Point(456, 0);
            buttonStrip.Margin = new Padding(0);
            buttonStrip.Name = "buttonStrip";
            buttonStrip.Padding = new Padding(0);
            buttonStrip.Size = new Size(138, 32);
            buttonStrip.TabIndex = 2;
            buttonStrip.WrapContents = false;

            //
            // minimizeButton
            //
            minimizeButton.FlatStyle = FlatStyle.Flat;
            minimizeButton.Location = new Point(80, 6);
            minimizeButton.Margin = new Padding(0);
            minimizeButton.Name = "minimizeButton";
            minimizeButton.Size = new Size(52, 32);
            minimizeButton.TabIndex = 2;
            minimizeButton.UseVisualStyleBackColor = true;
            minimizeButton.Click += minimizeButton_Click;

            //
            // maximizeButton
            //
            maximizeButton.FlatStyle = FlatStyle.Flat;
            maximizeButton.Location = new Point(28, 6);
            maximizeButton.Margin = new Padding(0);
            maximizeButton.Name = "maximizeButton";
            maximizeButton.Size = new Size(52, 32);
            maximizeButton.TabIndex = 1;
            maximizeButton.UseVisualStyleBackColor = true;
            maximizeButton.Click += maximizeButton_Click;

            //
            // closeButton
            //
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Location = new Point(0, 6);
            closeButton.Margin = new Padding(0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(28, 32);
            closeButton.TabIndex = 0;
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;

            //
            // TitleBarWindow
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(600, 44);
            ControlBox = false;
            buttonStrip.Controls.Add(closeButton);
            buttonStrip.Controls.Add(maximizeButton);
            buttonStrip.Controls.Add(minimizeButton);
            rootPanel.Controls.Add(titleLabel);
            rootPanel.Controls.Add(buttonStrip);
            Controls.Add(rootPanel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "TitleBarWindow";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "TitleBarWindow";
            rootPanel.ResumeLayout(false);
            rootPanel.PerformLayout();
            buttonStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel rootPanel;
        private Label titleLabel;
        private FlowLayoutPanel buttonStrip;
        private Button minimizeButton;
        private Button maximizeButton;
        private Button closeButton;
    }
}
