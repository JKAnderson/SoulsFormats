using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Demon's Souls models; read-only.
    /// </summary>
    public partial class FLVER0 : SoulsFile<FLVER0>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool BigEndian;

        public int Version;

        public Vector3 BoundingBoxMin;

        public Vector3 BoundingBoxMax;

        public byte VertexIndexSize;

        public bool Unicode;

        public byte Unk4A;

        public byte Unk4B;

        public int Unk4C;

        public List<Dummy> Dummies;

        public List<Material> Materials;

        public List<Bone> Bones;

        public List<Mesh> Meshes;

        internal override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0xC)
                return false;

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
            BigEndian = br.AssertASCII("L\0", "B\0") == "B\0";
            br.BigEndian = BigEndian;

            Version = br.AssertInt32(0x0E, 0x0F, 0x10, 0x12, 0x13, 0x14, 0x15);
            int dataOffset = br.ReadInt32();
            br.ReadInt32(); // Data length
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            br.AssertInt32(meshCount); // Vertex buffer count
            BoundingBoxMin = br.ReadVector3();
            BoundingBoxMax = br.ReadVector3();
            br.ReadInt32(); // Face count not including motion blur meshes or degenerate faces
            br.ReadInt32(); // Total face count
            VertexIndexSize = br.ReadByte();
            Unicode = br.ReadBoolean();
            Unk4A = br.ReadByte();
            Unk4B = br.ReadByte();
            Unk4C = br.ReadInt32();

            for (int i = 0; i < 12; i++)
                br.AssertInt32(0);

            Dummies = new List<Dummy>(dummyCount);
            for (int i = 0; i < dummyCount; i++)
                Dummies.Add(new Dummy(br));

            Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++)
                Materials.Add(new Material(br, this));

            Bones = new List<Bone>(boneCount);
            for (int i = 0; i < boneCount; i++)
                Bones.Add(new Bone(br, this));

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br, Materials, dataOffset));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
