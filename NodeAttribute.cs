using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NodeEditor
{
    /// <summary>
    /// Attribute resposible for exposing a method to the NodesControl.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NodeAttribute : Attribute
    {
        /// <summary>
        /// Where should be node menuitem located - don't set if it should be in the main menu level.
        /// </summary>
        public string Menu { get; set; }

        /// <summary>
        /// Optional category for the node.
        /// </summary>        
        public string Category { get; set; }
        
        /// <summary>
        /// If true, the node is able to be executed during execution process (will have exec input and output socket).
        /// </summary>
        public bool IsCallable { get; set; }

        /// <summary>
        /// Name of the node that will be displayed in the node caption.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description that should tell more precisely what the node is performing.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If true the node will be the start point of the execution.
        /// </summary>
        public bool IsExecutionInitiator { get; set; }

        /// <summary>
        /// Given type should be subclass of System.Windows.Forms.Control, and represents what will be displayed in the middle of the node.
        /// </summary>
        public Type CustomEditor { get; set; }

        /// <summary>
        /// Attribute for exposing method as node.
        /// </summary>
        /// <param name="name">Name of the node that will be displayed in the node caption.</param>
        /// <param name="menu">Where should be node menuitem located - don't set if it should be in the main menu level.</param>
        /// <param name="category">Optional category for the node.</param>
        /// <param name="description">Description that should tell more precisely what the node is performing.</param>
        /// <param name="isCallable">If true, the node is able to be executed during execution process (will have exec input and output socket).</param>
        /// <param name="isExecutionInitiator">If true the node will be the start point of the execution.</param>
        /// <param name="customEditor">Given type should be subclass of System.Windows.Forms.Control, and represents what will be displayed in the middle of the node.</param>
        public NodeAttribute(string name = "Node", string menu = "", string category = "General",
            string description = "Some node.", bool isCallable = true, bool isExecutionInitiator = false, Type customEditor = null)
        {
            Name = name;
            Menu = menu;
            Category = category;
            Description = description;
            IsCallable = isCallable;
            IsExecutionInitiator = isExecutionInitiator;
            CustomEditor = customEditor;
        }

        /// <summary>
        /// Full path in the context menu.
        /// </summary>
        public string Path
        {
            get { return Menu + "/" + Name; }
        }
    }
}
