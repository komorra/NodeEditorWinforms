using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeEditor
{
    /// <summary>
    /// Interface that every NodesControl context should implement.
    /// </summary>
    public interface INodesContext
    {
        /// <summary>
        /// Property that is set to actual processed node during execution process.
        /// </summary>
        NodeVisual CurrentProcessingNode { get; set; }
    }
}
