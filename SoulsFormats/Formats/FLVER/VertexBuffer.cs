using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// Represents a block of vertex data.
        /// </summary>
        public class VertexBuffer
        {
            /// <summary>
            /// Index of this buffer for meshes with vertex data split into two layouts for whatever reason.
            /// </summary>
            public int BufferIndex { get; set; }

            /// <summary>
            /// Index to a layout in the FLVER's layout collection.
            /// </summary>
            public int LayoutIndex { get; set; }

            /// <summary>
            /// Size of the data for each vertex; -1 means it matches the buffer layout size, which it should, but often doesn't in DSR.
            /// </summary>
            public int VertexSize { get; set; }

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
                br.ReadInt32(); // Buffer length
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

            internal void Write(BinaryWriterEx bw, FLVERHeader header, int index, List<BufferLayout> layouts, int vertexCount)
            {
                BufferLayout layout = layouts[LayoutIndex];

                bw.WriteInt32(BufferIndex);
                bw.WriteInt32(LayoutIndex);

                int vertexSize = VertexSize == -1 ? layout.Size : VertexSize;
                bw.WriteInt32(vertexSize);

                bw.WriteInt32(vertexCount);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(header.Version > 0x20005 ? vertexSize * vertexCount : 0);
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
    }
}
