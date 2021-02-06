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
            get
            {
                if (!dynamicProperties.ContainsKey(key)) return null;
                return dynamicProperties[key];
            }
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
