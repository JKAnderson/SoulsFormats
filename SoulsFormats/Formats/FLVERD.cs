using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Demon's Souls models; read-only.
    /// </summary>
    public class FLVERD : SoulsFile<FLVERD>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool BigEndian;

        public int Version;

        public Vector3 BoundingBoxMin;

        public Vector3 BoundingBoxMax;

        public int Unk40, Unk48, Unk4C;

        public List<Dummy> Dummies;

        public List<Material> Materials;

        public List<Bone> Bones;

        public List<Mesh> Meshes;

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.ReadASCII(6);
            string endian = br.ReadASCII(2);
            if (endian == "L\0")
                br.BigEndian = false;
            else if (endian == "B\0")
                br.BigEndian = true;
            int version = br.ReadInt32();
            return magic == "FLVER\0" && version <= 0x15;
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("FLVER\0");
            string endian = br.ReadASCII(2);
            if (endian == "L\0")
                BigEndian = false;
            else if (endian == "B\0")
                BigEndian = true;
            else
                throw new FormatException("FLVER endian character must be either L or B.");
            br.BigEndian = BigEndian;

            int version = br.AssertInt32(0x0E, 0x0F, 0x10, 0x12, 0x13, 0x14, 0x15);
            int dataOffset = br.ReadInt32();
            int dataSize = br.ReadInt32();
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            br.AssertInt32(meshCount);
            BoundingBoxMin = br.ReadVector3();
            BoundingBoxMax = br.ReadVector3();
            Unk40 = br.ReadInt32();
            int totalFaceCount = br.ReadInt32();
            Unk48 = br.ReadInt32();
            Unk4C = br.ReadInt32();

            for (int i = 0; i < 12; i++)
                br.AssertInt32(0);

            Dummies = new List<Dummy>(dummyCount);
            for (int i = 0; i < dummyCount; i++)
                Dummies.Add(new Dummy(br));

            Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++)
                Materials.Add(new Material(br));

            Bones = new List<Bone>(boneCount);
            for (int i = 0; i < boneCount; i++)
                Bones.Add(new Bone(br));

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br, Materials, dataOffset));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        public class Dummy
        {
            public Vector3 Position;

            public Vector3 Forward;

            public Vector3 Upward;

            public short ReferenceID;

            public short DummyBoneIndex;

            public short AttachBoneIndex;

            public int Unk04;

            public bool Flag1, Flag2;

            internal Dummy(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Unk04 = br.ReadInt32();
                Forward = br.ReadVector3();
                ReferenceID = br.ReadInt16();
                DummyBoneIndex = br.ReadInt16();
                Upward = br.ReadVector3();
                AttachBoneIndex = br.ReadInt16();
                Flag1 = br.ReadBoolean();
                Flag2 = br.ReadBoolean();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
        }

        public class Material
        {
            public string Name;

            public string MTD;

            public List<Texture> Textures;

            public List<BufferLayout> Layouts;

            internal Material(BinaryReaderEx br)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                int texturesOffset = br.ReadInt32();
                int layoutsOffset = br.ReadInt32();
                int dataSize = br.ReadInt32(); // From name offset to end of buffer layouts
                int layoutHeaderOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = br.BigEndian ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
                MTD = br.BigEndian ? br.GetUTF16(mtdOffset) : br.GetShiftJIS(mtdOffset);

                br.StepIn(texturesOffset);
                {
                    byte textureCount = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    Textures = new List<Texture>(textureCount);
                    for (int i = 0; i < textureCount; i++)
                        Textures.Add(new Texture(br));
                }
                br.StepOut();

                br.StepIn(layoutHeaderOffset);
                {
                    int layoutCount = br.ReadInt32();
                    br.AssertInt32((int)br.Position + 0xC);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    Layouts = new List<BufferLayout>(layoutCount);
                    for (int i = 0; i < layoutCount; i++)
                    {
                        int layoutOffset = br.ReadInt32();
                        br.StepIn(layoutOffset);
                        {
                            Layouts.Add(new BufferLayout(br));
                        }
                        br.StepOut();
                    }
                }
                br.StepOut();
            }
        }

        public class Texture
        {
            public string Type;

            public string Path;

            internal Texture(BinaryReaderEx br)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Path = br.BigEndian ? br.GetUTF16(pathOffset) : br.GetShiftJIS(pathOffset);
                if (typeOffset > 0)
                    Type = br.BigEndian ? br.GetUTF16(typeOffset) : br.GetShiftJIS(typeOffset);
                else
                    Type = null;
            }
        }

        public class BufferLayout : List<BufferLayout.Member>
        {
            /// <summary>
            /// The total size of all ValueTypes in this layout.
            /// </summary>
            public int Size => this.Sum(member => member.Size);

            internal BufferLayout(BinaryReaderEx br) : base()
            {
                short memberCount = br.ReadInt16();
                short structSize = br.ReadInt16();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                for (int i = 0; i < memberCount; i++)
                    Add(new Member(br));

                if (Size != structSize)
                    throw new FormatException();
            }

            public class Member
            {
                public MemberType Type;

                public MemberSemantic Semantic;

                public int StructOffset;

                public int Index;

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

                internal Member(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    StructOffset = br.ReadInt32();
                    Type = br.ReadEnum32<MemberType>();
                    Semantic = br.ReadEnum32<MemberSemantic>();
                    Index = br.ReadInt32();
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

        public class Bone
        {
            public string Name;

            public Vector3 Translation;

            public Vector3 Rotation;

            public Vector3 Scale;

            public Vector3 BoundingBoxMin;

            public Vector3 BoundingBoxMax;

            public short ParentIndex;

            public short ChildIndex;

            public short NextSiblingIndex;

            public short PreviousSiblingIndex;

            internal Bone(BinaryReaderEx br)
            {
                Translation = br.ReadVector3();
                int nameOffset = br.ReadInt32();
                Rotation = br.ReadVector3();
                ParentIndex = br.ReadInt16();
                ChildIndex = br.ReadInt16();
                Scale = br.ReadVector3();
                NextSiblingIndex = br.ReadInt16();
                PreviousSiblingIndex = br.ReadInt16();
                BoundingBoxMin = br.ReadVector3();
                br.AssertInt32(0);
                BoundingBoxMax = br.ReadVector3();

                for (int i = 0; i < 13; i++)
                    br.AssertInt32(0);

                Name = br.BigEndian ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
            }
        }

        public class Mesh
        {
            public bool Dynamic;

            public byte MaterialIndex;

            public bool Unk02;
            
            public byte Unk03;

            public short[] BoneIndices;

            public ushort[] VertexIndices;

            public List<Vertex> Vertices;

            internal Mesh(BinaryReaderEx br, List<Material> materials, int dataOffset)
            {
                Dynamic = br.ReadBoolean();
                MaterialIndex = br.ReadByte();
                Unk02 = br.ReadBoolean();
                Unk03 = br.ReadByte();

                int vertexIndexCount = br.ReadInt32();
                int vertexCount = br.ReadInt32();
                BoneIndices = br.ReadInt16s(29);
                br.AssertInt16(0);
                br.AssertInt32(vertexIndexCount * 2);
                int vertexIndicesOffset = br.ReadInt32();
                int bufferSize = br.ReadInt32();
                int bufferOffset = br.ReadInt32();
                int bufferHeaderOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                VertexIndices = br.GetUInt16s(dataOffset + vertexIndicesOffset, vertexIndexCount);

                br.StepIn(dataOffset + bufferOffset);
                {
                    BufferLayout layout = null;
                    foreach (var bl in materials[MaterialIndex].Layouts)
                    {
                        if (bl.Size * vertexCount == bufferSize)
                            layout = bl;
                    }

                    Vertices = new List<Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++)
                    {
                        long pos = br.Position;
                        Vertices.Add(new Vertex(br, layout));
                        if (br.Position != pos + layout.Size)
                            throw null;

                        if (Vertices.Last().Position.X == float.MinValue)
                            throw null;
                    }
                }
                br.StepOut();
            }

            public List<Vertex[]> GetFaces()
            {
                var faces = new List<Vertex[]>();
                bool flip = false;
                for (int i = 0; i < VertexIndices.Length - 2; i++)
                {
                    ushort vi1 = VertexIndices[i];
                    ushort vi2 = VertexIndices[i + 1];
                    ushort vi3 = VertexIndices[i + 2];

                    if (vi1 == 0xFFFF)
                    {
                        flip = false;
                    }
                    else if (vi3 == 0xFFFF)
                    {
                        flip = false;
                        i += 2;
                    }
                    else
                    {
                        if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3)
                        {
                            if (!flip)
                                faces.Add(new Vertex[] { Vertices[vi1], Vertices[vi2], Vertices[vi3] });
                            else
                                faces.Add(new Vertex[] { Vertices[vi3], Vertices[vi2], Vertices[vi1] });
                        }
                        flip = !flip;
                    }
                }
                return faces;
            }

            public ushort[] ToTriangleList()
            {
                var converted = new List<ushort>();
                bool flip = false;
                for (int i = 0; i < VertexIndices.Length - 2; i++)
                {
                    ushort vi1 = VertexIndices[i];
                    ushort vi2 = VertexIndices[i + 1];
                    ushort vi3 = VertexIndices[i + 2];

                    if (vi1 == 0xFFFF)
                    {
                        flip = false;
                    }
                    else if (vi3 == 0xFFFF)
                    {
                        flip = false;
                        i += 2;
                    }
                    else
                    {
                        if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3)
                        {
                            if (!flip)
                            {
                                converted.Add(vi1);
                                converted.Add(vi2);
                                converted.Add(vi3);
                            }
                            else
                            {
                                converted.Add(vi3);
                                converted.Add(vi2);
                                converted.Add(vi1);
                            }
                        }
                        flip = !flip;
                    }
                }
                return converted.ToArray();
            }
        }

        /// <summary>
        /// A single point in a mesh.
        /// </summary>
        public class Vertex
        {
            /// <summary>
            /// Where the vertex is.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Bones the vertex is weighted to, indexing the parent mesh's bone indices; must be 4 length.
            /// </summary>
            public int[] BoneIndices;

            /// <summary>
            /// Weight of the vertex's attachment to bones; must be 4 length.
            /// </summary>
            public float[] BoneWeights;

            /// <summary>
            /// Texture coordinates of the vertex.
            /// </summary>
            public List<Vector3> UVs;

            /// <summary>
            /// Vector pointing away from the surface.
            /// </summary>
            public Vector4 Normal;

            /// <summary>
            /// Vector pointing perpendicular to the normal.
            /// </summary>
            public List<Vector4> Tangents;

            /// <summary>
            /// Color of the vertex (if untextured?)
            /// </summary>
            public List<Color> Colors;

            /// <summary>
            /// Unknown. Must be 4 length.
            /// </summary>
            public byte[] UnknownVector4;

            /// <summary>
            /// Create a new Vertex with null or empty values.
            /// </summary>
            public Vertex(BinaryReaderEx br, BufferLayout layout)
            {
                Position = Vector3.Zero;
                BoneIndices = null;
                BoneWeights = null;
                UVs = new List<Vector3>();
                Normal = Vector4.Zero;
                Tangents = new List<Vector4>();
                Colors = new List<Color>();
                UnknownVector4 = null;

                float uvFactor = 1024;

                foreach (BufferLayout.Member member in layout)
                {
                    switch (member.Semantic)
                    {
                        case BufferLayout.MemberSemantic.Position:
                            if (member.Type == BufferLayout.MemberType.Float3)
                            {
                                Position = br.ReadVector3();
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.BoneWeights:
                            if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadSByte() / (float)sbyte.MaxValue;
                            }
                            else if (member.Type == BufferLayout.MemberType.UVPair)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadInt16() / (float)short.MaxValue;
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4A)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadInt16() / (float)short.MaxValue;
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.BoneIndices:
                            if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else if (member.Type == BufferLayout.MemberType.ShortBoneIndices)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadUInt16();
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4E)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.Normal:
                            if (member.Type == BufferLayout.MemberType.Float3)
                            {
                                Normal = new Vector4(br.ReadVector3(), 0);
                            }
                            else if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                Normal = br.ReadVector4();
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = br.ReadInt16() / 32767f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadUInt16() - 32767) / 32767f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.UV:
                            if (member.Type == BufferLayout.MemberType.Float2)
                            {
                                UVs.Add(new Vector3(br.ReadVector2() / uvFactor, 0));
                            }
                            else if (member.Type == BufferLayout.MemberType.Float3)
                            {
                                UVs.Add(br.ReadVector3() / uvFactor);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short2toFloat2)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == BufferLayout.MemberType.UV)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == BufferLayout.MemberType.UVPair)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.Tangent:
                            if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                Tangents.Add(br.ReadVector4());
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = br.ReadInt16() / 32767f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.UnknownVector4A:
                            if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                UnknownVector4 = br.ReadBytes(4);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                UnknownVector4 = br.ReadBytes(4);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                UnknownVector4 = br.ReadBytes(4);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.VertexColor:
                            if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                float[] floats = br.ReadSingles(4);
                                Colors.Add(new Color(floats[3], floats[0], floats[1], floats[2]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                byte[] bytes = br.ReadBytes(4);
                                Colors.Add(new Color(bytes[0], bytes[1], bytes[2], bytes[3]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                byte[] bytes = br.ReadBytes(4);
                                Colors.Add(new Color(bytes[3], bytes[0], bytes[1], bytes[2]));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            internal void Write(BinaryWriterEx bw, BufferLayout layout, int vertexSize, int version)
            {
                var tangentQueue = new Queue<Vector4>(Tangents);
                var colorQueue = new Queue<Color>(Colors);
                var uvQueue = new Queue<Vector3>(UVs);

                float uvFactor = 1024;

                foreach (BufferLayout.Member member in layout)
                {
                    switch (member.Semantic)
                    {
                        case BufferLayout.MemberSemantic.Position:
                            if (member.Type == BufferLayout.MemberType.Float3)
                            {
                                bw.WriteVector3(Position);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.BoneWeights:
                            if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteSByte((sbyte)(BoneWeights[i] * sbyte.MaxValue));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteInt16((short)(BoneWeights[i] * short.MaxValue));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.BoneIndices:
                            if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteByte((byte)BoneIndices[i]);
                            }
                            else if (member.Type == BufferLayout.MemberType.ShortBoneIndices)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteUInt16((ushort)BoneIndices[i]);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4E)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteByte((byte)BoneIndices[i]);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.Normal:
                            if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                bw.WriteVector4(Normal);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                bw.WriteByte((byte)(Normal.X * 127 + 127));
                                bw.WriteByte((byte)(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)(Normal.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                bw.WriteByte((byte)(Normal.X * 127 + 127));
                                bw.WriteByte((byte)(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)(Normal.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteByte((byte)(Normal.X * 127 + 127));
                                bw.WriteByte((byte)(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)(Normal.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4B)
                            {
                                bw.WriteInt16((short)(Normal.X * 32767 + 32767));
                                bw.WriteInt16((short)(Normal.Y * 32767 + 32767));
                                bw.WriteInt16((short)(Normal.Z * 32767 + 32767));
                                bw.WriteInt16((short)(Normal.W * 32767 + 32767));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.UV:
                            Vector3 uv = uvQueue.Dequeue() * uvFactor;
                            if (member.Type == BufferLayout.MemberType.Float2)
                            {
                                bw.WriteSingle(uv.X);
                                bw.WriteSingle(uv.Y);
                            }
                            else if (member.Type == BufferLayout.MemberType.Float3)
                            {
                                bw.WriteVector3(uv);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.Type == BufferLayout.MemberType.Short2toFloat2)
                            {
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.Type == BufferLayout.MemberType.UV)
                            {
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.Type == BufferLayout.MemberType.UVPair)
                            {
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);

                                uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.Tangent:
                            Vector4 tangent = tangentQueue.Dequeue();
                            if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                bw.WriteByte((byte)(tangent.X * 127 + 127));
                                bw.WriteByte((byte)(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)(tangent.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                bw.WriteByte((byte)(tangent.X * 127 + 127));
                                bw.WriteByte((byte)(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)(tangent.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteByte((byte)(tangent.X * 127 + 127));
                                bw.WriteByte((byte)(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)(tangent.W * 127 + 127));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.UnknownVector4A:
                            if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                bw.WriteBytes(UnknownVector4);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteBytes(UnknownVector4);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.VertexColor:
                            Color color = colorQueue.Dequeue();
                            if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                bw.WriteSingle(color.R);
                                bw.WriteSingle(color.G);
                                bw.WriteSingle(color.B);
                                bw.WriteSingle(color.A);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                bw.WriteByte((byte)(color.A * 255));
                                bw.WriteByte((byte)(color.R * 255));
                                bw.WriteByte((byte)(color.G * 255));
                                bw.WriteByte((byte)(color.B * 255));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteByte((byte)(color.R * 255));
                                bw.WriteByte((byte)(color.G * 255));
                                bw.WriteByte((byte)(color.B * 255));
                                bw.WriteByte((byte)(color.A * 255));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            /// <summary>
            /// A vertex color with ARGB components, typically from 0 to 1.
            /// Used instead of System.Drawing.Color because some FLVERs use float colors with negative or >1 values.
            /// </summary>
            public class Color
            {
                /// <summary>
                /// Alpha component of the color.
                /// </summary>
                public float A;

                /// <summary>
                /// Red component of the color.
                /// </summary>
                public float R;

                /// <summary>
                /// Green component of the color.
                /// </summary>
                public float G;

                /// <summary>
                /// Blue component of the color.
                /// </summary>
                public float B;

                /// <summary>
                /// Creates a new color with the given ARGB values.
                /// </summary>
                public Color(float a, float r, float g, float b)
                {
                    A = a;
                    R = r;
                    G = g;
                    B = b;
                }

                /// <summary>
                /// Creates a new color with the given ARGB values, divided by 255.
                /// </summary>
                public Color(byte a, byte r, byte g, byte b)
                {
                    A = a / 255f;
                    R = r / 255f;
                    G = g / 255f;
                    B = b / 255f;
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
