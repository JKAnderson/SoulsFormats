using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose headered file container used in DS1 and DSR. Extensions: .*bhd (header) and .*bdt (data)
    /// </summary>
    public class BXF3 : IBinder
    {
        #region Public Is
        /// <summary>
        /// Returns true if the bytes appear to be a BXF3 header file.
        /// </summary>
        public static bool IsBHD(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return IsBHD(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a BXF3 header file.
        /// </summary>
        public static bool IsBHD(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, fs);
                return IsBHD(SFUtil.GetDecompressedBR(br, out _));
            }
        }

        /// <summary>
        /// Returns true if the file appears to be a BXF3 data file.
        /// </summary>
        public static bool IsBDT(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return IsBDT(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a BXF3 data file.
        /// </summary>
        public static bool IsBDT(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, fs);
                return IsBDT(SFUtil.GetDecompressedBR(br, out _));
            }
        }
        #endregion

        #region Public Read
        /// <summary>
        /// Reads two arrays of bytes as the BHD and BDT.
        /// </summary>
        public static BXF3 Read(byte[] bhdBytes, byte[] bdtBytes)
        {
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
            return new BXF3(bhdReader, bdtReader);
        }

        /// <summary>
        /// Reads an array of bytes as the BHD and a file as the BDT.
        /// </summary>
        public static BXF3 Read(byte[] bhdBytes, string bdtPath)
        {
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BXF3(bhdReader, bdtReader);
            }
        }

        /// <summary>
        /// Reads a file as the BHD and an array of bytes as the BDT.
        /// </summary>
        public static BXF3 Read(string bhdPath, byte[] bdtBytes)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
                return new BXF3(bhdReader, bdtReader);
            }
        }

        /// <summary>
        /// Reads a file as the BHD and a file as the BDT.
        /// </summary>
        public static BXF3 Read(string bhdPath, string bdtPath)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BXF3(bhdReader, bdtReader);
            }
        }
        #endregion

        #region Public Write
        /// <summary>
        /// Writes the BHD and BDT as two arrays of bytes.
        /// </summary>
        public void Write(out byte[] bhdBytes, out byte[] bdtBytes)
        {
            BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
            BinaryWriterEx bdtWriter = new BinaryWriterEx(false);
            Write(bhdWriter, bdtWriter);
            bhdBytes = bhdWriter.FinishBytes();
            bdtBytes = bdtWriter.FinishBytes();
        }

        /// <summary>
        /// Writes the BHD as an array of bytes and the BDT as a file.
        /// </summary>
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

        /// <summary>
        /// Writes the BHD as a file and the BDT as an array of bytes.
        /// </summary>
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

        /// <summary>
        /// Writes the BHD and BDT as two files.
        /// </summary>
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

        /// <summary>
        /// The files contained within this BXF3.
        /// </summary>
        public List<BinderFile> Files { get; set; }

        /// <summary>
        ///A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string BHDTimestamp;

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string BDTTimestamp;

        /// <summary>
        /// Indicates the format of this BXF3.
        /// </summary>
        public Binder.Format Format { get; set; }

        /// <summary>
        /// Creates an empty BXF3 formatted for DS1.
        /// </summary>
        public BXF3()
        {
            Files = new List<BinderFile>();
            BHDTimestamp = SFUtil.DateToBinderTimestamp(DateTime.Now);
            BDTTimestamp = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Format = Binder.Format.x74;
        }

        private static bool IsBHD(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BHF3";
        }

        private static bool IsBDT(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BDF3";
        }

        private BXF3(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            BHD3 bhd = new BHD3(bhdReader);
            BHDTimestamp = bhd.Timestamp;
            Format = bhd.Format;

            bdtReader.AssertASCII("BDF3");
            BDTTimestamp = bdtReader.ReadFixStr(8);
            bdtReader.AssertInt32(0);

            Files = new List<BinderFile>(bhd.FileHeaders.Count);
            for (int i = 0; i < bhd.FileHeaders.Count; i++)
            {
                BHD3.FileHeader fileHeader = bhd.FileHeaders[i];
                byte[] data = bdtReader.GetBytes(fileHeader.Offset, fileHeader.Size);

                Files.Add(new BinderFile(Binder.FileFlags.x40, fileHeader.ID, fileHeader.Name, data));
            }
        }

        private void Write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            bhdWriter.WriteASCII("BHF3");
            bhdWriter.WriteFixStr(BHDTimestamp, 8);
            bhdWriter.WriteByte((byte)Format);
            bhdWriter.WriteByte(0);
            bhdWriter.WriteByte(0);
            bhdWriter.WriteByte(0);
            bhdWriter.BigEndian = Binder.ForceBigEndian(Format);

            bhdWriter.WriteInt32(Files.Count);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);

            bdtWriter.WriteASCII("BDF3");
            bdtWriter.WriteFixStr(BDTTimestamp, 8);
            bdtWriter.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                BinderFile file = Files[i];
                bhdWriter.WriteByte(0x40);
                bhdWriter.WriteByte(0);
                bhdWriter.WriteByte(0);
                bhdWriter.WriteByte(0);

                bhdWriter.WriteInt32(file.Bytes.Length);
                bhdWriter.WriteUInt32((uint)bdtWriter.Position);
                bhdWriter.WriteInt32(i);
                bhdWriter.ReserveUInt32($"FileName{i}");

                if (Binder.HasUncompressedSize(Format))
                    bhdWriter.WriteInt32(file.Bytes.Length);

                bdtWriter.WriteBytes(file.Bytes);
                bdtWriter.Pad(0x10);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                BinderFile file = Files[i];
                bhdWriter.FillUInt32($"FileName{i}", (uint)bhdWriter.Position);
                bhdWriter.WriteShiftJIS(file.Name, true);
            }
        }

        private class BHD3
        {
            public string Timestamp;
            public List<FileHeader> FileHeaders;
            public Binder.Format Format;

            public BHD3(BinaryReaderEx br)
            {
                br.AssertASCII("BHF3");
                Timestamp = br.ReadFixStr(8);
                Format = br.ReadEnum8<Binder.Format>();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.BigEndian = Binder.ForceBigEndian(Format);

                int fileCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                FileHeaders = new List<FileHeader>(fileCount);
                for (int i = 0; i < fileCount; i++)
                {
                    FileHeaders.Add(new FileHeader(br, Format));
                }
            }

            public class FileHeader
            {
                public int ID;
                public string Name;
                public uint Offset;
                public int Size;

                public FileHeader(BinaryReaderEx br, Binder.Format format)
                {
                    br.AssertByte(0x40);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);

                    Size = br.ReadInt32();
                    Offset = br.ReadUInt32();
                    ID = br.ReadInt32();
                    uint fileNameOffset = br.ReadUInt32();

                    if (Binder.HasUncompressedSize(format))
                    {
                        int uncompressedSize = br.ReadInt32();
                    }

                    Name = br.GetShiftJIS(fileNameOffset);
                }
            }
        }
    }
}
