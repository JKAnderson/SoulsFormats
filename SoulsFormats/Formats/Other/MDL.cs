using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats.Other
{
    /// <summary>
    /// A 3D model format used in Xbox games. Extension: .mdl
    /// </summary>
    public class MDL : SoulsFile<MDL>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<Faceset> Facesets { get; set; }

        public ushort[] Indices { get; set; }

        public List<Vertex> Vertices { get; set; }

        public List<string> Textures { get; set; }

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(4, 4);
            return magic == "MDL ";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            int fileSize = br.ReadInt32();
            br.AssertASCII("MDL ");
            br.AssertInt16(1);
            br.AssertInt16(1);
            br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();
            int count1 = br.ReadInt32();
            int indexCount = br.ReadInt32();
            int vertexCount = br.ReadInt32();
            int count4 = br.ReadInt32();
            int count5 = br.ReadInt32();
            int count6 = br.ReadInt32();
            int count7 = br.ReadInt32();
            int count8 = br.ReadInt32();
            int textureCount = br.ReadInt32();
            int offset1 = br.ReadInt32();
            int indicesOffset = br.ReadInt32();
            int verticesOffset = br.ReadInt32();
            int offset4 = br.ReadInt32();
            int offset5 = br.ReadInt32();
            int offset6 = br.ReadInt32();
            int offset7 = br.ReadInt32();
            int offset8 = br.ReadInt32();
            int texturesOffset = br.ReadInt32();

            br.Position = offset1;
            Facesets = new List<Faceset>();
            for (int i = 0; i < count1; i++)
            {
                br.Skip(0x34);
                int facesetCount = br.ReadInt32();
                br.Skip(0xC);
                int facesetsOffset = br.ReadInt32();
                br.Skip(0x48);

                br.StepIn(facesetsOffset);
                {
                    for (int j = 0; j < facesetCount; j++)
                        Facesets.Add(new Faceset(br));
                }
                br.StepOut();
            }

            Indices = br.GetUInt16s(indicesOffset, indexCount);

            br.Position = verticesOffset;
            Vertices = new List<Vertex>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                Vertices.Add(new Vertex(br));

            br.Position = texturesOffset;
            Textures = new List<string>(textureCount);
            for (int i = 0; i < textureCount; i++)
                Textures.Add(br.ReadASCII());
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        public List<Vertex[]> GetFaces(Faceset faceset)
        {
            ushort[] indices = ToTriangleList(faceset);
            var faces = new List<Vertex[]>();
            for (int i = 0; i < indices.Length; i += 3)
            {
                faces.Add(new Vertex[]
                {
                    Vertices[indices[i + 0]],
                    Vertices[indices[i + 1]],
                    Vertices[indices[i + 2]],
                });
            }
            return faces;
        }

        public ushort[] ToTriangleList(Faceset faceset)
        {
            var converted = new List<ushort>();
            bool flip = false;
            for (int i = faceset.StartIndex; i < faceset.StartIndex + faceset.IndexCount - 2; i++)
            {
                ushort vi1 = Indices[i];
                ushort vi2 = Indices[i + 1];
                ushort vi3 = Indices[i + 2];

                if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF)
                {
                    flip = false;
                }
                else
                {
                    if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3)
                    {
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

        public class Faceset
        {
            public byte Unk00 { get; set; }

            public byte Unk01 { get; set; }

            public short VertexCount { get; set; }

            public int IndexCount { get; set; }

            public int StartVertex { get; set; }

            public int StartIndex { get; set; }

            internal Faceset(BinaryReaderEx br)
            {
                Unk00 = br.ReadByte();
                Unk01 = br.ReadByte();
                VertexCount = br.ReadInt16();
                IndexCount = br.ReadInt32();
                StartVertex = br.ReadInt32();
                StartIndex = br.ReadInt32();
            }
        }

        public class Vertex
        {
            public Vector3 Position { get; set; }

            public Vector4 Normal { get; set; }

            public int Unk18 { get; set; }

            public Vector2[] UVs { get; set; }

            internal Vertex(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                float x = (br.ReadByte() - 127) / 127f;
                float y = (br.ReadByte() - 127) / 127f;
                float z = (br.ReadByte() - 127) / 127f;
                float w = (br.ReadByte() - 127) / 127f;
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk18 = br.ReadInt32();

                UVs = new Vector2[4];
                for (int i = 0; i < 4; i++)
                    UVs[i] = br.ReadVector2();

                Normal = new Vector4(x, y, z, w);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
