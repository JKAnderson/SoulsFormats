using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SoulsFormats
{
    public class BND3
    {
        #region Public Read
        public static BND3 Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new BND3(br);
        }

        public static BND3 Read(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new BND3(br);
            }
        }
        #endregion

        public string Signature;
        public byte Format;
        private bool bigEndian, unk1;
        private bool writeNameEnd;
        private int unk2;
        public List<File> Files;

        private BND3(BinaryReaderEx br)
        {
            br.AssertASCII("BND3");
            Signature = br.ReadASCII(8);

            Format = br.AssertByte(0x0E, 0x2E, 0x54, 0x60, 0x64, 0x70, 0x74, 0xE0, 0xF0);
            bigEndian = br.ReadBoolean();
            unk1 = br.ReadBoolean();
            br.AssertByte(0);

            br.BigEndian = bigEndian || Format == 0xE0 || Format == 0xF0;
            int fileCount = br.ReadInt32();
            int fileNameEnd = br.ReadInt32();
            writeNameEnd = fileNameEnd != 0;
            unk2 = br.ReadInt32();
            br.AssertInt32(0);
            
            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, Format));
            }
        }

        #region Public Write
        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw);
            return bw.FinishBytes();
        }

        public void Write(string path)
        {
            using (FileStream stream = System.IO.File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, stream);
                Write(bw);
                bw.Finish();
            }
        }
        #endregion

        private void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("BND3");
            bw.WriteASCII(Signature);
            bw.WriteByte(Format);
            bw.WriteBoolean(bigEndian);
            bw.WriteBoolean(unk1);
            bw.WriteByte(0);

            bw.BigEndian = bigEndian || Format == 0xE0 || Format == 0xF0;
            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("NameEnd");
            bw.WriteInt32(unk2);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Write(bw, i, Format);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.FillInt32($"FileName{i}", (int)bw.Position);
                bw.WriteShiftJIS(file.Name, true);
            }
            bw.FillInt32($"NameEnd", writeNameEnd ? (int)bw.Position : 0);

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                if (file.Bytes.Length > 0)
                    bw.Pad(0x10);

                bw.FillInt32($"FileData{i}", (int)bw.Position);

                byte[] bytes = file.Bytes;
                if ((file.Flags & 0x80) != 0)
                {
                    byte[] compressed;
                    using (MemoryStream cmpStream = new MemoryStream())
                    using (MemoryStream dcmpStream = new MemoryStream(bytes))
                    {
                        DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Compress);
                        dcmpStream.CopyTo(dfltStream);
                        dfltStream.Close();
                        compressed = cmpStream.ToArray();
                    }

                    BinaryWriterEx byteWriter = new BinaryWriterEx(true);
                    byteWriter.WriteByte(0x78);
                    byteWriter.WriteByte(0x9C);
                    byteWriter.WriteBytes(compressed);
                    
                    uint adlerA = 1;
                    uint adlerB = 0;
                    foreach (byte b in bytes)
                    {
                        adlerA = (adlerA + b) % 65521;
                        adlerB = (adlerB + adlerA) % 65521;
                    }
                    byteWriter.WriteUInt32((adlerB << 16) | adlerA);

                    bytes = byteWriter.FinishBytes();
                }

                bw.WriteBytes(bytes);
                bw.FillInt32($"CompressedSize{i}", bytes.Length);
            }
        }

        public class File
        {
            public string Name;
            public int ID;
            public byte[] Bytes;
            public byte Flags;

            internal File(BinaryReaderEx br, byte format)
            {
                Flags = br.AssertByte(0x02, 0x40, 0xC0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                int fileSize = br.ReadInt32();
                int fileOffset = br.ReadInt32();
                ID = br.ReadInt32();
                int fileNameOffset = br.ReadInt32();
                
                int uncompressedSize = fileSize;
                if (format == 0x2E || format == 0x54 || format == 0x64 || format == 0x74)
                    uncompressedSize = br.ReadInt32();

                Name = br.GetShiftJIS(fileNameOffset);

                // Compressed
                if ((Flags & 0x80) != 0)
                {
                    long position = br.Position;
                    br.Position = fileOffset;
                    br.AssertByte(0x78);
                    br.AssertByte(0x9C);
                    byte[] compressed = br.ReadBytes(fileSize - 2);

                    Bytes = new byte[uncompressedSize];
                    using (MemoryStream cmpStream = new MemoryStream(compressed))
                    using (DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Decompress))
                    using (MemoryStream dcmpStream = new MemoryStream(Bytes))
                        dfltStream.CopyTo(dcmpStream);

                    br.Position = position;
                }
                else
                {
                    Bytes = br.GetBytes(fileOffset, fileSize);
                }
            }

            internal void Write(BinaryWriterEx bw, int index, byte format)
            {
                bw.WriteByte(Flags);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.ReserveInt32($"CompressedSize{index}");
                bw.ReserveInt32($"FileData{index}");
                bw.WriteInt32(ID);
                bw.ReserveInt32($"FileName{index}");

                if (format == 0x2E || format == 0x54 || format == 0x64 || format == 0x74)
                    bw.WriteInt32(Bytes.Length);
            }
        }
    }
}
