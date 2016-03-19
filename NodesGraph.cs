using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NodeEditor
{
    internal class NodesGraph
    {
        internal List<NodeVisual> Nodes = new List<NodeVisual>();
        internal List<NodeConnection> Connections = new List<NodeConnection>();

        public void Draw(Graphics g, Point mouseLocation, MouseButtons mouseButtons)
        {
            g.InterpolationMode = InterpolationMode.Low;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            foreach (var node in Nodes)
            {
                g.FillRectangle(Brushes.Black, new RectangleF(new PointF(node.X+6, node.Y+6), node.GetNodeBounds()));
            }

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.White)), g.ClipBounds);

            var cpen = Pens.Black;
            var epen = new Pen(Color.Gold, 3);
            var epen2 = new Pen(Color.Black, 5);
            foreach (var connection in Connections.Where(x=>x.IsExecution))
            {
                var osoc = connection.OutputNode.GetSockets().FirstOrDefault(x => x.Name == connection.OutputSocketName);
                var beginSocket = osoc.GetBounds();
                var isoc = connection.InputNode.GetSockets().FirstOrDefault(x => x.Name == connection.InputSocketName);
                var endSocket = isoc.GetBounds();
                var begin = beginSocket.Location + new SizeF(beginSocket.Width / 2f, beginSocket.Height / 2f);
                var end = endSocket.Location += new SizeF(endSocket.Width / 2f, endSocket.Height / 2f);                               

                DrawConnection(g, epen2, begin, end);
                DrawConnection(g, epen, begin, end);                
            }
            foreach (var connection in Connections.Where(x => !x.IsExecution))
            {
                var osoc = connection.OutputNode.GetSockets().FirstOrDefault(x => x.Name == connection.OutputSocketName);
                var beginSocket = osoc.GetBounds();
                var isoc = connection.InputNode.GetSockets().FirstOrDefault(x => x.Name == connection.InputSocketName);
                var endSocket = isoc.GetBounds();
                var begin = beginSocket.Location + new SizeF(beginSocket.Width / 2f, beginSocket.Height / 2f);
                var end = endSocket.Location += new SizeF(endSocket.Width / 2f, endSocket.Height / 2f);
                
                DrawConnection(g, cpen, begin, end);
               
            }

            var orderedNodes = Nodes.OrderByDescending(x => x.Order);
            foreach (var node in orderedNodes)
            {
                node.Draw(g, mouseLocation, mouseButtons);
            }
        }

        public static void DrawConnection(Graphics g, Pen pen, PointF output, PointF input)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (input == output) return;
            const int interpolation = 48;

            PointF[] points = new PointF[interpolation];
            for (int i = 0; i < interpolation; i++)
            {
                float amount = i/(float) (interpolation - 1);
               
                var lx = Lerp(output.X, input.X, amount);
                var d = Math.Min(Math.Abs(input.X - output.X), 100);
                var a = new PointF((float) Scale(amount, 0, 1, output.X, output.X + d),
                    output.Y);
                var b = new PointF((float) Scale(amount, 0, 1, input.X-d, input.X), input.Y);

                var bas = Sat(Scale(amount, 0.1, 0.9, 0, 1));       
                var cos = Math.Cos(bas*Math.PI);
                if (cos < 0)
                {
                    cos = -Math.Pow(-cos, 0.2);
                }
                else
                {
                    cos = Math.Pow(cos, 0.2);
                }
                amount = (float)cos * -0.5f + 0.5f;

                var f = Lerp(a, b, amount);
                points[i] = f;
            }

            g.DrawLines(pen, points);
        }

        public static double Sat(double x)
        {
            if (x < 0) return 0;
            if (x > 1) return 1;
            return x;
        }


        public static double Scale(double x, double a, double b, double c, double d)
        {
            double s = (x - a)/(b - a);
            return s*(d - c) + c;
        }

        public static float Lerp(float a, float b, float amount)
        {
            return a*(1f - amount) + b*amount;
        }

        public static PointF Lerp(PointF a, PointF b, float amount)
        {
            PointF result = new PointF();

            result.X = a.X*(1f - amount) + b.X*amount;
            result.Y = a.Y*(1f - amount) + b.Y*amount;

            return result;
        }
    }
}
