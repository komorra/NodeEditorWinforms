using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NodeEditor
{
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
