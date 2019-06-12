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
                /// Many meshes have a copy of each faceset with and without this flag. If you remove them, motion blur stops working.
                /// </summary>
                MotionBlur = 0x80000000,
            }

            /// <summary>
            /// FaceSet Flags on this FaceSet.
            /// </summary>
            public FSFlags Flags { get; set; }

            /// <summary>
            /// Whether vertices are defined as a triangle strip or individual triangles.
            /// </summary>
            public bool TriangleStrip { get; set; }

            /// <summary>
            /// Whether triangles can be seen through from behind.
            /// </summary>
            public bool CullBackfaces { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk06 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk07 { get; set; }

            /// <summary>
            /// Indices to vertices in a mesh.
            /// </summary>
            public List<int> Indices { get; set; }

            /// <summary>
            /// Creates a new FaceSet with default values and no indices.
            /// </summary>
            public FaceSet()
            {
                Flags = FSFlags.None;
                TriangleStrip = false;
                CullBackfaces = true;
                Indices = new List<int>();
            }

            /// <summary>
            /// Creates a new FaceSet with the specified values.
            /// </summary>
            public FaceSet(FSFlags flags, bool triangleStrip, bool cullBackfaces, byte unk06, bool unk07, List<int> indices)
            {
                Flags = flags;
                TriangleStrip = triangleStrip;
                CullBackfaces = cullBackfaces;
                Unk06 = unk06;
                Unk07 = unk07;
                Indices = indices;
            }

            internal FaceSet(BinaryReaderEx br, int dataOffset)
            {
                Flags = (FSFlags)br.ReadUInt32();

                TriangleStrip = br.ReadBoolean();
                CullBackfaces = br.ReadBoolean();
                Unk06 = br.ReadByte();
                Unk07 = br.ReadBoolean();

                int indexCount = br.ReadInt32();
                int indicesOffset = br.ReadInt32();
                br.ReadInt32(); // Indices size

                br.AssertInt32(0);
                int indexSize = br.AssertInt32(0, 16, 32);
                br.AssertInt32(0);

                if (indexSize == 0 || indexSize == 16)
                    Indices = br.GetUInt16s(dataOffset + indicesOffset, indexCount).Select(i => (int)i).ToList();
                else if (indexSize == 32)
                    Indices = br.GetInt32s(dataOffset + indicesOffset, indexCount).ToList();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                int indexSize = Indices.Any(i => i > ushort.MaxValue) ? 32 : 16;
                bw.WriteUInt32((uint)Flags);

                bw.WriteBoolean(TriangleStrip);
                bw.WriteBoolean(CullBackfaces);
                bw.WriteByte(Unk06);
                bw.WriteBoolean(Unk07);

                bw.WriteInt32(Indices.Count);
                bw.ReserveInt32($"FaceSetVertices{index}");
                bw.WriteInt32(Indices.Count * (indexSize / 8));

                bw.WriteInt32(0);
                bw.WriteInt32(indexSize == 16 ? 16 : indexSize);
                bw.WriteInt32(0);
            }

            internal void WriteVertices(BinaryWriterEx bw, int index, int dataStart)
            {
                int indexSize = Indices.Any(i => i > ushort.MaxValue) ? 32 : 16;
                bw.FillInt32($"FaceSetVertices{index}", (int)bw.Position - dataStart);
                if (indexSize == 0 || indexSize == 16)
                    bw.WriteUInt16s(Indices.Select(i => (ushort)i).ToArray());
                else if (indexSize == 32)
                    bw.WriteInt32s(Indices);
            }

            /// <summary>
            /// Returns a list of arrays of 3 vertex indices, each representing one triangle in a mesh.
            /// </summary>
            public List<int[]> GetFaces(bool allowPrimitiveRestarts, bool includeDegenerateFaces = false)
            {
                var faces = new List<int[]>();
                if (TriangleStrip)
                {
                    bool flip = false;
                    for (int i = 0; i < Indices.Count - 2; i++)
                    {
                        int vi1 = Indices[i];
                        int vi2 = Indices[i + 1];
                        int vi3 = Indices[i + 2];

                        if (allowPrimitiveRestarts && (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF))
                        {
                            flip = false;
                        }
                        else
                        {
                            bool degenerate = vi1 == vi2 || vi2 == vi3 || vi1 == vi3;
                            if ((!degenerate) || includeDegenerateFaces)
                            {
                                if (!flip)
                                    faces.Add(new int[] { vi1, vi2, vi3 });
                                else
                                    faces.Add(new int[] { vi3, vi2, vi1 });
                            }
                            flip = !flip;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Indices.Count - 2; i += 3)
                    {
                        faces.Add(new int[] { Indices[i], Indices[i + 1], Indices[i + 2] });
                    }
                }
                return faces;
            }
        }
    }
}
