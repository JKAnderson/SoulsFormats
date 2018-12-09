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
        /// Indicates file format; 0 - DeS, 1 - DS1/DS2, 2 - DS3/BB.
        /// </summary>
        public byte Version;

        /// <summary>
        /// FMG file endianness. (Big = true)
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// Unknown; 0xFF in version 0, 0x00 in version 1/2.
        /// </summary>
        public byte Unk09;

        /// <summary>
        /// Creates an empty FMG configured for DS1/DS2.
        /// </summary>
        public FMG()
        {
            Entries = new List<Entry>();
            Version = 1;
            BigEndian = false;
            Unk09 = 0;
        }

        /// <summary>
        /// Creates an empty FMG configured for the specified version.
        /// </summary>
        public FMG(byte version)
        {
            Entries = new List<Entry>();
            Version = version;
            BigEndian = Version == 0;
            Unk09 = (byte)(Version == 0 ? 0xFF : 0);
        }

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.AssertByte(0);
            BigEndian = br.ReadBoolean();
            Version = br.AssertByte(0, 1, 2);
            br.AssertByte(0);

            br.BigEndian = BigEndian;
            bool wide = Version == 2;

            int fileSize = br.ReadInt32();
            br.AssertByte(1);
            Unk09 = br.ReadByte();
            br.AssertByte(0);
            br.AssertByte(0);
            int groupCount = br.ReadInt32();
            int stringCount = br.ReadInt32();

            if (wide)
                br.AssertInt32(0xFF);

            long stringOffsetsOffset;
            if (wide)
                stringOffsetsOffset = br.ReadInt64();
            else
                stringOffsetsOffset = br.ReadInt32();

            if (wide)
                br.AssertInt64(0);
            else
                br.AssertInt32(0);

            Entries = new List<Entry>(groupCount);
            for (int i = 0; i < groupCount; i++)
            {
                int offsetIndex = br.ReadInt32();
                int firstID = br.ReadInt32();
                int lastID = br.ReadInt32();

                if (wide)
                    br.AssertInt32(0);

                br.StepIn(stringOffsetsOffset + offsetIndex * (wide ? 8 : 4));
                {
                    for (int j = 0; j < lastID - firstID + 1; j++)
                    {
                        long stringOffset;
                        if (wide)
                            stringOffset = br.ReadInt64();
                        else
                            stringOffset = br.ReadInt32();

                        int id = firstID + j;
                        string text = stringOffset != 0 ? br.GetUTF16(stringOffset) : null;
                        Entries.Add(new Entry(id, text));
                    }
                }
                br.StepOut();
            }
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bool wide = Version == 2;

            bw.WriteByte(0);
            bw.WriteBoolean(bw.BigEndian);
            bw.WriteByte(Version);
            bw.WriteByte(0);

            bw.ReserveInt32("FileSize");
            bw.WriteByte(1);
            bw.WriteByte(Unk09);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.ReserveInt32("GroupCount");
            bw.WriteInt32(Entries.Count);

            if (wide)
                bw.WriteInt32(0xFF);

            if (wide)
                bw.ReserveInt64("StringOffsets");
            else
                bw.ReserveInt32("StringOffsets");

            if (wide)
                bw.WriteInt64(0);
            else
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

                if (wide)
                    bw.WriteInt32(0);

                groupCount++;
            }
            bw.FillInt32("GroupCount", groupCount);

            if (wide)
                bw.FillInt64("StringOffsets", bw.Position);
            else
                bw.FillInt32("StringOffsets", (int)bw.Position);

            for (int i = 0; i < Entries.Count; i++)
            {
                if (wide)
                    bw.ReserveInt64($"StringOffset{i}");
                else
                    bw.ReserveInt32($"StringOffset{i}");
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                string text = Entries[i].Text;

                if (wide)
                    bw.FillInt64($"StringOffset{i}", text == null ? 0 : bw.Position);
                else
                    bw.FillInt32($"StringOffset{i}", text == null ? 0 : (int)bw.Position);

                if (text != null)
                    bw.WriteUTF16(Entries[i].Text, true);
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Returns the string with the given ID, or null if not present.
        /// </summary>
        public string this[int id] => Entries.Find(entry => entry.ID == id)?.Text;

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
