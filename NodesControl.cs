using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace NodeEditor
{
    public partial class NodesControl : UserControl
    {
        public class NodeToken
        {
            public MethodInfo Method;
            public NodeAttribute Attribute;
        }

        private NodesGraph graph = new NodesGraph();
        private bool needRepaint = true;
        private Timer timer = new Timer();
        private bool mdown;
        private Point lastmpos;
        private NodeVisual dragNode;
        private SocketVisual dragSocket;
        private NodeVisual dragSocketNode;
        private PointF dragConnectionBegin;
        private PointF dragConnectionEnd;
        private NodeVisual highlightedNode;

        public INodesContext Context { get; set; }

        public Action<object> OnNodeContextSelected = delegate { };
        public Action<string> OnNodeHint = delegate { };

        private readonly Dictionary<ToolStripMenuItem,int> allContextItems = new Dictionary<ToolStripMenuItem, int>();
        private Point lastMouseLocation;
        private Point autoScroll;

        public NodesControl()
        {
            InitializeComponent();
            timer.Interval = 30;
            timer.Tick += TimerOnTick;
            timer.Start();                           
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (DesignMode) return;
            if (needRepaint)
            {
                Invalidate();
            }
        }

        private void NodesControl_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;            

            graph.Draw(e.Graphics, PointToClient(MousePosition), MouseButtons);            

            if (dragSocket != null)
            {
                var pen = new Pen(Color.Black, 2);
                NodesGraph.DrawConnection(e.Graphics, pen, dragConnectionBegin, dragConnectionEnd);
            }

            needRepaint = false;
        }

        private void NodesControl_MouseMove(object sender, MouseEventArgs e)
        {            
            if (mdown)
            {
                if (dragNode != null)
                {                    
                    dragNode.X += e.X - lastmpos.X;
                    dragNode.Y += e.Y - lastmpos.Y;
                    dragNode.LayoutEditor();
                    Refresh();
                }
                if (dragSocket != null)
                {
                    var center = new PointF(dragSocket.X + dragSocket.Width/2f, dragSocket.Y + dragSocket.Height/2f);
                    if (dragSocket.Input)
                    {
                        dragConnectionBegin = e.Location;
                        dragConnectionEnd = center;
                    }
                    else
                    {
                        dragConnectionBegin = center;
                        dragConnectionEnd = e.Location;
                    }
                }
                lastmpos = e.Location;
            }
            else
            {
                highlightedNode =
                    graph.Nodes.OrderBy(x => x.Order).FirstOrDefault(
                        x => new RectangleF(new PointF(x.X, x.Y), x.GetNodeBounds()).Contains(e.Location));
            }

            needRepaint = true;
        }

        private void NodesControl_MouseDown(object sender, MouseEventArgs e)
        {                        
            if (e.Button == MouseButtons.Left)
            {
                var node =
                    graph.Nodes.OrderBy(x => x.Order).FirstOrDefault(
                        x => new RectangleF(new PointF(x.X, x.Y), x.GetHeaderSize()).Contains(e.Location));

                if (node != null && !mdown)
                {
                    node.Order = graph.Nodes.Min(x => x.Order) - 1;
                    dragNode = node;
                    mdown = true;
                    lastmpos = e.Location;
                }
                if (node == null && !mdown)
                {
                    var nodeWhole =
                    graph.Nodes.OrderBy(x => x.Order).FirstOrDefault(
                        x => new RectangleF(new PointF(x.X, x.Y), x.GetNodeBounds()).Contains(e.Location));
                    if (nodeWhole != null)
                    {
                        node = nodeWhole;
                        var socket = nodeWhole.GetSockets().FirstOrDefault(x => x.GetBounds().Contains(e.Location));
                        if (socket != null)
                        {
                            if ((ModifierKeys & Keys.Control) == Keys.Control)
                            {
                                var connection =
                                    graph.Connections.FirstOrDefault(
                                        x => x.InputNode == nodeWhole && x.InputSocketName == socket.Name);

                                if (connection != null)
                                {
                                    dragSocket =
                                        connection.OutputNode.GetSockets()
                                            .FirstOrDefault(x => x.Name == connection.OutputSocketName);
                                    dragSocketNode = connection.OutputNode;
                                }
                                else
                                {
                                    connection =
                                        graph.Connections.FirstOrDefault(
                                            x => x.OutputNode == nodeWhole && x.OutputSocketName == socket.Name);

                                    if (connection != null)
                                    {
                                        dragSocket =
                                            connection.InputNode.GetSockets()
                                                .FirstOrDefault(x => x.Name == connection.InputSocketName);
                                        dragSocketNode = connection.InputNode;
                                    }
                                }

                                graph.Connections.Remove(connection);
                            }
                            else
                            {
                                dragSocket = socket;
                                dragSocketNode = nodeWhole;
                            }
                            dragConnectionBegin = e.Location;
                            dragConnectionEnd = e.Location;
                            mdown = true;
                            lastmpos = e.Location;
                        }
                    }
                }
                if (node != null)
                {
                    OnNodeContextSelected(node.GetNodeContext());
                }
            }

            needRepaint = true;
        }

        private bool IsConnectable(SocketVisual a, SocketVisual b)
        {
            var input = a.Input ? a : b;
            var output = a.Input ? b : a;
            var otype = Type.GetType(output.Type.FullName.Replace("&", ""), AssemblyResolver, TypeResolver);
            var itype = Type.GetType(input.Type.FullName.Replace("&", ""), AssemblyResolver, TypeResolver);
            if (otype == null || itype == null) return false;
            var allow = otype == itype || otype.IsSubclassOf(itype);
            return allow;
        }

        private Type TypeResolver(Assembly assembly, string name, bool inh)
        {
            if (assembly == null) assembly = ResolveAssembly(name);
            if (assembly == null) return null;
            return assembly.GetType(name);
        }

        private Assembly ResolveAssembly(string fullTypeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => x.GetTypes().Any(o => o.FullName == fullTypeName));
        }

        private Assembly AssemblyResolver(AssemblyName assemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName() == assemblyName);
        }

        private void NodesControl_MouseUp(object sender, MouseEventArgs e)
        {            
            if (dragSocket != null)
            {
                var nodeWhole =
                    graph.Nodes.OrderBy(x => x.Order).FirstOrDefault(
                        x => new RectangleF(new PointF(x.X, x.Y), x.GetNodeBounds()).Contains(e.Location));
                if (nodeWhole != null)
                {
                    var socket = nodeWhole.GetSockets().FirstOrDefault(x => x.GetBounds().Contains(e.Location));
                    if (socket != null)
                    {
                        if (IsConnectable(dragSocket,socket) && dragSocket.Input != socket.Input)
                        {                                                        
                            var nc = new NodeConnection();
                            if (!dragSocket.Input)
                            {
                                nc.OutputNode = dragSocketNode;
                                nc.OutputSocketName = dragSocket.Name;
                                nc.InputNode = nodeWhole;
                                nc.InputSocketName = socket.Name;
                            }
                            else
                            {
                                nc.InputNode = dragSocketNode;
                                nc.InputSocketName = dragSocket.Name;
                                nc.OutputNode = nodeWhole;
                                nc.OutputSocketName = socket.Name;
                            }

                            graph.Connections.RemoveAll(
                                x => x.InputNode == nc.InputNode && x.InputSocketName == nc.InputSocketName);

                            graph.Connections.Add(nc);
                        }
                    }
                }
            }

            dragNode = null;
            dragSocket = null;
            mdown = false;
            needRepaint = true;
        }

        private void AddToMenu(ToolStripItemCollection items, NodeToken token, string path, EventHandler click)
        {
            var pathParts = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var first = pathParts.FirstOrDefault();
            ToolStripMenuItem item = null;
            if (!items.ContainsKey(first))
            {
                item = new ToolStripMenuItem(first);
                item.Name = first;                
                item.Tag = token;
                items.Add(item);
            }
            else
            {
                item = items[first] as ToolStripMenuItem;
            }
            var next = string.Join("/", pathParts.Skip(1));
            if (!string.IsNullOrEmpty(next))
            {
                item.MouseEnter += (sender, args) => OnNodeHint("");
                AddToMenu(item.DropDownItems, token, next, click);
            }
            else
            {
                item.Click += click;
                item.Click += (sender, args) =>
                {
                    var i = allContextItems.Keys.FirstOrDefault(x => x.Name == item.Name);
                    allContextItems[i]++;
                };
                item.MouseEnter += (sender, args) => OnNodeHint(token.Attribute.Description ?? "");
                if (!allContextItems.Keys.Any(x => x.Name == item.Name))
                {
                    allContextItems.Add(item, 0);
                }
            }
        }

        private void NodesControl_MouseClick(object sender, MouseEventArgs e)
        {
            lastMouseLocation = e.Location;

            if (Context == null) return;

            if (e.Button == MouseButtons.Right)
            {
                var methods = Context.GetType().GetMethods();
                var nodes =
                    methods.Select(
                        x =>
                            new
                                NodeToken()
                            {
                                Method = x,
                                Attribute =
                                    x.GetCustomAttributes(typeof (NodeAttribute), false)
                                        .Cast<NodeAttribute>()
                                        .FirstOrDefault()
                            }).Where(x => x.Attribute != null);

                var context = new ContextMenuStrip();
                if (highlightedNode != null)
                {
                    context.Items.Add("Delete Node", null, ((o, args) =>
                    {
                        if (highlightedNode != null)
                        {
                            graph.Nodes.Remove(highlightedNode);
                            graph.Connections.RemoveAll(
                                x => x.OutputNode == highlightedNode || x.InputNode == highlightedNode);
                            Controls.Remove(highlightedNode.CustomEditor);
                        }
                    }));
                    context.Items.Add(new ToolStripSeparator());
                }
                if (allContextItems.Values.Any(x => x > 0))
                {
                    var handy = allContextItems.Where(x => x.Value > 0).OrderByDescending(x => x.Value).Take(8);
                    foreach (var kv in handy)
                    {
                        context.Items.Add(kv.Key);
                    }
                    context.Items.Add(new ToolStripSeparator());
                }
                foreach (var node in nodes.OrderBy(x=>x.Attribute.Path))
                {
                    AddToMenu(context.Items, node, node.Attribute.Path, (s,ev) =>
                    {
                        var tag = (s as ToolStripMenuItem).Tag as NodeToken;

                        var nv = new NodeVisual();
                        nv.X = lastMouseLocation.X;
                        nv.Y = lastMouseLocation.Y;
                        nv.Type = node.Method;
                        nv.Callable = node.Attribute.IsCallable;
                        nv.Name = node.Attribute.Name;
                        nv.Order = graph.Nodes.Count;
                        nv.ExecInit = node.Attribute.IsExecutionInitiator;
                        if (node.Attribute.CustomEditor != null)
                        {
                            Control ctrl = null;
                            nv.CustomEditor = ctrl = Activator.CreateInstance(node.Attribute.CustomEditor) as Control;
                            if (ctrl != null)
                            {
                                ctrl.Tag = nv;                                
                                Controls.Add(ctrl);                                                               
                            }
                            nv.LayoutEditor();
                        }

                        graph.Nodes.Add(nv);
                        Refresh();
                        needRepaint = true;
                    });                    
                }
                context.Show(MousePosition);
            }
        }

        public void Execute(NodeVisual node = null)
        {
            var init = node ?? graph.Nodes.FirstOrDefault(x => x.ExecInit);
            if (init != null)
            {
                Resolve(init);
                init.Execute(Context);
                var connection = graph.Connections.FirstOrDefault(
                    x => x.OutputNode == init && x.OutputSocket.Type == typeof (ExecutionPath));
                if (connection != null)
                {                    
                    Execute(connection.InputNode);
                }
            }
        }

        private void Resolve(NodeVisual node)
        {
            var icontext = (node.GetNodeContext() as DynamicNodeContext);
            foreach (var input in node.GetInputs())
            {
                var connection =
                    graph.Connections.FirstOrDefault(x => x.InputNode == node && x.InputSocketName == input.Name);
                if (connection != null)
                {
                    Resolve(connection.OutputNode);
                    if (!connection.OutputNode.Callable)
                    {                        
                        connection.OutputNode.Execute(Context);
                    }
                    var ocontext = (connection.OutputNode.GetNodeContext() as DynamicNodeContext);
                    icontext[connection.InputSocketName] = ocontext[connection.OutputSocketName];                    
                }
            }
        }

        public byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write("NodeSystemP"); //recognization string
                bw.Write(1000); //version
                bw.Write(graph.Nodes.Count);
                foreach (var node in graph.Nodes)
                {
                    bw.Write(node.GUID);
                    bw.Write(node.X);
                    bw.Write(node.Y);
                    bw.Write(node.Callable);
                    bw.Write(node.ExecInit);
                    bw.Write(node.Name);
                    bw.Write(node.Order);
                    if (node.CustomEditor == null)
                    {
                        bw.Write("");
                        bw.Write("");
                    }
                    else
                    {
                        bw.Write(node.CustomEditor.GetType().Assembly.GetName().Name);
                        bw.Write(node.CustomEditor.GetType().FullName);
                    }
                    bw.Write(node.Type.Name);
                    var context = (node.GetNodeContext() as DynamicNodeContext).Serialize();
                    bw.Write(context.Length);
                    bw.Write(context);
                    bw.Write(4); //additional data size per node
                    bw.Write(node.Int32Tag);
                }
                bw.Write(graph.Connections.Count);
                foreach (var connection in graph.Connections)
                {
                    bw.Write(connection.OutputNode.GUID);
                    bw.Write(connection.OutputSocketName);

                    bw.Write(connection.InputNode.GUID);
                    bw.Write(connection.InputSocketName);
                    bw.Write(0); //additional data size per connection
                }
                bw.Write(0); //additional data size per graph
                return (bw.BaseStream as MemoryStream).ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                var ident = br.ReadString();
                if (ident != "NodeSystemP") return;
                graph.Connections.Clear();
                graph.Nodes.Clear();
                Controls.Clear();

                var version = br.ReadInt32();
                int nodeCount = br.ReadInt32();
                for (int i = 0; i < nodeCount; i++)
                {
                    var nv = new NodeVisual();
                    nv.GUID = br.ReadString();
                    nv.X = br.ReadSingle();
                    nv.Y = br.ReadSingle();
                    nv.Callable = br.ReadBoolean();
                    nv.ExecInit = br.ReadBoolean();
                    nv.Name = br.ReadString();
                    nv.Order = br.ReadInt32();
                    var customEditorAssembly = br.ReadString();
                    var customEditor = br.ReadString();                                                        
                    nv.Type = Context.GetType().GetMethod(br.ReadString());
                    (nv.GetNodeContext() as DynamicNodeContext).Deserialize(br.ReadBytes(br.ReadInt32()));
                    
                    var additional = br.ReadInt32(); //read additional data
                    if (additional >= 4)
                    {
                        nv.Int32Tag = br.ReadInt32();
                    }
                    if (additional > 4)
                    {
                        br.ReadBytes(additional - 4);
                    }

                    if (customEditor != "")
                    {
                        nv.CustomEditor =
                            Activator.CreateInstance(AppDomain.CurrentDomain, customEditorAssembly, customEditor).Unwrap() as Control;

                        Control ctrl = nv.CustomEditor;
                        if (ctrl != null)
                        {                            
                            ctrl.Tag = nv;                            
                            Controls.Add(ctrl);
                        }
                        nv.LayoutEditor();
                    }

                    graph.Nodes.Add(nv);
                }
                var connectionsCount = br.ReadInt32();
                for (int i = 0; i < connectionsCount; i++)
                {
                    var con = new NodeConnection();
                    var og = br.ReadString();
                    con.OutputNode = graph.Nodes.FirstOrDefault(x => x.GUID == og);
                    con.OutputSocketName = br.ReadString();
                    var ig = br.ReadString();
                    con.InputNode = graph.Nodes.FirstOrDefault(x => x.GUID == ig);
                    con.InputSocketName = br.ReadString();
                    br.ReadBytes(br.ReadInt32()); //read additional data

                    graph.Connections.Add(con);
                }
                br.ReadBytes(br.ReadInt32()); //read additional data
            }
            Refresh();
        }

        public void Clear()
        {
            graph.Nodes.Clear();
            graph.Connections.Clear();
            Controls.Clear();
            Refresh();
        }
    }
}
