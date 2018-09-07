using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoulsFormats
{
    class PARAM : SoulsFile<PARAM>
    {
        public string Format;

        public List<Row> Rows;

        private BinaryReaderEx br;
        private List<RowHeader> rowHeaders;

        public PARAM() { }

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            int nameOffset = br.ReadInt32();
            br.AssertInt16(0);
            short unk1 = br.ReadInt16();
            short unk2 = br.ReadInt16();
            ushort rowCount = br.ReadUInt16();
            br.AssertInt32(0);
            br.AssertInt32(nameOffset);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0x00078500);
            int dataStart = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            
            rowHeaders = new List<RowHeader>();
            for (int i = 0; i < rowCount; i++)
                rowHeaders.Add(new RowHeader(br));

            Format = br.GetASCII(nameOffset);
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        public List<Row> ReadRows(Layout layout)
        {
            var rows = new List<Row>();
            foreach (RowHeader header in rowHeaders)
                rows.Add(new Row(br, header.ID, header.Offset, layout));
            return rows;
        }

        private class RowHeader
        {
            public long ID;
            public string Name;
            public long Offset;

            internal RowHeader(BinaryReaderEx br)
            {
                ID = br.ReadInt64();
                Offset = br.ReadInt64();
                long nameOffset = br.ReadInt64();
                Name = br.GetShiftJIS(nameOffset);
            }
        }

        public class Row
        {
            public long ID;
            public List<Cell> Cells;

            internal Row(BinaryReaderEx br, long id, long offset, Layout layout)
            {
                ID = id;
                Cells = new List<Cell>();
                br.StepIn(offset);

                foreach (LayoutValue lv in layout)
                {
                    object value;

                    switch (lv.Type)
                    {
                        case "u8":
                            value = br.ReadByte();
                            break;
                        default:
                            throw new NotImplementedException("Unsupported LayoutValue type: " + lv.Type);
                    }

                    Cells.Add(new Cell(lv.Type, lv.Name, value));
                }

                br.StepOut();
            }
        }

        public class Cell
        {
            public string Type;
            public string Name;
            public object Value;

            internal Cell(string type, string name, object value)
            {
                Type = type;
                Name = name;
                Value = value;
            }
        }

        public class Layout : List<LayoutValue>
        {
            public Layout(string layout) : base()
            {
                foreach (string line in Regex.Split(layout, "[\r\n]+"))
                {
                    if (line.Trim().Length > 0)
                    {
                        Match match = Regex.Match(line.Trim(), @"^\S+\s+.+$");
                        Add(new LayoutValue(match.Groups[0].Value, match.Groups[1].Value));
                    }
                }
            }
        }

        public class LayoutValue
        {
            public string Type;
            public string Name;

            internal LayoutValue(string type, string name)
            {
                Type = type;
                Name = name;
            }
        }
    }
}
