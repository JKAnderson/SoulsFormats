using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SoulsFormats
{
    /// <summary>
    /// A param data file for DS3.
    /// </summary>
    public class PARAM64 : SoulsFile<PARAM64>
    {
        /// <summary>
        /// A name given to this param; no functional significance.
        /// </summary>
        public string Name;

        /// <summary>
        /// The param format ID of rows in this param.
        /// </summary>
        public string ID;

        /// <summary>
        /// Automatically determined based on spacing of row offsets; could be wrong in theory, but never seems to be.
        /// </summary>
        public long DetectedSize;

        /// <summary>
        /// If true, use an older DS1-like format found in some DS3 network test params.
        /// </summary>
        public bool FixStrID;

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk1;

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk2;

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Unk3;

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Unk4;

        /// <summary>
        /// The rows of this param; must be loaded with PARAM64.ReadRows() before cells can be used.
        /// </summary>
        public List<Row> Rows;

        private BinaryReaderEx brRows;
        private Layout layout;

        /// <summary>
        /// Creates an uninitialized PARAM64. Should not be used publicly; use PARAM64.Read instead.
        /// </summary>
        public PARAM64() { }

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            layout = null;

            // Make a private copy of the file to read row data from later
            byte[] copy = br.GetBytes(0, (int)br.Stream.Length);
            brRows = new BinaryReaderEx(false, copy);

            int nameOffset = br.ReadInt32();
            FixStrID = br.GetInt32(0xC) != 0;

            br.AssertInt16(0);
            Unk1 = br.ReadInt16();
            Unk2 = br.ReadInt16();
            ushort rowCount = br.ReadUInt16();

            if (FixStrID)
            {
                ID = br.ReadFixStr(0x20);
            }
            else
            {
                br.AssertInt32(0);

                // Maybe long, but doesn't matter
                int idOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                ID = br.GetASCII(idOffset);
            }

            br.AssertByte(0);
            Unk3 = br.AssertByte(0x04, 0x85);
            Unk4 = br.AssertByte(0x06, 0x07);
            br.AssertByte(0);

            if (FixStrID)
            {
                if (Unk4 == 6)
                    Name = br.GetShiftJIS(nameOffset);
                else if (Unk4 == 7)
                    Name = br.GetUTF16(nameOffset);
            }
            else
            {
                Name = br.GetShiftJIS(nameOffset);
            }
            
            long dataStart = br.ReadInt64();
            br.AssertInt32(0);
            br.AssertInt32(0);

            Rows = new List<Row>();
            for (int i = 0; i < rowCount; i++)
                Rows.Add(new Row(br, Unk4));

            if (Rows.Count > 1)
                DetectedSize = Rows[1].Offset - Rows[0].Offset;
            else
                DetectedSize = nameOffset - Rows[0].Offset;
        }

        internal override void Write(BinaryWriterEx bw)
        {
            if (layout == null)
                throw new InvalidOperationException("Params cannot be written without a layout.");

            bw.BigEndian = false;

            bw.ReserveInt32("NameOffset");
            bw.WriteInt16(0);
            bw.WriteInt16(Unk1);
            bw.WriteInt16(Unk2);
            bw.WriteUInt16((ushort)Rows.Count);

            if (FixStrID)
            {
                bw.WriteFixStr(ID, 0x20);
            }
            else
            {
                bw.WriteInt32(0);
                bw.ReserveInt32("IDOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            bw.WriteByte(0);
            bw.WriteByte(Unk3);
            bw.WriteByte(Unk4);
            bw.WriteByte(0);

            bw.ReserveInt64("DataStart");
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteHeader(bw, i);

            bw.FillInt64("DataStart", bw.Position);

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteCells(bw, i, layout);

            bw.FillInt32("NameOffset", (int)bw.Position);
            if (FixStrID)
            {
                if (Unk4 == 6)
                    bw.WriteShiftJIS(Name, true);
                else if (Unk4 == 7)
                    bw.WriteUTF16(Name, true);
            }
            else
            {
                bw.WriteShiftJIS(Name, true);
                bw.FillInt32("IDOffset", (int)bw.Position);
                bw.WriteASCII(ID, true);
            }

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteName(bw, i, Unk4);
        }

        /// <summary>
        /// Sets the layout to use when writing
        /// </summary>
        /// <param name="layout"></param>
        public void SetLayout(Layout layout)
        {
            this.layout = layout;
            foreach (Row row in Rows)
                row.ReadRow(brRows, layout);
        }

        /// <summary>
        /// One row in a param file.
        /// </summary>
        public class Row
        {
            /// <summary>
            /// The ID number of this row.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// A name given to this row; no functional significance, may be null.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Cells contained in this row. Must be loaded with PARAM64.ReadRows() before use.
            /// </summary>
            public List<Cell> Cells { get; private set; }

            internal long Offset;

            /// <summary>
            /// Creates a new row based on the given layout with default values.
            /// </summary>
            public Row(long id, string name, Layout layout)
            {
                ID = id;
                Name = name;
                Cells = new List<Cell>();

                foreach (Layout.Entry entry in layout)
                {
                    object value;
                    string type = entry.Type;

                    if (type == "s8")
                        value = (sbyte)0;
                    else if (type == "u8" || type == "x8")
                        value = (byte)0;
                    else if (type == "s16")
                        value = (short)0;
                    else if (type == "u16" || type == "x16")
                        value = (ushort)0;
                    else if (type == "s32")
                        value = (int)0;
                    else if (type == "u32" || type == "x32")
                        value = (uint)0;
                    else if (type == "f32")
                        value = (float)0;
                    else if (type == "b8" || type == "b32")
                        value = false;
                    else if (type.StartsWith("fixstr"))
                        value = "";
                    else if (type.StartsWith("dummy8"))
                    {
                        int count = int.Parse(type.Substring(7, type.Length - 8));
                        value = new byte[count];
                    }
                    else
                    {
                        throw new NotImplementedException($"Unsupported param layout type: {type}");
                    }

                    Cells.Add(new Cell(type, entry.Name, value));
                }
            }

            internal Row(BinaryReaderEx br, short Unk4)
            {
                ID = br.ReadInt64();
                Offset = br.ReadInt64();
                long nameOffset = br.ReadInt64();

                // Name is always empty in DS3, but not in the network test
                if (nameOffset == 0 || br.GetByte(nameOffset) == 0)
                    Name = null;
                else
                {
                    if (Unk4 == 6)
                        Name = br.GetShiftJIS(nameOffset);
                    else if (Unk4 == 7)
                        Name = br.GetUTF16(nameOffset);
                }

                Cells = null;
            }

            internal void ReadRow(BinaryReaderEx br, Layout layout)
            {
                br.StepIn(Offset);
                Cells = new List<Cell>();

                for (int i = 0; i < layout.Count; i++)
                {
                    string type = layout[i].Type;
                    string name = layout[i].Name;

                    object value = null;

                    if (type == "s8")
                        value = br.ReadSByte();
                    else if (type == "u8" || type == "x8")
                        value = br.ReadByte();
                    else if (type == "s16")
                        value = br.ReadInt16();
                    else if (type == "u16" || type == "x16")
                        value = br.ReadUInt16();
                    else if (type == "s32")
                        value = br.ReadInt32();
                    else if (type == "u32" || type == "x32")
                        value = br.ReadUInt32();
                    else if (type == "f32")
                        value = br.ReadSingle();
                    else if (type.StartsWith("dummy8["))
                    {
                        int count = int.Parse(type.Substring(7, type.Length - 8));
                        value = br.ReadBytes(count);
                    }
                    else if (type.StartsWith("fixstr["))
                    {
                        int count = int.Parse(type.Substring(7, type.Length - 8));
                        value = br.ReadFixStr(count);
                    }
                    else if (type.StartsWith("fixstrW["))
                    {
                        int count = int.Parse(type.Substring(8, type.Length - 9));
                        value = br.ReadFixStrW(count);
                    }
                    else if (type == "b8")
                    {
                        byte b = br.ReadByte();
                        int j;
                        for (j = 0; j < 8; j++)
                        {
                            if (i + j >= layout.Count || layout[i + j].Type != "b8")
                                break;

                            byte mask = (byte)(1 << j);
                            Cells.Add(new Cell("b8", layout[i + j].Name, (b & mask) != 0));
                        }
                        i += j - 1;
                    }
                    else if (type == "b32")
                    {
                        byte[] b = br.ReadBytes(4);
                        int j;
                        for (j = 0; j < 32; j++)
                        {
                            if (i + j >= layout.Count || layout[i + j].Type != "b32")
                                break;

                            byte mask = (byte)(1 << (j % 8));
                            Cells.Add(new Cell("b32", layout[i + j].Name, (b[j / 8] & mask) != 0));
                        }
                        i += j - 1;
                    }
                    else
                        throw new NotImplementedException($"Unsupported param layout type: {type}");

                    if (value != null)
                        Cells.Add(new Cell(type, name, value));
                }

                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i)
            {
                bw.WriteInt64(ID);
                bw.ReserveInt64($"RowOffset{i}");
                bw.ReserveInt64($"NameOffset{i}");
            }

            internal void WriteCells(BinaryWriterEx bw, int i, Layout layout)
            {
                bw.FillInt64($"RowOffset{i}", bw.Position);
                for (int j = 0; j < layout.Count; j++)
                {
                    Cell cell = Cells[j];
                    Layout.Entry entry = layout[j];
                    string type = entry.Type;
                    object value = cell.Value;

                    if (entry.Name != cell.Name || type != cell.Type)
                        throw new FormatException("Layout does not match cells.");

                    if (type == "s8")
                        bw.WriteSByte((sbyte)value);
                    else if (type == "u8" || type == "x8")
                        bw.WriteByte((byte)value);
                    else if (type == "s16")
                        bw.WriteInt16((short)value);
                    else if (type == "u16" || type == "x16")
                        bw.WriteUInt16((ushort)value);
                    else if (type == "s32")
                        bw.WriteInt32((int)value);
                    else if (type == "u32" || type == "x32")
                        bw.WriteUInt32((uint)value);
                    else if (type == "f32")
                        bw.WriteSingle((float)value);
                    else if (type.StartsWith("dummy8["))
                        bw.WriteBytes((byte[])value);
                    else if (type.StartsWith("fixstr["))
                    {
                        int count = int.Parse(type.Substring(7, type.Length - 8));
                        bw.WriteFixStr((string)value, count);
                    }
                    else if (type.StartsWith("fixstrW["))
                    {
                        int count = int.Parse(type.Substring(8, type.Length - 9));
                        bw.WriteFixStrW((string)value, count);
                    }
                    else if (type == "b8")
                    {
                        byte b = 0;
                        int k;
                        for (k = 0; k < 8; k++)
                        {
                            if (j + k >= layout.Count || layout[j + k].Type != "b8")
                                break;

                            if ((bool)Cells[j + k].Value)
                                b |= (byte)(1 << k);
                        }
                        j += k - 1;
                        bw.WriteByte(b);
                    }
                    else if (type == "b32")
                    {
                        byte[] b = new byte[4];
                        int k;
                        for (k = 0; k < 32; k++)
                        {
                            if (j + k >= layout.Count || layout[j + k].Type != "b32")
                                break;

                            if ((bool)Cells[j + k].Value)
                                b[k / 8] |= (byte)(1 << (k % 8));
                        }
                        j += k - 1;
                        bw.WriteBytes(b);
                    }
                }
            }

            internal void WriteName(BinaryWriterEx bw, int i, byte Unk4)
            {
                if (Name == null || Name == "")
                {
                    bw.FillInt64($"NameOffset{i}", 0);
                }
                else
                {
                    bw.FillInt64($"NameOffset{i}", bw.Position);
                    if (Unk4 == 6)
                        bw.WriteShiftJIS(Name, true);
                    else if (Unk4 == 7)
                        bw.WriteUTF16(Name, true);
                }
            }

            /// <summary>
            /// Returns the first cell in the row with the given name.
            /// </summary>
            public Cell this[string name]
            {
                get
                {
                    foreach (Cell cell in Cells)
                    {
                        if (cell.Name == name)
                            return cell;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// One cell in one row in a param.
        /// </summary>
        public class Cell
        {
            /// <summary>
            /// The type of value stored in this cell.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// A name given to this cell based on the param layout; no functional significance.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The value of this cell.
            /// </summary>
            public object Value { get; set; }

            internal Cell(string type, string name, object value)
            {
                Type = type;
                Name = name;
                Value = value;
            }
        }

        /// <summary>
        /// The layout of cell data within each row in a param.
        /// </summary>
        public class Layout : List<Layout.Entry>
        {
            /// <summary>
            /// The size of the row, determined automatically from the layout.
            /// </summary>
            public int Size { get; private set; }

            /// <summary>
            /// Creates a new layout based on a simple text format.
            /// </summary>
            public Layout(string layout) : base()
            {
                foreach (string line in Regex.Split(layout, "[\r\n]+"))
                {
                    if (line.Trim().Length > 0)
                    {
                        Match match = Regex.Match(line.Trim(), @"^(\S+)\s*(.*)$");
                        string type = match.Groups[1].Value;
                        string name = match.Groups[2].Value;
                        Add(new Entry(type, name));
                    }
                }

                Size = 0;

                for (int i = 0; i < Count; i++)
                {
                    string type = this[i].Type;

                    if (type == "s8" || type == "u8" || type == "x8")
                        Size += 1;
                    else if (type == "s16" || type == "u16" || type == "x16")
                        Size += 2;
                    else if (type == "s32" || type == "u32" || type == "x32" || type == "f32")
                        Size += 4;
                    else if (type.StartsWith("fixstr") || type.StartsWith("dummy8"))
                    {
                        Match match = Regex.Match(type, @"^.+\[(\d+)\]");
                        Size += int.Parse(match.Groups[1].Value);
                    }
                    else if (type.StartsWith("b8"))
                    {
                        Size += 1;

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
                        Size += 4;

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
                        throw new NotImplementedException($"Unsupported param layout type: {type}");
                    }
                }
            }

            /// <summary>
            /// The type and name of one cell in a row.
            /// </summary>
            public class Entry
            {
                /// <summary>
                /// The type of the cell.
                /// </summary>
                public string Type;

                /// <summary>
                /// The name of the cell.
                /// </summary>
                public string Name;

                internal Entry(string type, string name)
                {
                    Type = type;
                    Name = name;
                }
            }
        }
    }
}
