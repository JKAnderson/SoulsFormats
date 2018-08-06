using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose headered file container used in DS2, DS3, and BB. Extensions: .*bhd (header) and .*bdt (data)
    /// </summary>
    public class BXF4
    {
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

        /// <summary>
        /// A timestamp of unknown purpose.
        /// </summary>
        public DateTime BHDTimestamp, BDTTimestamp;

        /// <summary>
        /// The files contained within this BXF4.
        /// </summary>
        public List<File> Files;

        private BHD4 bhd;

        private BXF4(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            bhd = new BHD4(bhdReader);
            BHDTimestamp = bhd.Timestamp;

            bdtReader.AssertASCII("BDF4");
            bdtReader.AssertInt32(0);
            bdtReader.AssertInt32(0x10000);
            bdtReader.AssertInt32(0);
            // Data start
            bdtReader.AssertInt64(0x30);
            BDTTimestamp = Util.ParseBNDTimestamp(bdtReader.ReadASCII(8));
            bdtReader.AssertInt64(0);
            bdtReader.AssertInt64(0);

            Files = new List<File>();
            foreach (BHD4.FileHeader fileHeader in bhd.FileHeaders)
            {
                Files.Add(new File(bdtReader, fileHeader));
            }
        }

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

        private void Write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            bdtWriter.WriteASCII("BDF4");
            bdtWriter.WriteInt32(0);
            bdtWriter.WriteInt32(0x10000);
            bdtWriter.WriteInt32(0);
            bdtWriter.WriteInt64(0x30);
            bdtWriter.WriteASCII(Util.UnparseBNDTimestamp(BDTTimestamp));
            bdtWriter.WriteInt64(0);
            bdtWriter.WriteInt64(0);

            bhd.Timestamp = BHDTimestamp;
            bhd.Write(bhdWriter, Files);

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bdtWriter.Pad(0x10);

                byte[] bytes = file.Bytes;
                if (file.Flags == 0xC0)
                    bytes = DCX.Compress(bytes, DCX.Type.DarkSouls1);

                bhdWriter.FillUInt32($"FileOffset{i}", (uint)bhdWriter.Position);
                bhdWriter.FillInt64($"FileSize{i}", bytes.LongLength);
                bdtWriter.WriteBytes(bytes);
            }
        }

        /// <summary>
        /// A generic file in a BXF4 container.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The name of the file, typically a virtual path.
            /// </summary>
            public string Name;

            /// <summary>
            /// Flags indicating compression (0x80) and possibly some other things.
            /// </summary>
            public byte Flags;

            /// <summary>
            /// The ID number of the file.
            /// </summary>
            public int? ID;

            /// <summary>
            /// The raw data of the file.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br, BHD4.FileHeader fileHeader)
            {
                Name = fileHeader.Name;
                Flags = fileHeader.Flags;
                ID = fileHeader.ID;
                Bytes = br.GetBytes(fileHeader.Offset, (int)fileHeader.Size);
                if (Flags == 0xC0)
                    Bytes = DCX.Decompress(Bytes, out DCX.Type type);
            }
        }

        internal class BHD4
        {
            public List<FileHeader> FileHeaders;
            public DateTime Timestamp;
            public byte Format;

            private bool unicode;
            private byte extended;

            public BHD4(BinaryReaderEx br)
            {
                br.AssertASCII("BHF4");
                br.AssertInt32(0);
                br.AssertInt32(0x10000);
                int fileCount = br.ReadInt32();
                // File headers start
                br.AssertInt64(0x40);
                Timestamp = Util.ParseBNDTimestamp(br.ReadASCII(8));
                // File header size
                long fileHeaderSize = br.AssertInt64(0x18, 0x24);
                // Would be data start in BND4
                br.AssertInt64(0);

                unicode = br.ReadBoolean();
                if (fileHeaderSize == 0x18)
                    Format = br.AssertByte(0x30);
                else if (fileHeaderSize == 0x24)
                    Format = br.AssertByte(0x74);
                extended = br.AssertByte(0, 4);
                br.AssertByte(0);

                br.AssertInt32(0);
                long hashGroupsOffset = br.ReadInt64();

                FileHeaders = new List<FileHeader>();
                for (int i = 0; i < fileCount; i++)
                {
                    FileHeaders.Add(new FileHeader(br, unicode, Format));
                }

                if (extended == 4)
                {
                    br.Position = hashGroupsOffset;
                    long pathHashesOffset = br.ReadInt64();
                    int hashGroupsCount = br.ReadInt32();
                    // Probably 4 bytes
                    br.AssertInt32(0x00080810);

                    var hashGroups = new List<HashGroup>();
                    for (int i = 0; i < hashGroupsCount; i++)
                    {
                        hashGroups.Add(new HashGroup(br));
                    }

                    br.Position = pathHashesOffset;
                    var pathHashes = new List<PathHash>();
                    for (int i = 0; i < fileCount; i++)
                    {
                        pathHashes.Add(new PathHash(br));
                    }
                }
            }

            public void Write(BinaryWriterEx bw, List<File> files)
            {
                bw.WriteASCII("BHF4");
                bw.WriteInt32(0);
                bw.WriteInt32(0x10000);
                bw.WriteInt32(files.Count);
                bw.WriteInt64(0x40);
                bw.WriteASCII(Util.UnparseBNDTimestamp(Timestamp));
                if (Format == 0x30)
                    bw.WriteInt64(0x18);
                else if (Format == 0x74)
                    bw.WriteInt64(0x24);
                bw.WriteInt64(0);

                bw.WriteBoolean(unicode);
                bw.WriteByte(Format);
                bw.WriteByte(extended);
                bw.WriteByte(0);

                bw.WriteInt32(0);
                if (extended == 4)
                    bw.ReserveInt64("HashGroups");
                else
                    bw.WriteInt64(0);

                for (int i = 0; i < files.Count; i++)
                {
                    FileHeader.Write(bw, files[i], i, Format);
                }

                for (int i = 0; i < files.Count; i++)
                {
                    File file = files[i];
                    bw.FillInt32($"FileName{i}", (int)bw.Position);
                    if (unicode)
                        bw.WriteUTF16(file.Name, true);
                    else
                        bw.WriteShiftJIS(file.Name, true);
                }

                if (extended == 4)
                {
                    uint groupCount = 0;
                    for (uint p = (uint)files.Count / 7; p <= 100000; p++)
                    {
                        if (Util.IsPrime(p))
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

            public class FileHeader
            {
                public string Name;
                public byte Flags;
                public uint Offset;
                public long Size;
                public int? ID;

                public FileHeader(BinaryReaderEx br, bool unicode, byte format)
                {
                    Flags = br.AssertByte(0x00, 0x40, 0xC0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);

                    br.AssertInt32(-1);
                    Size = br.ReadInt64();
                    if (format == 0x74)
                        br.AssertInt64(Size);
                    Offset = br.ReadUInt32();
                    if (format == 0x74)
                        ID = br.ReadInt32();
                    else
                        ID = null;

                    int nameOffset = br.ReadInt32();
                    if (unicode)
                        Name = br.GetUTF16(nameOffset);
                    else
                        Name = br.GetShiftJIS(nameOffset);
                }

                public static void Write(BinaryWriterEx bw, File file, int index, byte format)
                {
                    bw.WriteInt32(file.Flags);
                    bw.WriteInt32(-1);
                    bw.ReserveInt64($"FileSize{index}");
                    if (format == 0x74)
                        bw.WriteInt64(file.Bytes.LongLength);
                    bw.ReserveUInt32($"FileOffset{index}");
                    if (format == 0x74)
                        bw.WriteInt32(file.ID ?? 0);
                    bw.ReserveInt32($"FileName{index}");
                }
            }

            private class PathHash
            {
                public int Index;
                public uint Hash;

                public PathHash(BinaryReaderEx br)
                {
                    Hash = br.ReadUInt32();
                    Index = br.ReadInt32();
                }

                public PathHash(int index, string path)
                {
                    Index = index;
                    Hash = Util.FromPathHash(path);
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

                public HashGroup(BinaryReaderEx br)
                {
                    Length = br.ReadInt32();
                    Index = br.ReadInt32();
                }

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
    }
}
