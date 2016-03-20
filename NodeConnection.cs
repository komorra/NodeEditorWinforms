using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeEditor
{    
    internal class NodeConnection
    {
        public NodeVisual OutputNode { get; set; }
        public string OutputSocketName { get; set; }
        public NodeVisual InputNode { get; set; }
        public string InputSocketName { get; set; }

        public SocketVisual OutputSocket
        {
            get { return OutputNode.GetSockets().FirstOrDefault(x => x.Name == OutputSocketName); }
        }

        public SocketVisual InputSocket
        {
            get { return InputNode.GetSockets().FirstOrDefault(x => x.Name == InputSocketName); }
        }

        public bool IsExecution
        {
            get { return OutputSocket.Type == typeof (ExecutionPath); }
        }
    }
}
