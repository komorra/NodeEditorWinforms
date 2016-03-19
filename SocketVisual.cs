using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NodeEditor
{
    internal class SocketVisual
    {
        public const float SocketHeight = 16;

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool Input { get; set; }

        public void Draw(Graphics g, Point mouseLocation, MouseButtons mouseButtons)
        {
            

            var socketRect = new RectangleF(X, Y, Width, Height);
            var hover = socketRect.Contains(mouseLocation);
            var fontBrush = Brushes.Black;

            if (hover)
            {
                socketRect.Inflate(4, 4);
                fontBrush = Brushes.Blue;
            }

            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.InterpolationMode = InterpolationMode.Low;
            
            if (Input)
            {
                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;                
                g.DrawString(Name,SystemFonts.SmallCaptionFont, fontBrush, new RectangleF(X+Width+2,Y,1000,Height), sf);
            }
            else
            {
                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(Name, SystemFonts.SmallCaptionFont, fontBrush, new RectangleF(X-1000, Y, 1000, Height), sf);
            }

            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (Type == typeof (ExecutionPath))
            {
                g.DrawImage(Resources.exec, socketRect);
            }
            else
            {
                g.DrawImage(Resources.socket, socketRect);
            }
        }

        public RectangleF GetBounds()
        {
            return new RectangleF(X, Y, Width, Height);
        }
    }
}
