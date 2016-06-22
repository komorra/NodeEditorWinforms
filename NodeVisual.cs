/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2016 Mariusz Komorowski (komorra)
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
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NodeEditor
{
    /// <summary>
    /// Class that represents one instance of node.
    /// </summary>
    public class NodeVisual
    {
        public const float NodeWidth = 140;
        public const float HeaderHeight = 20;
        public const float ComponentPadding = 2;

        /// <summary>
        /// Current node name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Current node position X coordinate.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Current node position Y coordinate.
        /// </summary>
        public float Y { get; set; }
        internal MethodInfo Type { get; set; }
        internal int Order { get; set; }
        internal bool Callable { get; set; }
        internal bool ExecInit { get; set; }
        internal bool IsSelected { get; set; }
        internal FeedbackType Feedback { get; set; }
        private object nodeContext { get; set; } 
        internal Control CustomEditor { get; set; }
        internal string GUID = Guid.NewGuid().ToString();
        internal Color NodeColor = Color.LightCyan;
        public bool IsBackExecuted { get; internal set; }
        private SocketVisual[] socketCache;

        /// <summary>
        /// Tag for various puposes - may be used freely.
        /// </summary>
        public int Int32Tag = 0;


        internal NodeVisual()
        {
            Feedback = FeedbackType.Debug;
        }

        public string GetGuid()
        {
            return GUID;
        }

        internal SocketVisual[] GetSockets()
        {
            if(socketCache!=null)
            {
                return socketCache;
            }

            var socketList = new List<SocketVisual>();
            float curInputH = HeaderHeight + ComponentPadding;
            float curOutputH = HeaderHeight + ComponentPadding;

            var NodeWidth = GetNodeBounds().Width;

            if (Callable)
            {
                if (!ExecInit)
                {
                    socketList.Add(new SocketVisual()
                    {
                        Height = SocketVisual.SocketHeight,
                        Name = "Enter",
                        Type = typeof (ExecutionPath),
                        IsMainExecution = true,
                        Width = SocketVisual.SocketHeight,
                        X = X,
                        Y = Y + curInputH,
                        Input = true
                    });
                }
                socketList.Add(new SocketVisual()
                {
                    Height = SocketVisual.SocketHeight,
                    Name = "Exit",
                    IsMainExecution = true,
                    Type = typeof (ExecutionPath),
                    Width = SocketVisual.SocketHeight,
                    X = X + NodeWidth - SocketVisual.SocketHeight,
                    Y = Y + curOutputH
                });
                curOutputH += SocketVisual.SocketHeight + ComponentPadding;
                curInputH += SocketVisual.SocketHeight + ComponentPadding;
            }

            foreach (var input in GetInputs())
            {
                var socket = new SocketVisual();
                socket.Type = input.ParameterType;
                socket.Height = SocketVisual.SocketHeight;
                socket.Name = input.Name;
                socket.Width = SocketVisual.SocketHeight;
                socket.X = X;
                socket.Y = Y + curInputH;
                socket.Input = true;

                socketList.Add(socket);

                curInputH += SocketVisual.SocketHeight + ComponentPadding;
            }
            var ctx = GetNodeContext() as DynamicNodeContext;
            foreach (var output in GetOutputs())
            {
                var socket = new SocketVisual();
                socket.Type = output.ParameterType;
                socket.Height = SocketVisual.SocketHeight;
                socket.Name = output.Name;
                socket.Width = SocketVisual.SocketHeight;
                socket.X = X + NodeWidth - SocketVisual.SocketHeight;
                socket.Y = Y + curOutputH;
                socket.Value = ctx[socket.Name];              
                socketList.Add(socket);

                curOutputH += SocketVisual.SocketHeight + ComponentPadding;
            }

            socketCache = socketList.ToArray();
            return socketCache;
        }

        internal void DiscardCache()
        {
            socketCache = null;
        }

        /// <summary>
        /// Returns node context which is dynamic type. It will contain all node default input/output properties.
        /// </summary>
        public object GetNodeContext()
        {            
            if (nodeContext == null)
            {                
                dynamic context = new DynamicNodeContext();

                foreach (var input in GetInputs())
                {
                    var p = input.ParameterType == typeof(string) ? "" :
                        Activator.CreateInstance(AppDomain.CurrentDomain, input.ParameterType.Assembly.GetName().Name,
                            input.ParameterType.FullName.Replace("&", "").Replace(" ", "")).Unwrap();
                    if (!Convert.IsDBNull(input.DefaultValue))
                    {
                        var def = Convert.ChangeType(input.DefaultValue, p.GetType());
                        if (def != null)
                        {
                            p = def;
                        }
                    }
                    context[input.Name.Replace(" ", "")] = p;
                }
                foreach (var output in GetOutputs())
                {
                    var p = output.ParameterType == typeof(string) ? "" :
                       Activator.CreateInstance(AppDomain.CurrentDomain, output.ParameterType.Assembly.GetName().Name,
                            output.ParameterType.FullName.Replace("&", "").Replace(" ", "")).Unwrap();
                    if (!Convert.IsDBNull(output.DefaultValue))
                    {
                        var def = Convert.ChangeType(output.DefaultValue, p.GetType());
                        if (def != null)
                        {
                            p = def;
                        }
                    }
                    context[output.Name.Replace(" ", "")] = p;
                }

                nodeContext = context;
            }
            return nodeContext;
        }

        internal ParameterInfo[] GetInputs()
        {
            return Type.GetParameters().Where(x => !x.IsOut).ToArray();
        }

        internal ParameterInfo[] GetOutputs()
        {
            return Type.GetParameters().Where(x => x.IsOut).ToArray();
        }

        /// <summary>
        /// Returns current size of the node.
        /// </summary>        
        public SizeF GetNodeBounds()
        {
            var csize = new SizeF();
            if (CustomEditor != null)
            {
                csize = new SizeF(CustomEditor.ClientSize.Width + 2 + 80 +SocketVisual.SocketHeight*2,
                    CustomEditor.ClientSize.Height + HeaderHeight + 8);                
            }

            var inputs = GetInputs().Length;
            var outputs = GetOutputs().Length;
            if (Callable)
            {
                inputs++;
                outputs++;
            }
            var h = HeaderHeight + Math.Max(inputs*(SocketVisual.SocketHeight + ComponentPadding),
                outputs*(SocketVisual.SocketHeight + ComponentPadding)) + ComponentPadding*2f;

            return new SizeF(Math.Max(csize.Width, NodeWidth), Math.Max(csize.Height, h));
        }

        /// <summary>
        /// Returns current size of node caption (header belt).
        /// </summary>
        /// <returns></returns>
        public SizeF GetHeaderSize()
        {
            return new SizeF(GetNodeBounds().Width, HeaderHeight);
        }

        /// <summary>
        /// Allows node to be drawn on given Graphics context.       
        /// </summary>
        /// <param name="g">Graphics context.</param>
        /// <param name="mouseLocation">Location of the mouse relative to NodesControl instance.</param>
        /// <param name="mouseButtons">Mouse buttons that are pressed while drawing node.</param>
        public void Draw(Graphics g, Point mouseLocation, MouseButtons mouseButtons)
        {
            var rect = new RectangleF(new PointF(X,Y), GetNodeBounds());

            var feedrect = rect;
            feedrect.Inflate(10, 10);

            if (Feedback == FeedbackType.Warning)
            {
                g.DrawRectangle(new Pen(Color.Yellow, 4), Rectangle.Round(feedrect));
            }
            else if (Feedback == FeedbackType.Error)
            {
                g.DrawRectangle(new Pen(Color.Red, 5), Rectangle.Round(feedrect));
            }

            var caption = new RectangleF(new PointF(X,Y), GetHeaderSize());
            bool mouseHoverCaption = caption.Contains(mouseLocation);

            g.FillRectangle(new SolidBrush(NodeColor), rect);

            if (IsSelected)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(180,Color.WhiteSmoke)), rect);
                g.FillRectangle(mouseHoverCaption ? Brushes.Gold : Brushes.Goldenrod, caption);
            }
            else
            {                
                g.FillRectangle(mouseHoverCaption ? Brushes.Cyan : Brushes.Aquamarine, caption);
            }
            g.DrawRectangle(Pens.Gray, Rectangle.Round(caption));
            g.DrawRectangle(Pens.Black, Rectangle.Round(rect));

            g.DrawString(Name, SystemFonts.DefaultFont, Brushes.Black, new PointF(X + 3, Y + 3));       

            var sockets = GetSockets();
            foreach (var socet in sockets)
            {
                socet.Draw(g, mouseLocation, mouseButtons);
            }
        }

        internal void Execute(INodesContext context)
        {
            context.CurrentProcessingNode = this;

            var dc = (GetNodeContext() as DynamicNodeContext);
            var parametersDict = Type.GetParameters().OrderBy(x => x.Position).ToDictionary(x => x.Name, x => dc[x.Name]);
            var parameters = parametersDict.Values.ToArray();

            int ndx = 0;
            Type.Invoke(context, parameters);
            foreach (var kv in parametersDict.ToArray())
            {
                parametersDict[kv.Key] = parameters[ndx];
                ndx++;
            }

            var outs = GetSockets();

            
            foreach (var parameter in dc.ToArray())
            {
                dc[parameter] = parametersDict[parameter];
                var o = outs.FirstOrDefault(x => x.Name == parameter);
                //if (o != null)
                Debug.Assert(o != null, "Output not found");
                {
                    o.Value = dc[parameter];
                }                                
            }
        }

        internal void LayoutEditor()
        {
            if (CustomEditor != null)
            {
                CustomEditor.Location = new Point((int)( X + 1 + 40 + SocketVisual.SocketHeight), (int) (Y + HeaderHeight + 4));
            }
        }
    }
}
