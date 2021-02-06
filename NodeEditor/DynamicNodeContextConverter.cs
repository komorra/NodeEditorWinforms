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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NodeEditor
{
    /// <summary>
    /// Converter that allows to display node context object e.g. in property grids.
    /// </summary>
    public class DynamicNodeContextConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var obj = value as DynamicNodeContext;
            List<PropertyDescriptor> props = new List<PropertyDescriptor>();
            var type = obj.GetType();
            foreach (var p in obj)
            {
                var prop = new DynamicPropertyDescriptor(p, obj[p].GetType(), typeof (DynamicNodeContext));
                props.Add(prop);
            }

            return new PropertyDescriptorCollection(props.ToArray());            
        }
    }

    public class DynamicPropertyDescriptor : PropertyDescriptor
    {                
        private Type type;
        private Type parent;
        private string name;

        public DynamicPropertyDescriptor(string name, Type type, Type parent) : base(name, new Attribute[] {})
        {
            this.name = name;
            this.parent = parent;
            this.type = type;            
        }

        protected DynamicPropertyDescriptor(string name, Attribute[] attrs) : base(name, attrs)
        {
        }

        protected DynamicPropertyDescriptor(MemberDescriptor descr) : base(descr)
        {
        }

        protected DynamicPropertyDescriptor(MemberDescriptor descr, Attribute[] attrs) : base(descr, attrs)
        {
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return (component as DynamicNodeContext)[name];
        }

        public override void ResetValue(object component)
        {
            (component as DynamicNodeContext)[name] = Activator.CreateInstance(type);
        }

        public override void SetValue(object component, object value)
        {
            (component as DynamicNodeContext)[name] = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return GetValue(component) != null;
        }

        public override Type ComponentType { get { return parent; } }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return type; } }
    }
    
}
