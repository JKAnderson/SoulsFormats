using System;
using System.Collections.Generic;
using System.Linq;
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
        public FLVERHeader Header { get; set; }

        /// <summary>
        /// Dummy polygons in this model.
        /// </summary>
        public List<Dummy> Dummies { get; set; }

        /// <summary>
        /// Materials in this model, usually one per mesh.
        /// </summary>
        public List<Material> Materials { get; set; }

        /// <summary>
        /// Lists of GX elements referenced by materials in DS2 and beyond.
        /// </summary>
        public List<GXList> GXLists { get; set; }

        /// <summary>
        /// Bones used by this model, may or may not be the full skeleton.
        /// </summary>
        public List<Bone> Bones { get; set; }

        /// <summary>
        /// Individual chunks of the model.
        /// </summary>
        public List<Mesh> Meshes { get; set; }

        /// <summary>
        /// Layouts determining how to write vertex information.
        /// </summary>
        public List<BufferLayout> BufferLayouts { get; set; }

        /// <summary>
        /// Unknown; only present in Sekiro.
        /// </summary>
        public SekiroUnkStruct SekiroUnk { get; set; }

        /// <summary>
        /// Creates a FLVER with a default header and empty lists.
        /// </summary>
        public FLVER()
        {
            Header = new FLVERHeader();
            Dummies = new List<Dummy>();
            Materials = new List<Material>();
            GXLists = new List<GXList>();
            Bones = new List<Bone>();
            Meshes = new List<Mesh>();
            BufferLayouts = new List<BufferLayout>();
        }

        /// <summary>
        /// Returns true if the data appears to be a FLVER.
        /// </summary>
        internal override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0xC)
                return false;

            string magic = br.GetASCII(0, 6);
            string endian = br.GetASCII(6, 2);
            br.BigEndian = endian == "B\0";
            int version = br.GetInt32(8);
            return magic == "FLVER\0" && version >= 0x20000;
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

            // Gundam Unicorn: 0x20005, 0x2000E
            // DS1: 2000C, 2000D
            // DS2: 20010, 20009 (armor 9320)
            // SFS: 20010
            // BB:  20013, 20014
            // DS3: 20013, 20014
            // SDT: 2001A, 20016 (test chr)
            Header.Version = br.AssertInt32(0x20005, 0x20009, 0x2000C, 0x2000D, 0x2000E, 0x20010, 0x20013, 0x20014, 0x20016, 0x2001A);

            int dataOffset = br.ReadInt32();
            br.ReadInt32(); // Data length
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            int vertexBufferCount = br.ReadInt32();

            Header.BoundingBoxMin = br.ReadVector3();
            Header.BoundingBoxMax = br.ReadVector3();

            Header.Unk40 = br.ReadInt32();
            br.ReadInt32(); // Total face count

            int vertexIndicesSize = br.AssertByte(0, 16, 32);
            Header.Unicode = br.ReadBoolean();
            Header.Unk4A = br.ReadBoolean();
            br.AssertByte(0);

            br.AssertInt16(0);
            Header.Unk4E = br.AssertInt16(0, -1);

            int faceSetCount = br.ReadInt32();
            int bufferLayoutCount = br.ReadInt32();
            int textureCount = br.ReadInt32();

            Header.Unk5C = br.ReadByte();
            Header.Unk5D = br.ReadByte();
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
                Dummies.Add(new Dummy(br, Header.Version));

            Materials = new List<Material>(materialCount);
            var gxListIndices = new Dictionary<int, int>();
            GXLists = new List<GXList>();
            for (int i = 0; i < materialCount; i++)
                Materials.Add(new Material(br, Header, GXLists, gxListIndices));

            Bones = new List<Bone>(boneCount);
            for (int i = 0; i < boneCount; i++)
                Bones.Add(new Bone(br, Header));

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br, Header.Version));

            var faceSets = new List<FaceSet>(faceSetCount);
            for (int i = 0; i < faceSetCount; i++)
                faceSets.Add(new FaceSet(br, Header, vertexIndicesSize, dataOffset));

            var vertexBuffers = new List<VertexBuffer>(vertexBufferCount);
            for (int i = 0; i < vertexBufferCount; i++)
                vertexBuffers.Add(new VertexBuffer(br));

            BufferLayouts = new List<BufferLayout>(bufferLayoutCount);
            for (int i = 0; i < bufferLayoutCount; i++)
                BufferLayouts.Add(new BufferLayout(br));

            var textures = new List<Texture>(textureCount);
            for (int i = 0; i < textureCount; i++)
                textures.Add(new Texture(br, Header));

            if (Header.Version >= 0x2001A)
                SekiroUnk = new SekiroUnkStruct(br);

            Dictionary<int, Texture> textureDict = SFUtil.Dictionize(textures);
            foreach (Material material in Materials)
            {
                material.TakeTextures(textureDict);
            }
            if (textureDict.Count != 0)
                throw new NotSupportedException("Orphaned textures found.");

            Dictionary<int, FaceSet> faceSetDict = SFUtil.Dictionize(faceSets);
            Dictionary<int, VertexBuffer> vertexBufferDict = SFUtil.Dictionize(vertexBuffers);
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
        /// Writes FLVER data to a BinaryWriterEx.
        /// </summary>
        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = Header.BigEndian;
            bw.WriteASCII("FLVER\0");
            bw.WriteASCII(Header.BigEndian ? "B\0" : "L\0");
            bw.WriteInt32(Header.Version);

            bw.ReserveInt32("DataOffset");
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(Dummies.Count);
            bw.WriteInt32(Materials.Count);
            bw.WriteInt32(Bones.Count);
            bw.WriteInt32(Meshes.Count);
            bw.WriteInt32(Meshes.Sum(m => m.VertexBuffers.Count));
            bw.WriteVector3(Header.BoundingBoxMin);
            bw.WriteVector3(Header.BoundingBoxMax);
            bw.WriteInt32(Header.Unk40);

            // I hope this isn't super slow :^)
            int totalFaceCount = 0;
            foreach (Mesh mesh in Meshes)
                foreach (FaceSet faceSet in mesh.FaceSets)
                    totalFaceCount += faceSet.GetFaces(mesh.Vertices.Count < ushort.MaxValue, true).Count;
            bw.WriteInt32(totalFaceCount);

            byte vertexIndicesSize = 0;
            if (Header.Version < 0x20013)
            {
                vertexIndicesSize = 16;
                foreach (Mesh mesh in Meshes)
                {
                    foreach (FaceSet fs in mesh.FaceSets)
                    {
                        vertexIndicesSize = (byte)Math.Max(vertexIndicesSize, fs.GetVertexIndexSize());
                    }
                }
            }

            bw.WriteByte(vertexIndicesSize);
            bw.WriteBoolean(Header.Unicode);
            bw.WriteBoolean(Header.Unk4A);
            bw.WriteByte(0);

            bw.WriteInt16(0);
            bw.WriteInt16(Header.Unk4E);

            int faceSetCount = 0;
            foreach (Mesh mesh in Meshes)
                faceSetCount += mesh.FaceSets.Count;
            bw.WriteInt32(faceSetCount);

            bw.WriteInt32(BufferLayouts.Count);
            bw.WriteInt32(Materials.Sum(m => m.Textures.Count));

            bw.WriteByte(Header.Unk5C);
            bw.WriteByte(Header.Unk5D);
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
                dummy.Write(bw, Header.Version);

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
                {
                    int indexSize = vertexIndicesSize;
                    if (indexSize == 0)
                        indexSize = mesh.FaceSets[i].GetVertexIndexSize();

                    mesh.FaceSets[i].Write(bw, Header, indexSize, faceSetIndex + i);
                }
                faceSetIndex += mesh.FaceSets.Count;
            }

            int vertexBufferIndex = 0;
            foreach (Mesh mesh in Meshes)
            {
                for (int i = 0; i < mesh.VertexBuffers.Count; i++)
                    mesh.VertexBuffers[i].Write(bw, Header, vertexBufferIndex + i, BufferLayouts, mesh.Vertices.Count);
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
                BufferLayouts[i].WriteMembers(bw, i);

            bw.Pad(0x10);
            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].WriteBoundingBox(bw, i, Header.Version);

            bw.Pad(0x10);
            int boneIndicesStart = (int)bw.Position;
            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].WriteBoneIndices(bw, i, boneIndicesStart);

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
            var gxOffsets = new List<int>();
            foreach (GXList gxList in GXLists)
            {
                gxOffsets.Add((int)bw.Position);
                gxList.Write(bw);
            }
            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].FillGXOffset(bw, i, gxOffsets);
            }

            bw.Pad(0x10);
            textureIndex = 0;
            for (int i = 0; i < Materials.Count; i++)
            {
                Material material = Materials[i];
                material.WriteStrings(bw, Header, i);

                for (int j = 0; j < material.Textures.Count; j++)
                    material.Textures[j].WriteStrings(bw, Header, textureIndex + j);
                textureIndex += material.Textures.Count;
            }

            bw.Pad(0x10);
            for (int i = 0; i < Bones.Count; i++)
                Bones[i].WriteStrings(bw, Header, i);

            int alignment = Header.Version <= 0x2000E ? 0x20 : 0x10;
            bw.Pad(alignment);
            if (Header.Version == 0x20010)
                bw.Pad(0x20);

            int dataStart = (int)bw.Position;
            bw.FillInt32("DataOffset", dataStart);

            faceSetIndex = 0;
            vertexBufferIndex = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                Mesh mesh = Meshes[i];
                for (int j = 0; j < mesh.FaceSets.Count; j++)
                {
                    int indexSize = vertexIndicesSize;
                    if (indexSize == 0)
                        indexSize = mesh.FaceSets[j].GetVertexIndexSize();

                    bw.Pad(alignment);
                    mesh.FaceSets[j].WriteVertices(bw, indexSize, faceSetIndex + j, dataStart);
                }
                faceSetIndex += mesh.FaceSets.Count;

                foreach (Vertex vertex in mesh.Vertices)
                    vertex.PrepareWrite();

                for (int j = 0; j < mesh.VertexBuffers.Count; j++)
                {
                    bw.Pad(alignment);
                    mesh.VertexBuffers[j].WriteBuffer(bw, vertexBufferIndex + j, BufferLayouts, mesh.Vertices, dataStart, Header.Version);
                }

                foreach (Vertex vertex in mesh.Vertices)
                    vertex.FinishWrite();

                vertexBufferIndex += mesh.VertexBuffers.Count;
            }

            bw.Pad(alignment);
            bw.FillInt32("DataSize", (int)bw.Position - dataStart);
            if (Header.Version == 0x20010)
                bw.Pad(0x20);
        }

        /// <summary>
        /// General metadata about a FLVER.
        /// </summary>
        public class FLVERHeader
        {
            /// <summary>
            /// If true FLVER will be written big-endian, if false little-endian.
            /// </summary>
            public bool BigEndian { get; set; }

            /// <summary>
            /// Exact meaning unknown.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// Minimum extent of the entire model.
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            /// <summary>
            /// Maximum extent of the entire model.
            /// </summary>
            public Vector3 BoundingBoxMax { get; set; }

            /// <summary>
            /// Unknown; seems close to vertex or edge count.
            /// </summary>
            public int Unk40 { get; set; }

            /// <summary>
            /// If true strings are UTF-16, if false Shift-JIS.
            /// </summary>
            public bool Unicode { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk4A { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk4E { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk5C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk5D { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk68 { get; set; }

            /// <summary>
            /// Creates a FLVERHeader with default values.
            /// </summary>
            public FLVERHeader()
            {
                BigEndian = false;
                Version = 0x20014;
                Unicode = true;
            }
        }
    }
}
