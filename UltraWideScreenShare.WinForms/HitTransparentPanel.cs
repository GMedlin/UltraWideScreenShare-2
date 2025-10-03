using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UltraWideScreenShare.WinForms
{
    internal sealed class HitTransparentPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int EffectiveResizeMargin { get; set; } = 8;

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;

            if (m.Msg == WM_NCHITTEST)
            {
                var p = PointToClient(Cursor.Position);
                int mrg = EffectiveResizeMargin;

                bool nearLeft = p.X < mrg;
                bool nearTop = p.Y < mrg;
                bool nearRight = (Width - p.X) <= mrg;
                bool nearBottom = (Height - p.Y) <= mrg;

                if (nearLeft || nearTop || nearRight || nearBottom)
                {
                    m.Result = (IntPtr)HTTRANSPARENT; // let MainWindow handle resize
                    return;
                }
            }

            base.WndProc(ref m);
        }
    }
}
