using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public class BDT4
    {
        #region Public Read
        public static BDT4 Read(byte[] bhdBytes, byte[] bdtBytes)
        {
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
            return new BDT4(bhdReader, bdtReader);
        }

        public static BDT4 Read(byte[] bhdBytes, string bdtPath)
        {
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BDT4(bhdReader, bdtReader);
            }
        }

        public static BDT4 Read(string bhdPath, byte[] bdtBytes)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
                return new BDT4(bhdReader, bdtReader);
            }
        }

        public static BDT4 Read(string bhdPath, string bdtPath)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BDT4(bhdReader, bdtReader);
            }
        }
        #endregion

        public List<File> Files;
        private BHD4 bhd;
        private string signature;

        private BDT4(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            bhd = new BHD4(bhdReader);

            bdtReader.AssertASCII("BDF4");
            bdtReader.AssertInt32(0);
            bdtReader.AssertInt32(0x10000);
            bdtReader.AssertInt32(0);
            // Data start
            bdtReader.AssertInt64(0x30);
            signature = bdtReader.ReadASCII(8);
            bdtReader.AssertInt64(0);
            bdtReader.AssertInt64(0);

            Files = new List<File>();
            foreach (BHD4.FileHeader fileHeader in bhd.FileHeaders)
            {
                Files.Add(new File(bdtReader, fileHeader));
            }
        }

        #region Public Write
        public void Write(out byte[] bhdBytes, out byte[] bdtBytes)
        {
            BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
            BinaryWriterEx bdtWriter = new BinaryWriterEx(false);
            Write(bhdWriter, bdtWriter);
            bhdBytes = bhdWriter.FinishBytes();
            bdtBytes = bdtWriter.FinishBytes();
        }

        public void Write(out byte[] bhdBytes, string bdtPath)
        {
            using (FileStream bdtStream = System.IO.File.Create(bdtPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false, bdtStream);
                Write(bhdWriter, bdtWriter);
                bdtWriter.Finish();
                bhdBytes = bhdWriter.FinishBytes();
            }
        }

        public void Write(string bhdPath, out byte[] bdtBytes)
        {
            using (FileStream bhdStream = System.IO.File.Create(bhdPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false, bhdStream);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false);
                Write(bhdWriter, bdtWriter);
                bhdWriter.Finish();
                bdtBytes = bdtWriter.FinishBytes();
            }
        }

        public void Write(string bhdPath, string bdtPath)
        {
            using (FileStream bhdStream = System.IO.File.Create(bhdPath))
            using (FileStream bdtStream = System.IO.File.Create(bdtPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false, bhdStream);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false, bdtStream);
                Write(bhdWriter, bdtWriter);
                bhdWriter.Finish();
                bdtWriter.Finish();
            }
        }
        #endregion

        private void Write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            bdtWriter.WriteASCII("BDF4");
            bdtWriter.WriteInt32(0);
            bdtWriter.WriteInt32(0x10000);
            bdtWriter.WriteInt32(0);
            bdtWriter.WriteInt64(0x30);
            bdtWriter.WriteASCII(signature);
            bdtWriter.WriteInt64(0);
            bdtWriter.WriteInt64(0);

            var offsets = new List<int>();
            foreach (File file in Files)
            {
                bdtWriter.Pad(0x10);
                offsets.Add((int)bdtWriter.Position);
                bdtWriter.WriteBytes(file.Bytes);
            }
            bhd.Write(bhdWriter, Files, offsets);
        }

        public class File
        {
            public string Name;
            public int ID;
            public byte[] Bytes;

            internal File(BinaryReaderEx br, BHD4.FileHeader fileHeader)
            {
                Name = fileHeader.Name;
                ID = fileHeader.ID;
                Bytes = br.GetBytes(fileHeader.Offset, (int)fileHeader.Size);
            }
        }

        internal class BHD4
        {
            public List<FileHeader> FileHeaders;

            private List<UnkEntry1> unkEntries1;
            private List<UnkEntry2> unkEntries2;
            private string signature;
            private bool unicode;

            public BHD4(BinaryReaderEx br)
            {
                br.AssertASCII("BHF4");
                br.AssertInt32(0);
                br.AssertInt32(0x10000);
                int fileCount = br.ReadInt32();
                // File headers start
                br.AssertInt64(0x40);
                signature = br.ReadASCII(8);
                // File header size
                br.AssertInt64(0x24);
                // Would be data start in BND4
                br.AssertInt64(0);
                unicode = br.ReadBoolean();
                br.AssertByte(0x74);
                br.AssertByte(4);
                br.AssertByte(0);
                br.AssertInt32(0);
                long unkSection1Offset = br.ReadInt64();

                FileHeaders = new List<FileHeader>();
                for (int i = 0; i < fileCount; i++)
                {
                    FileHeaders.Add(new FileHeader(br, unicode));
                }

                br.Position = unkSection1Offset;
                long unkSection2Offset = br.ReadInt64();
                int unkEntry1Count = br.ReadInt32();
                // Probably 4 bytes
                br.AssertInt32(0x00080810);

                unkEntries1 = new List<UnkEntry1>();
                for (int i = 0; i < unkEntry1Count; i++)
                {
                    unkEntries1.Add(new UnkEntry1(br));
                }

                br.Position = unkSection2Offset;
                unkEntries2 = new List<UnkEntry2>();
                for (int i = 0; i < fileCount; i++)
                {
                    unkEntries2.Add(new UnkEntry2(br));
                }
            }

            public void Write(BinaryWriterEx bw, List<File> files, List<int> offsets)
            {
                bw.WriteASCII("BHF4");
                bw.WriteInt32(0);
                bw.WriteInt32(0x10000);
                bw.WriteInt32(files.Count);
                bw.WriteInt64(0x40);
                bw.WriteASCII(signature);
                bw.WriteInt64(0x24);
                bw.WriteInt64(0);
                bw.WriteBoolean(unicode);
                bw.WriteByte(0x74);
                bw.WriteByte(4);
                bw.WriteByte(0);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkSection1");

                for (int i = 0; i < files.Count; i++)
                {
                    FileHeader.Write(bw, files[i], offsets[i], i);
                }

                for (int i = 0; i < files.Count; i++)
                {
                    File file = files[i];
                    bw.FillInt32($"FileName{i}", (int)bw.Position);
                    if (unicode)
                        bw.WriteUTF16(file.Name, true);
                    else
                        bw.WriteASCII(file.Name, true);
                }
                bw.Pad(0x8);

                bw.FillInt64("UnkSection1", bw.Position);
                bw.ReserveInt64("UnkSection2");
                bw.WriteInt32(unkEntries1.Count);
                bw.WriteInt32(0x00080810);

                foreach (UnkEntry1 unkEntry1 in unkEntries1)
                {
                    unkEntry1.Write(bw);
                }

                // No padding after section 1
                bw.FillInt64("UnkSection2", bw.Position);
                foreach (UnkEntry2 unkEntry2 in unkEntries2)
                {
                    unkEntry2.Write(bw);
                }
            }

            public class FileHeader
            {
                public string Name;
                public int Offset;
                public long Size;
                public int ID;

                public FileHeader(BinaryReaderEx br, bool unicode)
                {
                    br.AssertInt32(0x40);
                    br.AssertInt32(-1);
                    Size = br.ReadInt64();
                    br.AssertInt64(Size);
                    Offset = br.ReadInt32();
                    ID = br.ReadInt32();

                    int nameOffset = br.ReadInt32();
                    if (unicode)
                        Name = br.GetUTF16(nameOffset);
                    else
                        Name = br.GetASCII(nameOffset);
                }

                public static void Write(BinaryWriterEx bw, File file, int offset, int index)
                {
                    bw.WriteInt32(0x40);
                    bw.WriteInt32(-1);
                    bw.WriteInt64(file.Bytes.Length);
                    bw.WriteInt64(file.Bytes.Length);
                    bw.WriteInt32(offset);
                    bw.WriteInt32(file.ID);
                    bw.ReserveInt32($"FileName{index}");
                }
            }

            private class UnkEntry1
            {
                private int unk1, unk2;

                public UnkEntry1(BinaryReaderEx br)
                {
                    unk1 = br.ReadInt32();
                    unk2 = br.ReadInt32();
                }

                public void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(unk1);
                    bw.WriteInt32(unk2);
                }
            }

            private class UnkEntry2
            {
                private int unk1, unk2;

                public UnkEntry2(BinaryReaderEx br)
                {
                    unk1 = br.ReadInt32();
                    unk2 = br.ReadInt32();
                }

                public void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(unk1);
                    bw.WriteInt32(unk2);
                }
            }
        }
    }
}
