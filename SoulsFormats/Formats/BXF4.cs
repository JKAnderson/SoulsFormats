using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose headered file container used in DS2, DS3, and BB. Extensions: .*bhd (header) and .*bdt (data)
    /// </summary>
    public class BXF4 : IBinder
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
        public static BXF4 Read(byte[] bhdBytes, byte[] bdtBytes)
        {
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
            return new BXF4(bhdReader, bdtReader);
        }

        /// <summary>
        /// Reads an array of bytes as the BHD and a file as the BDT.
        /// </summary>
        public static BXF4 Read(byte[] bhdBytes, string bdtPath)
        {
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BXF4(bhdReader, bdtReader);
            }
        }

        /// <summary>
        /// Reads a file as the BHD and an array of bytes as the BDT.
        /// </summary>
        public static BXF4 Read(string bhdPath, byte[] bdtBytes)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
                return new BXF4(bhdReader, bdtReader);
            }
        }

        /// <summary>
        /// Reads two files as the BHD and BDT.
        /// </summary>
        public static BXF4 Read(string bhdPath, string bdtPath)
        {
            using (FileStream bhdStream = System.IO.File.OpenRead(bhdPath))
            using (FileStream bdtStream = System.IO.File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BXF4(bhdReader, bdtReader);
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
        /// The files contained within this BXF4.
        /// </summary>
        public List<BinderFile> Files { get; set; }

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Indicates the format of the BXF4.
        /// </summary>
        public Binder.Format Format { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk04 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk05 { get; set; }

        /// <summary>
        /// Whether to use big-endian byte ordering.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Controls ordering of flag bits.
        /// </summary>
        public bool BitBigEndian { get; set; }

        /// <summary>
        /// Whether to write strings in UTF-16.
        /// </summary>
        public bool Unicode { get; set; }

        /// <summary>
        /// Indicates the presence of a filename hash table.
        /// </summary>
        public byte Extended { get; set; }

        /// <summary>
        /// Creates an empty BXF4 formatted for DS3.
        /// </summary>
        public BXF4()
        {
            Files = new List<BinderFile>();
            Version = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Unicode = true;
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression;
            Extended = 4;
        }

        private static bool IsBHD(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BHF4";
        }

        private static bool IsBDT(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BDF4";
        }

        private BXF4(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            ReadBDFHeader(bdtReader);

            bhdReader.AssertASCII("BHF4");

            Unk04 = bhdReader.ReadBoolean();
            Unk05 = bhdReader.ReadBoolean();
            bhdReader.AssertByte(0);
            bhdReader.AssertByte(0);

            bhdReader.AssertByte(0);
            BigEndian = bhdReader.ReadBoolean();
            BitBigEndian = !bhdReader.ReadBoolean();
            bhdReader.AssertByte(0);

            bhdReader.BigEndian = BigEndian;

            int fileCount = bhdReader.ReadInt32();
            bhdReader.AssertInt64(0x40); // Header size
            Version = bhdReader.ReadFixStr(8);
            long fileHeaderSize = bhdReader.ReadInt64();
            bhdReader.AssertInt64(0);

            Unicode = bhdReader.ReadBoolean();
            Format = Binder.ReadFormat(bhdReader, BitBigEndian);
            Extended = bhdReader.AssertByte(0, 4);
            bhdReader.AssertByte(0);

            if (fileHeaderSize != Binder.GetBND4FileHeaderSize(Format))
                throw new FormatException($"File header size for format {Format} is expected to be 0x{Binder.GetBND4FileHeaderSize(Format):X}, but was 0x{fileHeaderSize:X}");

            bhdReader.AssertInt32(0);

            if (Extended == 4)
            {
                long hashGroupsOffset = bhdReader.ReadInt64();
                bhdReader.StepIn(hashGroupsOffset);
                BinderHashTable.Assert(bhdReader);
                bhdReader.StepOut();
            }
            else
            {
                bhdReader.AssertInt64(0);
            }

            Files = new List<BinderFile>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                BinderFileHeader fileHeader = BinderFileHeader.ReadBinder4FileHeader(bhdReader, Format, BitBigEndian, Unicode);
                Files.Add(fileHeader.ReadFileData(bdtReader));
            }
        }

        // I am very tempted to preserve these since they don't always match the BHF,
        // but it makes the API messy and they don't actually do anything.
        private void ReadBDFHeader(BinaryReaderEx br)
        {
            br.AssertASCII("BDF4");
            br.ReadBoolean(); // Unk04
            br.ReadBoolean(); // Unk05
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.BigEndian = br.ReadBoolean();
            br.ReadBoolean(); // BitBigEndian
            br.AssertByte(0);
            br.AssertInt32(0);
            br.AssertInt64(0x30, 0x40); // Header size, pretty sure 0x40 is just a mistake
            br.ReadFixStr(8); // Version
            br.AssertInt64(0);
            br.AssertInt64(0);
        }

        private void Write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            WriteBDFHeader(bdtWriter);

            bhdWriter.BigEndian = BigEndian;

            bhdWriter.WriteASCII("BHF4");

            bhdWriter.WriteBoolean(Unk04);
            bhdWriter.WriteBoolean(Unk05);
            bhdWriter.WriteByte(0);
            bhdWriter.WriteByte(0);

            bhdWriter.WriteByte(0);
            bhdWriter.WriteBoolean(BigEndian);
            bhdWriter.WriteBoolean(!BitBigEndian);
            bhdWriter.WriteByte(0);

            bhdWriter.WriteInt32(Files.Count);
            bhdWriter.WriteInt64(0x40);
            bhdWriter.WriteFixStr(Version, 8);
            bhdWriter.WriteInt64(Binder.GetBND4FileHeaderSize(Format));
            bhdWriter.WriteInt64(0);

            bhdWriter.WriteBoolean(Unicode);
            Binder.WriteFormat(bhdWriter, BitBigEndian, Format);
            bhdWriter.WriteByte(Extended);
            bhdWriter.WriteByte(0);

            bhdWriter.WriteInt32(0);
            bhdWriter.ReserveInt64("HashTableOffset");

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder4FileHeader(Files[i], bhdWriter, Format, BitBigEndian, i);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteFileName(Files[i], bhdWriter, Format, Unicode, i);

            if (Extended == 4)
            {
                bhdWriter.Pad(0x8);
                bhdWriter.FillInt64("HashTableOffset", bhdWriter.Position);
                BinderHashTable.Write(bhdWriter, Files);
            }
            else
            {
                bhdWriter.FillInt64("HashTableOffset", 0);
            }

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder4FileData(Files[i], bdtWriter, Format, i);
        }

        private void WriteBDFHeader(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.WriteASCII("BDF4");
            bw.WriteBoolean(Unk04);
            bw.WriteBoolean(Unk05);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteBoolean(BigEndian);
            bw.WriteBoolean(!BitBigEndian);
            bw.WriteByte(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x30);
            bw.WriteFixStr(Version, 8);
            bw.WriteInt64(0);
            bw.WriteInt64(0);
        }
    }
}
