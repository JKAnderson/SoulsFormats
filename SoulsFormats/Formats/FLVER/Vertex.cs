using System;
using System.Collections.Generic;
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
            public float[] BoneWeights;

            /// <summary>
            /// Bones the vertex is weighted to, indexing the parent mesh's bone indices; must be 4 length.
            /// </summary>
            public int[] BoneIndices;

            /// <summary>
            /// Vector pointing away from the surface.
            /// </summary>
            public Vector4 Normal;

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
                BoneWeights = (float[])clone.BoneWeights?.Clone();
                BoneIndices = (int[])clone.BoneIndices?.Clone();
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
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadSByte() / (float)sbyte.MaxValue;
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadSByte() / (float)sbyte.MaxValue;
                            }
                            else if (member.Type == LayoutType.UVPair)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadInt16() / (float)short.MaxValue;
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                BoneWeights = new float[4];
                                for (int i = 0; i < 4; i++)
                                    BoneWeights[i] = br.ReadInt16() / (float)short.MaxValue;
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.BoneIndices:
                            if (member.Type == LayoutType.Byte4B)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else if (member.Type == LayoutType.ShortBoneIndices)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadUInt16();
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                BoneIndices = new int[4];
                                for (int i = 0; i < 4; i++)
                                    BoneIndices[i] = br.ReadByte();
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Normal:
                            if (member.Type == LayoutType.Float3)
                            {
                                Normal = new Vector4(br.ReadVector3(), 0);
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                Normal = br.ReadVector4();
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = br.ReadInt16() / 32767f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Short4toFloat4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadUInt16() - 32767) / 32767f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Normal = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.UV:
                            if (member.Type == LayoutType.Float2)
                            {
                                UVs.Add(new Vector3(br.ReadVector2() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.Float3)
                            {
                                UVs.Add(br.ReadVector3() / uvFactor);
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                UVs.Add(new Vector3(br.ReadVector2() / uvFactor, 0));
                                UVs.Add(new Vector3(br.ReadVector2() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.Short2toFloat2)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.UV)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.UVPair)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, 0));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4B)
                            {
                                UVs.Add(new Vector3(br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor, br.ReadInt16() / uvFactor));
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
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = br.ReadInt16() / 32767f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Tangents.Add(new Vector4(floats[0], floats[1], floats[2], floats[3]));
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Bitangent:
                            if (member.Type == LayoutType.Byte4A)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Bitangent = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Bitangent = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Bitangent = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                float[] floats = new float[4];
                                for (int i = 0; i < 4; i++)
                                    floats[i] = (br.ReadByte() - 127) / 127f;
                                Bitangent = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.VertexColor:
                            if (member.Type == LayoutType.Float4)
                            {
                                float[] floats = br.ReadSingles(4);
                                Colors.Add(new VertexColor(floats[3], floats[0], floats[1], floats[2]));
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                byte[] bytes = br.ReadBytes(4);
                                Colors.Add(new VertexColor(bytes[0], bytes[1], bytes[2], bytes[3]));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                byte[] bytes = br.ReadBytes(4);
                                Colors.Add(new VertexColor(bytes[3], bytes[0], bytes[1], bytes[2]));
                            }
                            else
                                throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                            break;

                        default:
                            throw new NotImplementedException($"Read not implemented for {member.Type} {member.Semantic}.");
                    }
                }
            }

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
                                    bw.WriteSByte((sbyte)Math.Round(BoneWeights[i] * sbyte.MaxValue));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteSByte((sbyte)Math.Round(BoneWeights[i] * sbyte.MaxValue));
                            }
                            else if (member.Type == LayoutType.UVPair)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteInt16((short)Math.Round(BoneWeights[i] * short.MaxValue));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                for (int i = 0; i < 4; i++)
                                    bw.WriteInt16((short)Math.Round(BoneWeights[i] * short.MaxValue));
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
                                bw.WriteSingle(Normal.X);
                                bw.WriteSingle(Normal.Y);
                                bw.WriteSingle(Normal.Z);
                            }
                            else if (member.Type == LayoutType.Float4)
                            {
                                bw.WriteVector4(Normal);
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                bw.WriteByte((byte)Math.Round(Normal.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                bw.WriteByte((byte)Math.Round(Normal.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                bw.WriteByte((byte)Math.Round(Normal.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                bw.WriteInt16((short)Math.Round(Normal.X * 32767));
                                bw.WriteInt16((short)Math.Round(Normal.Y * 32767));
                                bw.WriteInt16((short)Math.Round(Normal.Z * 32767));
                                bw.WriteInt16((short)Math.Round(Normal.W * 32767));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4B)
                            {
                                bw.WriteUInt16((ushort)Math.Round(Normal.X * 32767 + 32767));
                                bw.WriteUInt16((ushort)Math.Round(Normal.Y * 32767 + 32767));
                                bw.WriteUInt16((ushort)Math.Round(Normal.Z * 32767 + 32767));
                                bw.WriteUInt16((ushort)Math.Round(Normal.W * 32767 + 32767));
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                bw.WriteByte((byte)Math.Round(Normal.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Normal.W * 127 + 127));
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
                                bw.WriteByte((byte)Math.Round(tangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                bw.WriteByte((byte)Math.Round(tangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                bw.WriteByte((byte)Math.Round(tangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Short4toFloat4A)
                            {
                                bw.WriteInt16((short)Math.Round(tangent.X * 32767));
                                bw.WriteInt16((short)Math.Round(tangent.Y * 32767));
                                bw.WriteInt16((short)Math.Round(tangent.Z * 32767));
                                bw.WriteInt16((short)Math.Round(tangent.W * 32767));
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                bw.WriteByte((byte)Math.Round(tangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(tangent.W * 127 + 127));
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.Bitangent:
                            if (member.Type == LayoutType.Byte4A)
                            {
                                bw.WriteByte((byte)Math.Round(Bitangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Byte4B)
                            {
                                bw.WriteByte((byte)Math.Round(Bitangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                bw.WriteByte((byte)Math.Round(Bitangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.W * 127 + 127));
                            }
                            else if (member.Type == LayoutType.Byte4E)
                            {
                                bw.WriteByte((byte)Math.Round(Bitangent.X * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Y * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.Z * 127 + 127));
                                bw.WriteByte((byte)Math.Round(Bitangent.W * 127 + 127));
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        case LayoutSemantic.VertexColor:
                            FLVER.VertexColor color = colorQueue.Dequeue();
                            if (member.Type == LayoutType.Float4)
                            {
                                bw.WriteSingle(color.R);
                                bw.WriteSingle(color.G);
                                bw.WriteSingle(color.B);
                                bw.WriteSingle(color.A);
                            }
                            else if (member.Type == LayoutType.Byte4A)
                            {
                                bw.WriteByte((byte)Math.Round(color.A * 255));
                                bw.WriteByte((byte)Math.Round(color.R * 255));
                                bw.WriteByte((byte)Math.Round(color.G * 255));
                                bw.WriteByte((byte)Math.Round(color.B * 255));
                            }
                            else if (member.Type == LayoutType.Byte4C)
                            {
                                bw.WriteByte((byte)Math.Round(color.R * 255));
                                bw.WriteByte((byte)Math.Round(color.G * 255));
                                bw.WriteByte((byte)Math.Round(color.B * 255));
                                bw.WriteByte((byte)Math.Round(color.A * 255));
                            }
                            else
                                throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                            break;

                        default:
                            throw new NotImplementedException($"Write not implemented for {member.Type} {member.Semantic}.");
                    }
                }
            }
        }
    }
}
