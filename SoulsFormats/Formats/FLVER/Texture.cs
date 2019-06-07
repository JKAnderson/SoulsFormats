namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// A texture used by the shader specified in an MTD.
        /// </summary>
        public class Texture
        {
            /// <summary>
            /// The type of texture this is, corresponding to the entries in the MTD.
            /// </summary>
            public string Type;

            /// <summary>
            /// The virtual path to the texture file to use.
            /// </summary>
            public string Path;

            /// <summary>
            /// Unknown.
            /// </summary>
            public float ScaleX;

            /// <summary>
            /// Unknown.
            /// </summary>
            public float ScaleY;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk10;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk11;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14, Unk18, Unk1C;

            /// <summary>
            /// Creates a Texture with default values.
            /// </summary>
            public Texture()
            {
                Type = "";
                Path = "";
                ScaleX = 1;
                ScaleY = 1;
            }

            /// <summary>
            /// Creates a new Texture with the specified values.
            /// </summary>
            public Texture(string type, string path, float scaleX, float scaleY, byte unk10, bool unk11, int unk14, int unk18, int unk1C)
            {
                Type = type;
                Path = path;
                ScaleX = scaleX;
                ScaleY = scaleY;
                Unk10 = unk10;
                Unk11 = unk11;
                Unk14 = unk14;
                Unk18 = unk18;
                Unk1C = unk1C;
            }

            internal Texture(BinaryReaderEx br)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                ScaleX = br.ReadSingle();
                ScaleY = br.ReadSingle();

                Unk10 = br.AssertByte(0, 1, 2);
                Unk11 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);

                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Type = br.GetUTF16(typeOffset);
                Path = br.GetUTF16(pathOffset);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"TexturePath{index}");
                bw.ReserveInt32($"TextureType{index}");
                bw.WriteSingle(ScaleX);
                bw.WriteSingle(ScaleY);

                bw.WriteByte(Unk10);
                bw.WriteBoolean(Unk11);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }

            /// <summary>
            /// Returns this texture's type and path.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} = {Path}";
            }
        }
    }
}
