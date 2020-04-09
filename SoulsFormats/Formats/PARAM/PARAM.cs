using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose configuration file used throughout the series.
    /// </summary>
    public partial class PARAM : SoulsFile<PARAM>
    {
        /// <summary>
        /// Whether the file is big-endian; true for PS3/360 files, false otherwise.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// An unknown format or version indicator.
        /// </summary>
        public byte Format2D { get; set; }

        /// <summary>
        /// An unknown format or version indicator.
        /// </summary>
        public byte Format2E { get; set; }

        /// <summary>
        /// An unknown format or version indicator; usually 0x00, 0xFF in DS2 NT.
        /// </summary>
        public byte Format2F { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk06 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk08 { get; set; }

        /// <summary>
        /// Identifies corresponding params and paramdefs.
        /// </summary>
        public string ParamType { get; set; }

        /// <summary>
        /// Automatically determined based on spacing of row offsets; could be wrong in theory, but never seems to be.
        /// </summary>
        public long DetectedSize { get; private set; }

        /// <summary>
        /// The rows of this param; must be loaded with PARAM.ApplyParamdef() before cells can be used.
        /// </summary>
        public List<Row> Rows { get; set; }

        /// <summary>
        /// The current applied PARAMDEF.
        /// </summary>
        public PARAMDEF AppliedParamdef { get; private set; }

        private BinaryReaderEx RowReader;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.Position = 0x2C;
            BigEndian = br.AssertByte(0, 0xFF) == 0xFF;
            Format2D = br.ReadByte();
            Format2E = br.ReadByte();
            Format2F = br.AssertByte(0, 0xFF);
            br.Position = 0;
            br.BigEndian = BigEndian;

            // Make a private copy of the file to read row data from later
            byte[] copy = br.GetBytes(0, (int)br.Stream.Length);
            RowReader = new BinaryReaderEx(BigEndian, copy);

            ushort rowCount;
            long stringsOffset;

            // DeS, DS1
            if ((Format2D & 0x7F) < 3)
            {
                stringsOffset = br.ReadUInt32();
                br.ReadUInt16(); // Data start
                Unk06 = br.ReadInt16();
                Unk08 = br.ReadInt16();
                rowCount = br.ReadUInt16();
                ParamType = br.ReadFixStr(0x20);
                br.Skip(4); // Format
            }
            // DS2
            else if ((Format2D & 0x7F) == 3)
            {
                stringsOffset = br.ReadUInt32();
                br.AssertInt16(0);
                Unk06 = br.ReadInt16();
                Unk08 = br.ReadInt16();
                rowCount = br.ReadUInt16();
                ParamType = br.ReadFixStr(0x20);
                br.Skip(4); // Format
                br.ReadUInt32(); // Data start
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
            // SotFS, BB
            else if ((Format2D & 0x7F) == 4)
            {
                stringsOffset = br.ReadUInt32();
                br.AssertInt16(0);
                Unk06 = br.ReadInt16();
                Unk08 = br.ReadInt16();
                rowCount = br.ReadUInt16();
                ParamType = br.ReadFixStr(0x20);
                br.Skip(4); // Format
                br.ReadInt64(); // Data start
                br.AssertInt64(0);
            }
            // DS3, SDT
            else
            {
                stringsOffset = br.ReadUInt32();
                br.AssertInt16(0);
                Unk06 = br.ReadInt16();
                Unk08 = br.ReadInt16();
                rowCount = br.ReadUInt16();
                br.AssertInt32(0);
                long idOffset = br.ReadInt64();
                br.AssertPattern(0x14, 0x00);
                br.Skip(4); // Format
                br.ReadInt64(); // Data start
                br.AssertInt64(0);
                ParamType = br.GetASCII(idOffset);

                // This is stupid, but the strings offset is always aligned to 0x10,
                // which can put it right in the middle of the ID string
                stringsOffset = idOffset;
            }

            Rows = new List<Row>(rowCount);
            for (int i = 0; i < rowCount; i++)
                Rows.Add(new Row(br, Format2D, Format2E));

            if (Rows.Count > 1)
                DetectedSize = Rows[1].DataOffset - Rows[0].DataOffset;
            else
                DetectedSize = stringsOffset - Rows[0].DataOffset;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            if (AppliedParamdef == null)
                throw new InvalidOperationException("Params cannot be written without applying a paramdef.");

            bw.BigEndian = BigEndian;
            void WriteFormat()
            {
                bw.WriteByte((byte)(BigEndian ? 0xFF : 0x00));
                bw.WriteByte(Format2D);
                bw.WriteByte(Format2E);
                bw.WriteByte(Format2F);
            }

            // DeS, DS1
            if ((Format2D & 0x7F) < 3)
            {
                bw.ReserveUInt32("StringsOffset");
                bw.ReserveUInt16("DataStart");
                bw.WriteInt16(Unk06);
                bw.WriteInt16(Unk08);
                bw.WriteUInt16((ushort)Rows.Count);
                bw.WriteFixStr(ParamType, 0x20, (byte)((Format2D & 0x7F) < 2 ? 0x20 : 0x00));
                WriteFormat();
            }
            // DS2
            else if ((Format2D & 0x7F) == 3)
            {
                bw.ReserveUInt32("StringsOffset");
                bw.WriteInt16(0);
                bw.WriteInt16(Unk06);
                bw.WriteInt16(Unk08);
                bw.WriteUInt16((ushort)Rows.Count);
                bw.WriteFixStr(ParamType, 0x20, 0x20);
                WriteFormat();
                bw.ReserveUInt32("DataStart");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
            // SotFS, BB
            else if ((Format2D & 0x7F) == 4)
            {
                bw.ReserveUInt32("StringsOffset");
                bw.WriteInt16(0);
                bw.WriteInt16(Unk06);
                bw.WriteInt16(Unk08);
                bw.WriteUInt16((ushort)Rows.Count);
                bw.WriteFixStr(ParamType, 0x20, 0x00);
                WriteFormat();
                bw.ReserveInt64("DataStart");
                bw.WriteInt64(0);
            }
            // DS3, SDT
            else
            {
                bw.ReserveUInt32("StringsOffset");
                bw.WriteInt16(0);
                bw.WriteInt16(Unk06);
                bw.WriteInt16(Unk08);
                bw.WriteUInt16((ushort)Rows.Count);
                bw.WriteInt32(0);
                bw.ReserveInt64("IDOffset");
                bw.WritePattern(0x14, 0x00);
                WriteFormat();
                bw.ReserveInt64("DataStart");
                bw.WriteInt64(0);
            }

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteHeader(bw, Format2D, i);

            if ((Format2D & 0x7F) < 2)
                bw.WritePattern(0x20, 0x00);
            if ((Format2D & 0x7F) < 3)
                bw.FillUInt16("DataStart", (ushort)bw.Position);
            else if ((Format2D & 0x7F) == 3)
                bw.FillUInt32("DataStart", (uint)bw.Position);
            else
                bw.FillInt64("DataStart", bw.Position);

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteCells(bw, Format2D, i);

            bw.FillUInt32("StringsOffset", (uint)bw.Position);

            if ((Format2D & 0x7F) > 4)
            {
                bw.FillInt64("IDOffset", bw.Position);
                bw.WriteASCII(ParamType, true);
            }

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteName(bw, Format2D, Format2E, i);
            // DeS and BB sometimes (but not always) include some useless padding here
        }

        /// <summary>
        /// Interprets row data according to the given paramdef and stores it for later writing.
        /// </summary>
        public void ApplyParamdef(PARAMDEF paramdef)
        {
            AppliedParamdef = paramdef;
            foreach (Row row in Rows)
                row.ReadCells(RowReader, AppliedParamdef);
        }

        /// <summary>
        /// Returns the first row with the given ID, or null if not found.
        /// </summary>
        public Row this[int id] => Rows.Find(row => row.ID == id);
    }
}
