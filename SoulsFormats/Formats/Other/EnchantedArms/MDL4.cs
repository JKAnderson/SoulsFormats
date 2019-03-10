using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats.EnchantedArms
{
    /// <summary>
    /// A 3D model format used in Enchanted Arms. Extension: .mdl
    /// </summary>
    public class MDL4 : SoulsFile<MDL4>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<Dummy> Dummies;
        public List<Material> Materials;
        public List<Bone> Bones;
        public List<Mesh> Meshes;

        public int Version;
        public int Unk20;
        public Vector3 BoundingBoxMin, BoundingBoxMax;
        public int Unk3C, Unk40;

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "MDL4";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;
            br.AssertASCII("MDL4");
            Version = br.AssertInt32(0x40001, 0x40002);
            int dataStart = br.ReadInt32();
            int dataSize = br.ReadInt32();
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            Unk20 = br.ReadInt32();
            BoundingBoxMin = br.ReadVector3();
            BoundingBoxMax = br.ReadVector3();
            Unk3C = br.ReadInt32();
            Unk40 = br.ReadInt32();

            for (int i = 0; i < 15; i++)
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
                Meshes.Add(new Mesh(br, dataStart, Version));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        public class Dummy
        {
            public Vector3 Unk00, Unk0C;
            public int Unk18, Unk1C, Unk20;

            internal Dummy(BinaryReaderEx br)
            {
                Unk00 = br.ReadVector3();
                Unk0C = br.ReadVector3();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
        }

        public class Material
        {
            public string Name;
            public string Shader;
            public byte Unk3C, Unk3D, Unk3E;
            public List<Param> Params;

            internal Material(BinaryReaderEx br)
            {
                long start = br.Position;
                Name = br.ReadFixStr(0x1F);
                Shader = br.ReadFixStr(0x1D);
                Unk3C = br.ReadByte();
                Unk3D = br.ReadByte();
                Unk3E = br.ReadByte();
                byte paramCount = br.ReadByte();

                Params = new List<Param>(paramCount);
                for (int i = 0; i < paramCount; i++)
                    Params.Add(new Param(br));

                br.Position = start + 0x840;
            }

            public class Param
            {
                public ParamType Type;
                public string Name;
                public object Value;

                internal Param(BinaryReaderEx br)
                {
                    long start = br.Position;
                    Type = br.ReadEnum8<ParamType>();
                    Name = br.ReadFixStr(0x1F);

                    switch (Type)
                    {
                        case ParamType.Int:
                            Value = br.ReadInt32();
                            break;

                        case ParamType.Float:
                            Value = br.ReadSingle();
                            break;

                        case ParamType.Float4:
                            Value = br.ReadSingles(4);
                            break;

                        case ParamType.String:
                            Value = br.ReadShiftJIS();
                            break;

                        default:
                            throw new NotImplementedException("Unknown param type: " + Type);
                    }

                    br.Position = start + 0x40;
                }
            }

            public enum ParamType : byte
            {
                Int = 0,
                Float = 1,
                Float4 = 4,
                String = 5,
            }
        }

        public class Bone
        {
            public string Name;
            public Vector3 Translation;
            public Vector3 Rotation;
            public Vector3 Scale;
            public byte[] Unk44;
            public short[] Unk70;

            internal Bone(BinaryReaderEx br)
            {
                Name = br.ReadFixStr(0x20);
                Translation = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                Unk44 = br.ReadBytes(0x20);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk70 = br.ReadInt16s(16);
            }
        }

        public class Mesh
        {
            public byte VertexFormat;
            public byte MaterialIndex;
            public bool Unk02, Unk03;
            public short Unk08;
            public short[] BoneIndices;
            public ushort[] VertexIndices;
            public List<Vertex> Vertices;
            public int[] UnkFormat2;

            internal Mesh(BinaryReaderEx br, int dataStart, int version)
            {
                VertexFormat = br.AssertByte(0, 1, 2);
                MaterialIndex = br.ReadByte();
                Unk02 = br.ReadBoolean();
                Unk03 = br.ReadBoolean();
                ushort vertexIndexCount = br.ReadUInt16();
                Unk08 = br.ReadInt16();
                BoneIndices = br.ReadInt16s(28);
                int vertexIndicesSize = br.ReadInt32();
                int vertexIndicesOffset = br.ReadInt32();
                int bufferSize = br.ReadInt32();
                int bufferOffset = br.ReadInt32();

                if (VertexFormat == 2)
                    UnkFormat2 = br.ReadInt32s(0x20);

                VertexIndices = br.GetUInt16s(dataStart + vertexIndicesOffset, vertexIndexCount);

                br.StepIn(dataStart + bufferOffset);
                {
                    int vertexSize = 0;
                    if (version == 0x40001)
                    {
                        if (VertexFormat == 0)
                            vertexSize = 0x40;
                        else if (VertexFormat == 1)
                            vertexSize = 0x54;
                        else if (VertexFormat == 2)
                            vertexSize = 0x3C;
                    }
                    else if (version == 0x40002)
                    {
                        if (VertexFormat == 0)
                            vertexSize = 0x28;
                    }
                    int vertexCount = bufferSize / vertexSize;
                    Vertices = new List<Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++)
                        Vertices.Add(new Vertex(br, version, VertexFormat));
                }
                br.StepOut();
            }

            public List<Vertex[]> GetFaces()
            {
                ushort[] indices = ToTriangleList();
                var faces = new List<Vertex[]>();
                for (int i = 0; i < indices.Length; i += 3)
                {
                    faces.Add(new Vertex[]
                    {
                        Vertices[indices[i + 0]],
                        Vertices[indices[i + 1]],
                        Vertices[indices[i + 2]],
                    });
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

                    if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF)
                    {
                        flip = false;
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

        public class Vertex
        {
            public Vector3 Position;
            public Vector4 Normal;
            public int Unk0C;
            public int Unk10;
            public int Unk14;
            public byte[] Color;
            public List<Vector2> UVs;
            public short[] BoneIndices;
            public float[] BoneWeights;
            public int Unk3C;

            internal Vertex(BinaryReaderEx br, int version, byte format)
            {
                UVs = new List<Vector2>();
                if (version == 0x40001)
                {
                    if (format == 0)
                    {
                        Position = br.ReadVector3();
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                        Unk14 = br.ReadInt32();
                        Color = br.ReadBytes(4);
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        Unk3C = br.ReadInt32();
                    }
                    else if (format == 1)
                    {
                        Position = br.ReadVector3();
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                        Unk14 = br.ReadInt32();
                        Color = br.ReadBytes(4);
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        BoneIndices = br.ReadInt16s(4);
                        BoneWeights = br.ReadSingles(4);
                    }
                    else if (format == 2)
                    {
                        Color = br.ReadBytes(4);
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        UVs.Add(br.ReadVector2());
                        BoneIndices = br.ReadInt16s(4);
                        BoneWeights = br.ReadSingles(4);
                    }
                }
                else if (version == 0x40002)
                {
                    if (format == 0)
                    {
                        Position = br.ReadVector3();
                        Normal = ReadSByteVector4(br);
                        Unk10 = br.ReadInt32();
                        Color = br.ReadBytes(4);
                        UVs.Add(ReadShortUV(br));
                        UVs.Add(ReadShortUV(br));
                        UVs.Add(ReadShortUV(br));
                        UVs.Add(ReadShortUV(br));
                    }
                }
            }

            private Vector4 ReadByteVector4(BinaryReaderEx br)
            {
                byte[] bytes = br.ReadBytes(4);
                return new Vector4((bytes[3] - 127) / 127f, (bytes[2] - 127) / 127f, (bytes[1] - 127) / 127f, (bytes[0] - 127) / 127f);
            }

            private Vector4 ReadSByteVector4(BinaryReaderEx br)
            {
                sbyte[] bytes = br.ReadSBytes(4);
                return new Vector4(bytes[3] / 127f, bytes[2] / 127f, bytes[1] / 127f, bytes[0] / 127f);
            }

            private Vector2 ReadShortUV(BinaryReaderEx br)
            {
                short u = br.ReadInt16();
                short v = br.ReadInt16();
                return new Vector2(u / 2048f, v / 2048f);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
