using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A simple string container used throughout the series.
    /// </summary>
    public class FMG : SoulsFile<FMG>
    {
        /// <summary>
        /// The strings contained in this FMG.
        /// </summary>
        public List<Entry> Entries;

        /// <summary>
        /// If true, use DS3 format with 64-bit string offsets.
        /// </summary>
        public bool Long;

        /// <summary>
        /// Creates an uninitialized FMG. Should not be used publicly; use FMG.Read instead.
        /// </summary>
        public FMG() { }

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertByte(0);
            br.AssertByte(0);
            Long = br.AssertByte(1, 2) == 2;
            br.AssertByte(0);

            int fileSize = br.ReadInt32();
            br.AssertInt32(1);
            int groupCount = br.ReadInt32();
            int stringCount = br.ReadInt32();

            if (Long)
                br.AssertInt32(0xFF);

            long stringOffsetsOffset;
            if (Long)
                stringOffsetsOffset = br.ReadInt64();
            else
                stringOffsetsOffset = br.ReadInt32();

            br.AssertInt32(0);
            br.AssertInt32(0);

            Entries = new List<Entry>();
            for (int i = 0; i < groupCount; i++)
            {
                int offsetIndex = br.ReadInt32();
                int firstID = br.ReadInt32();
                int lastID = br.ReadInt32();

                if (Long)
                    br.AssertInt32(0);

                br.StepIn(stringOffsetsOffset + offsetIndex * (Long ? 8 : 4));
                for (int j = 0; j < lastID - firstID + 1; j++)
                {
                    long stringOffset;
                    if (Long)
                        stringOffset = br.ReadInt64();
                    else
                        stringOffset = br.ReadInt32();

                    int id = firstID + j;
                    string text = stringOffset != 0 ? br.GetUTF16(stringOffset) : null;
                    Entries.Add(new Entry(id, text));
                }
                br.StepOut();
            }
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte((byte)(Long ? 2 : 1));
            bw.WriteByte(0);

            bw.ReserveInt32("FileSize");
            bw.WriteInt32(1);
            bw.ReserveInt32("GroupCount");
            bw.WriteInt32(Entries.Count);

            if (Long)
                bw.WriteInt32(0xFF);

            if (Long)
                bw.ReserveInt64("StringOffsets");
            else
                bw.ReserveInt32("StringOffsets");

            bw.WriteInt32(0);
            bw.WriteInt32(0);

            int groupCount = 0;
            Entries.Sort((e1, e2) => e1.ID.CompareTo(e2.ID));
            for (int i = 0; i < Entries.Count; i++)
            {
                bw.WriteInt32(i);
                bw.WriteInt32(Entries[i].ID);
                while (i < Entries.Count - 1 && Entries[i + 1].ID == Entries[i].ID + 1)
                    i++;
                bw.WriteInt32(Entries[i].ID);

                if (Long)
                    bw.WriteInt32(0);

                groupCount++;
            }
            bw.FillInt32("GroupCount", groupCount);

            if (Long)
                bw.FillInt64("StringOffsets", bw.Position);
            else
                bw.FillInt32("StringOffsets", (int)bw.Position);

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Long)
                    bw.ReserveInt64($"StringOffset{i}");
                else
                    bw.ReserveInt32($"StringOffset{i}");
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                string text = Entries[i].Text;

                if (Long)
                    bw.FillInt64($"StringOffset{i}", text == null ? 0 : bw.Position);
                else
                    bw.FillInt64($"StringOffset{i}", text == null ? 0 : (int)bw.Position);

                if (text != null)
                    bw.WriteUTF16(Entries[i].Text, true);
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// A string in an FMG identified with an ID number.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// The ID of this entry.
            /// </summary>
            public int ID;

            /// <summary>
            /// The text of this entry.
            /// </summary>
            public string Text;

            /// <summary>
            /// Creates a new entry with the specified ID and text.
            /// </summary>
            public Entry(int id, string text)
            {
                ID = id;
                Text = text;
            }

            /// <summary>
            /// Returns the ID and text of this entry.
            /// </summary>
            public override string ToString()
            {
                return $"{ID}: {Text ?? "<null>"}";
            }
        }
    }
}
