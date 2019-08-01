using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Demon's Souls models; read-only.
    /// </summary>
    public class FLVER0 : SoulsFile<FLVER0>
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

        public class Dummy
        {
            public Vector3 Position;

            public Vector3 Forward;

            public Vector3 Upward;

            public short ReferenceID;

            public short DummyBoneIndex;

            public short AttachBoneIndex;

            public int Unk0C;

            public bool Flag1, Flag2;

            internal Dummy(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Unk0C = br.ReadInt32();
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

            internal Material(BinaryReaderEx br, FLVER0 flv)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                int texturesOffset = br.ReadInt32();
                int layoutsOffset = br.ReadInt32();
                br.ReadInt32(); // Data length from name offset to end of buffer layouts
                int layoutHeaderOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = flv.Unicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
                MTD = flv.Unicode ? br.GetUTF16(mtdOffset) : br.GetShiftJIS(mtdOffset);

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
                        Textures.Add(new Texture(br, flv));
                }
                br.StepOut();

                if (layoutHeaderOffset != 0)
                {
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
                else
                {
                    Layouts = new List<BufferLayout>(1);
                    br.StepIn(layoutsOffset);
                    {
                        Layouts.Add(new BufferLayout(br));
                    }
                    br.StepOut();
                }
            }
        }

        public class Texture
        {
            public string Type;

            public string Path;

            internal Texture(BinaryReaderEx br, FLVER0 flv)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Path = flv.Unicode ? br.GetUTF16(pathOffset) : br.GetShiftJIS(pathOffset);
                if (typeOffset > 0)
                    Type = flv.Unicode ? br.GetUTF16(typeOffset) : br.GetShiftJIS(typeOffset);
                else
                    Type = null;
            }
        }

        public class BufferLayout : List<FLVER.LayoutMember>
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

                int structOffset = 0;
                Capacity = memberCount;
                for (int i = 0; i < memberCount; i++)
                {
                    var member = new FLVER.LayoutMember(br, structOffset);
                    structOffset += member.Size;
                    Add(member);
                }

                if (Size != structSize)
                    throw new InvalidDataException("Mismatched buffer layout size.");
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

            internal Bone(BinaryReaderEx br, FLVER0 flv)
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

                Name = flv.Unicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
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

            public List<FLVER.Vertex> Vertices;

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

                    float uvFactor = 1024;
                    // NB hack
                    if (!br.BigEndian)
                        uvFactor = 2048;

                    Vertices = new List<FLVER.Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var vert = new FLVER.Vertex();
                        vert.Read(br, layout, layout.Size, uvFactor);
                        Vertices.Add(vert);
                    }
                }
                br.StepOut();
            }

            public List<FLVER.Vertex[]> GetFaces()
            {
                ushort[] indices = ToTriangleList();
                var faces = new List<FLVER.Vertex[]>();
                for (int i = 0; i < indices.Length; i += 3)
                {
                    faces.Add(new FLVER.Vertex[]
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
                bool checkFlip = false;
                bool flip = false;
                for (int i = 0; i < VertexIndices.Length - 2; i++)
                {
                    ushort vi1 = VertexIndices[i];
                    ushort vi2 = VertexIndices[i + 1];
                    ushort vi3 = VertexIndices[i + 2];

                    if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF)
                    {
                        checkFlip = true;
                    }
                    else
                    {
                        if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3)
                        {
                            // Every time the triangle strip restarts, compare the average vertex normal to the face normal
                            // and flip the starting direction if they're pointing away from each other.
                            // I don't know why this is necessary; in most models they always restart with the same orientation
                            // as you'd expect. But on some, I can't discern any logic to it, thus this approach.
                            // It's probably hideously slow because I don't know anything about math.
                            // Feel free to hit me with a PR. :slight_smile:
                            if (checkFlip)
                            {
                                FLVER.Vertex v1 = Vertices[vi1];
                                FLVER.Vertex v2 = Vertices[vi2];
                                FLVER.Vertex v3 = Vertices[vi3];
                                Vector3 n1 = new Vector3(v1.Normal.X, v1.Normal.Y, v1.Normal.Z);
                                Vector3 n2 = new Vector3(v2.Normal.X, v2.Normal.Y, v2.Normal.Z);
                                Vector3 n3 = new Vector3(v3.Normal.X, v3.Normal.Y, v3.Normal.Z);
                                Vector3 vertexNormal = Vector3.Normalize((n1 + n2 + n3) / 3);
                                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(v2.Position - v1.Position, v3.Position - v1.Position));
                                float angle = Vector3.Dot(faceNormal, vertexNormal) / (faceNormal.Length() * vertexNormal.Length());
                                flip = angle >= 0;
                                checkFlip = false;
                            }

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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
