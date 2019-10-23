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
            Directory.CreateDirectory(Path.GetDirectoryName(bdtPath));
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
            Directory.CreateDirectory(Path.GetDirectoryName(bhdPath));
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
            Directory.CreateDirectory(Path.GetDirectoryName(bhdPath));
            Directory.CreateDirectory(Path.GetDirectoryName(bdtPath));
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
        public string Version { get; set; }

        /// <summary>
        /// Indicates the format of this BXF3.
        /// </summary>
        public Binder.Format Format { get; set; }

        /// <summary>
        /// Write file in big-endian mode for PS3/X360.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Controls ordering of flag bits.
        /// </summary>
        public bool BitBigEndian { get; set; }

        /// <summary>
        /// Creates an empty BXF3 formatted for DS1.
        /// </summary>
        public BXF3()
        {
            Files = new List<BinderFile>();
            Version = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression;
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
            ReadBDFHeader(bdtReader);

            bhdReader.AssertASCII("BHF3");
            Version = bhdReader.ReadFixStr(8);

            BitBigEndian = bhdReader.GetBoolean(0xE);

            Format = Binder.ReadFormat(bhdReader, BitBigEndian);
            BigEndian = bhdReader.ReadBoolean();
            bhdReader.AssertBoolean(BitBigEndian);
            bhdReader.AssertByte(0);

            bhdReader.BigEndian = BigEndian || Binder.ForceBigEndian(Format);

            int fileCount = bhdReader.ReadInt32();
            bhdReader.AssertInt32(0);
            bhdReader.AssertInt32(0);
            bhdReader.AssertInt32(0);

            Files = new List<BinderFile>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                BinderFileHeader fileHeader = BinderFileHeader.ReadBinder3FileHeader(bhdReader, Format, BitBigEndian);
                Files.Add(fileHeader.ReadFileData(bdtReader));
            }
        }

        private void ReadBDFHeader(BinaryReaderEx br)
        {
            br.AssertASCII("BDF3");
            br.ReadFixStr(8); // Version
            br.AssertInt32(0);
        }

        private void Write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            WriteBDFHeader(bdtWriter);

            bhdWriter.BigEndian = BigEndian || Binder.ForceBigEndian(Format);

            bhdWriter.WriteASCII("BHF3");
            bhdWriter.WriteFixStr(Version, 8);

            Binder.WriteFormat(bhdWriter, BitBigEndian, Format);
            bhdWriter.WriteByte(0);
            bhdWriter.WriteByte(0);
            bhdWriter.WriteByte(0);

            bhdWriter.WriteInt32(Files.Count);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder3FileHeader(Files[i], bhdWriter, Format, BitBigEndian, i);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteFileName(Files[i], bhdWriter, Format, false, i);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder3FileData(Files[i], bhdWriter, bdtWriter, Format, i);
        }

        private void WriteBDFHeader(BinaryWriterEx bw)
        {
            bw.WriteASCII("BDF3");
            bw.WriteFixStr(Version, 8);
            bw.WriteInt32(0);
        }
    }
}
