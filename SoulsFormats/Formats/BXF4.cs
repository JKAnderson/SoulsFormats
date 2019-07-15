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
        /// The files contained within this BXF4.
        /// </summary>
        public List<BinderFile> Files { get; set; }

        /// <summary>
        /// Indicates the format of the BXF4.
        /// </summary>
        public Binder.Format Format
        {
            get => BHD.Format;
            set => BHD.Format = value;
        }

        /// <summary>
        /// Information about this BXF4's header file.
        /// </summary>
        public BHF4 BHD { get; private set; }

        /// <summary>
        /// Information about this BXF4's data file.
        /// </summary>
        public BDF4 BDT { get; private set; }

        /// <summary>
        /// Creates an empty BXF4 formatted for DS3.
        /// </summary>
        public BXF4()
        {
            Files = new List<BinderFile>();
            BHD = new BHF4();
            BDT = new BDF4();
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
            BHD = new BHF4(bhdReader);
            BDT = new BDF4(bdtReader);

            Files = new List<BinderFile>(BHD.FileHeaders.Count);
            foreach (BHF4.FileHeader fileHeader in BHD.FileHeaders)
            {
                byte[] bytes = bdtReader.GetBytes(fileHeader.Offset, (int)fileHeader.CompressedSize);
                if (Binder.IsCompressed(fileHeader.Flags))
                    bytes = DCX.Decompress(bytes);
                Files.Add(new BinderFile(fileHeader.Flags, fileHeader.ID, fileHeader.Name, bytes));
            }
        }

        private void Write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            BHD.Write(bhdWriter, Files);
            BDT.Write(bdtWriter);

            for (int i = 0; i < Files.Count; i++)
            {
                BinderFile file = Files[i];
                bdtWriter.Pad(0x10);

                byte[] bytes = file.Bytes;
                if (Binder.IsCompressed(file.Flags))
                    bytes = DCX.Compress(bytes, DCX.Type.DarkSouls1);

                if (Binder.HasLongOffsets(BHD.Format))
                    bhdWriter.FillInt64($"FileOffset{i}", bdtWriter.Position);
                else
                    bhdWriter.FillUInt32($"FileOffset{i}", (uint)bdtWriter.Position);

                bhdWriter.FillInt64($"FileSize{i}", bytes.LongLength);
                bdtWriter.WriteBytes(bytes);
            }
        }

        /// <summary>
        /// Information about the header file of a BXF4.
        /// </summary>
        public class BHF4
        {
            /// <summary>
            /// A timestamp or version number, 8 characters maximum.
            /// </summary>
            public string Timestamp;

            /// <summary>
            /// Indicates the format of the BXF4.
            /// </summary>
            public Binder.Format Format;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Flag1, Flag2;

            /// <summary>
            /// Whether to use big-endian byte ordering.
            /// </summary>
            public bool BigEndian;

            /// <summary>
            /// Whether to write strings in UTF-16.
            /// </summary>
            public bool Unicode;

            /// <summary>
            /// Indicates the presence of a filename hash table.
            /// </summary>
            public byte Extended;

            internal List<FileHeader> FileHeaders;

            internal BHF4()
            {
                Timestamp = SFUtil.DateToBinderTimestamp(DateTime.Now);
                Flag1 = false;
                Flag2 = false;
                Unicode = true;
                Format = Binder.Format.x74;
                Extended = 4;
            }

            internal BHF4(BinaryReaderEx br)
            {
                br.AssertASCII("BHF4");
                Flag1 = br.ReadBoolean();
                Flag2 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);
                BigEndian = br.AssertInt32(0x00010000, 0x00000100) == 0x00000100;
                br.BigEndian = BigEndian;

                int fileCount = br.ReadInt32();
                // File headers start
                br.AssertInt64(0x40);
                Timestamp = br.ReadFixStr(8);
                long fileHeaderSize = br.ReadInt64();
                // Would be data start in BND4
                br.AssertInt64(0);

                Unicode = br.ReadBoolean();
                Format = br.ReadEnum8<Binder.Format>();
                Extended = br.AssertByte(0, 4);
                br.AssertByte(0);

                if (fileHeaderSize != Binder.FileHeaderSize(Format))
                    throw new FormatException($"File header size 0x{fileHeaderSize} unexpected for format {Format}");

                br.AssertInt32(0);
                long hashGroupsOffset = br.ReadInt64();

                FileHeaders = new List<FileHeader>(fileCount);
                for (int i = 0; i < fileCount; i++)
                {
                    FileHeaders.Add(new FileHeader(br, Unicode, Format));
                }
            }

            internal void Write(BinaryWriterEx bw, List<BinderFile> files)
            {
                bw.BigEndian = BigEndian;
                bw.WriteASCII("BHF4");
                bw.WriteBoolean(Flag1);
                bw.WriteBoolean(Flag2);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteInt32(0x10000);
                bw.WriteInt32(files.Count);
                bw.WriteInt64(0x40);
                bw.WriteFixStr(Timestamp, 8);
                bw.WriteInt64(Binder.FileHeaderSize(Format));
                bw.WriteInt64(0);

                bw.WriteBoolean(Unicode);
                bw.WriteByte((byte)Format);
                bw.WriteByte(Extended);
                bw.WriteByte(0);

                bw.WriteInt32(0);
                if (Extended == 4)
                    bw.ReserveInt64("HashGroups");
                else
                    bw.WriteInt64(0);

                for (int i = 0; i < files.Count; i++)
                {
                    FileHeader.Write(bw, files[i], i, Format);
                }

                for (int i = 0; i < files.Count; i++)
                {
                    BinderFile file = files[i];
                    bw.FillUInt32($"FileName{i}", (uint)bw.Position);
                    if (Unicode)
                        bw.WriteUTF16(file.Name, true);
                    else
                        bw.WriteShiftJIS(file.Name, true);
                }

                if (Extended == 4)
                {
                    uint groupCount = 0;
                    for (uint p = (uint)files.Count / 7; p <= 100000; p++)
                    {
                        if (SFUtil.IsPrime(p))
                        {
                            groupCount = p;
                            break;
                        }
                    }

                    if (groupCount == 0)
                        throw new InvalidOperationException("Hash group count not determined in BXF4.");

                    var hashLists = new List<PathHash>[groupCount];
                    for (int i = 0; i < groupCount; i++)
                        hashLists[i] = new List<PathHash>();

                    for (int i = 0; i < files.Count; i++)
                    {
                        var pathHash = new PathHash(i, files[i].Name);
                        uint group = pathHash.Hash % groupCount;
                        hashLists[group].Add(pathHash);
                    }

                    for (int i = 0; i < groupCount; i++)
                        hashLists[i].Sort((ph1, ph2) => ph1.Hash.CompareTo(ph2.Hash));

                    var hashGroups = new List<HashGroup>();
                    var pathHashes = new List<PathHash>();

                    int count = 0;
                    foreach (List<PathHash> hashList in hashLists)
                    {
                        int index = count;
                        foreach (PathHash pathHash in hashList)
                        {
                            pathHashes.Add(pathHash);
                            count++;
                        }

                        hashGroups.Add(new HashGroup(index, count - index));
                    }

                    bw.Pad(0x8);
                    bw.FillInt64("HashGroups", bw.Position);
                    bw.ReserveInt64("PathHashes");
                    bw.WriteUInt32(groupCount);
                    bw.WriteInt32(0x00080810);

                    foreach (HashGroup hashGroup in hashGroups)
                        hashGroup.Write(bw);

                    // No padding after section 1
                    bw.FillInt64("PathHashes", bw.Position);
                    foreach (PathHash pathHash in pathHashes)
                        pathHash.Write(bw);
                }
            }

            internal class FileHeader
            {
                public string Name;
                public Binder.FileFlags Flags;
                public long Offset;
                public long CompressedSize, UncompressedSize;
                public int ID;

                public FileHeader(BinaryReaderEx br, bool unicode, Binder.Format format)
                {
                    Flags = br.ReadEnum8<Binder.FileFlags>();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);

                    br.AssertInt32(-1);
                    CompressedSize = br.ReadInt64();
                    if (Binder.HasUncompressedSize(format))
                        UncompressedSize = br.ReadInt64();

                    if (Binder.HasLongOffsets(format))
                        Offset = br.ReadInt64();
                    else
                        Offset = br.ReadUInt32();

                    if (Binder.HasID(format))
                        ID = br.ReadInt32();
                    else
                        ID = -1;

                    uint nameOffset = br.ReadUInt32();
                    if (unicode)
                        Name = br.GetUTF16(nameOffset);
                    else
                        Name = br.GetShiftJIS(nameOffset);
                }

                public static void Write(BinaryWriterEx bw, BinderFile file, int index, Binder.Format format)
                {
                    bw.WriteByte((byte)file.Flags);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(-1);
                    bw.ReserveInt64($"FileSize{index}");
                    if (Binder.HasUncompressedSize(format))
                        bw.WriteInt64(file.Bytes.LongLength);

                    if (Binder.HasLongOffsets(format))
                        bw.ReserveInt64($"FileOffset{index}");
                    else
                        bw.ReserveUInt32($"FileOffset{index}");

                    if (Binder.HasID(format))
                        bw.WriteInt32(file.ID);

                    bw.ReserveUInt32($"FileName{index}");
                }
            }

            private class PathHash
            {
                public int Index;
                public uint Hash;

                public PathHash(int index, string path)
                {
                    Index = index;
                    Hash = SFUtil.FromPathHash(path);
                }

                public void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(Hash);
                    bw.WriteInt32(Index);
                }
            }

            private class HashGroup
            {
                public int Index, Length;

                public HashGroup(int index, int length)
                {
                    Index = index;
                    Length = length;
                }

                public void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Length);
                    bw.WriteInt32(Index);
                }
            }
        }

        /// <summary>
        /// Information about the data file of a BXF4.
        /// </summary>
        public class BDF4
        {
            /// <summary>
            /// A timestamp or version number, 8 characters maximum.
            /// </summary>
            public string Timestamp;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Flag1, Flag2;

            /// <summary>
            /// Whether to use big-endian byte ordering.
            /// </summary>
            public bool BigEndian;

            /// <summary>
            /// Unknown; usually 0x30.
            /// </summary>
            public long Unk1;

            internal BDF4()
            {
                Timestamp = SFUtil.DateToBinderTimestamp(DateTime.Now);
                Flag1 = false;
                Flag2 = false;
                BigEndian = false;
                Unk1 = 0x30;
            }

            internal BDF4(BinaryReaderEx br)
            {
                br.AssertASCII("BDF4");
                Flag1 = br.ReadBoolean();
                Flag2 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);
                BigEndian = br.AssertInt32(0x00010000, 0x00000100) == 0x00000100;
                br.BigEndian = BigEndian;

                br.AssertInt32(0);
                // I thought this was data start, but it's 0x40 in ds2 network test gamedata.bdt, so I don't know
                Unk1 = br.AssertInt64(0x30, 0x40);
                Timestamp = br.ReadFixStr(8);
                br.AssertInt64(0);
                br.AssertInt64(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.BigEndian = BigEndian;
                bw.WriteASCII("BDF4");
                bw.WriteBoolean(Flag1);
                bw.WriteBoolean(Flag2);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteInt32(0x10000);
                bw.WriteInt32(0);
                bw.WriteInt64(Unk1);
                bw.WriteFixStr(Timestamp, 8);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
            }
        }
    }
}
