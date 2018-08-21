using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A model.
    /// </summary>
    public class FLVER : SoulsFile<FLVER>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool BigEndian;
        public short Version1, Version2;
        public int DataOffset, DataSize;
        public float[] BoundingBoxMin, BoundingBoxMax;
        public int Unk1, Unk2, Unk3, Unk4;

        public List<Dummy> Dummies;
        public List<Material> Materials;
        public List<Bone> Bones;
        public List<Mesh> Meshes;
        public List<FaceSet> FaceSets;
        public List<VertexGroup> VertexGroups;
        public List<VertexStructLayout> VertexStructLayouts;
        public List<MaterialParam> MaterialParams;

        public FLVER() { }

        protected internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("FLVER\0");
            string endian = br.ReadASCII(2);
            if (endian == "L\0")
                BigEndian = false;
            else if (endian == "B\0")
                BigEndian = true;
            else
                throw new FormatException("FLVER endian character must be either L or B.");
            br.BigEndian = BigEndian;

            Version1 = br.AssertInt16(0xC, 0xD, 0x10);
            Version2 = br.AssertInt16(0x2);

            DataOffset = br.ReadInt32();
            DataSize = br.ReadInt32();
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            int vertexGroupCount = br.ReadInt32();

            BoundingBoxMin = br.ReadSingles(3);
            BoundingBoxMax = br.ReadSingles(3);

            Unk1 = br.ReadInt32();
            Unk2 = br.ReadInt32();
            br.AssertInt32(0x110);
            br.AssertInt32(0);

            int faceSetCount = br.ReadInt32();
            int vertexStructLayoutCount = br.ReadInt32();
            int materialParameterCount = br.ReadInt32();

            Unk3 = br.AssertInt32(0, 1);
            br.AssertInt32(0);
            br.AssertInt32(0);
            Unk4 = br.AssertInt32(0, 2);
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

            FaceSets = new List<FaceSet>();
            for (int i = 0; i < faceSetCount; i++)
                FaceSets.Add(new FaceSet(br));

            VertexGroups = new List<VertexGroup>();
            for (int i = 0; i < vertexGroupCount; i++)
                VertexGroups.Add(new VertexGroup(br));

            VertexStructLayouts = new List<VertexStructLayout>();
            for (int i = 0; i < vertexStructLayoutCount; i++)
                VertexStructLayouts.Add(new VertexStructLayout(br));

            MaterialParams = new List<MaterialParam>();
            for (int i = 0; i < materialParameterCount; i++)
                MaterialParams.Add(new MaterialParam(br));
        }

        protected internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        public class Dummy
        {
            public float[] Position;
            public byte Unk1, Unk2;
            public short Unk3;
            public float[] Row2;
            public short TypeID;
            public short ParentBoneIndex;
            public float[] Row3;
            public short UnkParentIndex;
            public bool Flag1;

            internal Dummy(BinaryReaderEx br)
            {
                Position = br.ReadSingles(3);
                Unk1 = br.ReadByte();
                Unk2 = br.ReadByte();
                Unk3 = br.ReadInt16();

                Row2 = br.ReadSingles(3);
                TypeID = br.ReadInt16();
                ParentBoneIndex = br.ReadInt16();

                Row3 = br.ReadSingles(3);
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
            public int ParamCount;
            public int ParamIndex;
            public int Flags;
            public int Unk1;

            internal Material(BinaryReaderEx br)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                ParamCount = br.ReadInt32();
                ParamIndex = br.ReadInt32();
                Flags = br.ReadInt32();
                Unk1 = br.AssertInt32(0, 0x2760);

                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = br.GetUTF16(nameOffset);
                MTD = br.GetUTF16(mtdOffset);
            }

            public override string ToString()
            {
                return $"{Name} | {MTD}";
            }
        }

        public class Bone
        {
            public string Name;
            public float[] Translation;
            public float[] EulerRadian;
            public short ParentIndex;
            public short ChildIndex;
            public float[] Scale;
            public short NextSiblingIndex;
            public short PreviousSiblingIndex;
            public float[] BoundingBoxMin, BoundingBoxMax;
            public bool Nub;

            internal Bone(BinaryReaderEx br)
            {
                Translation = br.ReadSingles(3);
                int nameOffset = br.ReadInt32();
                EulerRadian = br.ReadSingles(3);
                ParentIndex = br.ReadInt16();
                ChildIndex = br.ReadInt16();
                Scale = br.ReadSingles(3);
                NextSiblingIndex = br.ReadInt16();
                PreviousSiblingIndex = br.ReadInt16();
                BoundingBoxMin = br.ReadSingles(3);

                Nub = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                BoundingBoxMax = br.ReadSingles(3);

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
            public int[] FaceSetIndices;
            public int[] VertexGroupIndices;

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
                FaceSetIndices = br.GetInt32s(faceSetOffset, faceSetCount);

                int vertexGroupCount = br.ReadInt32();
                int vertexGroupOffset = br.ReadInt32();
                VertexGroupIndices = br.GetInt32s(vertexGroupOffset, vertexGroupCount);
            }
        }

        public class FaceSet
        {
            public uint Flags;
            public bool CullBackfaces;
            public bool Unk3;
            public int VertexSize;
            public ushort[] Vertices;

            internal FaceSet(BinaryReaderEx br)
            {
                Flags = br.ReadUInt32();
                bool triangleStrip = br.AssertBoolean(true);
                CullBackfaces = br.ReadBoolean();
                Unk3 = br.ReadBoolean();
                br.AssertByte(0);

                int vertexCount = br.ReadInt32();
                int vertexOffset = br.ReadInt32();
                VertexSize = br.ReadInt32();

                Vertices = br.GetUInt16s(vertexOffset, vertexCount);

                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
        }

        public class VertexGroup
        {
            public int Unk1;
            public int VertexStructLayoutIndex;
            public int VertexSize;
            public int VertexCount;
            public int VertexBufferSize;
            public int VertexBufferOffset;

            internal VertexGroup(BinaryReaderEx br)
            {
                Unk1 = br.AssertInt32(0, 1);
                VertexStructLayoutIndex = br.ReadInt32();
                VertexSize = br.ReadInt32();
                VertexCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                VertexBufferSize = br.ReadInt32();
                VertexBufferOffset = br.ReadInt32();

                if (VertexSize == 20)
                    throw null;
            }
        }

        public class VertexStructLayout
        {
            public List<Member> Members;

            internal VertexStructLayout(BinaryReaderEx br)
            {
                int memberCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                int memberOffset = br.ReadInt32();

                Members = new List<Member>();
                br.StepIn(memberOffset);
                for (int i = 0; i < memberCount; i++)
                    Members.Add(new Member(br));
                br.StepOut();
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
                    BoneIndicesStruct = 0x11,
                    PackedVector4 = 0x13,
                    UV = 0x15,
                    UVPair = 0x16,
                    BoneWeightsStruct = 0x1A,
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
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member