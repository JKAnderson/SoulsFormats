using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SoulsFormats
{
    public partial class PARAM64 : SoulsFile<PARAM64>
    {
        /// <summary>
        /// The layout of cell data within each row in a param.
        /// </summary>
        public class Layout : List<Layout.Value>
        {
            /// <summary>
            /// The size of a row, determined automatically from the layout.
            /// </summary>
            public int Size
            {
                get
                {
                    int size = 0;

                    for (int i = 0; i < Count; i++)
                    {
                        string type = this[i].Type;

                        if (type.StartsWith("b8"))
                        {
                            size += 1;

                            int j;
                            for (j = 0; j < 8; j++)
                            {
                                if (i + j >= Count || this[i + j].Type != "b8")
                                    break;
                            }
                            i += j - 1;
                        }
                        else if (type.StartsWith("b32"))
                        {
                            size += 4;

                            int j;
                            for (j = 0; j < 32; j++)
                            {
                                if (i + j >= Count || this[i + j].Type != "b32")
                                    break;
                            }
                            i += j - 1;
                        }
                        else
                        {
                            size += this[i].Size;
                        }
                    }

                    return size;
                }
            }

            public static Layout ReadXMLFile(string path)
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                return new Layout(xml);
            }

            public static Layout ReadXMLText(string text)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(text);
                return new Layout(xml);
            }

            public static Layout ReadXMLDoc(XmlDocument xml)
            {
                return new Layout(xml);
            }

            /// <summary>
            /// Creates a new empty layout.
            /// </summary>
            public Layout() : base() { }

            private Layout(XmlDocument xml) : base()
            {
                foreach (XmlNode node in xml.SelectNodes("layout/entry"))
                {
                    Add(new Value(node));
                }
            }

            public void Write(string path)
            {
                var xws = new XmlWriterSettings()
                {
                    Indent = true,
                };
                var xw = XmlWriter.Create(path, xws);
                xw.WriteStartElement("layout");

                foreach (Value entry in this)
                    entry.Write(xw);

                xw.WriteEndElement();
                xw.Close();
            }

            public static object ParseParamValue(string type, string value)
            {
                if (type == "fixstr" || type == "fixstrW")
                    return value;
                else if (type == "b8" || type == "b32")
                    return bool.Parse(value);
                else if (type == "s8")
                    return sbyte.Parse(value);
                else if (type == "u8")
                    return byte.Parse(value);
                else if (type == "x8")
                    return Convert.ToByte(value, 16);
                else if (type == "s16")
                    return short.Parse(value);
                else if (type == "u16")
                    return ushort.Parse(value);
                else if (type == "x16")
                    return Convert.ToUInt16(value, 16);
                else if (type == "s32")
                    return int.Parse(value);
                else if (type == "u32")
                    return uint.Parse(value);
                else if (type == "x32")
                    return Convert.ToUInt32(value, 16);
                else if (type == "f32")
                    return float.Parse(value, CultureInfo.InvariantCulture);
                else
                    throw new InvalidCastException("Unparsable type: " + type);
            }

            public static string ParamValueToString(string type, object value)
            {
                if (type == "x8")
                    return $"0x{value:X2}";
                else if (type == "x16")
                    return $"0x{value:X4}";
                else if (type == "x32")
                    return $"0x{value:X8}";
                else if (type == "f32")
                    return Convert.ToString((float)value, CultureInfo.InvariantCulture);
                else
                    return value.ToString();
            }

            /// <summary>
            /// The type and name of one cell in a row.
            /// </summary>
            public class Value
            {
                /// <summary>
                /// The type of the cell.
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                /// The name of the cell.
                /// </summary>
                public string Name { get; set; }

                private int size;
                public int Size
                {
                    get
                    {
                        if (Type == "fixstr" || Type == "fixstrW" || Type == "dummy8")
                            return size;
                        else if (Type == "s8" || Type == "u8" || Type == "x8")
                            return 1;
                        else if (Type == "s16" || Type == "u16" || Type == "x16")
                            return 2;
                        else if (Type == "s32" || Type == "u32" || Type == "x32" || Type == "f32")
                            return 4;
                        // Not meaningful
                        else if (Type == "b8" || Type == "b32")
                            return 0;
                        else
                            throw new InvalidCastException("Unknown type: " + Type);
                    }

                    set
                    {
                        if (Type == "fixstr" || Type == "fixstrW" || Type == "dummy8")
                            size = value;
                        else
                            throw new InvalidOperationException("Size may only be set for variable-width types: fixstr, fixstrW, and dummy8.");
                    }
                }

                public object Default { get; set; }

                public bool IsVariableSize
                {
                    get
                    {
                        return Type == "fixstr" || Type == "fixstrW" || Type == "dummy8";
                    }
                }

                public Value(string type, string name, object def)
                {
                    Type = type;
                    Name = name;
                    Default = def;
                }

                public Value(string type, string name, int size, object def)
                {
                    Type = type;
                    Name = name;
                    this.size = size;
                    Default = def;
                }

                internal Value(XmlNode node)
                {
                    Name = node.SelectSingleNode("name").InnerText;
                    Type = node.SelectSingleNode("type").InnerText;

                    if (Type == "fixstr" || Type == "fixstrW" || Type == "dummy8")
                        size = int.Parse(node.SelectSingleNode("size").InnerText);

                    if (Type != "dummy8")
                        Default = ParseParamValue(Type, node.SelectSingleNode("default").InnerText);
                }

                internal void Write(XmlWriter xw)
                {
                    xw.WriteStartElement("entry");
                    xw.WriteElementString("name", Name);
                    xw.WriteElementString("type", Type);

                    if (Type == "fixstr" || Type == "fixstrW" || Type == "dummy8")
                        xw.WriteElementString("size", Size.ToString());

                    if (Type != "dummy8")
                        xw.WriteElementString("default", ParamValueToString(Type, Default));

                    xw.WriteEndElement();
                }
            }
        }
    }
}
