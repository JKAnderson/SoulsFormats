using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A model format used throughout the series.
    /// </summary>
    public class FLVER : SoulsFile<FLVER>
    {
        public FLVERHeader Header;
        public List<Dummy> Dummies;
        public List<Material> Materials;
        public List<Bone> Bones;
        public List<Mesh> Meshes;
        public List<VertexStructLayout> VertexStructLayouts;

        /// <summary>
        /// Creates an uninitialized FLVER. Should not be called publicly; use FLVER.Read instead.
        /// </summary>
        public FLVER() { }

        /// <summary>
        /// Reads FLVER data from a BinaryReaderEx.
        /// </summary>
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

            Header.Version = br.AssertInt32(0x2000C, 0x2000D, 0x20010);

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
            Header.UnkS1 = br.AssertInt16(0, -1);

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

        /// <summary>
        /// Convert a list to a dictionary with indices as keys.
        /// </summary>
        private static Dictionary<int, T> Dictionize<T>(List<T> items)
        {
            var dict = new Dictionary<int, T>();
            for (int i = 0; i < items.Count; i++)
                dict[i] = items[i];
            return dict;
        }

        protected internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = Header.BigEndian;
            bw.WriteASCII("FLVER\0");
            if (Header.BigEndian)
                bw.WriteASCII("B\0");
            else
                bw.WriteASCII("L\0");
            bw.WriteInt32(Header.Version);

            bw.ReserveInt32("DataOffset");
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(Dummies.Count);
            bw.WriteInt32(Materials.Count);
            bw.WriteInt32(Bones.Count);
            bw.WriteInt32(Meshes.Count);

            int vertexGroupCount = 0;
            foreach (Mesh mesh in Meshes)
                vertexGroupCount += mesh.VertexGroups.Count;
            bw.WriteInt32(vertexGroupCount);

            bw.WriteVector3(Header.BoundingBoxMin);
            bw.WriteVector3(Header.BoundingBoxMax);

            bw.WriteInt32(Header.UnkI1);
            bw.WriteInt32(Header.UnkI2);

            bw.WriteByte(0x10);
            bw.WriteBoolean(true);
            bw.WriteBoolean(Header.UnkB1);
            bw.WriteByte(0);

            bw.WriteInt16(0);
            bw.WriteInt16(Header.UnkS1);

            int faceSetCount = 0;
            foreach (Mesh mesh in Meshes)
                faceSetCount += mesh.FaceSets.Count;
            bw.WriteInt32(faceSetCount);

            bw.WriteInt32(VertexStructLayouts.Count);

            int materialParamsCount = 0;
            foreach (Material material in Materials)
                materialParamsCount += material.Params.Count;
            bw.WriteInt32(materialParamsCount);

            bw.WriteBoolean(Header.UnkB2);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(Header.UnkI3);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            foreach (Dummy dummy in Dummies)
                dummy.Write(bw);

            for (int i = 0; i < Materials.Count; i++)
                Materials[i].Write(bw, i);

            for (int i = 0; i < Bones.Count; i++)
                Bones[i].Write(bw, i);

            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].Write(bw, i);

            int faceSetIndex = 0;
            foreach (Mesh mesh in Meshes)
            {
                for (int i = 0; i < mesh.FaceSets.Count; i++)
                    mesh.FaceSets[i].Write(bw, faceSetIndex + i);
                faceSetIndex += mesh.FaceSets.Count;
            }

            int vertexGroupIndex = 0;
            foreach (Mesh mesh in Meshes)
            {
                for (int i = 0; i < mesh.VertexGroups.Count; i++)
                    mesh.VertexGroups[i].Write(bw, vertexGroupIndex + i, VertexStructLayouts);
                vertexGroupIndex += mesh.VertexGroups.Count;
            }

            for (int i = 0; i < VertexStructLayouts.Count; i++)
                VertexStructLayouts[i].Write(bw, i);

            int materialParamIndex = 0;
            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].WriteParams(bw, i, materialParamIndex);
                materialParamIndex += Materials[i].Params.Count;
            }

            bw.Pad(0x10);
            for (int i = 0; i < VertexStructLayouts.Count; i++)
            {
                VertexStructLayouts[i].WriteMembers(bw, i);
            }

            bw.Pad(0x10);
            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].WriteBoneIndices(bw, i);
            }

            bw.Pad(0x10);
            faceSetIndex = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                bw.FillInt32($"MeshFaceSetIndices{i}", (int)bw.Position);
                for (int j = 0; j < Meshes[i].FaceSets.Count; j++)
                    bw.WriteInt32(faceSetIndex + j);
                faceSetIndex += Meshes[i].FaceSets.Count;
            }

            bw.Pad(0x10);
            vertexGroupIndex = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                bw.FillInt32($"MeshVertexGroupIndices{i}", (int)bw.Position);
                for (int j = 0; j < Meshes[i].VertexGroups.Count; j++)
                    bw.WriteInt32(vertexGroupIndex + j);
                vertexGroupIndex += Meshes[i].VertexGroups.Count;
            }

            bw.Pad(0x10);
            materialParamIndex = 0;
            for (int i = 0; i < Materials.Count; i++)
            {
                Material material = Materials[i];
                bw.FillInt32($"MaterialName{i}", (int)bw.Position);
                bw.WriteUTF16(material.Name, true);
                bw.FillInt32($"MaterialMTD{i}", (int)bw.Position);
                bw.WriteUTF16(material.MTD, true);

                for (int j = 0; j < material.Params.Count; j++)
                {
                    bw.FillInt32($"MaterialParamValue{materialParamIndex + j}", (int)bw.Position);
                    bw.WriteUTF16(material.Params[j].Value, true);
                    bw.FillInt32($"MaterialParamParam{materialParamIndex + j}", (int)bw.Position);
                    bw.WriteUTF16(material.Params[j].Param, true);
                }
                materialParamIndex += material.Params.Count;
            }

            bw.Pad(0x10);
            for (int i = 0; i < Bones.Count; i++)
            {
                bw.FillInt32($"BoneName{i}", (int)bw.Position);
                bw.WriteUTF16(Bones[i].Name, true);
            }

            bw.Pad(0x20);
            int dataStart = (int)bw.Position;
            bw.FillInt32("DataOffset", dataStart);
            
            faceSetIndex = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                Mesh mesh = Meshes[i];
                for (int j = 0; j < mesh.FaceSets.Count; j++)
                    mesh.FaceSets[j].WriteVertices(bw, faceSetIndex + j, dataStart);
                faceSetIndex += mesh.FaceSets.Count;
                bw.Pad(0x20);
            }

            vertexGroupIndex = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                Mesh mesh = Meshes[i];
                for (int j = 0; j < mesh.VertexGroups.Count; j++)
                    mesh.VertexGroups[j].WriteVertices(bw, vertexGroupIndex + j, dataStart, VertexStructLayouts);
                vertexGroupIndex += mesh.VertexGroups.Count;
                bw.Pad(0x20);
            }

            bw.FillInt32("DataSize", (int)bw.Position - dataStart);
        }

        /// <summary>
        /// General metadata about a FLVER.
        /// </summary>
        public class FLVERHeader
        {
            /// <summary>
            /// If true FLVER will be written big-endian, if false little-endian.
            /// </summary>
            public bool BigEndian;

            /// <summary>
            /// Exact meaning unknown. 0x2000C or 0x2000D for DS1, 0x20010 for DS3.
            /// </summary>
            public int Version;

            public Vector3 BoundingBoxMin;
            public Vector3 BoundingBoxMax;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkI1, UnkI2, UnkI3;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool UnkB1, UnkB2;

            /// <summary>
            /// Unknown.
            /// </summary>
            public short UnkS1;
        }

        /// <summary>
        /// "Dummy polygons" used for hit detection, particle effect locations, and much more.
        /// </summary>
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

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Position);
                bw.WriteByte(Unk1);
                bw.WriteByte(Unk2);
                bw.WriteInt16(Unk3);

                bw.WriteVector3(Row2);
                bw.WriteInt16(TypeID);
                bw.WriteInt16(ParentBoneIndex);

                bw.WriteVector3(Row3);
                bw.WriteInt16(UnkParentIndex);
                bw.WriteBoolean(Flag1);
                bw.WriteBoolean(true);

                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
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

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"MaterialName{index}");
                bw.ReserveInt32($"MaterialMTD{index}");
                bw.WriteInt32(Params.Count);
                bw.ReserveInt32($"MaterialParamsIndex{index}");
                bw.WriteInt32(Flags);
                bw.WriteInt32(Unk1);

                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            internal void WriteParams(BinaryWriterEx bw, int index, int paramIndex)
            {
                bw.FillInt32($"MaterialParamsIndex{index}", paramIndex);
                for (int i = 0; i < Params.Count; i++)
                {
                    Params[i].Write(bw, paramIndex + i);
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

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteVector3(Translation);
                bw.ReserveInt32($"BoneName{index}");
                bw.WriteVector3(EulerRadian);
                bw.WriteInt16(ParentIndex);
                bw.WriteInt16(ChildIndex);
                bw.WriteVector3(Scale);
                bw.WriteInt16(NextSiblingIndex);
                bw.WriteInt16(PreviousSiblingIndex);
                bw.WriteVector3(BoundingBoxMin);

                bw.WriteBoolean(Nub);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteVector3(BoundingBoxMax);

                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
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
            public List<int> BoneIndices;
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
                BoneIndices = new List<int>(br.GetInt32s(boneOffset, boneCount));

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

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteBoolean(Dynamic);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(MaterialIndex);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(DefaultBoneIndex);

                bw.WriteInt32(BoneIndices.Count);
                bw.WriteInt32(0);
                bw.ReserveInt32($"MeshBoneIndices{index}");

                bw.WriteInt32(FaceSets.Count);
                bw.ReserveInt32($"MeshFaceSetIndices{index}");

                bw.WriteInt32(VertexGroups.Count);
                bw.ReserveInt32($"MeshVertexGroupIndices{index}");
            }

            internal void WriteBoneIndices(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"MeshBoneIndices{index}", (int)bw.Position);
                bw.WriteInt32s(BoneIndices.ToArray());
            }
        }

        public class FaceSet
        {
            public uint Flags;
            public bool TriangleStrip;
            public bool CullBackfaces;
            public byte Unk3;
            public byte Unk4;
            public ushort[] Vertices;

            internal FaceSet(BinaryReaderEx br, int dataOffset)
            {
                Flags = br.ReadUInt32();
                TriangleStrip = br.ReadBoolean();
                CullBackfaces = br.ReadBoolean();
                Unk3 = br.ReadByte();
                Unk4 = br.ReadByte();

                int vertexCount = br.ReadInt32();
                int vertexOffset = br.ReadInt32();
                int vertexSize = br.ReadInt32();

                Vertices = br.GetUInt16s(dataOffset + vertexOffset, vertexCount);

                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteUInt32(Flags);

                bw.WriteBoolean(TriangleStrip);
                bw.WriteBoolean(CullBackfaces);
                bw.WriteByte(Unk3);
                bw.WriteByte(Unk4);

                bw.WriteInt32(Vertices.Length);
                bw.ReserveInt32($"FaceSetVertices{index}");
                bw.WriteInt32(Vertices.Length * 2);

                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            internal void WriteVertices(BinaryWriterEx bw, int index, int dataStart)
            {
                bw.FillInt32($"FaceSetVertices{index}", (int)bw.Position - dataStart);
                bw.WriteUInt16s(Vertices);
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
                vertexCount = -1;
                vertexBufferOffset = -1;
            }

            internal void Write(BinaryWriterEx bw, int index, List<VertexStructLayout> layouts)
            {
                VertexStructLayout layout = layouts[VertexStructLayoutIndex];

                bw.WriteInt32(Unk1);
                bw.WriteInt32(VertexStructLayoutIndex);

                int vertexSize = 0;
                foreach (VertexStructLayout.Member member in layout)
                {
                    switch (member.ValueType)
                    {
                        case VertexStructLayout.Member.MemberValueType.Vector3:
                            vertexSize += 12;
                            break;

                        case VertexStructLayout.Member.MemberValueType.Unknown10:
                        case VertexStructLayout.Member.MemberValueType.BoneIndicesStruct:
                        case VertexStructLayout.Member.MemberValueType.PackedVector4:
                        case VertexStructLayout.Member.MemberValueType.UV:
                        case VertexStructLayout.Member.MemberValueType.Unknown2F:
                            vertexSize += 4;
                            break;

                        case VertexStructLayout.Member.MemberValueType.UVPair:
                        case VertexStructLayout.Member.MemberValueType.BoneWeightsStruct:
                            vertexSize += 8;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
                bw.WriteInt32(vertexSize);

                bw.WriteInt32(Vertices.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(vertexSize * Vertices.Count);
                bw.ReserveInt32($"VertexGroupVertices{index}");
            }

            public void WriteVertices(BinaryWriterEx bw, int index, int dataStart, List<VertexStructLayout> layouts)
            {
                VertexStructLayout layout = layouts[VertexStructLayoutIndex];
                bw.FillInt32($"VertexGroupVertices{index}", (int)bw.Position - dataStart);
                foreach (Vertex vertex in Vertices)
                    vertex.Write(bw, layout);
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
                    if (member.Semantic != Member.MemberSemantic.VertexColor
                        && member.Semantic != Member.MemberSemantic.UV
                        && member.Semantic != Member.MemberSemantic.BiTangent)
                    {
                        if (semantics.Contains(member.Semantic))
                            throw new NotImplementedException();
                        semantics.Add(member.Semantic);
                    }
                }
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
                foreach (Member member in this)
                {
                    member.Write(bw);
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

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk1);
                    bw.WriteInt32(StructOffset);
                    bw.WriteUInt32((uint)ValueType);
                    bw.WriteUInt32((uint)Semantic);
                    bw.WriteInt32(Index);
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

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"MaterialParamValue{index}");
                bw.ReserveInt32($"MaterialParamParam{index}");
                bw.WriteSingle(Unk1);
                bw.WriteSingle(Unk2);

                bw.WriteBoolean(Unk3);
                bw.WriteBoolean(Unk4);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
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
            public List<byte[]> BiTangents;
            public List<Color> Colors;
            public byte[] UnknownVector4A;

            internal Vertex(BinaryReaderEx br, VertexStructLayout layout)
            {
                Position = null;
                BoneIndices = null;
                BoneWeights = null;
                UVs = new List<Vector2>();
                Normal = null;
                BiTangents = new List<byte[]>();
                Colors = new List<Color>();
                UnknownVector4A = null;

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
                                BiTangents.Add(br.ReadBytes(4));
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.UnknownVector4A)
                            {
                                UnknownVector4A = br.ReadBytes(4);
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

            public void Write(BinaryWriterEx bw, VertexStructLayout layout)
            {
                foreach (VertexStructLayout.Member member in layout)
                {
                    switch (member.ValueType)
                    {
                        case VertexStructLayout.Member.MemberValueType.Vector3:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.Position)
                            {
                                bw.WriteVector3((Vector3)Position);
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
                                bw.WriteBytes(BoneIndices);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.Normal)
                            {
                                bw.WriteBytes(Normal);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.BiTangent)
                            {
                                bw.WriteBytes(BiTangents[member.Index]);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.UnknownVector4A)
                            {
                                bw.WriteBytes(UnknownVector4A);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.VertexColor)
                            {
                                WriteColor(bw, Colors[member.Index]);
                            }
                            else if (member.Semantic == VertexStructLayout.Member.MemberSemantic.BoneWeights)
                            {
                                bw.WriteByte((byte)(BoneWeights[0] * 255));
                                bw.WriteByte((byte)(BoneWeights[1] * 255));
                                bw.WriteByte((byte)(BoneWeights[2] * 255));
                                bw.WriteByte((byte)(BoneWeights[3] * 255));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberValueType.UV:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.UV)
                            {
                                // TODO Make this respect multiple UVs
                                WriteUV(bw, UVs[0]);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberValueType.UVPair:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.UV)
                            {
                                // TODO Make this respect multiple UVs
                                WriteUV(bw, UVs[0]);
                                WriteUV(bw, UVs[1]);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberValueType.BoneWeightsStruct:
                            if (member.Semantic == VertexStructLayout.Member.MemberSemantic.BoneWeights)
                            {
                                bw.WriteUInt16((ushort)(BoneWeights[0] * 65535));
                                bw.WriteUInt16((ushort)(BoneWeights[1] * 65535));
                                bw.WriteUInt16((ushort)(BoneWeights[2] * 65535));
                                bw.WriteUInt16((ushort)(BoneWeights[3] * 65535));
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
                float u = br.ReadInt16();
                float v = br.ReadInt16();
                return new Vector2(u, v);
            }

            private static void WriteUV(BinaryWriterEx bw, Vector2 uv)
            {
                bw.WriteInt16((short)(uv.X));
                bw.WriteInt16((short)(uv.Y));
            }

            private static Color ReadColor(BinaryReaderEx br)
            {
                byte r = br.ReadByte();
                byte g = br.ReadByte();
                byte b = br.ReadByte();
                byte a = br.ReadByte();
                return Color.FromArgb(a, r, g, b);
            }

            private static void WriteColor(BinaryWriterEx bw, Color color)
            {
                bw.WriteByte(color.R);
                bw.WriteByte(color.G);
                bw.WriteByte(color.B);
                bw.WriteByte(color.A);
            }
        }
    }
}
