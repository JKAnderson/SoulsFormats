using System;

namespace SoulsFormats.EnchantedArms
{
    /// <summary>
    /// A weird BND3 containing a single compressed file.
    /// </summary>
    public class COMPBND : SoulsFile<COMPBND>
    {
        /// <summary>
        /// A version string like "1.00".
        /// </summary>
        public string Version;

        /// <summary>
        /// Name of the inner file.
        /// </summary>
        public string Name;

        /// <summary>
        /// Uncompressed bytes of the inner file.
        /// </summary>
        public byte[] Data;

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;

            br.AssertASCII("BND3");
            Version = br.ReadASCII(8);
            br.AssertByte(0xE4);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            int fileCount = br.AssertInt32(1);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            br.AssertByte(0xC0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            int compressedSize = br.ReadInt32();
            int dataOffset = br.ReadInt32();
            br.AssertInt32(0);
            int nameOffset = br.ReadInt32();
            int uncompressedSize = br.ReadInt32();

            Name = br.GetShiftJIS(nameOffset);
            br.Position = dataOffset;
            Data = SFUtil.ReadZlib(br, compressedSize);
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }
    }
}
