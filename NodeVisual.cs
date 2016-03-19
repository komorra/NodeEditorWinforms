using System;
using System.Collections.Generic;
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
    public class NodeVisual
    {
        public const float NodeWidth = 140;
        public const float HeaderHeight = 20;
        public const float ComponentPadding = 2;

        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        internal MethodInfo Type { get; set; }
        internal int Order { get; set; }
        internal bool Callable { get; set; }
        internal bool ExecInit { get; set; }
        private object nodeContext { get; set; } 
        internal Control CustomEditor { get; set; }
        internal string GUID = Guid.NewGuid().ToString();
        public int Int32Tag = 0;

        internal SocketVisual[] GetSockets()
        {
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

            foreach (var output in GetOutputs())
            {
                var socket = new SocketVisual();
                socket.Type = output.ParameterType;
                socket.Height = SocketVisual.SocketHeight;
                socket.Name = output.Name;
                socket.Width = SocketVisual.SocketHeight;
                socket.X = X + NodeWidth - SocketVisual.SocketHeight;
                socket.Y = Y + curOutputH;                

                socketList.Add(socket);

                curOutputH += SocketVisual.SocketHeight + ComponentPadding;
            }

            return socketList.ToArray();
        }

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
                    context[input.Name.Replace(" ", "")] = p;
                }
                foreach (var output in GetOutputs())
                {
                    var p = output.ParameterType == typeof(string) ? "" :
                       Activator.CreateInstance(AppDomain.CurrentDomain, output.ParameterType.Assembly.GetName().Name,
                            output.ParameterType.FullName.Replace("&", "").Replace(" ", "")).Unwrap();
                    context[output.Name.Replace(" ", "")] = p;
                }

                nodeContext = context;
            }
            return nodeContext;
        }

        public ParameterInfo[] GetInputs()
        {
            return Type.GetParameters().Where(x => !x.IsOut).ToArray();
        }

        public ParameterInfo[] GetOutputs()
        {
            return Type.GetParameters().Where(x => x.IsOut).ToArray();
        }

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

        public SizeF GetHeaderSize()
        {
            return new SizeF(GetNodeBounds().Width, HeaderHeight);
        }

        public void Draw(Graphics g, Point mouseLocation, MouseButtons mouseButtons)
        {
            var rect = new RectangleF(new PointF(X,Y), GetNodeBounds());

            g.FillRectangle(Brushes.LightCyan, rect);

            var caption = new RectangleF(new PointF(X,Y), GetHeaderSize());
            bool mouseHoverCaption = caption.Contains(mouseLocation);                        

            g.FillRectangle(mouseHoverCaption ? Brushes.Cyan : Brushes.Aquamarine, caption);
            g.DrawRectangle(Pens.Gray, Rectangle.Round(caption));
            g.DrawRectangle(Pens.Black, Rectangle.Round(rect));

            g.DrawString(Name, SystemFonts.DefaultFont, Brushes.Black, new PointF(X + 3, Y + 3));       

            var sockets = GetSockets();
            foreach (var socet in sockets)
            {
                socet.Draw(g, mouseLocation, mouseButtons);
            }
        }

        public void Execute(INodesContext context)
        {
            context.CurrentProcessingNode = this;

            var dc = (GetNodeContext() as DynamicNodeContext);
            var parameters = dc.ToArray().Select(x => dc[x]).ToArray();

            Type.Invoke(context, parameters);

            int ndx = 0;
            foreach (var parameter in dc.ToArray())
            {
                dc[parameter] = parameters[ndx];
                ndx++;
            }
        }

        public void LayoutEditor()
        {
            if (CustomEditor != null)
            {
                CustomEditor.Location = new Point((int)( X + 1 + 40 + SocketVisual.SocketHeight), (int) (Y + HeaderHeight + 4));
            }
        }
    }
}
