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
                        CellType type = this[i].Type;

                        if (type == CellType.b8)
                        {
                            size += 1;

                            int j;
                            for (j = 0; j < 8; j++)
                            {
                                if (i + j >= Count || this[i + j].Type != CellType.b8)
                                    break;
                            }
                            i += j - 1;
                        }
                        else if (type == CellType.b32)
                        {
                            size += 4;

                            int j;
                            for (j = 0; j < 32; j++)
                            {
                                if (i + j >= Count || this[i + j].Type != CellType.b32)
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
            /// Parse a string according to the given param type and culture.
            /// </summary>
            public static object ParseParamValue(CellType type, string value, CultureInfo culture)
            {
                if (type == CellType.fixstr || type == CellType.fixstrW)
                    return value;
                else if (type == CellType.b8 || type == CellType.b32)
                    return bool.Parse(value);
                else if (type == CellType.s8)
                    return sbyte.Parse(value);
                else if (type == CellType.u8)
                    return byte.Parse(value);
                else if (type == CellType.x8)
                    return Convert.ToByte(value, 16);
                else if (type == CellType.s16)
                    return short.Parse(value);
                else if (type == CellType.u16)
                    return ushort.Parse(value);
                else if (type == CellType.x16)
                    return Convert.ToUInt16(value, 16);
                else if (type == CellType.s32)
                    return int.Parse(value);
                else if (type == CellType.u32)
                    return uint.Parse(value);
                else if (type == CellType.x32)
                    return Convert.ToUInt32(value, 16);
                else if (type == CellType.f32)
                    return float.Parse(value, culture);
                else
                    throw new InvalidCastException("Unparsable type: " + type);
            }

            /// <summary>
            /// Parse a string according to the given param type and invariant culture.
            /// </summary>
            public static object ParseParamValue(CellType type, string value)
            {
                return ParseParamValue(type, value, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Convert a param value of the specified type to a string using the given culture.
            /// </summary>
            public static string ParamValueToString(CellType type, object value, CultureInfo culture)
            {
                if (type == CellType.x8)
                    return $"0x{value:X2}";
                else if (type == CellType.x16)
                    return $"0x{value:X4}";
                else if (type == CellType.x32)
                    return $"0x{value:X8}";
                else if (type == CellType.f32)
                    return Convert.ToString(value, culture);
                else
                    return value.ToString();
            }

            /// <summary>
            /// Convert a param value of the specified type to a string using invariant culture.
            /// </summary>
            public static string ParamValueToString(CellType type, object value)
            {
                return ParamValueToString(type, value, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// The type and name of one cell in a row.
            /// </summary>
            public class Entry
            {
                /// <summary>
                /// The type of the cell.
                /// </summary>
                public CellType Type { get; set; }

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
                        if (IsVariableSize)
                            return size;
                        else if (Type == CellType.s8 || Type == CellType.u8 || Type == CellType.x8)
                            return 1;
                        else if (Type == CellType.s16 || Type == CellType.u16 || Type == CellType.x16)
                            return 2;
                        else if (Type == CellType.s32 || Type == CellType.u32 || Type == CellType.x32 || Type == CellType.f32)
                            return 4;
                        // Not meaningful
                        else if (Type == CellType.b8 || Type == CellType.b32)
                            return 0;
                        else
                            throw new InvalidCastException("Unknown type: " + Type);
                    }

                    set
                    {
                        if (IsVariableSize)
                            size = value;
                        else
                            throw new InvalidOperationException("Size may only be set for variable-width types: fixstr, fixstrW, and dummy8.");
                    }
                }

                private object def;

                /// <summary>
                /// The default value to use when creating a new row.
                /// </summary>
                public object Default
                {
                    get
                    {
                        if (Type == CellType.dummy8)
                            return new byte[Size];
                        else
                            return def;
                    }

                    set
                    {
                        if (Type == CellType.dummy8)
                            throw new InvalidOperationException("Default may not be set for dummy8.");
                        else
                            def = value;
                    }
                }

                /// <summary>
                /// Whether the size can be modified.
                /// </summary>
                public bool IsVariableSize => Type == CellType.fixstr || Type == CellType.fixstrW || Type == CellType.dummy8;

                /// <summary>
                /// A description of this field's purpose; may be null.
                /// </summary>
                public string Description;

                /// <summary>
                /// Create a new entry of a fixed-width type.
                /// </summary>
                public Entry(CellType type, string name, object def)
                {
                    Type = type;
                    Name = name;
                    Default = def;
                }

                /// <summary>
                /// Create a new entry of a variable-width type. Default is ignored for dummy8.
                /// </summary>
                public Entry(CellType type, string name, int size, object def)
                {
                    Type = type;
                    Name = name;
                    Size = size;
                    this.def = Type == CellType.dummy8 ? null : def;
                }

                internal Entry(XmlNode node)
                {
                    Name = node.SelectSingleNode("name").InnerText;
                    Type = (CellType)Enum.Parse(typeof(CellType), node.SelectSingleNode("type").InnerText, true);

                    if (IsVariableSize)
                        size = int.Parse(node.SelectSingleNode("size").InnerText);

                    if (Type != CellType.dummy8)
                        Default = ParseParamValue(Type, node.SelectSingleNode("default").InnerText);

                    if (node.SelectSingleNode("description") != null)
                        Description = node.SelectSingleNode("description").InnerText;
                }

                internal void Write(XmlWriter xw)
                {
                    xw.WriteStartElement("entry");
                    xw.WriteElementString("name", Name);
                    xw.WriteElementString("type", Type.ToString());

                    if (IsVariableSize)
                        xw.WriteElementString("size", Size.ToString());

                    if (Type != CellType.dummy8)
                        xw.WriteElementString("default", ParamValueToString(Type, Default));

                    if (Description != null)
                        xw.WriteElementString("description", Description);

                    xw.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Possible types for values in a param.
        /// </summary>
        public enum CellType
        {
            /// <summary>
            /// Array of bytes.
            /// </summary>
            dummy8,

            /// <summary>
            /// 1-bit bool in a 1-byte field.
            /// </summary>
            b8,

            /// <summary>
            /// 1-bit bool in a 4-byte field.
            /// </summary>
            b32,

            /// <summary>
            /// Unsigned byte.
            /// </summary>
            u8,

            /// <summary>
            /// Unsigned byte, display as hex.
            /// </summary>
            x8,

            /// <summary>
            /// Signed byte.
            /// </summary>
            s8,

            /// <summary>
            /// Unsigned short.
            /// </summary>
            u16,

            /// <summary>
            /// Unsigned short, display as hex.
            /// </summary>
            x16,

            /// <summary>
            /// Signed short.
            /// </summary>
            s16,

            /// <summary>
            /// Unsigned int.
            /// </summary>
            u32,

            /// <summary>
            /// Unsigned int, display as hex.
            /// </summary>
            x32,

            /// <summary>
            /// Signed int.
            /// </summary>
            s32,

            /// <summary>
            /// Single-precision float.
            /// </summary>
            f32,

            /// <summary>
            /// Shift-JIS encoded string.
            /// </summary>
            fixstr,

            /// <summary>
            /// UTF-16 encoded string.
            /// </summary>
            fixstrW,
        }
    }
}
