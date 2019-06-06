using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    public partial class FLVER
    {
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
    }
}
