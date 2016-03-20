using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NodeEditor
{
    /// <summary>
    /// Class used as internal context of each node.
    /// </summary>
    [TypeConverter(typeof(DynamicNodeContextConverter))]   
    public class DynamicNodeContext : DynamicObject, IEnumerable<String>
    {
        private readonly IDictionary<string, object> dynamicProperties =
            new Dictionary<string, object>();

        internal byte[] Serialize()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                foreach (var prop in dynamicProperties)
                {
                    if (prop.Value.GetType().IsSerializable)
                    {
                        using (var ps = new MemoryStream())
                        {
                            new BinaryFormatter().Serialize(ps, prop.Value);
                            bw.Write(prop.Key);
                            bw.Write((int) ps.Length);
                            bw.Write(ps.ToArray());
                        }
                    }
                }
                return (bw.BaseStream as MemoryStream).ToArray();
            }
        }

        internal void Deserialize(byte[] data)
        {
            dynamicProperties.Clear();
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var key = br.ReadString();
                    var propData = br.ReadBytes(br.ReadInt32());
                    using (var ms = new MemoryStream(propData))
                    {
                        var val = new BinaryFormatter().Deserialize(ms);
                        dynamicProperties.Add(key, val);
                    }
                }
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {            
            var memberName = binder.Name;
            return dynamicProperties.TryGetValue(memberName, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var memberName = binder.Name;
            dynamicProperties[memberName] = value;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return dynamicProperties.Keys;
        }        

        public object this[string key]
        {
            get { return dynamicProperties[key]; }
            set
            {
                if (!dynamicProperties.ContainsKey(key))
                {
                    dynamicProperties.Add(key, value);
                }
                else
                {
                    dynamicProperties[key] = value;
                }
            }
        }

        public IEnumerator<String> GetEnumerator()
        {
            return dynamicProperties.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
