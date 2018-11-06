using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A model format used throughout the series.
    /// </summary>
    public class FLVER : SoulsFile<FLVER>
    {
        /// <summary>
        /// General values for this model.
        /// </summary>
        public FLVERHeader Header;

        /// <summary>
        /// Dummy polygons in this model.
        /// </summary>
        public List<Dummy> Dummies;

        /// <summary>
        /// Materials in this model, usually one per mesh.
        /// </summary>
        public List<Material> Materials;

        /// <summary>
        /// Bones used by this model, often the full skeleton.
        /// </summary>
        public List<Bone> Bones;

        /// <summary>
        /// Individual chunks of the model.
        /// </summary>
        public List<Mesh> Meshes;

        /// <summary>
        /// Layouts determining how to write vertex information.
        /// </summary>
        public List<VertexStructLayout> VertexStructLayouts;

        /// <summary>
        /// Creates an uninitialized FLVER. Should not be called publicly; use FLVER.Read instead.
        /// </summary>
        public FLVER() { }

        /// <summary>
        /// Returns true if the data appears to be a FLVER.
        /// </summary>
        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 6);
            return magic == "FLVER\0";
        }

        /// <summary>
        /// Reads FLVER data from a BinaryReaderEx.
        /// </summary>
        internal override void Read(BinaryReaderEx br)
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

            // DS1: 2000C, 2000D
            // DS2: 20009, 20010
            // SFS: 20010
            // DS3: 20013, 20014
            // BB:  20013, 20014
            Header.Version = br.AssertInt32(0x20009, 0x2000C, 0x2000D, 0x20010, 0x20013, 0x20014);

            int dataOffset = br.ReadInt32();
            int dataSize = br.ReadInt32();
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            int vertexGroupCount = br.ReadInt32();

            Header.BoundingBoxMin = br.ReadVector3();
            Header.BoundingBoxMax = br.ReadVector3();

            Header.Unk40 = br.ReadInt32();
            Header.Unk44 = br.ReadInt32();

            Header.Unk48 = br.AssertByte(0x00, 0x10);
            br.AssertBoolean(true);
            Header.Unk4A = br.ReadBoolean();
            br.AssertByte(0);

            br.AssertInt16(0);
            Header.Unk4E = br.AssertInt16(0, -1);

            int faceSetCount = br.ReadInt32();
            int vertexStructLayoutCount = br.ReadInt32();
            int materialParameterCount = br.ReadInt32();

            Header.Unk5C = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);

            br.AssertInt32(0);
            br.AssertInt32(0);
            Header.Unk68 = br.AssertInt32(0, 1, 2, 3, 4);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
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
                Meshes.Add(new Mesh(br, Header.Version));

            var faceSets = new List<FaceSet>(faceSetCount);
            for (int i = 0; i < faceSetCount; i++)
                faceSets.Add(new FaceSet(br, dataOffset));

            var vertexGroups = new List<VertexGroup>(vertexGroupCount);
            for (int i = 0; i < vertexGroupCount; i++)
                vertexGroups.Add(new VertexGroup(br));

            VertexStructLayouts = new List<VertexStructLayout>(vertexStructLayoutCount);
            for (int i = 0; i < vertexStructLayoutCount; i++)
                VertexStructLayouts.Add(new VertexStructLayout(br));

            var materialParams = new List<MaterialParam>(materialParameterCount);
            for (int i = 0; i < materialParameterCount; i++)
                materialParams.Add(new MaterialParam(br));

            foreach (VertexGroup vertexGroup in vertexGroups)
                vertexGroup.ReadVertices(br, dataOffset, VertexStructLayouts, Header.Version);

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
            var dict = new Dictionary<int, T>(items.Count);
            for (int i = 0; i < items.Count; i++)
                dict[i] = items[i];
            return dict;
        }

        /// <summary>
        /// Writes FLVER data to a BinaryWriterEx.
        /// </summary>
        internal override void Write(BinaryWriterEx bw)
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

            bw.WriteInt32(Header.Unk40);
            bw.WriteInt32(Header.Unk44);

            bw.WriteByte(Header.Unk48);
            bw.WriteBoolean(true);
            bw.WriteBoolean(Header.Unk4A);
            bw.WriteByte(0);

            bw.WriteInt16(0);
            bw.WriteInt16(Header.Unk4E);

            int faceSetCount = 0;
            foreach (Mesh mesh in Meshes)
                faceSetCount += mesh.FaceSets.Count;
            bw.WriteInt32(faceSetCount);

            bw.WriteInt32(VertexStructLayouts.Count);

            int materialParamsCount = 0;
            foreach (Material material in Materials)
                materialParamsCount += material.Params.Count;
            bw.WriteInt32(materialParamsCount);

            bw.WriteBoolean(Header.Unk5C);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(Header.Unk68);
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
                Meshes[i].Write(bw, i, Header.Version);

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

            if (Header.Version >= 0x20013)
            {
                bw.Pad(0x10);
                for (int i = 0; i < Meshes.Count; i++)
                {
                    Meshes[i].WriteUnkFloats(bw, i);
                }
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
            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].WriteUnkGX(bw, i);
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
                    mesh.VertexGroups[j].WriteVertices(bw, vertexGroupIndex + j, dataStart, VertexStructLayouts, Header.Version);
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
            /// Exact meaning unknown.
            /// </summary>
            public int Version;

            /// <summary>
            /// Minimum extent of the entire model.
            /// </summary>
            public Vector3 BoundingBoxMin;

            /// <summary>
            /// Maximum extent of the entire model.
            /// </summary>
            public Vector3 BoundingBoxMax;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk40;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk44;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk48;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk4A;

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk4E;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk5C;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk68;
        }

        /// <summary>
        /// "Dummy polygons" used for hit detection, particle effect locations, and much more.
        /// </summary>
        public class Dummy
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Vector indicating the dummy point's forward direction.
            /// </summary>
            public Vector3 Forward;

            /// <summary>
            /// Vector indicating the dummy point's upward direction.
            /// </summary>
            public Vector3 Upward;

            /// <summary>
            /// Indicates the type of dummy point this is (hitbox, sfx, etc).
            /// </summary>
            public short ReferenceID;

            /// <summary>
            /// Presumably the index of a bone the dummy points would be listed under in an editor. Not known to mean anything ingame.
            /// </summary>
            public short DummyBoneIndex;

            /// <summary>
            /// Index of the bone that the dummy point follows physically.
            /// </summary>
            public short AttachBoneIndex;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk0C, Unk0D;

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk0E;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Flag1, Flag2;

            internal Dummy(BinaryReaderEx br)
            {
                Position = br.ReadVector3();

                Unk0C = br.ReadByte();
                Unk0D = br.ReadByte();
                Unk0E = br.ReadInt16();

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

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Position);

                bw.WriteByte(Unk0C);
                bw.WriteByte(Unk0D);
                bw.WriteInt16(Unk0E);

                bw.WriteVector3(Forward);

                bw.WriteInt16(ReferenceID);
                bw.WriteInt16(DummyBoneIndex);

                bw.WriteVector3(Upward);

                bw.WriteInt16(AttachBoneIndex);
                bw.WriteBoolean(Flag1);
                bw.WriteBoolean(Flag2);

                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
        }

        /// <summary>
        /// A reference to an MTD file, with params for specific textures used.
        /// </summary>
        public class Material
        {
            /// <summary>
            /// Identifies the mesh that uses this material, may include keywords that determine hideable parts.
            /// </summary>
            public string Name;

            /// <summary>
            /// Virtual path to an MTD file.
            /// </summary>
            public string MTD;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Flags;

            /// <summary>
            /// External params of the MTD, usually specifying textures to use.
            /// </summary>
            public List<MaterialParam> Params;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte[] GXBytes;

            private int paramIndex, paramCount;

            internal Material(BinaryReaderEx br)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                paramCount = br.ReadInt32();
                paramIndex = br.ReadInt32();
                Flags = br.ReadInt32();
                int gxOffset = br.ReadInt32();

                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = br.GetUTF16(nameOffset);
                MTD = br.GetUTF16(mtdOffset);

                if (gxOffset == 0)
                {
                    GXBytes = null;
                }
                else
                {
                    br.StepIn(gxOffset);

                    // Other than the terminating section, should be GX** in ASCII
                    int section;
                    do
                    {
                        section = br.ReadInt32();
                        br.ReadInt32();
                        br.Skip(br.ReadInt32() - 0xC);
                    } while (section != 0x7FFFFFFF);

                    GXBytes = br.GetBytes(gxOffset, (int)br.Position - gxOffset);
                    br.StepOut();
                }
            }

            internal void TakeParams(Dictionary<int, MaterialParam> paramDict)
            {
                Params = new List<MaterialParam>(paramCount);
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
                bw.ReserveInt32($"MaterialUnk{index}");

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

            internal void WriteUnkGX(BinaryWriterEx bw, int index)
            {
                if (GXBytes == null)
                {
                    bw.FillInt32($"MaterialUnk{index}", 0);
                }
                else
                {
                    bw.FillInt32($"MaterialUnk{index}", (int)bw.Position);
                    bw.WriteBytes(GXBytes);
                }
            }

            /// <summary>
            /// Returns the name and MTD path of the material.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} | {MTD}";
            }
        }

        /// <summary>
        /// Bones available for vertices to be weighted to.
        /// </summary>
        public class Bone
        {
            /// <summary>
            /// Corresponds to the name of a bone in the parent skeleton. May also have a dummy name.
            /// </summary>
            public string Name;

            /// <summary>
            /// Index of the parent in this FLVER's bone collection, or -1 for none.
            /// </summary>
            public short ParentIndex;

            /// <summary>
            /// Index of the first child in this FLVER's bone collection, or -1 for none.
            /// </summary>
            public short ChildIndex;

            /// <summary>
            /// Index of the next child of this bone's parent, or -1 for none.
            /// </summary>
            public short NextSiblingIndex;

            /// <summary>
            /// Index of the previous child of this bone's parent, or -1 for none.
            /// </summary>
            public short PreviousSiblingIndex;

            /// <summary>
            /// Translation of this bone.
            /// </summary>
            public Vector3 Translation;

            /// <summary>
            /// Rotation of this bone; euler radians.
            /// </summary>
            public Vector3 Rotation;

            /// <summary>
            /// Scale of this bone.
            /// </summary>
            public Vector3 Scale;

            /// <summary>
            /// Minimum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMin;

            /// <summary>
            /// Maximum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMax;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Nub;

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
                bw.WriteVector3(Rotation);
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

            /// <summary>
            /// Returns the name of this bone.
            /// </summary>
            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// An individual chunk of a model.
        /// </summary>
        public class Mesh
        {
            /// <summary>
            /// Unknown. Seems to always be true.
            /// </summary>
            public bool Dynamic;

            /// <summary>
            /// Index of the material used by all triangles in this mesh.
            /// </summary>
            public int MaterialIndex;

            /// <summary>
            /// Apparently does nothing. Usually points to a dummy bone named after the model, possibly just for labelling.
            /// </summary>
            public int DefaultBoneIndex;

            /// <summary>
            /// Indexes of bones in the bone collection which may be used by vertices in this mesh.
            /// </summary>
            public List<int> BoneIndices;

            /// <summary>
            /// Triangles in this mesh.
            /// </summary>
            public List<FaceSet> FaceSets;

            /// <summary>
            /// Vertex groups in this mesh.
            /// </summary>
            public List<VertexGroup> VertexGroups;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1;

            private float[] unkFloats;

            private int[] faceSetIndices, vertexGroupIndices;

            internal Mesh(BinaryReaderEx br, int version)
            {
                Dynamic = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                MaterialIndex = br.ReadInt32();
                br.AssertInt32(0);
                if (version <= 0x20010)
                    br.AssertInt32(0);
                DefaultBoneIndex = br.ReadInt32();

                int boneCount = br.ReadInt32();
                Unk1 = br.AssertInt32(0, 1, 0xA);
                if (version >= 0x20013)
                {
                    int unkOffset = br.ReadInt32();
                    // Always 6 as far as I can tell, even when <6 facesets.
                    unkFloats = br.GetSingles(unkOffset, 6);
                }
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
                FaceSets = new List<FaceSet>(faceSetIndices.Length);
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
                VertexGroups = new List<VertexGroup>(vertexGroupIndices.Length);
                foreach (int i in vertexGroupIndices)
                {
                    if (!vertexGroupDict.ContainsKey(i))
                        throw new NotSupportedException("Vertex group not found or already taken: " + i);

                    VertexGroups.Add(vertexGroupDict[i]);
                    vertexGroupDict.Remove(i);
                }
                vertexGroupIndices = null;
            }

            internal void Write(BinaryWriterEx bw, int index, int version)
            {
                bw.WriteBoolean(Dynamic);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(MaterialIndex);
                bw.WriteInt32(0);
                if (version <= 0x20010)
                    bw.WriteInt32(0);
                bw.WriteInt32(DefaultBoneIndex);

                bw.WriteInt32(BoneIndices.Count);
                bw.WriteInt32(Unk1);
                if (version >= 0x20013)
                    bw.ReserveInt32($"MeshUnk{index}");
                bw.ReserveInt32($"MeshBoneIndices{index}");

                bw.WriteInt32(FaceSets.Count);
                bw.ReserveInt32($"MeshFaceSetIndices{index}");

                bw.WriteInt32(VertexGroups.Count);
                bw.ReserveInt32($"MeshVertexGroupIndices{index}");
            }

            internal void WriteUnkFloats(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"MeshUnk{index}", (int)bw.Position);
                bw.WriteSingles(unkFloats);
            }

            internal void WriteBoneIndices(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"MeshBoneIndices{index}", (int)bw.Position);
                bw.WriteInt32s(BoneIndices.ToArray());
            }
        }

        /// <summary>
        /// Determines how vertices in a vertex group are connected to form triangles.
        /// </summary>
        public class FaceSet
        {
            /// <summary>
            /// Flags on a faceset, mostly just used to determine lod level.
            /// </summary>
            [Flags]
            public enum FSFlags : uint
            {
                /// <summary>
                /// Just your average everyday face set.
                /// </summary>
                None = 0,

                /// <summary>
                /// Low detail mesh.
                /// </summary>
                LodLevel1 = 0x01000000,

                /// <summary>
                /// Really low detail mesh.
                /// </summary>
                LodLevel2 = 0x02000000,

                /// <summary>
                /// Some meshes have what appears to be a copy of each faceset, and the copy has this flag.
                /// </summary>
                Unk80000000 = 0x80000000,
            }

            /// <summary>
            /// FaceSet Flags on this FaceSet.
            /// </summary>
            public FSFlags Flags;

            /// <summary>
            /// Whether vertices are defined as a triangle strip or individual triangles.
            /// </summary>
            public bool TriangleStrip;

            /// <summary>
            /// Whether triangles can be seen through from behind.
            /// </summary>
            public bool CullBackfaces;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk06;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk07;

            /// <summary>
            /// Bits per index; 0 or 16 for shorts, 32 for ints.
            /// </summary>
            public int IndexSize;

            /// <summary>
            /// Indexes to vertices in a vertex group.
            /// </summary>
            public uint[] Vertices;

            internal FaceSet(BinaryReaderEx br, int dataOffset)
            {
                Flags = (FSFlags)br.ReadUInt32();

                TriangleStrip = br.ReadBoolean();
                CullBackfaces = br.ReadBoolean();
                Unk06 = br.ReadByte();
                Unk07 = br.ReadByte();

                int vertexCount = br.ReadInt32();
                int vertexOffset = br.ReadInt32();
                int vertexSize = br.ReadInt32();

                br.AssertInt32(0);
                IndexSize = br.AssertInt32(0, 16, 32);
                br.AssertInt32(0);

                if (IndexSize == 0 || IndexSize == 16)
                    Vertices = br.GetUInt16s(dataOffset + vertexOffset, vertexCount).Select(i => (uint)i).ToArray();
                else if (IndexSize == 32)
                    Vertices = br.GetUInt32s(dataOffset + vertexOffset, vertexCount);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteUInt32((uint)Flags);

                bw.WriteBoolean(TriangleStrip);
                bw.WriteBoolean(CullBackfaces);
                bw.WriteByte(Unk06);
                bw.WriteByte(Unk07);

                bw.WriteInt32(Vertices.Length);
                bw.ReserveInt32($"FaceSetVertices{index}");
                bw.WriteInt32(Vertices.Length * 2);

                bw.WriteInt32(0);
                bw.WriteInt32(IndexSize);
                bw.WriteInt32(0);
            }

            internal void WriteVertices(BinaryWriterEx bw, int index, int dataStart)
            {
                bw.FillInt32($"FaceSetVertices{index}", (int)bw.Position - dataStart);
                if (IndexSize == 0 || IndexSize == 16)
                    bw.WriteUInt16s(Vertices.Select(i => checked((ushort)i)).ToArray());
                else if (IndexSize == 32)
                    bw.WriteUInt32s(Vertices);
            }
        }

        /// <summary>
        /// A collection of vertices with a layout indicating what information they contain.
        /// </summary>
        public class VertexGroup
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk00;

            /// <summary>
            /// Index to a layout in the FLVER's layout collection.
            /// </summary>
            public int VertexStructLayoutIndex;

            /// <summary>
            /// Vertices in this vertex group.
            /// </summary>
            public List<Vertex> Vertices;

            /// <summary>
            /// Size of the data for each vertex; -1 means it matches the VSL size, which it should, but often doesn't in DSR.
            /// </summary>
            public int VertexSize;

            private int vertexCount, vertexBufferOffset;

            internal VertexGroup(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                VertexStructLayoutIndex = br.ReadInt32();
                VertexSize = br.ReadInt32();
                vertexCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(VertexSize * vertexCount);
                vertexBufferOffset = br.ReadInt32();
            }

            internal void ReadVertices(BinaryReaderEx br, int dataOffset, List<VertexStructLayout> layouts, int version)
            {
                VertexStructLayout layout = layouts[VertexStructLayoutIndex];
                Vertices = new List<Vertex>(vertexCount);
                int vertexDataStart = dataOffset + vertexBufferOffset;
                br.StepIn(vertexDataStart);
                for (int i = 0; i < vertexCount; i++)
                {
                    br.Position = vertexDataStart + i * VertexSize;
                    Vertices.Add(new Vertex(br, layout, VertexSize, version));
                }
                br.StepOut();

                if (VertexSize == layout.Size)
                    VertexSize = -1;

                vertexCount = -1;
                vertexBufferOffset = -1;
            }

            internal void Write(BinaryWriterEx bw, int index, List<VertexStructLayout> layouts)
            {
                VertexStructLayout layout = layouts[VertexStructLayoutIndex];

                bw.WriteInt32(Unk00);
                bw.WriteInt32(VertexStructLayoutIndex);

                int vertexSize = VertexSize == -1 ? layout.Size : VertexSize;
                bw.WriteInt32(vertexSize);

                bw.WriteInt32(Vertices.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(vertexSize * Vertices.Count);
                bw.ReserveInt32($"VertexGroupVertices{index}");
            }

            internal void WriteVertices(BinaryWriterEx bw, int index, int dataStart, List<VertexStructLayout> layouts, int version)
            {
                VertexStructLayout layout = layouts[VertexStructLayoutIndex];
                bw.FillInt32($"VertexGroupVertices{index}", (int)bw.Position - dataStart);

                int vertexSize = VertexSize == -1 ? layout.Size : VertexSize;
                foreach (Vertex vertex in Vertices)
                    vertex.Write(bw, layout, vertexSize, version);
            }
        }

        /// <summary>
        /// Determines which properties of a vertex are read and written, and in what order and format.
        /// </summary>
        public class VertexStructLayout : List<VertexStructLayout.Member>
        {
            /// <summary>
            /// The total size of all ValueTypes in this layout.
            /// </summary>
            public int Size
            {
                get
                {
                    return this.Aggregate(0, (size, member) => size + member.Size);
                }
            }

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

                // Make sure no semantics repeat that aren't known to
                var semantics = new List<Member.MemberSemantic>();
                foreach (Member member in this)
                {
                    if (member.Semantic != Member.MemberSemantic.VertexColor
                        && member.Semantic != Member.MemberSemantic.UV
                        && member.Semantic != Member.MemberSemantic.Tangent)
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

            /// <summary>
            /// Represents one property of a vertex.
            /// </summary>
            public class Member
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00;

                /// <summary>
                /// Offset of this member from the start of a vertex struct.
                /// </summary>
                public int StructOffset;

                /// <summary>
                /// Format used to store this member.
                /// </summary>
                public MemberValueType ValueType;

                /// <summary>
                /// Vertex property being stored.
                /// </summary>
                public MemberSemantic Semantic;

                /// <summary>
                /// For semantics that may appear more than once such as UVs, which one this member is.
                /// </summary>
                public int Index;

                /// <summary>
                /// The size of this member's ValueType, in bytes.
                /// </summary>
                public int Size
                {
                    get
                    {
                        switch (ValueType)
                        {
                            case MemberValueType.Byte4A:
                            case MemberValueType.Byte4B:
                            case MemberValueType.Short2toFloat2:
                            case MemberValueType.Byte4C:
                            case MemberValueType.UV:
                            case MemberValueType.Byte4E:
                                return 4;

                            case MemberValueType.Float2:
                            case MemberValueType.UVPair:
                            case MemberValueType.ShortBoneIndices:
                            case MemberValueType.Short4toFloat4A:
                            case MemberValueType.Short4toFloat4B:
                                return 8;

                            case MemberValueType.Float3:
                                return 12;

                            case MemberValueType.Float4:
                                return 16;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                internal Member(BinaryReaderEx br)
                {
                    Unk00 = br.AssertInt32(0, 1);
                    StructOffset = br.ReadInt32();
                    ValueType = br.ReadEnum32<MemberValueType>();
                    Semantic = br.ReadEnum32<MemberSemantic>();
                    Index = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(StructOffset);
                    bw.WriteUInt32((uint)ValueType);
                    bw.WriteUInt32((uint)Semantic);
                    bw.WriteInt32(Index);
                }

                /// <summary>
                /// Returns the value type and semantic of this member.
                /// </summary>
                public override string ToString()
                {
                    return $"{ValueType}: {Semantic}";
                }

                /// <summary>
                /// Format of a vertex property.
                /// </summary>
                public enum MemberValueType : uint
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

        /// <summary>
        /// An MTD external parameter.
        /// </summary>
        public class MaterialParam
        {
            /// <summary>
            /// The external parameter of the MTD.
            /// </summary>
            public string Param;

            /// <summary>
            /// The value of the external parameter.
            /// </summary>
            public string Value;

            /// <summary>
            /// Unknown.
            /// </summary>
            public float ScaleX;

            /// <summary>
            /// Unknown.
            /// </summary>
            public float ScaleY;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk10;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk11;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14, Unk1C;

            internal MaterialParam(BinaryReaderEx br)
            {
                int valueOffset = br.ReadInt32();
                int paramOffset = br.ReadInt32();
                ScaleX = br.ReadSingle();
                ScaleY = br.ReadSingle();

                Unk10 = br.AssertByte(0, 1, 2);
                Unk11 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);

                Unk14 = br.ReadInt32();
                br.AssertInt32(0);
                Unk1C = br.ReadInt32();

                Param = br.GetUTF16(paramOffset);
                Value = br.GetUTF16(valueOffset);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"MaterialParamValue{index}");
                bw.ReserveInt32($"MaterialParamParam{index}");
                bw.WriteSingle(ScaleX);
                bw.WriteSingle(ScaleY);

                bw.WriteByte(Unk10);
                bw.WriteBoolean(Unk11);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(Unk14);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk1C);
            }

            /// <summary>
            /// Returns the param name and value of this param.
            /// </summary>
            public override string ToString()
            {
                return $"{Param} = {Value}";
            }
        }

        /// <summary>
        /// A single point in a vertex group.
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
            /// Orientation of the vertex.
            /// </summary>
            public Vector4 Normal;

            /// <summary>
            /// Unknown. Items must be 4 length.
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
            /// Extra data in the vertex struct not accounted for by the VSL. Should be null for none, but often isn't in DSR.
            /// </summary>
            public byte[] ExtraBytes;

            internal Vertex(BinaryReaderEx br, VertexStructLayout layout, int vertexSize, int version)
            {
                Position = Vector3.Zero;
                BoneIndices = null;
                BoneWeights = null;
                UVs = new List<Vector3>();
                Normal = Vector4.Zero;
                Tangents = new List<Vector4>();
                Colors = new List<Color>();
                UnknownVector4 = null;
                ExtraBytes = null;

                float uvFactor = 1024;
                if (version == 0x20009 || version >= 0x20010)
                    uvFactor = 2048;

                int currentSize = 0;
                foreach (VertexStructLayout.Member member in layout)
                {
                    if (currentSize + member.Size > vertexSize)
                        break;
                    else
                        currentSize += member.Size;

                    switch (member.Semantic)
                    {
                        case VertexStructLayout.Member.MemberSemantic.Position:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float3)
                            {
                                Position = br.ReadVector3();
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.BoneWeights:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadSByte() / (float)sbyte.MaxValue;
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Short4toFloat4A)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadInt16() / (float)short.MaxValue;
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.BoneIndices:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.ShortBoneIndices)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadUInt16();
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4E)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.Normal:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float4)
                            {
                                Normal = br.ReadVector4();
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Short4toFloat4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadUInt16() - 32767) / 32767f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.UV:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float2)
                            {
                                UVs.Add(new Vector3(br.ReadVector2() / uvFactor, 0));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float3)
                            {
                                UVs.Add(br.ReadVector3() / uvFactor);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Short2toFloat2)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.UV)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.UVPair)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.Tangent:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.UnknownVector4A:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                UnknownVector4 = br.ReadBytes(4);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                UnknownVector4 = br.ReadBytes(4);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.VertexColor:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float4)
                            {
                                // TODO
                                byte[] bytes = new byte[4];
                                for (int i = 0; i < 4; i++)
                                    bytes[i] = (byte)(br.ReadSingle() * byte.MaxValue);
                                Colors.Add(Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                byte[] bytes = br.ReadBytes(4);
                                Colors.Add(Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                byte[] bytes = br.ReadBytes(4);
                                Colors.Add(Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                if (currentSize < vertexSize)
                    ExtraBytes = br.ReadBytes(vertexSize - currentSize);
            }

            internal void Write(BinaryWriterEx bw, VertexStructLayout layout, int vertexSize, int version)
            {
                var uvQueue = new Queue<Vector3>(UVs);

                float uvFactor = 1024;
                if (version == 0x20009 || version >= 0x20010)
                    uvFactor = 2048;

                int currentSize = 0;
                foreach (VertexStructLayout.Member member in layout)
                {
                    if (currentSize + member.Size > vertexSize)
                        break;
                    else
                        currentSize += member.Size;

                    switch (member.Semantic)
                    {
                        case VertexStructLayout.Member.MemberSemantic.Position:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float3)
                            {
                                bw.WriteVector3(Position);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.BoneWeights:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteSByte((sbyte)(BoneWeights[i] * sbyte.MaxValue));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Short4toFloat4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteInt16((short)(BoneWeights[i] * short.MaxValue));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.BoneIndices:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteByte((byte)BoneIndices[i]);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.ShortBoneIndices)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteUInt16((ushort)BoneIndices[i]);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4E)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteByte((byte)BoneIndices[i]);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.Normal:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float4)
                            {
                                bw.WriteVector4(Normal);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                bw.WriteByte((byte)(Normal.X * 127 + 127));
                                bw.WriteByte((byte)(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)(Normal.W * 127 + 127));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                bw.WriteByte((byte)(Normal.X * 127 + 127));
                                bw.WriteByte((byte)(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)(Normal.W * 127 + 127));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                bw.WriteByte((byte)(Normal.X * 127 + 127));
                                bw.WriteByte((byte)(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)(Normal.W * 127 + 127));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Short4toFloat4B)
                            {
                                bw.WriteInt16((short)(Normal.X * 32767 + 32767));
                                bw.WriteInt16((short)(Normal.Y * 32767 + 32767));
                                bw.WriteInt16((short)(Normal.Z * 32767 + 32767));
                                bw.WriteInt16((short)(Normal.W * 32767 + 32767));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.UV:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float2)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteSingle(uv.X);
                                bw.WriteSingle(uv.Y);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float3)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteVector3(uv);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Short2toFloat2)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.UV)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.UVPair)
                            {
                                Vector3 uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);

                                uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)uv.X);
                                bw.WriteInt16((short)uv.Y);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.Tangent:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                Vector4 tangent = Tangents[member.Index];
                                bw.WriteByte((byte)(tangent.X * 127 + 127));
                                bw.WriteByte((byte)(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)(tangent.W * 127 + 127));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                Vector4 tangent = Tangents[member.Index];
                                bw.WriteByte((byte)(tangent.X * 127 + 127));
                                bw.WriteByte((byte)(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)(tangent.W * 127 + 127));
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                Vector4 tangent = Tangents[member.Index];
                                bw.WriteByte((byte)(tangent.X * 127 + 127));
                                bw.WriteByte((byte)(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)(tangent.W * 127 + 127));
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.UnknownVector4A:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4B)
                            {
                                bw.WriteBytes(UnknownVector4);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                bw.WriteBytes(UnknownVector4);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case VertexStructLayout.Member.MemberSemantic.VertexColor:
                            if (member.ValueType == VertexStructLayout.Member.MemberValueType.Float4)
                            {
                                Color color = Colors[member.Index];
                                bw.WriteSingle(color.R / (float)byte.MaxValue);
                                bw.WriteSingle(color.G / (float)byte.MaxValue);
                                bw.WriteSingle(color.B / (float)byte.MaxValue);
                                bw.WriteSingle(color.A / (float)byte.MaxValue);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4A)
                            {
                                Color color = Colors[member.Index];
                                bw.WriteByte(color.A);
                                bw.WriteByte(color.R);
                                bw.WriteByte(color.G);
                                bw.WriteByte(color.B);
                            }
                            else if (member.ValueType == VertexStructLayout.Member.MemberValueType.Byte4C)
                            {
                                Color color = Colors[member.Index];
                                bw.WriteByte(color.R);
                                bw.WriteByte(color.G);
                                bw.WriteByte(color.B);
                                bw.WriteByte(color.A);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                if (currentSize < vertexSize)
                    bw.WriteBytes(ExtraBytes);
            }
        }
    }
}
