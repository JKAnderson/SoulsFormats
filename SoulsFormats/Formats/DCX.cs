using System;
using System.IO;
using System.IO.Compression;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose single compressed file wrapper used in DS1, DSR, DS2, DS3, DeS, and BB.
    /// </summary>
    public static class DCX
    {
        internal static bool Is(BinaryReaderEx br)
        {
            if (br.Stream.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "DCX\0" || magic == "DCP\0";
        }

        /// <summary>
        /// Returns true if the bytes appear to be a DCX file.
        /// </summary>
        public static bool Is(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(true, bytes);
            return Is(br);
        }

        /// <summary>
        /// Returns true if the file appears to be a DCX file.
        /// </summary>
        public static bool Is(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(true, stream);
                return Is(br);
            }
        }

        #region Public Decompress
        /// <summary>
        /// Decompress a DCX file from an array of bytes and return the detected DCX type.
        /// </summary>
        public static byte[] Decompress(byte[] data, out Type type)
        {
            BinaryReaderEx br = new BinaryReaderEx(true, data);
            return Decompress(br, out type);
        }

        /// <summary>
        /// Decompress a DCX file from an array of bytes.
        /// </summary>
        public static byte[] Decompress(byte[] data)
        {
            return Decompress(data, out _);
        }

        /// <summary>
        /// Decompress a DCX file from the specified path and return the detected DCX type.
        /// </summary>
        public static byte[] Decompress(string path, out Type type)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(true, stream);
                return Decompress(br, out type);
            }
        }

        /// <summary>
        /// Decompress a DCX file from the specified path.
        /// </summary>
        public static byte[] Decompress(string path)
        {
            return Decompress(path, out _);
        }
        #endregion

        internal static byte[] Decompress(BinaryReaderEx br, out Type type)
        {
            br.BigEndian = true;
            type = Type.Unknown;

            string magic = br.ReadASCII(4);
            if (magic == "DCP\0")
            {
                string format = br.GetASCII(0x4, 4);
                if (format == "DFLT")
                {
                    type = Type.DemonsSoulsDFLT;
                }
                else if (format == "EDGE")
                {
                    type = Type.ACEREDGE;
                }
            }
            else if (magic == "DCX\0")
            {
                string format = br.GetASCII(0x28, 4);
                if (format == "EDGE")
                {
                    type = Type.DemonsSoulsEDGE;
                }
                else if (format == "DFLT")
                {
                    int unk04 = br.GetInt32(0x4);
                    int unk10 = br.GetInt32(0x10);
                    int unk30 = br.GetInt32(0x30);
                    if (unk10 == 0x24)
                    {
                        type = Type.DarkSouls1;
                    }
                    else if (unk10 == 0x44)
                    {
                        if (unk04 == 0x10000)
                        {
                            type = Type.DarkSouls3;
                        }
                        else if (unk04 == 0x11000)
                        {
                            if (unk30 == 0x8000000)
                            {
                                type = Type.DarkSouls3SL2;
                            }
                            else if (unk30 == 0x9000000)
                            {
                                type = Type.SekiroDFLT;
                            }
                        }
                    }
                }
                else if (format == "KRAK")
                {
                    type = Type.SekiroKRAK;
                }
            }

            br.Position = 0;
            if (type == Type.ACEREDGE)
                return DecompressDCPEDGE(br);
            else if (type == Type.DemonsSoulsDFLT)
                return DecompressDCPDFLT(br);
            else if (type == Type.DemonsSoulsEDGE)
                return DecompressDCXEDGE(br);
            else if (type == Type.DarkSouls1 || type == Type.DarkSouls3 || type == Type.DarkSouls3SL2 || type == Type.SekiroDFLT)
                return DecompressDCXDFLT(br, type);
            else if (type == Type.SekiroKRAK)
                return DecompressDCXKRAK(br);
            else
                throw new FormatException("Unknown DCX format.");
        }

        private static byte[] DecompressDCPDFLT(BinaryReaderEx br)
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

            byte[] decompressed = SFUtil.ReadZlib(br, compressedSize);

            br.AssertASCII("DCA\0");
            br.AssertInt32(8);

            return decompressed;
        }

        private static byte[] DecompressDCPEDGE(BinaryReaderEx br)
        {
            br.AssertASCII("DCP\0");
            br.AssertASCII("EDGE");
            br.AssertInt32(0x20);
            br.AssertInt32(0x9000000);
            br.AssertInt32(0x10000);
            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            br.AssertInt32(0x00100100);

            br.AssertASCII("DCS\0");
            int uncompressedSize = br.ReadInt32();
            int compressedSize = br.ReadInt32();
            br.AssertInt32(0);
            long dataStart = br.Position;
            br.Skip(compressedSize);

            br.AssertASCII("DCA\0");
            int dcaSize = br.ReadInt32();
            // ???
            br.AssertASCII("EgdT");
            br.AssertInt32(0x00010000);
            br.AssertInt32(0x20);
            br.AssertInt32(0x10);
            br.AssertInt32(0x10000);
            int egdtSize = br.ReadInt32();
            int chunkCount = br.ReadInt32();
            br.AssertInt32(0x100000);

            if (egdtSize != 0x20 + chunkCount * 0x10)
                throw new InvalidDataException("Unexpected EgdT size in EDGE DCX.");

            byte[] decompressed = new byte[uncompressedSize];
            using (MemoryStream dcmpStream = new MemoryStream(decompressed))
            {
                for (int i = 0; i < chunkCount; i++)
                {
                    br.AssertInt32(0);
                    int offset = br.ReadInt32();
                    int size = br.ReadInt32();
                    bool compressed = br.AssertInt32(0, 1) == 1;

                    byte[] chunk = br.GetBytes(dataStart + offset, size);

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

        private static byte[] DecompressDCXEDGE(BinaryReaderEx br)
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
            // Uncompressed size of last block
            int trailingUncompressedSize = br.AssertInt32(uncompressedSize % 0x10000, 0x10000);
            int egdtSize = br.ReadInt32();
            int chunkCount = br.ReadInt32();
            br.AssertInt32(0x100000);

            if (unk1 != 0x50 + chunkCount * 0x10)
                throw new InvalidDataException("Unexpected unk1 value in EDGE DCX.");

            if (egdtSize != 0x24 + chunkCount * 0x10)
                throw new InvalidDataException("Unexpected EgdT size in EDGE DCX.");

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

        private static byte[] DecompressDCXDFLT(BinaryReaderEx br, Type type)
        {
            br.AssertASCII("DCX\0");

            if (type == Type.DarkSouls1 || type == Type.DarkSouls3)
            {
                br.AssertInt32(0x10000);
            }
            else if (type == Type.DarkSouls3SL2 || type == Type.SekiroDFLT)
            {
                br.AssertInt32(0x11000);
            }

            br.AssertInt32(0x18);
            br.AssertInt32(0x24);

            if (type == Type.DarkSouls1)
            {
                br.AssertInt32(0x24);
                br.AssertInt32(0x2C);
            }
            else if (type == Type.DarkSouls3 || type == Type.DarkSouls3SL2 || type == Type.SekiroDFLT)
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

            if (type == Type.DarkSouls1 || type == Type.DarkSouls3 || type == Type.SekiroDFLT)
            {
                br.AssertInt32(0x9000000);
            }
            else if (type == Type.DarkSouls3SL2)
            {
                br.AssertInt32(0x8000000);
            }

            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            // These look suspiciously like flags
            br.AssertInt32(0x00010100);

            br.AssertASCII("DCA\0");
            int compressedHeaderLength = br.ReadInt32();

            return SFUtil.ReadZlib(br, compressedSize);
        }

        private static byte[] DecompressDCXKRAK(BinaryReaderEx br)
        {
            br.AssertASCII("DCX\0");
            br.AssertInt32(0x11000);
            br.AssertInt32(0x18);
            br.AssertInt32(0x24);
            br.AssertInt32(0x44);
            br.AssertInt32(0x4C);
            br.AssertASCII("DCS\0");
            uint uncompressedSize = br.ReadUInt32();
            uint compressedSize = br.ReadUInt32();
            br.AssertASCII("DCP\0");
            br.AssertASCII("KRAK");
            br.AssertInt32(0x20);
            br.AssertInt32(0x6000000);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0x10100);
            br.AssertASCII("DCA\0");
            br.AssertInt32(8);

            byte[] compressed = br.ReadBytes((int)compressedSize);
            return Oodle26.Decompress(compressed, uncompressedSize);
        }

        #region Public Compress
        /// <summary>
        /// Compress a DCX file to an array of bytes using the specified DCX type.
        /// </summary>
        public static byte[] Compress(byte[] data, Type type)
        {
            BinaryWriterEx bw = new BinaryWriterEx(true);
            Compress(data, bw, type);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Compress a DCX file to the specified path using the specified DCX type.
        /// </summary>
        public static void Compress(byte[] data, Type type, string path)
        {
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(true, stream);
                Compress(data, bw, type);
                bw.Finish();
            }
        }
        #endregion

        internal static void Compress(byte[] data, BinaryWriterEx bw, Type type)
        {
            // Some day I hope to get Oodle compression working, but not today
            if (type == Type.SekiroKRAK)
                type = Type.SekiroDFLT;

            bw.BigEndian = true;
            if (type == Type.DemonsSoulsDFLT)
                CompressDCPDFLT(data, bw);
            else if (type == Type.DemonsSoulsEDGE)
                CompressDCXEDGE(data, bw);
            else if (type == Type.DarkSouls1 || type == Type.DarkSouls3 || type == Type.DarkSouls3SL2 || type == Type.SekiroDFLT)
                CompressDCXDFLT(data, bw, type);
            else if (type == Type.SekiroKRAK)
                CompressDCXKRAK(data, bw);
            else if (type == Type.Unknown)
                throw new ArgumentException("You cannot compress a DCX with an unknown type.");
            else
                throw new NotImplementedException("Compression for the given type is not implemented.");
        }

        private static void CompressDCPDFLT(byte[] data, BinaryWriterEx bw)
        {
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
            bw.ReserveInt32("CompressedSize");

            int compressedSize = SFUtil.WriteZlib(bw, 0xDA, data);
            bw.FillInt32("CompressedSize", compressedSize);

            bw.WriteASCII("DCA\0");
            bw.WriteInt32(8);
        }

        private static void CompressDCXEDGE(byte[] data, BinaryWriterEx bw)
        {
            int chunkCount = data.Length / 0x10000;
            if (data.Length % 0x10000 > 0)
                chunkCount++;

            bw.WriteASCII("DCX\0");
            bw.WriteInt32(0x10000);
            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x50 + chunkCount * 0x10);

            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            bw.ReserveInt32("CompressedSize");

            bw.WriteASCII("DCP\0");
            bw.WriteASCII("EDGE");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x9000000);
            bw.WriteInt32(0x10000);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x00100100);

            long dcaStart = bw.Position;
            bw.WriteASCII("DCA\0");
            bw.ReserveInt32("DCASize");
            long egdtStart = bw.Position;
            bw.WriteASCII("EgdT");
            bw.WriteInt32(0x00010100);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x10);
            bw.WriteInt32(0x10000);
            bw.WriteInt32(data.Length % 0x10000);
            bw.ReserveInt32("EGDTSize");
            bw.WriteInt32(chunkCount);
            bw.WriteInt32(0x100000);

            for (int i = 0; i < chunkCount; i++)
            {
                bw.WriteInt32(0);
                bw.ReserveInt32($"ChunkOffset{i}");
                bw.ReserveInt32($"ChunkSize{i}");
                bw.ReserveInt32($"ChunkCompressed{i}");
            }

            bw.FillInt32("DCASize", (int)(bw.Position - dcaStart));
            bw.FillInt32("EGDTSize", (int)(bw.Position - egdtStart));
            long dataStart = bw.Position;

            int compressedSize = 0;
            for (int i = 0; i < chunkCount; i++)
            {
                int chunkSize = 0x10000;
                if (i == chunkCount - 1)
                    chunkSize = data.Length % 0x10000;

                byte[] chunk;
                using (MemoryStream cmpStream = new MemoryStream())
                using (MemoryStream dcmpStream = new MemoryStream(data, i * 0x10000, chunkSize))
                {
                    DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Compress);
                    dcmpStream.CopyTo(dfltStream);
                    dfltStream.Close();
                    chunk = cmpStream.ToArray();
                }

                if (chunk.Length < chunkSize)
                    bw.FillInt32($"ChunkCompressed{i}", 1);
                else
                {
                    bw.FillInt32($"ChunkCompressed{i}", 0);
                    chunk = data;
                }

                compressedSize += chunk.Length;
                bw.FillInt32($"ChunkOffset{i}", (int)(bw.Position - dataStart));
                bw.FillInt32($"ChunkSize{i}", chunk.Length);
                bw.WriteBytes(chunk);
                bw.Pad(0x10);
            }

            bw.FillInt32("CompressedSize", compressedSize);
        }

        private static void CompressDCXDFLT(byte[] data, BinaryWriterEx bw, Type type)
        {
            bw.WriteASCII("DCX\0");

            if (type == Type.DarkSouls1 || type == Type.DarkSouls3)
            {
                bw.WriteInt32(0x10000);
            }
            else if (type == Type.DarkSouls3SL2 || type == Type.SekiroDFLT)
            {
                bw.WriteInt32(0x11000);
            }

            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);

            if (type == Type.DarkSouls1)
            {
                bw.WriteInt32(0x24);
                bw.WriteInt32(0x2C);
            }
            else if (type == Type.DarkSouls3 || type == Type.DarkSouls3SL2 || type == Type.SekiroDFLT)
            {
                bw.WriteInt32(0x44);
                bw.WriteInt32(0x4C);
            }

            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            bw.ReserveInt32("CompressedSize");
            bw.WriteASCII("DCP\0");
            bw.WriteASCII("DFLT");
            bw.WriteInt32(0x20);

            if (type == Type.DarkSouls1 || type == Type.DarkSouls3 || type == Type.SekiroDFLT)
            {
                bw.WriteInt32(0x9000000);
            }
            else if (type == Type.DarkSouls3SL2)
            {
                bw.WriteInt32(0x8000000);
            }

            bw.WriteInt32(0x0);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x00010100);
            bw.WriteASCII("DCA\0");
            bw.WriteInt32(0x8);

            long compressedStart = bw.Position;
            SFUtil.WriteZlib(bw, 0xDA, data);
            bw.FillInt32("CompressedSize", (int)(bw.Position - compressedStart));
        }

        private static void CompressDCXKRAK(byte[] data, BinaryWriterEx bw)
        {
            byte[] compressed = Oodle26.Compress(data, Oodle26.Codec.Kraken, Oodle26.Level.Optimal2);

            bw.WriteASCII("DCX\0");
            bw.WriteInt32(0x11000);
            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x44);
            bw.WriteInt32(0x4C);
            bw.WriteASCII("DCS\0");
            bw.WriteUInt32((uint)data.Length);
            bw.WriteUInt32((uint)compressed.Length);
            bw.WriteASCII("DCP\0");
            bw.WriteASCII("KRAK");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x6000000);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x10100);
            bw.WriteASCII("DCA\0");
            bw.WriteInt32(8);
            bw.WriteBytes(compressed);
            bw.Pad(0x10);
        }

        /// <summary>
        /// Specific DCX format used for a certain file.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// DCX type could not be detected.
            /// </summary>
            Unknown,

            /// <summary>
            /// The file is not compressed.
            /// </summary>
            None,

            /// <summary>
            /// A multi-block format used in ACE R TPFs.
            /// </summary>
            ACEREDGE,

            /// <summary>
            /// An single-block format used for DeS debug map files.
            /// </summary>
            DemonsSoulsDFLT,

            /// <summary>
            /// A multi-block format used for most DeS files.
            /// </summary>
            DemonsSoulsEDGE,

            /// <summary>
            /// The standard single-block format used in DS1, DSR, and DS2.
            /// </summary>
            DarkSouls1,

            /// <summary>
            /// The standard single-block format used in DS3 and BB.
            /// </summary>
            DarkSouls3,

            /// <summary>
            /// Used for the copy of the regulation stored in DS3 save files.
            /// </summary>
            DarkSouls3SL2,

            /// <summary>
            /// Deflate format used in Sekiro.
            /// </summary>
            SekiroDFLT,

            /// <summary>
            /// Oodle compression used in Sekiro.
            /// </summary>
            SekiroKRAK,
        }
    }
}
