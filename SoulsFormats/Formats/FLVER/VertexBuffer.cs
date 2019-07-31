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
            /// Index to a layout in the FLVER's layout collection.
            /// </summary>
            public int LayoutIndex { get; set; }

            /// <summary>
            /// Size of the data for each vertex; -1 means it matches the buffer layout size, which it should, but often doesn't in DSR.
            /// </summary>
            public int VertexSize { get; set; }

            internal int BufferIndex;
            internal int VertexCount;
            internal int BufferOffset;

            /// <summary>
            /// Creates a VertexBuffer with the specified layout and vertex size; leave size -1 for automatic.
            /// </summary>
            public VertexBuffer(int layoutIndex, int vertexSize = -1)
            {
                LayoutIndex = layoutIndex;
                VertexSize = vertexSize;
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

            internal void ReadBuffer(BinaryReaderEx br, List<BufferLayout> layouts, List<Vertex> vertices, int dataOffset, FLVERHeader header)
            {
                BufferLayout layout = layouts[LayoutIndex];
                br.StepIn(dataOffset + BufferOffset);
                {
                    float uvFactor = 1024;
                    if (header.Version >= 0x2000F)
                        uvFactor = 2048;

                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i].Read(br, layout, VertexSize, uvFactor);
                }
                br.StepOut();

                if (VertexSize == layout.Size)
                    VertexSize = -1;

                BufferIndex = -1;
                VertexCount = -1;
                BufferOffset = -1;
            }

            internal void Write(BinaryWriterEx bw, FLVERHeader header, int index, int bufferIndex, List<BufferLayout> layouts, int vertexCount)
            {
                BufferLayout layout = layouts[LayoutIndex];
                int vertexSize = VertexSize == -1 ? layout.Size : VertexSize;

                bw.WriteInt32(bufferIndex);
                bw.WriteInt32(LayoutIndex);
                bw.WriteInt32(vertexSize);
                bw.WriteInt32(vertexCount);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(header.Version > 0x20005 ? vertexSize * vertexCount : 0);
                bw.ReserveInt32($"VertexBufferOffset{index}");
            }

            internal void WriteBuffer(BinaryWriterEx bw, int index, List<BufferLayout> layouts, List<Vertex> Vertices, int dataStart, FLVERHeader header)
            {
                BufferLayout layout = layouts[LayoutIndex];
                bw.FillInt32($"VertexBufferOffset{index}", (int)bw.Position - dataStart);

                float uvFactor = 1024;
                if (header.Version >= 0x2000F)
                    uvFactor = 2048;

                int vertexSize = VertexSize == -1 ? layout.Size : VertexSize;
                foreach (Vertex vertex in Vertices)
                    vertex.Write(bw, layout, vertexSize, uvFactor);
            }
        }
    }
}
