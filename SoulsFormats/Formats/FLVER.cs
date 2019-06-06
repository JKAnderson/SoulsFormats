using System;
using System.Collections.Generic;
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
        public List<BufferLayout> BufferLayouts;

        /// <summary>
        /// Unknown; only present in Sekiro.
        /// </summary>
        public SekiroUnkStruct SekiroUnk;

        /// <summary>
        /// Creates a new FLVER with a default header and empty lists.
        /// </summary>
        public FLVER()
        {
            Header = new FLVERHeader();
            Dummies = new List<Dummy>();
            Materials = new List<Material>();
            Bones = new List<Bone>();
            Meshes = new List<Mesh>();
            BufferLayouts = new List<BufferLayout>();
        }

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
            Header.BigEndian = br.AssertASCII("L\0", "B\0") == "B\0";
            br.BigEndian = Header.BigEndian;

            // DS1: 2000C, 2000D
            // DS2: 20009, 20010
            // SFS: 20010
            // BB:  20013, 20014
            // DS3: 20013, 20014
            // SDT: 2001A
            Header.Version = br.AssertInt32(0x20009, 0x2000C, 0x2000D, 0x20010, 0x20013, 0x20014, 0x20016, 0x2001A);

            int dataOffset = br.ReadInt32();
            int dataSize = br.ReadInt32();
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            int vertexBufferCount = br.ReadInt32();

            Header.BoundingBoxMin = br.ReadVector3();
            Header.BoundingBoxMax = br.ReadVector3();

            Header.Unk40 = br.ReadInt32();
            int totalFaceCount = br.ReadInt32();

            Header.Unk48 = br.AssertByte(0x00, 0x10);
            br.AssertBoolean(true);
            Header.Unk4A = br.ReadBoolean();
            br.AssertByte(0);

            br.AssertInt16(0);
            Header.Unk4E = br.AssertInt16(0, -1);

            int faceSetCount = br.ReadInt32();
            int bufferLayoutCount = br.ReadInt32();
            int textureCount = br.ReadInt32();

            Header.Unk5C = br.ReadInt32();
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

            var vertexBuffers = new List<VertexBuffer>(vertexBufferCount);
            for (int i = 0; i < vertexBufferCount; i++)
                vertexBuffers.Add(new VertexBuffer(br));

            BufferLayouts = new List<BufferLayout>(bufferLayoutCount);
            for (int i = 0; i < bufferLayoutCount; i++)
                BufferLayouts.Add(new BufferLayout(br));

            var textures = new List<Texture>(textureCount);
            for (int i = 0; i < textureCount; i++)
                textures.Add(new Texture(br));

            if (Header.Version >= 0x2001A)
                SekiroUnk = new SekiroUnkStruct(br);

            Dictionary<int, Texture> textureDict = Dictionize(textures);
            foreach (Material material in Materials)
            {
                material.TakeTextures(textureDict);
            }
            if (textureDict.Count != 0)
                throw new NotSupportedException("Orphaned textures found.");

            Dictionary<int, FaceSet> faceSetDict = Dictionize(faceSets);
            Dictionary<int, VertexBuffer> vertexBufferDict = Dictionize(vertexBuffers);
            foreach (Mesh mesh in Meshes)
            {
                mesh.TakeFaceSets(faceSetDict);
                mesh.TakeVertexBuffers(vertexBufferDict, BufferLayouts);
                mesh.ReadVertices(br, dataOffset, BufferLayouts, Header.Version);
            }
            if (faceSetDict.Count != 0)
                throw new NotSupportedException("Orphaned face sets found.");
            if (vertexBufferDict.Count != 0)
                throw new NotSupportedException("Orphaned vertex buffers found.");
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

            int vertexBufferCount = 0;
            foreach (Mesh mesh in Meshes)
                vertexBufferCount += mesh.VertexBuffers.Count;
            bw.WriteInt32(vertexBufferCount);

            bw.WriteVector3(Header.BoundingBoxMin);
            bw.WriteVector3(Header.BoundingBoxMax);

            bw.WriteInt32(Header.Unk40);

            // I hope this isn't super slow :^)
            int totalFaceCount = 0;
            foreach (Mesh mesh in Meshes)
                foreach (FaceSet faceSet in mesh.FaceSets)
                    totalFaceCount += faceSet.GetFaces().Count;
            bw.WriteInt32(totalFaceCount);

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

            bw.WriteInt32(BufferLayouts.Count);

            int textureCount = 0;
            foreach (Material material in Materials)
                textureCount += material.Textures.Count;
            bw.WriteInt32(textureCount);
            bw.WriteInt32(Header.Unk5C);
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

            int vertexBufferIndex = 0;
            foreach (Mesh mesh in Meshes)
            {
                for (int i = 0; i < mesh.VertexBuffers.Count; i++)
                    mesh.VertexBuffers[i].Write(bw, vertexBufferIndex + i, BufferLayouts, mesh.Vertices.Count);
                vertexBufferIndex += mesh.VertexBuffers.Count;
            }

            for (int i = 0; i < BufferLayouts.Count; i++)
                BufferLayouts[i].Write(bw, i);

            int textureIndex = 0;
            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].WriteTextures(bw, i, textureIndex);
                textureIndex += Materials[i].Textures.Count;
            }

            if (Header.Version >= 0x2001A)
                SekiroUnk.Write(bw);

            bw.Pad(0x10);
            for (int i = 0; i < BufferLayouts.Count; i++)
            {
                BufferLayouts[i].WriteMembers(bw, i);
            }

            if (Header.Version >= 0x20013)
            {
                bw.Pad(0x10);
                for (int i = 0; i < Meshes.Count; i++)
                {
                    Meshes[i].WriteBoundingBox(bw, i, Header.Version);
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
            vertexBufferIndex = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                bw.FillInt32($"MeshVertexBufferIndices{i}", (int)bw.Position);
                for (int j = 0; j < Meshes[i].VertexBuffers.Count; j++)
                    bw.WriteInt32(vertexBufferIndex + j);
                vertexBufferIndex += Meshes[i].VertexBuffers.Count;
            }

            bw.Pad(0x10);
            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].WriteUnkGX(bw, i);
            }

            bw.Pad(0x10);
            textureIndex = 0;
            for (int i = 0; i < Materials.Count; i++)
            {
                Material material = Materials[i];
                bw.FillInt32($"MaterialName{i}", (int)bw.Position);
                bw.WriteUTF16(material.Name, true);
                bw.FillInt32($"MaterialMTD{i}", (int)bw.Position);
                bw.WriteUTF16(material.MTD, true);

                for (int j = 0; j < material.Textures.Count; j++)
                {
                    bw.FillInt32($"TexturePath{textureIndex + j}", (int)bw.Position);
                    bw.WriteUTF16(material.Textures[j].Path, true);
                    bw.FillInt32($"TextureType{textureIndex + j}", (int)bw.Position);
                    bw.WriteUTF16(material.Textures[j].Type, true);
                }
                textureIndex += material.Textures.Count;
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

            vertexBufferIndex = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                Mesh mesh = Meshes[i];

                foreach (Vertex vertex in mesh.Vertices)
                    vertex.PrepareWrite();

                for (int j = 0; j < mesh.VertexBuffers.Count; j++)
                    mesh.VertexBuffers[j].WriteBuffer(bw, vertexBufferIndex + j, BufferLayouts, mesh.Vertices, dataStart, Header.Version);

                foreach (Vertex vertex in mesh.Vertices)
                    vertex.FinishWrite();

                vertexBufferIndex += mesh.VertexBuffers.Count;
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
            public int Unk5C;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk68;

            /// <summary>
            /// Creates a new FLVERHeader with default values.
            /// </summary>
            public FLVERHeader()
            {
                BigEndian = false;
                BoundingBoxMin = Vector3.Zero;
                BoundingBoxMax = Vector3.Zero;
                Unk40 = 0;
                Unk48 = 0;
                Unk4A = false;
                Unk4E = 0;
                Unk5C = 0;
                Unk68 = 0;
            }
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

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk30, Unk34;

            /// <summary>
            /// Creates a new dummy point with default values.
            /// </summary>
            public Dummy()
            {
                Position = Vector3.Zero;
                Forward = Vector3.Zero;
                Upward = Vector3.Zero;
                ReferenceID = 0;
                DummyBoneIndex = -1;
                AttachBoneIndex = -1;
                Unk0C = 0;
                Unk0D = 0;
                Unk0E = 0;
                Flag1 = false;
                Flag2 = false;
                Unk30 = 0;
                Unk34 = 0;
            }

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

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
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

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns the dummy point's reference ID.
            /// </summary>
            public override string ToString()
            {
                return $"{ReferenceID}";
            }
        }

        /// <summary>
        /// A reference to an MTD file, specifying textures to use.
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
            /// Textures used by this material.
            /// </summary>
            public List<Texture> Textures;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte[] GXBytes;

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk18;

            private int textureIndex, textureCount;

            /// <summary>
            /// Creates a new Material with null or default values.
            /// </summary>
            public Material()
            {
                Name = null;
                MTD = null;
                Flags = 0;
                Textures = new List<Texture>();
                GXBytes = null;
                Unk18 = 0;
            }

            /// <summary>
            /// Creates a new Material with the given values and an empty texture list.
            /// </summary>
            public Material(string name, string mtd, int flags, byte[] gxBytes = null)
            {
                Name = name;
                MTD = mtd;
                Flags = flags;
                Textures = new List<Texture>();
                GXBytes = gxBytes;
                Unk18 = 0;
            }

            internal Material(BinaryReaderEx br)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                textureCount = br.ReadInt32();
                textureIndex = br.ReadInt32();
                Flags = br.ReadInt32();
                int gxOffset = br.ReadInt32();
                Unk18 = br.ReadInt32();
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

            internal void TakeTextures(Dictionary<int, Texture> textureDict)
            {
                Textures = new List<Texture>(textureCount);
                for (int i = textureIndex; i < textureIndex + textureCount; i++)
                {
                    if (!textureDict.ContainsKey(i))
                        throw new NotSupportedException("Texture not found or already taken: " + i);

                    Textures.Add(textureDict[i]);
                    textureDict.Remove(i);
                }

                textureIndex = -1;
                textureCount = -1;
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"MaterialName{index}");
                bw.ReserveInt32($"MaterialMTD{index}");
                bw.WriteInt32(Textures.Count);
                bw.ReserveInt32($"TextureIndex{index}");
                bw.WriteInt32(Flags);
                bw.ReserveInt32($"MaterialUnk{index}");
                bw.WriteInt32(Unk18);
                bw.WriteInt32(0);
            }

            internal void WriteTextures(BinaryWriterEx bw, int index, int textureIndex)
            {
                bw.FillInt32($"TextureIndex{index}", textureIndex);
                for (int i = 0; i < Textures.Count; i++)
                {
                    Textures[i].Write(bw, textureIndex + i);
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
            /// Unknown; only 0 or 1 before Sekiro.
            /// </summary>
            public int Unk3C;

            /// <summary>
            /// Creates a new Bone with default values.
            /// </summary>
            public Bone()
            {
                Name = "";
                ParentIndex = -1;
                ChildIndex = -1;
                NextSiblingIndex = -1;
                PreviousSiblingIndex = -1;
                Translation = Vector3.Zero;
                Rotation = Vector3.Zero;
                Scale = Vector3.One;
                BoundingBoxMin = Vector3.Zero;
                BoundingBoxMax = Vector3.Zero;
                Unk3C = 0;
            }

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
                Unk3C = br.ReadInt32();
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
                bw.WriteInt32(Unk3C);
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
            /// Unknown.
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
            /// Vertex buffers in this mesh.
            /// </summary>
            public List<VertexBuffer> VertexBuffers;

            /// <summary>
            /// Vertices in this mesh.
            /// </summary>
            public List<Vertex> Vertices;

            /// <summary>
            /// Minimum extent of the mesh.
            /// </summary>
            public Vector3 BoundingBoxMin;

            /// <summary>
            /// Maximum extent of the mesh.
            /// </summary>
            public Vector3 BoundingBoxMax;

            /// <summary>
            /// Unknown; only present in Sekiro.
            /// </summary>
            public Vector3 BoundingBoxUnk;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1;

            private int[] faceSetIndices, vertexBufferIndices;

            /// <summary>
            /// Creates a new Mesh with default values.
            /// </summary>
            public Mesh()
            {
                Dynamic = false;
                MaterialIndex = 0;
                DefaultBoneIndex = -1;
                BoneIndices = new List<int>();
                FaceSets = new List<FaceSet>();
                VertexBuffers = new List<VertexBuffer>();
                Vertices = new List<Vertex>();
                BoundingBoxMin = Vector3.Zero;
                BoundingBoxMax = Vector3.Zero;
                BoundingBoxUnk = Vector3.Zero;
                Unk1 = 0;
            }

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
                Unk1 = br.AssertInt32(0, 1, 10);
                if (version >= 0x20013)
                {
                    int boundingBoxOffset = br.ReadInt32();
                    br.StepIn(boundingBoxOffset);
                    {
                        BoundingBoxMin = br.ReadVector3();
                        BoundingBoxMax = br.ReadVector3();
                        if (version >= 0x2001A)
                            BoundingBoxUnk = br.ReadVector3();
                    }
                    br.StepOut();
                }
                int boneOffset = br.ReadInt32();
                BoneIndices = new List<int>(br.GetInt32s(boneOffset, boneCount));

                int faceSetCount = br.ReadInt32();
                int faceSetOffset = br.ReadInt32();
                faceSetIndices = br.GetInt32s(faceSetOffset, faceSetCount);

                int vertexBufferCount = br.AssertInt32(1, 2, 3);
                int vertexBufferOffset = br.ReadInt32();
                vertexBufferIndices = br.GetInt32s(vertexBufferOffset, vertexBufferCount);
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

            internal void TakeVertexBuffers(Dictionary<int, VertexBuffer> vertexBufferDict, List<BufferLayout> layouts)
            {
                VertexBuffers = new List<VertexBuffer>(vertexBufferIndices.Length);
                foreach (int i in vertexBufferIndices)
                {
                    if (!vertexBufferDict.ContainsKey(i))
                        throw new NotSupportedException("Vertex buffer not found or already taken: " + i);

                    VertexBuffers.Add(vertexBufferDict[i]);
                    vertexBufferDict.Remove(i);
                }
                vertexBufferIndices = null;

                // Make sure no semantics repeat that aren't known to
                var semantics = new List<BufferLayout.MemberSemantic>();
                foreach (VertexBuffer buffer in VertexBuffers)
                {
                    foreach (var member in layouts[buffer.LayoutIndex])
                    {
                        if (member.Semantic != BufferLayout.MemberSemantic.UV
                            && member.Semantic != BufferLayout.MemberSemantic.Tangent
                            && member.Semantic != BufferLayout.MemberSemantic.VertexColor
                            && member.Semantic != BufferLayout.MemberSemantic.Position
                            && member.Semantic != BufferLayout.MemberSemantic.Normal)
                        {
                            if (semantics.Contains(member.Semantic))
                                throw new NotImplementedException("Unexpected semantic list.");
                            semantics.Add(member.Semantic);
                        }
                    }
                }

                for (int i = 0; i < VertexBuffers.Count; i++)
                {
                    VertexBuffer buffer = VertexBuffers[i];
                    if (buffer.BufferIndex != i)
                        throw new FormatException("Unexpected vertex buffer indices.");

                    BufferLayout layout = layouts[buffer.LayoutIndex];
                    if (layout.Size != buffer.VertexSize)
                        throw new FormatException("Mismatched vertex sizes are not supported for split buffers.");
                }
            }

            internal void ReadVertices(BinaryReaderEx br, int dataOffset, List<BufferLayout> layouts, int version)
            {
                int vertexCount = VertexBuffers[0].VertexCount;
                Vertices = new List<Vertex>(vertexCount);
                for (int i = 0; i < vertexCount; i++)
                    Vertices.Add(new Vertex());

                foreach (VertexBuffer buffer in VertexBuffers)
                    buffer.ReadBuffer(br, layouts, Vertices, dataOffset, version);
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
                    bw.ReserveInt32($"MeshBoundingBox{index}");
                bw.ReserveInt32($"MeshBoneIndices{index}");

                bw.WriteInt32(FaceSets.Count);
                bw.ReserveInt32($"MeshFaceSetIndices{index}");

                bw.WriteInt32(VertexBuffers.Count);
                bw.ReserveInt32($"MeshVertexBufferIndices{index}");
            }

            internal void WriteBoundingBox(BinaryWriterEx bw, int index, int version)
            {
                bw.FillInt32($"MeshBoundingBox{index}", (int)bw.Position);
                bw.WriteVector3(BoundingBoxMin);
                bw.WriteVector3(BoundingBoxMax);
                if (version >= 0x2001A)
                    bw.WriteVector3(BoundingBoxUnk);
            }

            internal void WriteBoneIndices(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"MeshBoneIndices{index}", (int)bw.Position);
                bw.WriteInt32s(BoneIndices.ToArray());
            }

            /// <summary>
            /// Returns a list of arrays of 3 vertices, each representing a triangle in the mesh.
            /// Faces are taken from the first FaceSet in the mesh with the given flags,
            /// using None by default for the highest detail mesh. If not found, the first FaceSet is used.
            /// </summary>
            public List<Vertex[]> GetFaces(FaceSet.FSFlags fsFlags = FaceSet.FSFlags.None)
            {
                FaceSet faceset = FaceSets.Find(fs => fs.Flags == fsFlags) ?? FaceSets[0];
                List<uint[]> indices = faceset.GetFaces();
                var vertices = new List<Vertex[]>(indices.Count);
                foreach (uint[] face in indices)
                    vertices.Add(new Vertex[] { Vertices[(int)face[0]], Vertices[(int)face[1]], Vertices[(int)face[2]] });
                return vertices;
            }
        }

        /// <summary>
        /// Determines how vertices in a mesh are connected to form triangles.
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
            /// Indexes to vertices in a mesh.
            /// </summary>
            public uint[] Vertices;

            /// <summary>
            /// Creates a new FaceSet with default values and null vertices.
            /// </summary>
            public FaceSet()
            {
                Flags = FSFlags.None;
                TriangleStrip = false;
                CullBackfaces = true;
                Unk06 = 0;
                Unk07 = 0;
                IndexSize = 16;
                Vertices = null;
            }

            /// <summary>
            /// Creates a new FaceSet with the specified values.
            /// </summary>
            public FaceSet(FSFlags flags, bool triangleStrip, bool cullBackfaces, byte unk06, byte unk07, int indexSize, uint[] vertices)
            {
                Flags = flags;
                TriangleStrip = triangleStrip;
                CullBackfaces = cullBackfaces;
                Unk06 = unk06;
                Unk07 = unk07;
                IndexSize = indexSize;
                Vertices = vertices;
            }

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

            /// <summary>
            /// Returns a list of arrays of 3 vertex indices, each representing one triangle in a mesh.
            /// </summary>
            public List<uint[]> GetFaces()
            {
                var faces = new List<uint[]>();
                if (TriangleStrip)
                {
                    bool flip = false;
                    for (int i = 0; i < Vertices.Length - 2; i++)
                    {
                        uint vi1 = Vertices[i];
                        uint vi2 = Vertices[i + 1];
                        uint vi3 = Vertices[i + 2];

                        if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3)
                        {
                            if (!flip)
                                faces.Add(new uint[] { vi1, vi2, vi3 });
                            else
                                faces.Add(new uint[] { vi3, vi2, vi1 });
                        }

                        flip = !flip;
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Length - 2; i += 3)
                    {
                        faces.Add(new uint[] { Vertices[i], Vertices[i + 1], Vertices[i + 2] });
                    }
                }
                return faces;
            }
        }

        /// <summary>
        /// Represents a block of vertex data.
        /// </summary>
        public class VertexBuffer
        {
            /// <summary>
            /// Index of this buffer for meshes with vertex data split into two layouts for whatever reason.
            /// </summary>
            public int BufferIndex;

            /// <summary>
            /// Index to a layout in the FLVER's layout collection.
            /// </summary>
            public int LayoutIndex;

            /// <summary>
            /// Size of the data for each vertex; -1 means it matches the buffer layout size, which it should, but often doesn't in DSR.
            /// </summary>
            public int VertexSize;

            internal int VertexCount;

            internal int BufferOffset;

            /// <summary>
            /// Creates a new VertexBuffer with the specified values.
            /// </summary>
            public VertexBuffer(int bufferIndex, int layoutIndex, int vertexSize)
            {
                BufferIndex = bufferIndex;
                LayoutIndex = layoutIndex;
                VertexSize = vertexSize;
            }

            /// <summary>
            /// Creates a new VertexBuffer with the specified layout index, buffer index 0, and vertex size -1.
            /// </summary>
            public VertexBuffer(int layoutIndex)
            {
                BufferIndex = 0;
                LayoutIndex = layoutIndex;
                VertexSize = -1;
            }

            internal VertexBuffer(BinaryReaderEx br)
            {
                BufferIndex = br.ReadInt32();
                LayoutIndex = br.ReadInt32();
                VertexSize = br.ReadInt32();
                VertexCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(VertexSize * VertexCount);
                BufferOffset = br.ReadInt32();
            }

            internal void ReadBuffer(BinaryReaderEx br, List<BufferLayout> layouts, List<Vertex> vertices, int dataOffset, int version)
            {
                BufferLayout layout = layouts[LayoutIndex];
                br.StepIn(dataOffset + BufferOffset);
                {
                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i].Read(br, layout, VertexSize, version);
                }
                br.StepOut();

                if (VertexSize == layout.Size)
                    VertexSize = -1;

                VertexCount = -1;
                BufferOffset = -1;
            }

            internal void Write(BinaryWriterEx bw, int index, List<BufferLayout> layouts, int vertexCount)
            {
                BufferLayout layout = layouts[LayoutIndex];

                bw.WriteInt32(BufferIndex);
                bw.WriteInt32(LayoutIndex);

                int vertexSize = VertexSize == -1 ? layout.Size : VertexSize;
                bw.WriteInt32(vertexSize);

                bw.WriteInt32(vertexCount);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(vertexSize * vertexCount);
                bw.ReserveInt32($"VertexBufferOffset{index}");
            }

            internal void WriteBuffer(BinaryWriterEx bw, int index, List<BufferLayout> layouts, List<Vertex> Vertices, int dataStart, int version)
            {
                BufferLayout layout = layouts[LayoutIndex];
                bw.FillInt32($"VertexBufferOffset{index}", (int)bw.Position - dataStart);

                int vertexSize = VertexSize == -1 ? layout.Size : VertexSize;
                foreach (Vertex vertex in Vertices)
                    vertex.Write(bw, layout, vertexSize, version);
            }
        }

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
                for (int i = 0; i < memberCount; i++)
                    Add(new Member(br));
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
                foreach (Member member in this)
                    member.Write(bw);
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
                public MemberType Type;

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
                public Member(int unk00, int structOffset, MemberType type, MemberSemantic semantic, int index)
                {
                    Unk00 = unk00;
                    StructOffset = structOffset;
                    Type = type;
                    Semantic = semantic;
                    Index = index;
                }

                internal Member(BinaryReaderEx br)
                {
                    Unk00 = br.AssertInt32(0, 1, 2);
                    StructOffset = br.ReadInt32();
                    Type = br.ReadEnum32<MemberType>();
                    Semantic = br.ReadEnum32<MemberSemantic>();
                    Index = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(StructOffset);
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

        /// <summary>
        /// A texture used by the shader specified in an MTD.
        /// </summary>
        public class Texture
        {
            /// <summary>
            /// The type of texture this is, corresponding to the entries in the MTD.
            /// </summary>
            public string Type;

            /// <summary>
            /// The virtual path to the texture file to use.
            /// </summary>
            public string Path;

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
            public int Unk14, Unk18, Unk1C;

            /// <summary>
            /// Creates a new Texture with default values.
            /// </summary>
            public Texture()
            {
                Type = "";
                Path = "";
                ScaleX = 1;
                ScaleY = 1;
                Unk10 = 0;
                Unk11 = false;
                Unk14 = 0;
                Unk18 = 0;
                Unk1C = 0;
            }

            /// <summary>
            /// Creates a new Texture with the specified values.
            /// </summary>
            public Texture(string type, string path, float scaleX, float scaleY, byte unk10, bool unk11, int unk14, int unk18, int unk1C)
            {
                Type = type;
                Path = path;
                ScaleX = scaleX;
                ScaleY = scaleY;
                Unk10 = unk10;
                Unk11 = unk11;
                Unk14 = unk14;
                Unk18 = unk18;
                Unk1C = unk1C;
            }

            internal Texture(BinaryReaderEx br)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                ScaleX = br.ReadSingle();
                ScaleY = br.ReadSingle();

                Unk10 = br.AssertByte(0, 1, 2);
                Unk11 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);

                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Type = br.GetUTF16(typeOffset);
                Path = br.GetUTF16(pathOffset);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"TexturePath{index}");
                bw.ReserveInt32($"TextureType{index}");
                bw.WriteSingle(ScaleX);
                bw.WriteSingle(ScaleY);

                bw.WriteByte(Unk10);
                bw.WriteBoolean(Unk11);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }

            /// <summary>
            /// Returns this texture's type and path.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} = {Path}";
            }
        }

        /// <summary>
        /// Unknown; only present in Sekiro.
        /// </summary>
        public class SekiroUnkStruct
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Member> Members1, Members2;

            internal SekiroUnkStruct(BinaryReaderEx br)
            {
                short count1 = br.ReadInt16();
                short count2 = br.ReadInt16();
                uint offset1 = br.ReadUInt32();
                uint offset2 = br.ReadUInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                br.StepIn(offset1);
                {
                    Members1 = new List<Member>(count1);
                    for (int i = 0; i < count1; i++)
                        Members1.Add(new Member(br));
                }
                br.StepOut();

                br.StepIn(offset2);
                {
                    Members2 = new List<Member>(count2);
                    for (int i = 0; i < count2; i++)
                        Members2.Add(new Member(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16((short)Members1.Count);
                bw.WriteInt16((short)Members2.Count);
                bw.ReserveUInt32("SekiroUnkOffset1");
                bw.ReserveUInt32("SekiroUnkOffset2");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillUInt32("SekiroUnkOffset1", (uint)bw.Position);
                foreach (Member member in Members1)
                    member.Write(bw);

                bw.FillUInt32("SekiroUnkOffset2", (uint)bw.Position);
                foreach (Member member in Members2)
                    member.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Member
            {
                /// <summary>
                /// Unknown; maybe bone indices?
                /// </summary>
                public short[] Unk00;

                /// <summary>
                /// Unknown; seems to just count up from 0.
                /// </summary>
                public int Index;

                internal Member(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt16s(4);
                    Index = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16s(Unk00);
                    bw.WriteInt32(Index);
                    bw.WriteInt32(0);
                }
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
            public List<Vector3> Positions;

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
            public List<Vector4> Normals;

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
            /// Extra data in the vertex struct not accounted for by the buffer layout. Should be null for none, but often isn't in DSR.
            /// </summary>
            public byte[] ExtraBytes;

            private Queue<Vector3> positionQueue;
            private Queue<Vector4> normalQueue;
            private Queue<Vector3> uvQueue;
            private Queue<Vector4> tangentQueue;
            private Queue<Color> colorQueue;

            /// <summary>
            /// Create a new Vertex with null or empty values.
            /// </summary>
            public Vertex()
            {
                Positions = new List<Vector3>();
                BoneIndices = null;
                BoneWeights = null;
                UVs = new List<Vector3>();
                Normals = new List<Vector4>();
                Tangents = new List<Vector4>();
                Colors = new List<Color>();
                UnknownVector4 = null;
                ExtraBytes = null;
            }

            /// <summary>
            /// Creates a new Vertex with values copied from another.
            /// </summary>
            public Vertex(Vertex clone)
            {
                Positions = new List<Vector3>(clone.Positions);
                BoneIndices = (int[])clone.BoneIndices?.Clone();
                BoneWeights = (float[])clone.BoneWeights?.Clone();
                UVs = new List<Vector3>(clone.UVs);
                Normals = new List<Vector4>(clone.Normals);
                Tangents = new List<Vector4>(clone.Tangents);
                Colors = new List<Color>(clone.Colors);
                UnknownVector4 = (byte[])clone.UnknownVector4?.Clone();
                ExtraBytes = (byte[])clone.ExtraBytes?.Clone();
            }

            internal void Read(BinaryReaderEx br, BufferLayout layout, int vertexSize, int version)
            {
                float uvFactor = 1024;
                if (version == 0x20009 || version >= 0x20010)
                    uvFactor = 2048;

                int currentSize = 0;
                foreach (BufferLayout.Member member in layout)
                {
                    if (currentSize + member.Size > vertexSize)
                        break;
                    else
                        currentSize += member.Size;

                    switch (member.Semantic)
                    {
                        case BufferLayout.MemberSemantic.Position:
                            if (member.Type == BufferLayout.MemberType.Float3)
                            {
                                Positions.Add(br.ReadVector3());
                            }
                            else if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                Positions.Add(br.ReadVector3());
                                br.AssertSingle(0);
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
                            if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                Normals.Add(br.ReadVector4());
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normals.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normals.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normals.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadUInt16() - 32767) / 32767f;
                                Normals.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
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
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4B)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor));
                                br.AssertInt16(0);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.Tangent:
                            if (member.Type == BufferLayout.MemberType.Byte4A)
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
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.UnknownVector4A:
                            if (member.Type == BufferLayout.MemberType.Byte4B)
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

                if (currentSize < vertexSize)
                    ExtraBytes = br.ReadBytes(vertexSize - currentSize);
            }

            /// <summary>
            /// Must be called before writing any buffers. Queues list types so they will be split across buffers properly.
            /// </summary>
            internal void PrepareWrite()
            {
                positionQueue = new Queue<Vector3>(Positions);
                normalQueue = new Queue<Vector4>(Normals);
                tangentQueue = new Queue<Vector4>(Tangents);
                colorQueue = new Queue<Color>(Colors);
                uvQueue = new Queue<Vector3>(UVs);
            }

            /// <summary>
            /// Should be called after writing all buffers. Throws out queues to free memory.
            /// </summary>
            internal void FinishWrite()
            {
                positionQueue = null;
                normalQueue = null;
                tangentQueue = null;
                colorQueue = null;
                uvQueue = null;
            }

            internal void Write(BinaryWriterEx bw, BufferLayout layout, int vertexSize, int version)
            {
                float uvFactor = 1024;
                if (version == 0x20009 || version >= 0x20010)
                    uvFactor = 2048;

                int currentSize = 0;
                foreach (BufferLayout.Member member in layout)
                {
                    if (currentSize + member.Size > vertexSize)
                        break;
                    else
                        currentSize += member.Size;

                    switch (member.Semantic)
                    {
                        case BufferLayout.MemberSemantic.Position:
                            if (member.Type == BufferLayout.MemberType.Float3)
                            {
                                bw.WriteVector3(positionQueue.Dequeue());
                            }
                            else if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                bw.WriteVector3(positionQueue.Dequeue());
                                bw.WriteSingle(0);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.BoneWeights:
                            if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteSByte((sbyte)Math.Round(BoneWeights[i] * sbyte.MaxValue));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteInt16((short)Math.Round(BoneWeights[i] * short.MaxValue));
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
                            Vector4 normal = normalQueue.Dequeue();
                            if (member.Type == BufferLayout.MemberType.Float4)
                            {
                                bw.WriteVector4(normal);
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                bw.WriteByte((byte)Math.Round(normal.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                bw.WriteByte((byte)Math.Round(normal.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteByte((byte)Math.Round(normal.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(normal.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4B)
                            {
                                bw.WriteInt16((short)Math.Round(normal.X * 32767 + 32767));
                                bw.WriteInt16((short)Math.Round(normal.Y * 32767 + 32767));
                                bw.WriteInt16((short)Math.Round(normal.Z * 32767 + 32767));
                                bw.WriteInt16((short)Math.Round(normal.W * 32767 + 32767));
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
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short2toFloat2)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == BufferLayout.MemberType.UV)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == BufferLayout.MemberType.UVPair)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));

                                uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == BufferLayout.MemberType.Short4toFloat4B)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                                bw.WriteInt16((short)Math.Round(uv.Z));
                                bw.WriteInt16(0);
                            }
                            else
                                throw new NotImplementedException();
                            break;

                        case BufferLayout.MemberSemantic.Tangent:
                            Vector4 tangent = tangentQueue.Dequeue();
                            if (member.Type == BufferLayout.MemberType.Byte4A)
                            {
                                bw.WriteByte((byte)Math.Round(tangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4B)
                            {
                                bw.WriteByte((byte)Math.Round(tangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.W * 127 + 127));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteByte((byte)Math.Round(tangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.W * 127 + 127));
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
                                bw.WriteByte((byte)Math.Round(color.A * 255));
                                bw.WriteByte((byte)Math.Round(color.R * 255));
                                bw.WriteByte((byte)Math.Round(color.G * 255));
                                bw.WriteByte((byte)Math.Round(color.B * 255));
                            }
                            else if (member.Type == BufferLayout.MemberType.Byte4C)
                            {
                                bw.WriteByte((byte)Math.Round(color.R * 255));
                                bw.WriteByte((byte)Math.Round(color.G * 255));
                                bw.WriteByte((byte)Math.Round(color.B * 255));
                                bw.WriteByte((byte)Math.Round(color.A * 255));
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
    }
}
