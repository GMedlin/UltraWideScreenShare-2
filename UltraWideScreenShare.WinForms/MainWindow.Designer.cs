using System.Drawing;
using System.Windows.Forms;

namespace UltraWideScreenShare.WinForms
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            magnifierPanel = new HitTransparentPanel();
            SuspendLayout();

            //
            // magnifierPanel
            //
            magnifierPanel.BackColor = Color.Transparent;
            magnifierPanel.Dock = DockStyle.Fill;
            magnifierPanel.Location = new Point(0, 0);
            magnifierPanel.Margin = new Padding(0);
            magnifierPanel.Name = "magnifierPanel";
            magnifierPanel.Size = new Size(800, 600);
            magnifierPanel.TabIndex = 0;

            //
            // MainWindow
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Magenta;
            ClientSize = new Size(800, 600);
            ControlBox = false;
            Controls.Add(magnifierPanel);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(300, 200);
            Name = "MainWindow";
            Padding = new Padding(6);
            Text = "Ultra Wide Screen Share 2.0";
            TopMost = true;
            TransparencyKey = Color.Magenta;
            Load += MainWindow_Load;
            ResizeBegin += MainWindow_ResizeBegin;
            ResizeEnd += MainWindow_ResizeEnd;
            Paint += MainWindow_Paint;
            ResumeLayout(false);
        }

        #endregion

        private HitTransparentPanel magnifierPanel;
    }
}
