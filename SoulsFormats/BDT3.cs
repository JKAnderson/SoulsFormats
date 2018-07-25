using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public class BDT3
    {
        public List<File> Files;
        private int flag;

        public static BDT3 Read(byte[] bhdBytes, byte[] bdtBytes)
        {
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
            return new BDT3(bhdReader, bdtReader);
        }

        public static BDT3 Read(byte[] bhdBytes, string bdtPath)
        {
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BDT3(bhdReader, bdtReader);
            }
        }

        public static BDT3 Read(string bhdPath, byte[] bdtBytes)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
                return new BDT3(bhdReader, bdtReader);
            }
        }

        public static BDT3 Read(string bhdPath, string bdtPath)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BDT3(bhdReader, bdtReader);
            }
        }

        private BDT3(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            BHD3 bhd = new BHD3(bhdReader);
            flag = bhd.Flag;

            bdtReader.AssertASCII("BDF3");
            bdtReader.AssertASCII("07D7R6\0\0");
            bdtReader.AssertInt32(0);

            Files = new List<File>();
            for (int i = 0; i < bhd.FileHeaders.Count; i++)
            {
                BHD3.FileHeader fileHeader = bhd.FileHeaders[i];
                string name = fileHeader.Name;
                byte[] data = bdtReader.GetBytes(fileHeader.Offset, fileHeader.Size);

                File file = new File
                {
                    Name = name,
                    Bytes = data
                };
                Files.Add(file);
            }
        }

        public void Write(out byte[] bhdBytes, out byte[] bdtBytes)
        {
            BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
            BinaryWriterEx bdtWriter = new BinaryWriterEx(false);
            write(bhdWriter, bdtWriter);
            bhdBytes = bhdWriter.FinishBytes();
            bdtBytes = bdtWriter.FinishBytes();
        }

        public void Write(out byte[] bhdBytes, string bdtPath)
        {
            using (FileStream bdtStream = System.IO.File.Create(bdtPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false, bdtStream);
                write(bhdWriter, bdtWriter);
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
                write(bhdWriter, bdtWriter);
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
                write(bhdWriter, bdtWriter);
                bhdWriter.Finish();
                bdtWriter.Finish();
            }
        }

        private void write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            bhdWriter.WriteASCII("BHF3");
            bhdWriter.WriteASCII("07D7R6\0\0");
            bhdWriter.WriteInt32(flag);
            bhdWriter.WriteInt32(Files.Count);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);

            bdtWriter.WriteASCII("BDF3");
            bdtWriter.WriteASCII("07D7R6\0\0");
            bdtWriter.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bhdWriter.WriteInt32(0x40);
                bhdWriter.WriteInt32(file.Bytes.Length);
                bhdWriter.WriteInt32((int)bdtWriter.Position);
                bhdWriter.WriteInt32(i);
                bhdWriter.ReserveInt32($"FileName{i}");
                bhdWriter.WriteInt32(file.Bytes.Length);

                bdtWriter.WriteBytes(file.Bytes);
                bdtWriter.Pad(0x10);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bhdWriter.FillInt32($"FileName{i}", (int)bhdWriter.Position);
                bhdWriter.WriteShiftJIS(file.Name, true);
            }
        }

        private class BHD3
        {
            public List<FileHeader> FileHeaders;
            public int Flag;

            public BHD3(BinaryReaderEx br)
            {
                br.AssertASCII("BHF3");
                br.AssertASCII("07D7R6\0\0");
                Flag = br.ReadInt32();
                if (Flag != 0x54 && Flag != 0x74)
                    throw new NotSupportedException($"Unrecognized BHD flag: 0x{Flag:X}");

                int fileCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                FileHeaders = new List<FileHeader>();
                for (int i = 0; i < fileCount; i++)
                {
                    br.AssertInt32(0x40);
                    int fileSize = br.ReadInt32();
                    int fileOffset = br.ReadInt32();
                    br.AssertInt32(i);
                    int fileNameOffset = br.ReadInt32();
                    // Why is this here twice?
                    br.AssertInt32(fileSize);

                    string name = br.GetShiftJIS(fileNameOffset);
                    FileHeader fileHeader = new FileHeader()
                    {
                        Name = name,
                        Offset = fileOffset,
                        Size = fileSize,
                    };
                    FileHeaders.Add(fileHeader);
                }
            }

            public class FileHeader
            {
                public string Name;
                public int Offset;
                public int Size;
            }
        }

        public class File
        {
            public string Name;
            public byte[] Bytes;
        }
    }
}
