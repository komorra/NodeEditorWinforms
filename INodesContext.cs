using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeEditor
{
    public interface INodesContext
    {
        NodeVisual CurrentProcessingNode { get; set; }
    }
}
