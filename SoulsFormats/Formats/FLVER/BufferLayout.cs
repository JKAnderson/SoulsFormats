using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// Determines which properties of a vertex are read and written, and in what order and format.
        /// </summary>
        public class BufferLayout : List<BufferLayout.Member>
        {
            /// <summary>
            /// The total size of all ValueTypes in this layout.
            /// </summary>
            public int Size => this.Sum(member => member.Size);

            /// <summary>
            /// Creates a new empty BufferLayout.
            /// </summary>
            public BufferLayout() : base() { }

            internal BufferLayout(BinaryReaderEx br) : base()
            {
                int memberCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                int memberOffset = br.ReadInt32();

                br.StepIn(memberOffset);
                {
                    int structOffset = 0;
                    Capacity = memberCount;
                    for (int i = 0; i < memberCount; i++)
                    {
                        var member = new Member(br, structOffset);
                        structOffset += member.Size;
                        Add(member);
                    }
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.ReserveInt32($"VertexStructLayout{index}");
            }

            internal void WriteMembers(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"VertexStructLayout{index}", (int)bw.Position);
                int structOffset = 0;
                foreach (Member member in this)
                {
                    member.Write(bw, structOffset);
                    structOffset += member.Size;
                }
            }

            /// <summary>
            /// Represents one property of a vertex.
            /// </summary>
            public class Member
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Format used to store this member.
                /// </summary>
                public MemberType Type { get; set; }

                /// <summary>
                /// Vertex property being stored.
                /// </summary>
                public MemberSemantic Semantic { get; set; }

                /// <summary>
                /// For semantics that may appear more than once such as UVs, which one this member is.
                /// </summary>
                public int Index { get; set; }

                /// <summary>
                /// The size of this member's ValueType, in bytes.
                /// </summary>
                public int Size
                {
                    get
                    {
                        switch (Type)
                        {
                            case MemberType.Byte4A:
                            case MemberType.Byte4B:
                            case MemberType.Short2toFloat2:
                            case MemberType.Byte4C:
                            case MemberType.UV:
                            case MemberType.Byte4E:
                                return 4;

                            case MemberType.Float2:
                            case MemberType.UVPair:
                            case MemberType.ShortBoneIndices:
                            case MemberType.Short4toFloat4A:
                            case MemberType.Short4toFloat4B:
                                return 8;

                            case MemberType.Float3:
                                return 12;

                            case MemberType.Float4:
                                return 16;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                /// <summary>
                /// Creates a new Member with the specified values.
                /// </summary>
                public Member(int unk00, MemberType type, MemberSemantic semantic, int index)
                {
                    Unk00 = unk00;
                    Type = type;
                    Semantic = semantic;
                    Index = index;
                }

                internal Member(BinaryReaderEx br, int structOffset)
                {
                    Unk00 = br.AssertInt32(0, 1, 2);
                    br.AssertInt32(structOffset);
                    Type = br.ReadEnum32<MemberType>();
                    Semantic = br.ReadEnum32<MemberSemantic>();
                    Index = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw, int structOffset)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(structOffset);
                    bw.WriteUInt32((uint)Type);
                    bw.WriteUInt32((uint)Semantic);
                    bw.WriteInt32(Index);
                }

                /// <summary>
                /// Returns the value type and semantic of this member.
                /// </summary>
                public override string ToString()
                {
                    return $"{Type}: {Semantic}";
                }
            }

            /// <summary>
            /// Format of a vertex property.
            /// </summary>
            public enum MemberType : uint
            {
                /// <summary>
                /// Two single-precision floats.
                /// </summary>
                Float2 = 0x01,

                /// <summary>
                /// Three single-precision floats.
                /// </summary>
                Float3 = 0x02,

                /// <summary>
                /// Four single-precision floats.
                /// </summary>
                Float4 = 0x03,

                /// <summary>
                /// Unknown.
                /// </summary>
                Byte4A = 0x10,

                /// <summary>
                /// Four bytes.
                /// </summary>
                Byte4B = 0x11,

                /// <summary>
                /// Two shorts?
                /// </summary>
                Short2toFloat2 = 0x12,

                /// <summary>
                /// Four bytes.
                /// </summary>
                Byte4C = 0x13,

                /// <summary>
                /// Two shorts.
                /// </summary>
                UV = 0x15,

                /// <summary>
                /// Two shorts and two shorts.
                /// </summary>
                UVPair = 0x16,

                /// <summary>
                /// Four shorts, maybe unsigned?
                /// </summary>
                ShortBoneIndices = 0x18,

                /// <summary>
                /// Four shorts.
                /// </summary>
                Short4toFloat4A = 0x1A,

                /// <summary>
                /// Unknown.
                /// </summary>
                Short4toFloat4B = 0x2E,

                /// <summary>
                /// Unknown.
                /// </summary>
                Byte4E = 0x2F,
            }

            /// <summary>
            /// Property of a vertex.
            /// </summary>
            public enum MemberSemantic : uint
            {
                /// <summary>
                /// Where the vertex is.
                /// </summary>
                Position = 0x00,

                /// <summary>
                /// Weight of the vertex's attachment to bones.
                /// </summary>
                BoneWeights = 0x01,

                /// <summary>
                /// Bones the vertex is weighted to, indexing the parent mesh's bone indices.
                /// </summary>
                BoneIndices = 0x02,

                /// <summary>
                /// Orientation of the vertex.
                /// </summary>
                Normal = 0x03,

                /// <summary>
                /// Texture coordinates of the vertex.
                /// </summary>
                UV = 0x05,

                /// <summary>
                /// Unknown.
                /// </summary>
                Tangent = 0x06,

                /// <summary>
                /// Unknown.
                /// </summary>
                UnknownVector4A = 0x07,

                /// <summary>
                /// Color of the vertex (if untextured?)
                /// </summary>
                VertexColor = 0x0A,
            }
        }
    }
}
