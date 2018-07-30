using System;
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
            type = Type.Unknown;

            string magic = br.ReadASCII(4);
            if (magic == "DCP\0")
            {
                type = Type.DemonsSoulsDFLT;
            }
            else if (magic == "DCX\0")
            {
                int flag = br.GetInt32(0x10);
                string format = br.GetASCII(0x28, 4);
                if (format == "EDGE")
                {
                    type = Type.DemonsSoulsEDGE;
                }
                else if (flag == 0x24)
                {
                    type = Type.DarkSouls1;
                }
                else if (flag == 0x44)
                {
                    type = Type.DarkSouls3;
                }
            }

            br.Position = 0;
            if (type == Type.DemonsSoulsDFLT)
                return DecompressDemonsSoulsDFLT(br);
            else if (type == Type.DemonsSoulsEDGE)
                return DecompressDemonsSoulsEDGE(br);
            else if (type == Type.DarkSouls1 || type == Type.DarkSouls3)
                return DecompressDarkSouls(br, type);
            else
                throw new FormatException("Unknown DCX format.");
        }

        private static byte[] DecompressDemonsSoulsDFLT(BinaryReaderEx br)
        {
            br.AssertASCII("DCP\0");
            br.AssertASCII("DFLT");
            br.AssertInt32(0x20);
            br.AssertInt32(0x9000000);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0x00010100);

            br.AssertASCII("DCS\0");
            int uncompressedSize = br.ReadInt32();
            int compressedSize = br.ReadInt32();
            br.AssertByte(0x78);
            br.AssertByte(0xDA);

            byte[] compressed = br.ReadBytes(compressedSize - 2);
            byte[] decompressed = new byte[uncompressedSize];

            using (MemoryStream cmpStream = new MemoryStream(compressed))
            using (DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Decompress))
            using (MemoryStream dcmpStream = new MemoryStream(decompressed))
                dfltStream.CopyTo(dcmpStream);

            br.AssertASCII("DCA\0");
            br.AssertInt32(8);

            return decompressed;
        }

        private static byte[] DecompressDemonsSoulsEDGE(BinaryReaderEx br)
        {
            br.AssertASCII("DCX\0");
            br.AssertInt32(0x10000);
            br.AssertInt32(0x18);
            br.AssertInt32(0x24);
            br.AssertInt32(0x24);
            int unk1 = br.ReadInt32();

            br.AssertASCII("DCS\0");
            int uncompressedSize = br.ReadInt32();
            int compressedSize = br.ReadInt32();

            br.AssertASCII("DCP\0");
            br.AssertASCII("EDGE");
            br.AssertInt32(0x20);
            br.AssertInt32(0x9000000);
            br.AssertInt32(0x10000);
            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            br.AssertInt32(0x00100100);

            long dcaStart = br.Position;
            br.AssertASCII("DCA\0");
            int dcaSize = br.ReadInt32();
            // ???
            br.AssertASCII("EgdT");
            br.AssertInt32(0x00010100);
            br.AssertInt32(0x24);
            br.AssertInt32(0x10);
            br.AssertInt32(0x10000);
            int unk2 = br.ReadInt32();
            int unk3 = br.ReadInt32();
            int chunkCount = br.ReadInt32();
            br.AssertInt32(0x100000);

            byte[] decompressed = new byte[uncompressedSize];
            using (MemoryStream dcmpStream = new MemoryStream(decompressed))
            {
                for (int i = 0; i < chunkCount; i++)
                {
                    br.AssertInt32(0);
                    int offset = br.ReadInt32();
                    int size = br.ReadInt32();
                    bool compressed = br.AssertInt32(0, 1) == 1;

                    byte[] chunk = br.GetBytes(dcaStart + dcaSize + offset, size);

                    if (compressed)
                    {
                        using (MemoryStream cmpStream = new MemoryStream(chunk))
                        using (DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Decompress))
                            dfltStream.CopyTo(dcmpStream);
                    }
                    else
                    {
                        dcmpStream.Write(chunk, 0, chunk.Length);
                    }
                }
            }

            return decompressed;
        }

        private static byte[] DecompressDarkSouls(BinaryReaderEx br, Type type)
        { 
            br.AssertASCII("DCX\0");
            br.AssertInt32(0x10000);
            br.AssertInt32(0x18);
            br.AssertInt32(0x24);
            if (type == Type.DarkSouls1)
            {
                br.AssertInt32(0x24);
                br.AssertInt32(0x2C);
            }
            else if (type == Type.DarkSouls3)
            {
                br.AssertInt32(0x44);
                br.AssertInt32(0x4C);
            }
            
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
            if (type == Type.DemonsSoulsDFLT)
                CompressDemonsSoulsDFLT(data, bw);
            if (type == Type.DarkSouls1 || type == Type.DarkSouls3)
                CompressDarkSouls(data, bw, type);
            else if (type == Type.Unknown)
                throw new ArgumentException("You cannot compress a DCX with an unknown type.");
            else
                throw new NotImplementedException("Compression for the given type is not implemented.");
        }

        private static void CompressDemonsSoulsDFLT(byte[] data, BinaryWriterEx bw)
        {
            byte[] compressed;
            using (MemoryStream cmpStream = new MemoryStream())
            using (MemoryStream dcmpStream = new MemoryStream(data))
            {
                DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionLevel.Optimal);
                dcmpStream.CopyTo(dfltStream);
                dfltStream.Close();
                compressed = cmpStream.ToArray();
            }

            bw.WriteASCII("DCP\0");
            bw.WriteASCII("DFLT");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x9000000);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x00010100);

            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            bw.WriteInt32(compressed.Length + 2 + 4);
            bw.WriteByte(0x78);
            bw.WriteByte(0xDA);
            bw.WriteBytes(compressed);

            uint adlerA = 1;
            uint adlerB = 0;
            foreach (byte b in data)
            {
                adlerA = (adlerA + b) % 65521;
                adlerB = (adlerB + adlerA) % 65521;
            }
            bw.WriteUInt32((adlerB << 16) | adlerA);

            bw.WriteASCII("DCA\0");
            bw.WriteInt32(8);
        }

        private static void CompressDarkSouls(byte[] data, BinaryWriterEx bw, Type type)
        {
            byte[] compressed;
            using (MemoryStream cmpStream = new MemoryStream())
            using (MemoryStream dcmpStream = new MemoryStream(data))
            {
                DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionLevel.Optimal);
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
            bw.WriteInt32(compressed.Length + 2 + 4);
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

            uint adlerA = 1;
            uint adlerB = 0;
            foreach (byte b in data)
            {
                adlerA = (adlerA + b) % 65521;
                adlerB = (adlerB + adlerA) % 65521;
            }
            bw.WriteUInt32((adlerB << 16) | adlerA);
        }

        public enum Type
        {
            Unknown,
            DemonsSoulsDFLT,
            DemonsSoulsEDGE,
            DarkSouls1,
            DarkSouls3,
        }
    }
}
