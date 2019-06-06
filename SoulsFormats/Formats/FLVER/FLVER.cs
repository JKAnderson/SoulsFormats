using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A model format used throughout the series.
    /// </summary>
    public partial class FLVER : SoulsFile<FLVER>
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
            // SDT: 2001A, 20016 (test chr)
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
    }
}
