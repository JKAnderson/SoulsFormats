using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// A reference to an MTD file, specifying textures to use.
        /// </summary>
        public class Material
        {
            /// <summary>
            /// Identifies the mesh that uses this material, may include keywords that determine hideable parts.
            /// </summary>
            public string Name;

            /// <summary>
            /// Virtual path to an MTD file.
            /// </summary>
            public string MTD;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Flags;

            /// <summary>
            /// Textures used by this material.
            /// </summary>
            public List<Texture> Textures;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte[] GXBytes;

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk18;

            private int textureIndex, textureCount;

            /// <summary>
            /// Creates a new Material with null or default values.
            /// </summary>
            public Material()
            {
                Name = null;
                MTD = null;
                Flags = 0;
                Textures = new List<Texture>();
                GXBytes = null;
                Unk18 = 0;
            }

            /// <summary>
            /// Creates a new Material with the given values and an empty texture list.
            /// </summary>
            public Material(string name, string mtd, int flags, byte[] gxBytes = null)
            {
                Name = name;
                MTD = mtd;
                Flags = flags;
                Textures = new List<Texture>();
                GXBytes = gxBytes;
                Unk18 = 0;
            }

            internal Material(BinaryReaderEx br)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                textureCount = br.ReadInt32();
                textureIndex = br.ReadInt32();
                Flags = br.ReadInt32();
                int gxOffset = br.ReadInt32();
                Unk18 = br.ReadInt32();
                br.AssertInt32(0);

                Name = br.GetUTF16(nameOffset);
                MTD = br.GetUTF16(mtdOffset);

                if (gxOffset == 0)
                {
                    GXBytes = null;
                }
                else
                {
                    br.StepIn(gxOffset);

                    // Other than the terminating section, should be GX** in ASCII
                    int section;
                    do
                    {
                        section = br.ReadInt32();
                        br.ReadInt32();
                        br.Skip(br.ReadInt32() - 0xC);
                    } while (section != 0x7FFFFFFF);

                    GXBytes = br.GetBytes(gxOffset, (int)br.Position - gxOffset);
                    br.StepOut();
                }
            }

            internal void TakeTextures(Dictionary<int, Texture> textureDict)
            {
                Textures = new List<Texture>(textureCount);
                for (int i = textureIndex; i < textureIndex + textureCount; i++)
                {
                    if (!textureDict.ContainsKey(i))
                        throw new NotSupportedException("Texture not found or already taken: " + i);

                    Textures.Add(textureDict[i]);
                    textureDict.Remove(i);
                }

                textureIndex = -1;
                textureCount = -1;
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"MaterialName{index}");
                bw.ReserveInt32($"MaterialMTD{index}");
                bw.WriteInt32(Textures.Count);
                bw.ReserveInt32($"TextureIndex{index}");
                bw.WriteInt32(Flags);
                bw.ReserveInt32($"MaterialUnk{index}");
                bw.WriteInt32(Unk18);
                bw.WriteInt32(0);
            }

            internal void WriteTextures(BinaryWriterEx bw, int index, int textureIndex)
            {
                bw.FillInt32($"TextureIndex{index}", textureIndex);
                for (int i = 0; i < Textures.Count; i++)
                {
                    Textures[i].Write(bw, textureIndex + i);
                }
            }

            internal void WriteUnkGX(BinaryWriterEx bw, int index)
            {
                if (GXBytes == null)
                {
                    bw.FillInt32($"MaterialUnk{index}", 0);
                }
                else
                {
                    bw.FillInt32($"MaterialUnk{index}", (int)bw.Position);
                    bw.WriteBytes(GXBytes);
                }
            }

            /// <summary>
            /// Returns the name and MTD path of the material.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} | {MTD}";
            }
        }
    }
}
