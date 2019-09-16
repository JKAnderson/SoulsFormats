using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Common classes for FLVER0 and FLVER2.
    /// </summary>
    public static partial class FLVER
    {
        /// <summary>
        /// A single point in a mesh.
        /// </summary>
        public class Vertex
        {
            /// <summary>
            /// Where the vertex is.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Weight of the vertex's attachment to bones; must be 4 length.
            /// </summary>
            public VertexBoneWeights BoneWeights;

            /// <summary>
            /// Bones the vertex is weighted to, indexing the parent mesh's bone indices; must be 4 length.
            /// </summary>
            public VertexBoneIndices BoneIndices;

            /// <summary>
            /// Vector pointing away from the surface.
            /// </summary>
            public Vector3 Normal;

            /// <summary>
            /// Fourth component of the normal, read without transforming into a float; used as a bone index for binding to a single bone.
            /// </summary>
            public int NormalW;

            /// <summary>
            /// Texture coordinates of the vertex.
            /// </summary>
            public List<Vector3> UVs;

            /// <summary>
            /// Vector pointing perpendicular to the normal.
            /// </summary>
            public List<Vector4> Tangents;

            /// <summary>
            /// Vector pointing perpendicular to the normal and tangent.
            /// </summary>
            public Vector4 Bitangent;

            /// <summary>
            /// Data used for alpha, blending, etc.
            /// </summary>
            public List<VertexColor> Colors;

            private Queue<Vector3> uvQueue;
            private Queue<Vector4> tangentQueue;
            private Queue<VertexColor> colorQueue;

            /// <summary>
            /// Create a Vertex with null or empty values.
            /// </summary>
            public Vertex(int uvCapacity = 0, int tangentCapacity = 0, int colorCapacity = 0)
            {
                UVs = new List<Vector3>(uvCapacity);
                Tangents = new List<Vector4>(tangentCapacity);
                Colors = new List<VertexColor>(colorCapacity);
            }

            /// <summary>
            /// Creates a new Vertex with values copied from another.
            /// </summary>
            public Vertex(Vertex clone)
            {
                Position = clone.Position;
                BoneWeights = clone.BoneWeights;
                BoneIndices = clone.BoneIndices;
                Normal = clone.Normal;
                UVs = new List<Vector3>(clone.UVs);
                Tangents = new List<Vector4>(clone.Tangents);
                Bitangent = clone.Bitangent;
                Colors = new List<VertexColor>(clone.Colors);
            }

            /// <summary>
            /// Must be called before writing any buffers. Queues list types so they will be split across buffers properly.
            /// </summary>
            internal void PrepareWrite()
            {
                uvQueue = new Queue<Vector3>(UVs);
                tangentQueue = new Queue<Vector4>(Tangents);
                colorQueue = new Queue<VertexColor>(Colors);
            }

            /// <summary>
            /// Should be called after writing all buffers. Throws out queues to free memory.
            /// </summary>
            internal void FinishWrite()
            {
                uvQueue = null;
                tangentQueue = null;
                colorQueue = null;
            }

            internal void Read(BinaryReaderEx br, List<LayoutMember> layout, float uvFactor)
            {
                foreach (LayoutMember member in layout)
                {
                    switch (member.Semantic)
                    {
                        case LayoutSemantic.Position:
                            if (member.Type == LayoutType.Float3)
                            {
                                Position = br.ReadVector3();
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                Position = br.ReadVector3();
                                br.AssertSingle(0);
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.BoneWeights:
                            if (member.Type == LayoutType.Byte4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadSByte() / 127f;
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadByte() / 255f;
                            }
                            else if (member.Type == LayoutType.UVPair)
                            {
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadInt16() / 32767f;
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadInt16() / 32767f;
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.BoneIndices:
                            if (member.Type == LayoutType.Byte4B)
                            {
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else if (member.Type == LayoutType.ShortBoneIndices)
                            {
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadUInt16();
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Normal:
                            if (member.Type == LayoutType.Float3)
                            {
                                Normal = br.ReadVector3();
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                Normal = br.ReadVector3();
                                float w = br.ReadSingle();
                                NormalW = (int)w;
                                if (w != NormalW)
                                    throw new InvalidDataException($"Float4 Normal W was not a whole number: {w}");
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                Normal = ReadByteNormVector3(br);
                                NormalW = br.ReadByte();
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                Normal = ReadByteNormVector3(br);
                                NormalW = br.ReadByte();
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                Normal = ReadByteNormVector3(br);
                                NormalW = br.ReadByte();
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                Normal = ReadShortNormVector3(br);
                                NormalW = br.ReadInt16();
                            }
                            else if (member.Type == LayoutType.Short4toFloat4B)
                            {
                                Normal = ReadUShortNormVector3(br);
                                NormalW = br.ReadInt16();
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                Normal = ReadByteNormVector3(br);
                                NormalW = br.ReadByte();
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.UV:
                            if (member.Type == LayoutType.Float2)
                            {
                                UVs.Add(new Vector3(br.ReadVector2(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.Float3)
                            {
                                UVs.Add(br.ReadVector3() / uvFactor);
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                UVs.Add(new Vector3(br.ReadVector2(), 0) / uvFactor);
                                UVs.Add(new Vector3(br.ReadVector2(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.Short2toFloat2)
                            {
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.UV)
                            {
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.UVPair)
                            {
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor);
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor);
                            }
                            else if (member.Type == LayoutType.Short4toFloat4B)
                            {
                                UVs.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()) / uvFactor);
                                br.AssertInt16(0);
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Tangent:
                            if (member.Type == LayoutType.Float4)
                            {
                                Tangents.Add(br.ReadVector4());
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                Tangents.Add(ReadByteNormVector4(br));
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                Tangents.Add(ReadByteNormVector4(br));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                Tangents.Add(ReadByteNormVector4(br));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                Tangents.Add(ReadShortNormVector4(br));
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                Tangents.Add(ReadByteNormVector4(br));
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Bitangent:
                            if (member.Type == LayoutType.Byte4A)
                            {
                                Bitangent = ReadByteNormVector4(br);
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                Bitangent = ReadByteNormVector4(br);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                Bitangent = ReadByteNormVector4(br);
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                Bitangent = ReadByteNormVector4(br);
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.VertexColor:
                            if (member.Type == LayoutType.Float4)
                            {
                                Colors.Add(VertexColor.ReadFloatRGBA(br));
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                Colors.Add(VertexColor.ReadByteARGB(br));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                Colors.Add(VertexColor.ReadByteRGBA(br));
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        default:
                            throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                    }
                }
            }

            #region Read Helpers
            private static float ReadByteNorm(BinaryReaderEx br)
                => (br.ReadByte() - 127) / 127f;

            private static Vector3 ReadByteNormVector3(BinaryReaderEx br)
                => new Vector3(ReadByteNorm(br), ReadByteNorm(br), ReadByteNorm(br));

            private static Vector4 ReadByteNormVector4(BinaryReaderEx br)
                => new Vector4(ReadByteNorm(br), ReadByteNorm(br), ReadByteNorm(br), ReadByteNorm(br));

            private static float ReadShortNorm(BinaryReaderEx br)
                => br.ReadInt16() / 32767f;

            private static Vector3 ReadShortNormVector3(BinaryReaderEx br)
                => new Vector3(ReadShortNorm(br), ReadShortNorm(br), ReadShortNorm(br));

            private static Vector4 ReadShortNormVector4(BinaryReaderEx br)
                => new Vector4(ReadShortNorm(br), ReadShortNorm(br), ReadShortNorm(br), ReadShortNorm(br));

            private static float ReadUShortNorm(BinaryReaderEx br)
                => (br.ReadUInt16() - 32767) / 32767f;

            private static Vector3 ReadUShortNormVector3(BinaryReaderEx br)
                => new Vector3(ReadUShortNorm(br), ReadUShortNorm(br), ReadUShortNorm(br));
            #endregion

            internal void Write(BinaryWriterEx bw, List<LayoutMember> layout, float uvFactor)
            {
                foreach (LayoutMember member in layout)
                {
                    switch (member.Semantic)
                    {
                        case LayoutSemantic.Position:
                            if (member.Type == LayoutType.Float3)
                            {
                                bw.WriteVector3(Position);
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                bw.WriteVector3(Position);
                                bw.WriteSingle(0);
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.BoneWeights:
                            if (member.Type == LayoutType.Byte4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteSByte((sbyte)Math.Round(BoneWeights[i] * 127));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteByte((byte)Math.Round(BoneWeights[i] * 255));
                            }
                            else if (member.Type == LayoutType.UVPair)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteInt16((short)Math.Round(BoneWeights[i] * 32767));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteInt16((short)Math.Round(BoneWeights[i] * 32767));
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.BoneIndices:
                            if (member.Type == LayoutType.Byte4B)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteByte((byte)BoneIndices[i]);
                            }
                            else if (member.Type == LayoutType.ShortBoneIndices)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteUInt16((ushort)BoneIndices[i]);
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteByte((byte)BoneIndices[i]);
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Normal:
                            if (member.Type == LayoutType.Float3)
                            {
                                bw.WriteVector3(Normal);
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                bw.WriteVector3(Normal);
                                bw.WriteSingle(NormalW);
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                WriteByteNormVector3(bw, Normal);
                                bw.WriteByte((byte)NormalW);
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                WriteByteNormVector3(bw, Normal);
                                bw.WriteByte((byte)NormalW);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                WriteByteNormVector3(bw, Normal);
                                bw.WriteByte((byte)NormalW);
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                WriteShortNormVector3(bw, Normal);
                                bw.WriteInt16((short)NormalW);
                            }
                            else if (member.Type == LayoutType.Short4toFloat4B)
                            {
                                WriteUShortNormVector3(bw, Normal);
                                bw.WriteInt16((short)NormalW);
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                WriteByteNormVector3(bw, Normal);
                                bw.WriteByte((byte)NormalW);
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.UV:
                            Vector3 uv = uvQueue.Dequeue() * uvFactor;
                            if (member.Type == LayoutType.Float2)
                            {
                                bw.WriteSingle(uv.X);
                                bw.WriteSingle(uv.Y);
                            }
                            else if (member.Type == LayoutType.Float3)
                            {
                                bw.WriteVector3(uv);
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                bw.WriteSingle(uv.X);
                                bw.WriteSingle(uv.Y);

                                uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteSingle(uv.X);
                                bw.WriteSingle(uv.Y);
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == LayoutType.Short2toFloat2)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == LayoutType.UV)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == LayoutType.UVPair)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));

                                uv = uvQueue.Dequeue() * uvFactor;
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4B)
                            {
                                bw.WriteInt16((short)Math.Round(uv.X));
                                bw.WriteInt16((short)Math.Round(uv.Y));
                                bw.WriteInt16((short)Math.Round(uv.Z));
                                bw.WriteInt16(0);
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Tangent:
                            Vector4 tangent = tangentQueue.Dequeue();
                            if (member.Type == LayoutType.Float4)
                            {
                                bw.WriteVector4(tangent);
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                WriteByteNormVector4(bw, tangent);
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                WriteByteNormVector4(bw, tangent);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                WriteByteNormVector4(bw, tangent);
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                WriteShortNormVector4(bw, tangent);
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                WriteByteNormVector4(bw, tangent);
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Bitangent:
                            if (member.Type == LayoutType.Byte4A)
                            {
                                WriteByteNormVector4(bw, Bitangent);
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                WriteByteNormVector4(bw, Bitangent);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                WriteByteNormVector4(bw, Bitangent);
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                WriteByteNormVector4(bw, Bitangent);
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.VertexColor:
                            VertexColor color = colorQueue.Dequeue();
                            if (member.Type == LayoutType.Float4)
                            {
                                color.WriteFloatRGBA(bw);
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                color.WriteByteARGB(bw);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                color.WriteByteRGBA(bw);
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        default:
                            throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                    }
                }
            }

            #region Write Helpers
            private static void WriteByteNorm(BinaryWriterEx bw, float value)
                => bw.WriteByte((byte)Math.Round(value * 127 + 127));

            private static void WriteByteNormVector3(BinaryWriterEx bw, Vector3 value)
            {
                WriteByteNorm(bw, value.X);
                WriteByteNorm(bw, value.Y);
                WriteByteNorm(bw, value.Z);
            }

            private static void WriteByteNormVector4(BinaryWriterEx bw, Vector4 value)
            {
                WriteByteNorm(bw, value.X);
                WriteByteNorm(bw, value.Y);
                WriteByteNorm(bw, value.Z);
                WriteByteNorm(bw, value.W);
            }

            private static void WriteShortNorm(BinaryWriterEx bw, float value)
                => bw.WriteInt16((short)Math.Round(value * 32767));

            private static void WriteShortNormVector3(BinaryWriterEx bw, Vector3 value)
            {
                WriteShortNorm(bw, value.X);
                WriteShortNorm(bw, value.Y);
                WriteShortNorm(bw, value.Z);
            }

            private static void WriteShortNormVector4(BinaryWriterEx bw, Vector4 value)
            {
                WriteShortNorm(bw, value.X);
                WriteShortNorm(bw, value.Y);
                WriteShortNorm(bw, value.Z);
                WriteShortNorm(bw, value.W);
            }

            private static void WriteUShortNorm(BinaryWriterEx bw, float value)
                => bw.WriteUInt16((ushort)Math.Round(value * 32767 + 32767));

            private static void WriteUShortNormVector3(BinaryWriterEx bw, Vector3 value)
            {
                WriteUShortNorm(bw, value.X);
                WriteUShortNorm(bw, value.Y);
                WriteUShortNorm(bw, value.Z);
            }
            #endregion
        }
    }
}
