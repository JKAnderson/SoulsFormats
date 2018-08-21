using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A model.
    /// </summary>
    public class FLVER : SoulsFile<FLVER>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public FLVERHeader Header;
        public List<Dummy> Dummies;
        public List<Material> Materials;
        public List<Bone> Bones;
        public List<Mesh> Meshes;
        public List<VertexStructLayout> VertexStructLayouts;

        public FLVER() { }

        protected internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            Header = new FLVERHeader();
            br.AssertASCII("FLVER\0");
            string endian = br.ReadASCII(2);
            if (endian == "L\0")
                Header.BigEndian = false;
            else if (endian == "B\0")
                Header.BigEndian = true;
            else
                throw new FormatException("FLVER endian character must be either L or B.");
            br.BigEndian = Header.BigEndian;

            Header.Version = br.AssertInt32(0x2000C, 0x20010);

            int dataOffset = br.ReadInt32();
            int dataSize = br.ReadInt32();
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            int vertexGroupCount = br.ReadInt32();

            Header.BoundingBoxMin = br.ReadVector3();
            Header.BoundingBoxMax = br.ReadVector3();

            Header.UnkI1 = br.ReadInt32();
            Header.UnkI2 = br.ReadInt32();

            br.AssertByte(0x10);
            br.AssertBoolean(true);
            Header.UnkB1 = br.ReadBoolean();
            br.AssertByte(0);

            br.AssertInt16(0);
            br.AssertInt16(0, -1);

            int faceSetCount = br.ReadInt32();
            int vertexStructLayoutCount = br.ReadInt32();
            int materialParameterCount = br.ReadInt32();

            Header.UnkB2 = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);

            br.AssertInt32(0);
            br.AssertInt32(0);
            Header.UnkI3 = br.AssertInt32(0, 2);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            Dummies = new List<Dummy>();
            for (int i = 0; i < dummyCount; i++)
                Dummies.Add(new Dummy(br));

            Materials = new List<Material>();
            for (int i = 0; i < materialCount; i++)
                Materials.Add(new Material(br));

            Bones = new List<Bone>();
            for (int i = 0; i < boneCount; i++)
                Bones.Add(new Bone(br));

            Meshes = new List<Mesh>();
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br));

            var faceSets = new List<FaceSet>();
            for (int i = 0; i < faceSetCount; i++)
                faceSets.Add(new FaceSet(br, dataOffset));

            var vertexGroups = new List<VertexGroup>();
            for (int i = 0; i < vertexGroupCount; i++)
                vertexGroups.Add(new VertexGroup(br));

            VertexStructLayouts = new List<VertexStructLayout>();
            for (int i = 0; i < vertexStructLayoutCount; i++)
                VertexStructLayouts.Add(new VertexStructLayout(br));

            var materialParams = new List<MaterialParam>();
            for (int i = 0; i < materialParameterCount; i++)
                materialParams.Add(new MaterialParam(br));

            foreach (VertexGroup vertexGroup in vertexGroups)
                vertexGroup.ReadVertices(br, dataOffset, VertexStructLayouts);

            Dictionary<int, MaterialParam> materialParamDict = Dictionize(materialParams);
            foreach (Material material in Materials)
            {
                material.TakeParams(materialParamDict);
            }
            if (materialParamDict.Count != 0)
                throw new NotSupportedException("Orphaned material params found.");

            Dictionary<int, FaceSet> faceSetDict = Dictionize(faceSets);
            Dictionary<int, VertexGroup> vertexGroupDict = Dictionize(vertexGroups);
            foreach (Mesh mesh in Meshes)
            {
                mesh.TakeFaceSets(faceSetDict);
                mesh.TakeVertexGroups(vertexGroupDict);
            }
            if (faceSetDict.Count != 0)
                throw new NotSupportedException("Orphaned face sets found.");
            if (vertexGroupDict.Count != 0)
                throw new NotSupportedException("Orphaned vertex groups found.");

        }

        private static Dictionary<int, T> Dictionize<T>(List<T> items)
        {
            var dict = new Dictionary<int, T>();
            for (int i = 0; i < items.Count; i++)
                dict[i] = items[i];
            return dict;
        }

        protected internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        public class FLVERHeader
        {
            public bool BigEndian;
            public int Version;
            public Vector3 BoundingBoxMin;
            public Vector3 BoundingBoxMax;

            public int UnkI1, UnkI2, UnkI3;
            public bool UnkB1, UnkB2;
        }

        public class Dummy
        {
            public Vector3 Position;
            public byte Unk1, Unk2;
            public short Unk3;
            public Vector3 Row2;
            public short TypeID;
            public short ParentBoneIndex;
            public Vector3 Row3;
            public short UnkParentIndex;
            public bool Flag1;

            internal Dummy(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Unk1 = br.ReadByte();
                Unk2 = br.ReadByte();
                Unk3 = br.ReadInt16();

                Row2 = br.ReadVector3();
                TypeID = br.ReadInt16();
                ParentBoneIndex = br.ReadInt16();

                Row3 = br.ReadVector3();
                UnkParentIndex = br.ReadInt16();
                Flag1 = br.ReadBoolean();
                br.AssertBoolean(true);

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
            public int Flags;
            public int Unk1;
            public List<MaterialParam> Params;

            private int paramIndex, paramCount;

            internal Material(BinaryReaderEx br)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                paramCount = br.ReadInt32();
                paramIndex = br.ReadInt32();
                Flags = br.ReadInt32();
                Unk1 = br.ReadInt32();

                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = br.GetUTF16(nameOffset);
                MTD = br.GetUTF16(mtdOffset);
            }

            internal void TakeParams(Dictionary<int, MaterialParam> paramDict)
            {
                Params = new List<MaterialParam>();
                for (int i = paramIndex; i < paramIndex + paramCount; i++)
                {
                    if (!paramDict.ContainsKey(i))
                        throw new NotSupportedException("Material param not found or already taken: " + i);

                    Params.Add(paramDict[i]);
                    paramDict.Remove(i);
                }
            }

            public override string ToString()
            {
                return $"{Name} | {MTD}";
            }
        }

        public class Bone
        {
            public string Name;
            public Vector3 Translation;
            public Vector3 EulerRadian;
            public short ParentIndex;
            public short ChildIndex;
            public Vector3 Scale;
            public short NextSiblingIndex;
            public short PreviousSiblingIndex;
            public Vector3 BoundingBoxMin, BoundingBoxMax;
            public bool Nub;

            internal Bone(BinaryReaderEx br)
            {
                Translation = br.ReadVector3();
                int nameOffset = br.ReadInt32();
                EulerRadian = br.ReadVector3();
                ParentIndex = br.ReadInt16();
                ChildIndex = br.ReadInt16();
                Scale = br.ReadVector3();
                NextSiblingIndex = br.ReadInt16();
                PreviousSiblingIndex = br.ReadInt16();
                BoundingBoxMin = br.ReadVector3();

                Nub = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                BoundingBoxMax = br.ReadVector3();

                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = br.GetUTF16(nameOffset);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public class Mesh
        {
            public bool Dynamic;
            public int MaterialIndex;
            public int DefaultBoneIndex;
            public int[] BoneIndices;
            public List<FaceSet> FaceSets;
            public List<VertexGroup> VertexGroups;

            private int[] faceSetIndices, vertexGroupIndices;

            internal Mesh(BinaryReaderEx br)
            {
                Dynamic = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                MaterialIndex = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                DefaultBoneIndex = br.ReadInt32();

                int boneCount = br.ReadInt32();
                br.AssertInt32(0);
                int boneOffset = br.ReadInt32();
                BoneIndices = br.GetInt32s(boneOffset, boneCount);

                int faceSetCount = br.ReadInt32();
                int faceSetOffset = br.ReadInt32();
                faceSetIndices = br.GetInt32s(faceSetOffset, faceSetCount);

                int vertexGroupCount = br.ReadInt32();
                int vertexGroupOffset = br.ReadInt32();
                vertexGroupIndices = br.GetInt32s(vertexGroupOffset, vertexGroupCount);
            }

            internal void TakeFaceSets(Dictionary<int, FaceSet> faceSetDict)
            {
                FaceSets = new List<FaceSet>();
                foreach (int i in faceSetIndices)
                {
                    if (!faceSetDict.ContainsKey(i))
                        throw new NotSupportedException("Face set not found or already taken: " + i);

                    FaceSets.Add(faceSetDict[i]);
                    faceSetDict.Remove(i);
                }
                faceSetIndices = null;
            }

            internal void TakeVertexGroups(Dictionary<int, VertexGroup> vertexGroupDict)
            {
                VertexGroups = new List<VertexGroup>();
                foreach (int i in vertexGroupIndices)
                {
                    if (!vertexGroupDict.ContainsKey(i))
                        throw new NotSupportedException("Vertex group not found or already taken: " + i);

                    VertexGroups.Add(vertexGroupDict[i]);
                    vertexGroupDict.Remove(i);
                }
                vertexGroupIndices = null;
            }
        }

        public class FaceSet
        {
            public uint Flags;
            public bool TriangleStrip;
            public bool CullBackfaces;
            public bool Unk3;
            public byte Unk4;
            public int VertexSize;
            public ushort[] Vertices;

            internal FaceSet(BinaryReaderEx br, int dataOffset)
            {
                Flags = br.ReadUInt32();
                TriangleStrip = br.ReadBoolean();
                CullBackfaces = br.ReadBoolean();
                Unk3 = br.ReadBoolean();
                Unk4 = br.ReadByte();

                int vertexCount = br.ReadInt32();
                int vertexOffset = br.ReadInt32();
                VertexSize = br.ReadInt32();

                Vertices = br.GetUInt16s(dataOffset + vertexOffset, vertexCount);

                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
        }

        public class VertexGroup
        {
            public int Unk1;
            public int VertexStructLayoutIndex;
            public List<Vertex> Vertices;

            private int vertexCount, vertexBufferOffset;

            internal VertexGroup(BinaryReaderEx br)
            {
                Unk1 = br.AssertInt32(0, 1);
                VertexStructLayoutIndex = br.ReadInt32();
                int vertexSize = br.ReadInt32();
                vertexCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                int vertexBufferSize = br.ReadInt32();
                vertexBufferOffset = br.ReadInt32();
            }

            public void ReadVertices(BinaryReaderEx br, int dataOffset, List<VertexStructLayout> layouts)
            {
                VertexStructLayout layout = layouts[VertexStructLayoutIndex];
                Vertices = new List<Vertex>();
                br.StepIn(dataOffset + vertexBufferOffset);
                for (int i = 0; i < vertexCount; i++)
                    Vertices.Add(new Vertex(br, layout));
                br.StepOut();
            }
        }

        public class VertexStructLayout : List<VertexStructLayout.Member>
        {
            internal VertexStructLayout(BinaryReaderEx br) : base()
            {
                int memberCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                int memberOffset = br.ReadInt32();

                br.StepIn(memberOffset);
                for (int i = 0; i < memberCount; i++)
                    Add(new Member(br));
                br.StepOut();

                // Make sure no semantics repeat except for color
                var semantics = new List<Member.MemberSemantic>();
                foreach (Member member in this)
                {
                    if (member.Semantic != Member.MemberSemantic.VertexColor)
                    {
                        if (semantics.Contains(member.Semantic))
                            throw new NotImplementedException();
                        semantics.Add(member.Semantic);
                    }
                }
            }

            public class Member
            {
                public int Unk1;
                public int StructOffset;
                public MemberValueType ValueType;
                public MemberSemantic Semantic;
                public int Index;

                internal Member(BinaryReaderEx br)
                {
                    Unk1 = br.AssertInt32(0, 1);
                    StructOffset = br.ReadInt32();
                    ValueType = br.ReadEnum32<MemberValueType>();
                    Semantic = br.ReadEnum32<MemberSemantic>();
                    Index = br.ReadInt32();
                }

                public override string ToString()
                {
                    return $"{ValueType}: {Semantic}";
                }

                public enum MemberValueType : uint
                {
                    Vector3 = 0x02,
                    Unknown10 = 0x10,
                    BoneIndicesStruct = 0x11,
                    PackedVector4 = 0x13,
                    UV = 0x15,
                    UVPair = 0x16,
                    BoneWeightsStruct = 0x1A,
                    Unknown2F = 0x2F,
                }

                public enum MemberSemantic : uint
                {
                    Position = 0x00,
                    BoneWeights = 0x01,
                    BoneIndices = 0x02,
                    Normal = 0x03,
                    UV = 0x05,
                    BiTangent = 0x06,
                    UnknownVector4A = 0x07,
                    VertexColor = 0x0A,
                }
            }
        }

        public class MaterialParam
        {
            public string Param;
            public string Value;
            public float Unk1;
            public float Unk2;
            public bool Unk3;
            public bool Unk4;

            internal MaterialParam(BinaryReaderEx br)
            {
                int valueOffset = br.ReadInt32();
                int paramOffset = br.ReadInt32();
                Unk1 = br.AssertSingle(1, 2);
                Unk2 = br.AssertSingle(1, 2);

                Unk3 = br.ReadBoolean();
                Unk4 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);

                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Param = br.GetUTF16(paramOffset);
                Value = br.GetUTF16(valueOffset);
            }

            public override string ToString()
            {
                return $"{Param} = {Value}";
            }
        }

        public class Vertex
        {
            public Vector3? Position;
            public byte[] BoneIndices;
            public float[] BoneWeights;
            public List<Vector2> UVs;
            public byte[] Normal;
            public byte[] BiTangent;
            public List<Color> Colors;

            internal Vertex(BinaryReaderEx br, VertexStructLayout layout)
            {
                Position = null;
                BoneIndices = null;
                BoneWeights = null;
                UVs = new List<Vector2>();
                Normal = null;
                BiTangent = null;
                Colors = new List<Color>();

                foreach (VertexStructLayout.Member member in layout)
                {
                    switch (member.ValueType)
                    {
                        case VertexStructLayout.Member.MemberValueType.Vector3:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.Position)
                            {
                                Position = br.ReadVector3();
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberValueType.Unknown10:
                        case VertexStructLayout.Member.MemberValueType.BoneIndicesStruct:
                        case VertexStructLayout.Member.MemberValueType.PackedVector4:
                        case VertexStructLayout.Member.MemberValueType.Unknown2F:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.BoneIndices)
                            {
                                BoneIndices = br.ReadBytes(4);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.Normal)
                            {
                                Normal = br.ReadBytes(4);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.BiTangent)
                            {
                                BiTangent = br.ReadBytes(4);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.VertexColor)
                            {
                                Colors.Add(ReadColor(br));
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.BoneWeights)
                            {
                                BoneWeights = new float[] {
                                    br.ReadByte() / 255f,
                                    br.ReadByte() / 255f,
                                    br.ReadByte() / 255f,
                                    br.ReadByte() / 255f,
                                };
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberValueType.UV:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.UV)
                            {
                                UVs.Add(ReadUV(br));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberValueType.UVPair:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.UV)
                            {
                                UVs.Add(ReadUV(br));
                                UVs.Add(ReadUV(br));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberValueType.BoneWeightsStruct:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.BoneWeights)
                            {
                                BoneWeights = new float[] {
                                    br.ReadUInt16() / 65535f,
                                    br.ReadUInt16() / 65535f,
                                    br.ReadUInt16() / 65535f,
                                    br.ReadUInt16() / 65535f,
                                };
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            private static Vector2 ReadUV(BinaryReaderEx br)
            {
                float u = br.ReadInt16() / 2048f;
                float v = br.ReadInt16() / 2048f;
                return new Vector2(u, v);
            }

            private static Color ReadColor(BinaryReaderEx br)
            {
                byte r = br.ReadByte();
                byte g = br.ReadByte();
                byte b = br.ReadByte();
                byte a = br.ReadByte();
                return Color.FromArgb(a, r, g, b);
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member