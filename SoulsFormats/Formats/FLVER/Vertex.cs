using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER
    {
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
