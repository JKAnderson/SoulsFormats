using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Mesh
        {
            public bool Dynamic;

            public byte MaterialIndex;

            public bool Unk02;

            public byte Unk03;

            public short DefaultBoneIndex;

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
                DefaultBoneIndex = br.ReadInt16();
                BoneIndices = br.ReadInt16s(28);
                br.AssertInt16(0);
                br.AssertInt32(vertexIndexCount * 2);
                int vertexIndicesOffset = br.ReadInt32();
                int bufferSize = br.ReadInt32();
                int bufferOffset = br.ReadInt32();
                br.ReadInt32(); // Buffers header offset
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
                        vert.Read(br, layout, uvFactor);
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
