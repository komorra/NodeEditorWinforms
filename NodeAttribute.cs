using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NodeEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NodeAttribute : Attribute
    {
        public string Menu { get; set; }
        public string Category { get; set; }
        public bool IsCallable { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsExecutionInitiator { get; set; }
        public Type CustomEditor { get; set; }

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

        public string Path
        {
            get { return Menu + "/" + Name; }
        }
    }
}
