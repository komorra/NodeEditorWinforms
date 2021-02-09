/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2021 Mariusz Komorowski (komorra)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES 
 * OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
 * OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
        static Pen executionPen;
        static Pen executionPen2;

        public void Draw(Graphics g, Point mouseLocation, MouseButtons mouseButtons, bool preferFastRendering, DrawInfo info)
        {            
            foreach (var node in Nodes)
            {
                g.FillRectangle(Brushes.Black, new RectangleF(new PointF(node.X+6, node.Y+6), node.GetNodeBounds()));
            }

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.White)), g.ClipBounds);
            
            executionPen = (executionPen ?? new Pen(Color.Gold, 3));
            executionPen2 = (executionPen2 ?? new Pen(Color.Black, 5));
            foreach (var connection in Connections.Where(x=>x.IsExecution))
            {
                var osoc = connection.OutputNode.GetSockets().FirstOrDefault(x => x.Name == connection.OutputSocketName);
                var beginSocket = osoc.GetBounds();
                var isoc = connection.InputNode.GetSockets().FirstOrDefault(x => x.Name == connection.InputSocketName);
                var endSocket = isoc.GetBounds();
                var begin = beginSocket.Location + new SizeF(beginSocket.Width / 2f, beginSocket.Height / 2f);
                var end = endSocket.Location += new SizeF(endSocket.Width / 2f, endSocket.Height / 2f);                               

                DrawConnection(g, executionPen2, begin, end, preferFastRendering);
                DrawConnection(g, executionPen, begin, end, preferFastRendering);                
            }
            foreach (var connection in Connections.Where(x => !x.IsExecution))
            {
                var osoc = connection.OutputNode.GetSockets().FirstOrDefault(x => x.Name == connection.OutputSocketName);
                var beginSocket = osoc.GetBounds();
                var isoc = connection.InputNode.GetSockets().FirstOrDefault(x => x.Name == connection.InputSocketName);
                var endSocket = isoc.GetBounds();
                var begin = beginSocket.Location + new SizeF(beginSocket.Width / 2f, beginSocket.Height / 2f);
                var end = endSocket.Location += new SizeF(endSocket.Width / 2f, endSocket.Height / 2f);

                var cpen = info.GetConnectionStyle(connection.InputSocket.Type, false);
                DrawConnection(g, cpen, begin, end, preferFastRendering);
               
            }

            var orderedNodes = Nodes.OrderByDescending(x => x.Order);
            foreach (var node in orderedNodes)
            {
                node.Draw(g, mouseLocation, mouseButtons);
            }
        }

        public static void DrawConnection(Graphics g, Pen pen, PointF output, PointF input, bool preferFastRendering = false)
        {            
            if (input == output) return;
            int interpolation = preferFastRendering ? 16 : 48;

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
