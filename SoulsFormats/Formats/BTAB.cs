using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A DS3 and BB file that seems to modify material parameters to light certain static objects. Used to darken objects in shadows for example.
    /// </summary>
    public class BTAB : SoulsFile<BTAB>
    {
        public List<Entry> Entries;

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertInt32(1);
            br.AssertInt32(0);
            int entryCount = br.ReadInt32();
            int nameSize = br.ReadInt32();
            br.AssertInt32(0);
            // Entry size
            br.AssertInt32(0x28);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            long nameStart = br.Position;
            br.Position = nameStart + nameSize;
            Entries = new List<Entry>();
            for (int i = 0; i < entryCount; i++)
                Entries.Add(new Entry(br, nameStart));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.WriteInt32(Entries.Count);
            bw.ReserveInt32("NameSize");
            bw.WriteInt32(0);
            bw.WriteInt32(0x28);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            long nameStart = bw.Position;
            var nameOffsets = new List<int>();
            foreach (Entry entry in Entries)
            {
                int nameOffset = (int)(bw.Position - nameStart);
                nameOffsets.Add(nameOffset);
                bw.WriteUTF16(entry.MSBPartName, true);
                if (nameOffset % 0x10 != 0)
                {
                    for (int i = 0; i < 0x10 - (nameOffset % 0x10); i++)
                        bw.WriteByte(0);
                }

                int nameOffset2 = (int)(bw.Position - nameStart);
                nameOffsets.Add(nameOffset2);
                bw.WriteUTF16(entry.MaterialName, true);
                if (nameOffset2 % 0x10 != 0)
                {
                    for (int i = 0; i < 0x10 - (nameOffset2 % 0x10); i++)
                        bw.WriteByte(0);
                }
            }

            bw.FillInt32("NameSize", (int)(bw.Position - nameStart));
            for (int i = 0; i < Entries.Count; i++)
                Entries[i].Write(bw, nameOffsets[i * 2], nameOffsets[i * 2 + 1]);
        }

        public class Entry
        {
            /// <summary>
            /// The name of the target part defined in the MSB file
            /// </summary>
            public string MSBPartName;

            /// <summary>
            /// Material name?
            /// </summary>
            public string MaterialName;


            public int Unk1C;

            // These floats are used to control the lighting/material parameters in some way.
            // Seem to be between 0.0-1.0 and sum up to 1.0 in some cases
            public float Unk20;
            public float Unk24;
            public float Unk28;
            public float Unk2C;

            internal Entry(BinaryReaderEx br, long nameStart)
            {

                int nameOffset = br.ReadInt32();
                MSBPartName = br.GetUTF16(nameStart + nameOffset);
                br.AssertInt32(0);

                int nameOffset2 = br.ReadInt32();
                MaterialName = br.GetUTF16(nameStart + nameOffset2);
                br.AssertInt32(0);

                Unk1C = br.ReadInt32();
                Unk20 = br.ReadSingle();
                Unk24 = br.ReadSingle();
                Unk28 = br.ReadSingle();
                Unk2C = br.ReadSingle();
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, int nameOffset, int nameOffset2)
            {
                bw.WriteInt32(nameOffset);
                bw.WriteInt32(0);
                bw.WriteInt32(nameOffset2);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk1C);
                bw.WriteSingle(Unk20);
                bw.WriteSingle(Unk24);
                bw.WriteSingle(Unk28);
                bw.WriteSingle(Unk2C);
                bw.WriteInt32(0);
            }

            public override string ToString()
            {
                return $"{MSBPartName} : {MaterialName}";
            }
        }
    }
}
