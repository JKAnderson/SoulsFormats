using System.IO;
using System.IO.Compression;

namespace SoulsFormats
{
    public static class DCX
    {
        #region Public Decompress
        public static byte[] Decompress(byte[] data, out Type type)
        {
            BinaryReaderEx br = new BinaryReaderEx(true, data);
            return Decompress(br, out type);
        }

        public static byte[] Decompress(string path, out Type type)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(true, stream);
                return Decompress(br, out type);
            }
        }
        #endregion

        private static byte[] Decompress(BinaryReaderEx br, out Type type)
        {
            br.AssertASCII("DCX\0");
            br.AssertInt32(0x10000);
            br.AssertInt32(0x18);
            br.AssertInt32(0x24);
            int flag = br.AssertInt32(0x24, 0x44);
            if (flag == 0x24)
                type = Type.DarkSouls1;
            else
                type = Type.DarkSouls3;

            br.AssertInt32(type == Type.DarkSouls1 ? 0x2C : 0x4C);
            br.AssertASCII("DCS\0");
            int uncompressedSize = br.ReadInt32();
            int compressedSize = br.ReadInt32();
            br.AssertASCII("DCP\0");
            br.AssertASCII("DFLT");
            br.AssertInt32(0x20);
            br.AssertInt32(0x9000000);
            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            // These look suspiciously like flags
            br.AssertInt32(0x00010100);
            br.AssertASCII("DCA\0");
            int compressedHeaderLength = br.ReadInt32();
            // Some kind of magic values for zlib
            br.AssertByte(0x78);
            br.AssertByte(0xDA);

            // Size includes 78DA
            byte[] compressed = br.ReadBytes(compressedSize - 2);
            byte[] decompressed = new byte[uncompressedSize];

            using (MemoryStream cmpStream = new MemoryStream(compressed))
            using (DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Decompress))
            using (MemoryStream dcmpStream = new MemoryStream(decompressed))
                dfltStream.CopyTo(dcmpStream);

            return decompressed;
        }

        #region Public Compress
        public static byte[] Compress(byte[] data, Type type)
        {
            BinaryWriterEx bw = new BinaryWriterEx(true);
            Compress(data, bw, type);
            return bw.FinishBytes();
        }

        public static void Compress(byte[] data, string path, Type type)
        {
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(true, stream);
                Compress(data, bw, type);
                bw.Finish();
            }
        }
        #endregion

        private static void Compress(byte[] data, BinaryWriterEx bw, Type type)
        {
            byte[] compressed;
            using (MemoryStream cmpStream = new MemoryStream())
            using (MemoryStream dcmpStream = new MemoryStream(data))
            {
                DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Compress);
                dcmpStream.CopyTo(dfltStream);
                dfltStream.Close();
                compressed = cmpStream.ToArray();
            }

            bw.WriteASCII("DCX\0");
            bw.WriteInt32(0x10000);
            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);

            if (type == Type.DarkSouls1)
            {
                bw.WriteInt32(0x24);
                bw.WriteInt32(0x2C);
            }
            else
            {
                bw.WriteInt32(0x44);
                bw.WriteInt32(0x4C);
            }

            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            // Size includes 78DA
            bw.WriteInt32(compressed.Length + 2);
            bw.WriteASCII("DCP\0");
            bw.WriteASCII("DFLT");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x9000000);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x00010100);
            bw.WriteASCII("DCA\0");
            bw.WriteInt32(0x8);
            bw.WriteByte(0x78);
            bw.WriteByte(0xDA);
            bw.WriteBytes(compressed);
        }

        public enum Type
        {
            DarkSouls1,
            DarkSouls3,
        }
    }
}
