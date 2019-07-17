using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public class ACB : SoulsFile<ACB>
    {
        public List<Entry> Entries { get; set; }

        internal override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            return br.GetASCII(0, 4) == "ACB\0";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("ACB\0");
            br.AssertInt32(0x00000102);
            int entryCount = br.ReadInt32();
            br.ReadInt32(); // Offset index offset

            Entries = new List<Entry>(entryCount);
            foreach (int entryOffset in br.ReadInt32s(entryCount))
            {
                br.Position = entryOffset;
                EntryType type = br.GetEnum16<EntryType>(br.Position + 8);
                if (type == EntryType.General)
                    Entries.Add(new Entry.General(br));
                else if (type == EntryType.Model)
                    Entries.Add(new Entry.Model(br));
                else if (type == EntryType.Texture)
                    Entries.Add(new Entry.Texture(br));
                else if (type == EntryType.GITexture)
                    Entries.Add(new Entry.GITexture(br));
                else
                    throw new NotImplementedException($"Unsupported entry type: {type}");
            }
        }

        internal override void Write(BinaryWriterEx bw)
        {
            var offsetIndex = new List<int>();
            var memberOffsetsIndex = new SortedDictionary<int, List<int>>();

            bw.BigEndian = false;
            bw.WriteASCII("ACB\0");
            bw.WriteInt32(0x00000102);
            bw.WriteInt32(Entries.Count);
            bw.ReserveInt32("OffsetIndexOffset");

            for (int i = 0; i < Entries.Count; i++)
            {
                offsetIndex.Add((int)bw.Position);
                bw.ReserveInt32($"EntryOffset{i}");
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                bw.FillInt32($"EntryOffset{i}", (int)bw.Position);
                Entries[i].Write(bw, i, offsetIndex, memberOffsetsIndex);
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] is Entry.Model model)
                {
                    model.WriteMembers(bw, i, offsetIndex, memberOffsetsIndex);
                }
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].WritePaths(bw, i);
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] is Entry.Model model && model.Members != null)
                {
                    for (int j = 0; j < model.Members.Count; j++)
                    {
                        model.Members[j].WriteText(bw, i, j);
                    }
                }
            }

            bw.Pad(4);
            bw.FillInt32("OffsetIndexOffset", (int)bw.Position);
            bw.WriteInt32s(offsetIndex);
            foreach (List<int> offsets in memberOffsetsIndex.Values)
                bw.WriteInt32s(offsets);
        }

        public enum EntryType : ushort
        {
            General = 1,
            Model = 2,
            Texture = 3,
            GITexture = 4,
        }

        public abstract class Entry
        {
            public abstract EntryType Type { get; }

            public string AbsolutePath { get; set; }

            public string RelativePath { get; set; }

            internal Entry()
            {
                AbsolutePath = "";
                RelativePath = "";
            }

            internal Entry(BinaryReaderEx br)
            {
                int absolutePathOffset = br.ReadInt32();
                int relativePathOffset = br.ReadInt32();
                br.AssertUInt16((ushort)Type);

                AbsolutePath = br.GetUTF16(absolutePathOffset);
                RelativePath = br.GetUTF16(relativePathOffset);
            }

            internal virtual void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex)
            {
                offsetIndex.Add((int)bw.Position);
                bw.ReserveInt32($"AbsolutePathOffset{index}");
                offsetIndex.Add((int)bw.Position);
                bw.ReserveInt32($"RelativePathOffset{index}");
                bw.WriteUInt16((ushort)Type);
            }

            internal void WritePaths(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"AbsolutePathOffset{index}", (int)bw.Position);
                bw.WriteUTF16(AbsolutePath, true);

                bw.FillInt32($"RelativePathOffset{index}", (int)bw.Position);
                bw.WriteUTF16(RelativePath, true);
            }

            public class General : Entry
            {
                public override EntryType Type => EntryType.General;

                public General() : base() { }

                internal General(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex)
                {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            public class Model : Entry
            {
                public override EntryType Type => EntryType.Model;

                public short Unk0A { get; set; }

                public List<Member> Members { get; set; }

                public int Unk10 { get; set; }

                public int Unk1C { get; set; }

                public int Unk20 { get; set; }

                public int Unk24 { get; set; }

                public float Unk28 { get; set; }

                public int Unk2C { get; set; }

                public int Unk30 { get; set; }

                public int Unk34 { get; set; }

                public Model() : base() { }

                internal Model(BinaryReaderEx br) : base(br)
                {
                    Unk0A = br.ReadInt16();
                    int membersOffset = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadSingle();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    br.AssertPattern(0x18, 0x00);

                    if (membersOffset != 0)
                    {
                        br.Position = membersOffset;
                        br.AssertInt16(-1);
                        short memberCount = br.ReadInt16();
                        int memberOffsetsOffset = br.ReadInt32();

                        br.Position = memberOffsetsOffset;
                        Members = new List<Member>(memberCount);
                        foreach (int memberOffset in br.ReadInt32s(memberCount))
                        {
                            br.Position = memberOffset;
                            Members.Add(new Member(br));
                        }
                    }
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex)
                {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(Unk0A);
                    membersOffsetIndex[index] = new List<int>();
                    membersOffsetIndex[index].Add((int)bw.Position);
                    bw.ReserveInt32($"MembersOffset{index}");
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteSingle(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(Unk34);
                    bw.WritePattern(0x18, 0x00);
                }

                internal void WriteMembers(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex)
                {
                    if (Members == null)
                    {
                        bw.FillInt32($"MembersOffset{index}", 0);
                    }
                    else
                    {
                        bw.FillInt32($"MembersOffset{index}", (int)bw.Position);
                        bw.WriteInt16(-1);
                        bw.WriteInt16((short)Members.Count);
                        membersOffsetIndex[index].Add((int)bw.Position);
                        bw.ReserveInt32($"MemberOffsetsOffset{index}");

                        // :^)
                        bw.FillInt32($"MemberOffsetsOffset{index}", (int)bw.Position);
                        for (int i = 0; i < Members.Count; i++)
                        {
                            membersOffsetIndex[index].Add((int)bw.Position);
                            bw.ReserveInt32($"MemberOffset{index}:{i}");
                        }

                        for (int i = 0; i < Members.Count; i++)
                        {
                            bw.FillInt32($"MemberOffset{index}:{i}", (int)bw.Position);
                            Members[i].Write(bw, index, i, offsetIndex);
                        }
                    }
                }

                public class Member
                {
                    public string Text { get; set; }

                    public int Unk04 { get; set; }

                    public Member()
                    {
                        Text = "";
                    }

                    internal Member(BinaryReaderEx br)
                    {
                        int textOffset = br.ReadInt32();
                        Unk04 = br.ReadInt32();

                        Text = br.GetUTF16(textOffset);
                    }

                    internal void Write(BinaryWriterEx bw, int entryIndex, int memberIndex, List<int> offsetIndex)
                    {
                        offsetIndex.Add((int)bw.Position);
                        bw.ReserveInt32($"MemberTextOffset{entryIndex}:{memberIndex}");
                        bw.WriteInt32(Unk04);
                    }

                    internal void WriteText(BinaryWriterEx bw, int entryIndex, int memberIndex)
                    {
                        bw.FillInt32($"MemberTextOffset{entryIndex}:{memberIndex}", (int)bw.Position);
                        bw.WriteUTF16(Text, true);
                    }
                }
            }

            public class Texture : Entry
            {
                public override EntryType Type => EntryType.Texture;

                public Texture() : base() { }

                internal Texture(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex)
                {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            public class GITexture : Entry
            {
                public override EntryType Type => EntryType.GITexture;

                public int Unk10 { get; set; }

                public int Unk14 { get; set; }

                public GITexture() : base() { }

                internal GITexture(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex)
                {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                }
            }
        }
    }
}
