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
        /// Const used for width and height if they are not defined by user
        /// </summary>
        private const int Auto = -1;

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
        /// Name that will be used in the xml export of the graph.
        /// </summary>
        public string XmlExportName { get; set; }

        /// <summary>
        /// Width of single node
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of single node
        /// </summary>
        public int Height { get; set; }

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
        /// <param name="xmlExportName">Name that will be used in the xml export of the graph.</param>
        /// <param name="width">Width of single node, or Auto if not determined</param>
        /// <param name="height">Height of single node, or Auto if not determined</param>
        public NodeAttribute(string name = "Node", string menu = "", string category = "General",
            string description = "Some node.", bool isCallable = true, bool isExecutionInitiator = false, Type customEditor = null, string xmlExportName = "",
            int width = Auto, int height = Auto)
        {
            Name = name;
            Menu = menu;
            Category = category;
            Description = description;
            IsCallable = isCallable;
            IsExecutionInitiator = isExecutionInitiator;
            CustomEditor = customEditor;
            XmlExportName = xmlExportName;
            Width = width;
            Height = height;
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
