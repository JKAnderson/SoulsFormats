using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace SoulsFormats
{
    public partial class PARAM64 : SoulsFile<PARAM64>
    {
        /// <summary>
        /// The layout of cell data within each row in a param.
        /// </summary>
        public class Layout : List<Layout.Entry>
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

            /// <summary>
            /// Read a PARAM64 layout from an XML file.
            /// </summary>
            public static Layout ReadXMLFile(string path)
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                return new Layout(xml);
            }

            /// <summary>
            /// Read a PARAM64 layout from an XML string.
            /// </summary>
            public static Layout ReadXMLText(string text)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(text);
                return new Layout(xml);
            }

            /// <summary>
            /// Read a PARAM64 layout from an XML document.
            /// </summary>
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
                    Add(new Entry(node));
                }
            }

            /// <summary>
            /// Write the layout to an XML file.
            /// </summary>
            public void Write(string path)
            {
                var xws = new XmlWriterSettings()
                {
                    Indent = true,
                };
                var xw = XmlWriter.Create(path, xws);
                xw.WriteStartElement("layout");

                foreach (Entry entry in this)
                    entry.Write(xw);

                xw.WriteEndElement();
                xw.Close();
            }

            /// <summary>
            /// Parse a string according to the given param type.
            /// </summary>
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

            /// <summary>
            /// Convert a param value of the specified type to a string.
            /// </summary>
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
            public class Entry
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

                /// <summary>
                /// Size in bytes of the entry; may only be set for fixstr, fixstrW, and dummy8.
                /// </summary>
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

                /// <summary>
                /// The default value to use when creating a new row.
                /// </summary>
                public object Default { get; set; }

                /// <summary>
                /// Whether the size can be modified.
                /// </summary>
                public bool IsVariableSize
                {
                    get
                    {
                        return Type == "fixstr" || Type == "fixstrW" || Type == "dummy8";
                    }
                }

                /// <summary>
                /// Create a new entry of a fixed-width type.
                /// </summary>
                public Entry(string type, string name, object def)
                {
                    Type = type;
                    Name = name;
                    Default = def;
                }

                /// <summary>
                /// Create a new entry of a variable-width type. Default is ignored for dummy8.
                /// </summary>
                public Entry(string type, string name, int size, object def)
                {
                    Type = type;
                    Name = name;
                    this.size = size;
                    Default = Type == "dummy8" ? null : def;
                }

                internal Entry(XmlNode node)
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
