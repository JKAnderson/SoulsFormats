using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A graphics config file used in BB and DS3.
    /// </summary>
    public class GPARAM : SoulsFile<GPARAM>
    {
        /// <summary>
        /// Groups of params in this file.
        /// </summary>
        public List<Group> Groups;

        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk1;

        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Unk3> Unk3s;

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte[] UnkBlock2;

        /// <summary>
        /// Creates a new empty GPARAM.
        /// </summary>
        public GPARAM()
        {
            Groups = new List<Group>();
            Unk1 = 0;
            Unk3s = new List<Unk3>();
            UnkBlock2 = new byte[0];
        }

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 8);
            return magic == "f\0i\0l\0t\0";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            // Don't @ me.
            br.AssertASCII("f\0i\0l\0t\0");
            br.AssertInt32(3);
            br.AssertInt32(0);
            int groupCount = br.ReadInt32();
            Unk1 = br.ReadInt32();
            // Header size or group header headers offset, you decide
            br.AssertInt32(0x50);

            Offsets offsets;
            offsets.GroupHeaders = br.ReadInt32();
            offsets.ParamHeaderOffsets = br.ReadInt32();
            offsets.ParamHeaders = br.ReadInt32();
            offsets.Values = br.ReadInt32();
            offsets.Unk1 = br.ReadInt32();
            offsets.Unk2 = br.ReadInt32();

            int unk3Count = br.ReadInt32();
            offsets.Unk3 = br.ReadInt32();
            offsets.Unk3Values = br.ReadInt32();
            br.AssertInt32(0);

            offsets.CommentOffsetsOffsets = br.ReadInt32();
            offsets.CommentOffsets = br.ReadInt32();
            offsets.Comments = br.ReadInt32();

            Groups = new List<Group>(groupCount);
            for (int i = 0; i < groupCount; i++)
                Groups.Add(new Group(br, i, offsets));

            UnkBlock2 = br.GetBytes(offsets.Unk2, offsets.Unk3 - offsets.Unk2);

            br.Position = offsets.Unk3;
            Unk3s = new List<Unk3>(unk3Count);
            for (int i = 0; i < unk3Count; i++)
                Unk3s.Add(new Unk3(br, offsets));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteUTF16("filt");
            bw.WriteInt32(3);
            bw.WriteInt32(0);
            bw.WriteInt32(Groups.Count);
            bw.WriteInt32(Unk1);
            bw.WriteInt32(0x50);

            bw.ReserveInt32("GroupHeadersOffset");
            bw.ReserveInt32("ParamHeaderOffsetsOffset");
            bw.ReserveInt32("ParamHeadersOffset");
            bw.ReserveInt32("ValuesOffset");
            bw.ReserveInt32("UnkOffset1");
            bw.ReserveInt32("UnkOffset2");

            bw.WriteInt32(Unk3s.Count);
            bw.ReserveInt32("UnkOffset3");
            bw.ReserveInt32("Unk3ValuesOffset");
            bw.WriteInt32(0);

            bw.ReserveInt32("CommentOffsetsOffsetsOffset");
            bw.ReserveInt32("CommentOffsetsOffset");
            bw.ReserveInt32("CommentsOffset");

            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteHeaderOffset(bw, i);

            int groupHeadersOffset = (int)bw.Position;
            bw.FillInt32("GroupHeadersOffset", groupHeadersOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteHeader(bw, i, groupHeadersOffset);

            int paramHeaderOffsetsOffset = (int)bw.Position;
            bw.FillInt32("ParamHeaderOffsetsOffset", paramHeaderOffsetsOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteParamHeaderOffsets(bw, i, paramHeaderOffsetsOffset);

            int paramHeadersOffset = (int)bw.Position;
            bw.FillInt32("ParamHeadersOffset", paramHeadersOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteParamHeaders(bw, i, paramHeadersOffset);

            int valuesOffset = (int)bw.Position;
            bw.FillInt32("ValuesOffset", valuesOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteValues(bw, i, valuesOffset);

            int unkOffset1 = (int)bw.Position;
            bw.FillInt32("UnkOffset1", (int)bw.Position);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteUnk1(bw, i, unkOffset1);

            bw.FillInt32("UnkOffset2", (int)bw.Position);
            bw.WriteBytes(UnkBlock2);

            bw.FillInt32("UnkOffset3", (int)bw.Position);
            for (int i = 0; i < Unk3s.Count; i++)
                Unk3s[i].WriteHeader(bw, i);

            int unk3ValuesOffset = (int)bw.Position;
            bw.FillInt32("Unk3ValuesOffset", unk3ValuesOffset);
            for (int i = 0; i < Unk3s.Count; i++)
                Unk3s[i].WriteValues(bw, i, unk3ValuesOffset);

            bw.FillInt32("CommentOffsetsOffsetsOffset", (int)bw.Position);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteCommentOffsetsOffset(bw, i);

            int commentOffsetsOffset = (int)bw.Position;
            bw.FillInt32("CommentOffsetsOffset", commentOffsetsOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteCommentOffsets(bw, i, commentOffsetsOffset);

            int commentsOffset = (int)bw.Position;
            bw.FillInt32("CommentsOffset", commentsOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteComments(bw, i, commentsOffset);
        }

        /// <summary>
        /// Returns the first group with a matching name, or null if not found.
        /// </summary>
        public Group this[string name1] => Groups.Find(group => group.Name1 == name1);

        internal struct Offsets
        {
            public int GroupHeaders;
            public int ParamHeaderOffsets;
            public int ParamHeaders;
            public int Values;
            public int Unk1;
            public int Unk2;
            public int Unk3;
            public int Unk3Values;
            public int CommentOffsetsOffsets;
            public int CommentOffsets;
            public int Comments;
        }

        /// <summary>
        /// A group of graphics params.
        /// </summary>
        public class Group
        {
            /// <summary>
            /// Identifies the group.
            /// </summary>
            public string Name1;

            /// <summary>
            /// Identifies the group, but shorter?
            /// </summary>
            public string Name2;

            /// <summary>
            /// Params in this group.
            /// </summary>
            public List<Param> Params;

            /// <summary>
            /// Comments indicating the purpose of each entry in param values.
            /// </summary>
            public List<string> Comments;

            /// <summary>
            /// Creates a new Group with no params or comments.
            /// </summary>
            public Group(string name1, string name2)
            {
                Name1 = name1;
                Name2 = name2;
                Params = new List<Param>();
                Comments = new List<string>();
            }

            internal Group(BinaryReaderEx br, int index, Offsets offsets)
            {
                int groupHeaderOffset = br.ReadInt32();
                br.StepIn(offsets.GroupHeaders + groupHeaderOffset);

                int paramCount = br.ReadInt32();
                int paramHeaderOffsetsOffset = br.ReadInt32();
                Name1 = br.ReadUTF16();
                Name2 = br.ReadUTF16();

                br.StepIn(offsets.ParamHeaderOffsets + paramHeaderOffsetsOffset);
                {
                    Params = new List<Param>(paramCount);
                    for (int i = 0; i < paramCount; i++)
                        Params.Add(new Param(br, offsets));
                }
                br.StepOut();

                if (Params.Count > 0)
                {
                    int commentCount = Params[0].Values.Count;
                    Comments = new List<string>(commentCount);
                    int commentOffsetsOffset = br.GetInt32(offsets.CommentOffsetsOffsets + index * 4);
                    br.StepIn(offsets.CommentOffsets + commentOffsetsOffset);
                    {
                        for (int i = 0; i < commentCount; i++)
                        {
                            int commentOffset = br.ReadInt32();
                            Comments.Add(br.GetUTF16(offsets.Comments + commentOffset));
                        }
                    }
                    br.StepOut();
                }
                else
                {
                    Comments = new List<string>();
                }

                br.StepOut();
            }

            internal void WriteHeaderOffset(BinaryWriterEx bw, int groupIndex)
            {
                bw.ReserveInt32($"GroupHeaderOffset{groupIndex}");
            }

            internal void WriteHeader(BinaryWriterEx bw, int groupIndex, int groupHeadersOffset)
            {
                bw.FillInt32($"GroupHeaderOffset{groupIndex}", (int)bw.Position - groupHeadersOffset);
                bw.WriteInt32(Params.Count);
                bw.ReserveInt32($"ParamHeaderOffsetsOffset{groupIndex}");
                bw.WriteUTF16(Name1, true);
                bw.WriteUTF16(Name2, true);
                bw.Pad(4);
            }

            internal void WriteParamHeaderOffsets(BinaryWriterEx bw, int groupIndex, int paramHeaderOffsetsOffset)
            {
                bw.FillInt32($"ParamHeaderOffsetsOffset{groupIndex}", (int)bw.Position - paramHeaderOffsetsOffset);
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteParamHeaderOffset(bw, groupIndex, i);
            }

            internal void WriteParamHeaders(BinaryWriterEx bw, int groupindex, int paramHeadersOffset)
            {
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteParamHeader(bw, groupindex, i, paramHeadersOffset);
            }

            internal void WriteValues(BinaryWriterEx bw, int groupindex, int valuesOffset)
            {
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteValues(bw, groupindex, i, valuesOffset);
            }

            internal void WriteUnk1(BinaryWriterEx bw, int groupIndex, int unkOffset1)
            {
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteUnk1(bw, groupIndex, i, unkOffset1);
            }

            internal void WriteCommentOffsetsOffset(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"CommentOffsetsOffset{index}");
            }

            internal void WriteCommentOffsets(BinaryWriterEx bw, int index, int commentOffsetsOffset)
            {
                bw.FillInt32($"CommentOffsetsOffset{index}", (int)bw.Position - commentOffsetsOffset);
                for (int i = 0; i < Comments.Count; i++)
                    bw.ReserveInt32($"CommentOffset{index}:{i}");
            }

            internal void WriteComments(BinaryWriterEx bw, int index, int commentsOffset)
            {
                for (int i = 0; i < Comments.Count; i++)
                {
                    bw.FillInt32($"CommentOffset{index}:{i}", (int)bw.Position - commentsOffset);
                    bw.WriteUTF16(Comments[i], true);
                }
            }

            /// <summary>
            /// Returns the first param with a matching name, or null if not found.
            /// </summary>
            public Param this[string name1] => Params.Find(param => param.Name1 == name1);

            /// <summary>
            /// Returns the long and short names of the group.
            /// </summary>
            public override string ToString()
            {
                return $"{Name1} | {Name2}";
            }
        }

        /// <summary>
        /// Value types allowed in a param.
        /// </summary>
        public enum ParamType : byte
        {
            /// <summary>
            /// Unknown; only ever appears as a single value.
            /// </summary>
            Byte = 0x1,

            /// <summary>
            /// One short.
            /// </summary>
            Short = 0x2,

            /// <summary>
            /// One int.
            /// </summary>
            IntA = 0x3,

            /// <summary>
            /// One bool.
            /// </summary>
            BoolA = 0x5,

            /// <summary>
            /// One int.
            /// </summary>
            IntB = 0x7,

            /// <summary>
            /// One float.
            /// </summary>
            Float = 0x9,

            /// <summary>
            /// One bool.
            /// </summary>
            BoolB = 0xB,

            /// <summary>
            /// Two floats and 8 unused bytes.
            /// </summary>
            Float2 = 0xC,

            /// <summary>
            /// Four floats.
            /// </summary>
            Float4 = 0xE,

            /// <summary>
            /// Four bytes, used for BGRA.
            /// </summary>
            Byte4 = 0xF,
        }

        /// <summary>
        /// 
        /// </summary>
        public class Param
        {
            /// <summary>
            /// Identifies the param specifically.
            /// </summary>
            public string Name1;

            /// <summary>
            /// Identifies the param generically.
            /// </summary>
            public string Name2;

            /// <summary>
            /// Type of values in this param.
            /// </summary>
            public ParamType Type;

            /// <summary>
            /// Values in this param.
            /// </summary>
            public List<object> Values;

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<int> Unk1Values;

            /// <summary>
            /// Creates a new Param with no values or unk1s.
            /// </summary>
            public Param(string name1, string name2, ParamType type)
            {
                Name1 = name1;
                Name2 = name2;
                Type = type;
                Values = new List<object>();
                Unk1Values = new List<int>();
            }

            internal Param(BinaryReaderEx br, Offsets offsets)
            {
                int paramHeaderOffset = br.ReadInt32();
                br.StepIn(offsets.ParamHeaders + paramHeaderOffset);

                int valuesOffset = br.ReadInt32();
                int unkOffset1 = br.ReadInt32();

                Type = br.ReadEnum8<ParamType>();
                byte valueCount = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);

                if (Type == ParamType.Byte && valueCount > 1)
                    throw new Exception("Notify TKGP so he can look into this, please.");

                Name1 = br.ReadUTF16();
                Name2 = br.ReadUTF16();

                br.StepIn(offsets.Values + valuesOffset);
                Values = new List<object>(valueCount);
                for (int i = 0; i < valueCount; i++)
                {
                    switch (Type)
                    {
                        case ParamType.Byte:
                            Values.Add(br.ReadByte());
                            break;

                        case ParamType.Short:
                            Values.Add(br.ReadInt16());
                            break;

                        case ParamType.IntA:
                            Values.Add(br.ReadInt32());
                            break;

                        case ParamType.BoolA:
                            Values.Add(br.ReadBoolean());
                            break;

                        case ParamType.IntB:
                            Values.Add(br.ReadInt32());
                            break;

                        case ParamType.Float:
                            Values.Add(br.ReadSingle());
                            break;

                        case ParamType.BoolB:
                            Values.Add(br.ReadBoolean());
                            break;

                        case ParamType.Float2:
                            Values.Add(br.ReadVector2());
                            br.AssertInt32(0);
                            br.AssertInt32(0);
                            break;

                        case ParamType.Float4:
                            Values.Add(br.ReadVector4());
                            break;

                        case ParamType.Byte4:
                            Values.Add(br.ReadBytes(4));
                            break;
                    }
                }
                br.StepOut();

                Unk1Values = new List<int>(br.GetInt32s(offsets.Unk1 + unkOffset1, valueCount));

                br.StepOut();
            }

            internal void WriteParamHeaderOffset(BinaryWriterEx bw, int groupIndex, int paramIndex)
            {
                bw.ReserveInt32($"ParamHeaderOffset{groupIndex}:{paramIndex}");
            }

            internal void WriteParamHeader(BinaryWriterEx bw, int groupIndex, int paramIndex, int paramHeadersOffset)
            {
                bw.FillInt32($"ParamHeaderOffset{groupIndex}:{paramIndex}", (int)bw.Position - paramHeadersOffset);
                bw.ReserveInt32($"ValuesOffset{groupIndex}:{paramIndex}");
                bw.ReserveInt32($"Unk1Offset{groupIndex}:{paramIndex}");

                bw.WriteByte((byte)Type);
                bw.WriteByte((byte)Values.Count);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteUTF16(Name1, true);
                bw.WriteUTF16(Name2, true);
                bw.Pad(4);
            }

            internal void WriteValues(BinaryWriterEx bw, int groupIndex, int paramIndex, int valuesOffset)
            {
                bw.FillInt32($"ValuesOffset{groupIndex}:{paramIndex}", (int)bw.Position - valuesOffset);
                for (int i = 0; i < Values.Count; i++)
                {
                    object value = Values[i];
                    switch (Type)
                    {
                        case ParamType.Byte:
                            bw.WriteInt32((byte)value);
                            break;

                        case ParamType.Short:
                            bw.WriteInt16((short)value);
                            break;

                        case ParamType.IntA:
                            bw.WriteInt32((int)value);
                            break;

                        case ParamType.BoolA:
                            bw.WriteBoolean((bool)value);
                            break;

                        case ParamType.IntB:
                            bw.WriteInt32((int)value);
                            break;

                        case ParamType.Float:
                            bw.WriteSingle((float)value);
                            break;

                        case ParamType.BoolB:
                            bw.WriteBoolean((bool)value);
                            break;

                        case ParamType.Float2:
                            bw.WriteVector2((Vector2)value);
                            bw.WriteInt32(0);
                            bw.WriteInt32(0);
                            break;

                        case ParamType.Float4:
                            bw.WriteVector4((Vector4)value);
                            break;

                        case ParamType.Byte4:
                            bw.WriteBytes((byte[])value);
                            break;
                    }
                }
                bw.Pad(4);
            }

            internal void WriteUnk1(BinaryWriterEx bw, int groupIndex, int paramIndex, int unkOffset1)
            {
                bw.FillInt32($"Unk1Offset{groupIndex}:{paramIndex}", (int)bw.Position - unkOffset1);
                bw.WriteInt32s(Unk1Values);
            }

            /// <summary>
            /// Returns the value in this param at the given index.
            /// </summary>
            public object this[int index]
            {
                get => Values[index];
                set => Values[index] = value;
            }

            /// <summary>
            /// Returns the specific and generic names of the param.
            /// </summary>
            public override string ToString()
            {
                return $"{Name1} | {Name2}";
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Unk3
        {
            /// <summary>
            /// Almost the index, but skips numbers sometimes.
            /// </summary>
            public int ID;

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<int> Values;

            /// <summary>
            /// Creates a new Unk3 with no values.
            /// </summary>
            public Unk3(int id)
            {
                ID = id;
                Values = new List<int>();
            }

            internal Unk3(BinaryReaderEx br, Offsets offsets)
            {
                ID = br.ReadInt32();
                int count = br.ReadInt32();
                int valuesOffset = br.ReadInt32();

                Values = new List<int>(br.GetInt32s(offsets.Unk3Values + valuesOffset, count));
            }

            internal void WriteHeader(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(ID);
                bw.WriteInt32(Values.Count);
                bw.ReserveInt32($"Unk3ValuesOffset{index}");
            }

            internal void WriteValues(BinaryWriterEx bw, int index, int unk3ValuesOffset)
            {
                bw.FillInt32($"Unk3ValuesOffset{index}", (int)bw.Position - unk3ValuesOffset);
                bw.WriteInt32s(Values);
            }
        }
    }
}
