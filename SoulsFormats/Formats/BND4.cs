using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used in DS2, DS3, and BB. Extension: .*bnd
    /// </summary>
    public class BND4 : SoulsFile<BND4>, IBinder
    {
        /// <summary>
        /// The files contained within this BND4.
        /// </summary>
        public List<File> Files;

        IReadOnlyList<IBinderFile> IBinder.Files => Files;

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Timestamp;

        /// <summary>
        /// Indicates the format of this BND4.
        /// </summary>
        public Binder.Format Format;

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Flag1;

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Flag2;

        /// <summary>
        /// Whether to write in big-endian format or not (little-endian).
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// Whether to encode filenames as UTF-8 or Shift JIS.
        /// </summary>
        public bool Unicode;

        /// <summary>
        /// Indicates presence of filename hash table.
        /// </summary>
        public byte Extended;

        /// <summary>
        /// Creates an empty BND4 formatted for DS3.
        /// </summary>
        public BND4()
        {
            Files = new List<File>();
            Timestamp = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Format = Binder.Format.x74;
            BigEndian = false;
            Flag1 = false;
            Flag2 = false;
            Unicode = true;
            Extended = 4;
        }

        /// <summary>
        /// Returns true if the data appears to be a BND4.
        /// </summary>
        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "BND4";
        }

        /// <summary>
        /// Reads BND4 data from a BinaryReaderEx.
        /// </summary>
        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("BND4");
            Flag1 = br.ReadBoolean();
            Flag2 = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);
            BigEndian = br.AssertInt32(0x00010000, 0x00000100) == 0x00000100;
            br.BigEndian = BigEndian;

            int fileCount = br.ReadInt32();
            // Header size
            br.AssertInt64(0x40);
            Timestamp = br.ReadFixStr(8);
            // File header size
            long fileHeaderSize = br.ReadInt64();
            long dataStart = br.ReadInt64();

            Unicode = br.ReadBoolean();
            Format = br.ReadEnum8<Binder.Format>();
            Extended = br.AssertByte(0, 1, 4, 0x80);
            br.AssertByte(0);

            if (fileHeaderSize != Binder.FileHeaderSize(Format))
                throw new FormatException($"File header size 0x{fileHeaderSize} unexpected for format {Format}");

            br.AssertInt32(0);
            long hashGroupsOffset = 0;
            if (Extended == 4)
                hashGroupsOffset = br.ReadInt64();
            else
                br.AssertInt64(0);

            Files = new List<File>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, Unicode, Format));
            }
        }

        /// <summary>
        /// Writes BND4 data to a BinaryWriterEx.
        /// </summary>
        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;

            bw.WriteASCII("BND4");
            bw.WriteBoolean(Flag1);
            bw.WriteBoolean(Flag2);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteInt32(0x10000);
            bw.WriteInt32(Files.Count);
            bw.WriteInt64(0x40);
            bw.WriteFixStr(Timestamp, 8);
            bw.WriteInt64(Binder.FileHeaderSize(Format));
            bw.ReserveInt64("DataStart");

            bw.WriteBoolean(Unicode);
            bw.WriteByte((byte)Format);
            bw.WriteByte(Extended);
            bw.WriteByte(0);

            bw.WriteInt32(0);
            if (Extended == 4)
                bw.ReserveInt64("HashGroups");
            else
                bw.WriteInt64(0);

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Write(bw, i, Format);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.FillInt32($"FileName{i}", (int)bw.Position);
                if (Unicode)
                    bw.WriteUTF16(file.Name, true);
                else
                    bw.WriteShiftJIS(file.Name, true);
            }

            if (Extended == 4)
            {
                uint groupCount = 0;
                for (uint p = (uint)Files.Count / 7; p <= 100000; p++)
                {
                    if (SFUtil.IsPrime(p))
                    {
                        groupCount = p;
                        break;
                    }
                }

                if (groupCount == 0)
                    throw new InvalidOperationException("Hash group count not determined in BND4.");

                var hashLists = new List<PathHash>[groupCount];
                for (int i = 0; i < groupCount; i++)
                    hashLists[i] = new List<PathHash>();

                for (int i = 0; i < Files.Count; i++)
                {
                    var pathHash = new PathHash(i, Files[i].Name);
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

            bw.FillInt64("DataStart", bw.Position);
            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                if (file.Bytes.LongLength > 0)
                    bw.Pad(0x10);

                bw.FillInt32($"FileData{i}", (int)bw.Position);

                byte[] bytes = file.Bytes;
                int compressedSize = bytes.Length;

                if (Binder.IsCompressed(file.Flags))
                {
                    if (Format == Binder.Format.x2E)
                    {
                        bytes = DCX.Compress(bytes, DCX.Type.DemonsSoulsEDGE);
                        bw.WriteBytes(bytes);
                        compressedSize = bytes.Length;
                    }
                    else
                    {
                        compressedSize = SFUtil.WriteZlib(bw, 0x9C, bytes);
                    }
                }
                else
                {
                    bw.WriteBytes(bytes);
                }

                bw.FillInt64($"CompressedSize{i}", compressedSize);
            }
        }

        /// <summary>
        /// A generic file in a BND4 container.
        /// </summary>
        public class File : IBinderFile
        {
            /// <summary>
            /// The name of the file, typically a virtual path.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The ID number of the file, or -1 for formats without IDs.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Flags indicating whether to compress the file (0xC0) and other things we don't understand.
            /// </summary>
            public Binder.FileFlags Flags;

            /// <summary>
            /// The raw data of the file.
            /// </summary>
            public byte[] Bytes { get; set; }

            /// <summary>
            /// Creates a new File with the specified information.
            /// </summary>
            public File(int id, string name, Binder.FileFlags flags, byte[] bytes)
            {
                ID = id;
                Name = name;
                Flags = flags;
                Bytes = bytes;
            }

            internal File(BinaryReaderEx br, bool unicode, Binder.Format format)
            {
                Flags = br.ReadEnum8<Binder.FileFlags>();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                br.AssertInt32(-1);
                long compressedSize = br.ReadInt64();

                if (Binder.HasUncompressedSize(format))
                {
                    long uncompressedSize = br.ReadInt64();
                }

                uint fileOffset = br.ReadUInt32();

                if (Binder.HasID(format))
                {
                    ID = br.ReadInt32();
                }
                else
                {
                    ID = -1;
                }

                if (Binder.HasName(format))
                {
                    int nameOffset = br.ReadInt32();
                    if (unicode)
                        Name = br.GetUTF16(nameOffset);
                    else
                        Name = br.GetShiftJIS(nameOffset);
                }
                else
                {
                    Name = null;
                }

                if (format == Binder.Format.x20)
                {
                    br.AssertInt64(0);
                }

                if (Binder.IsCompressed(Flags))
                {
                    if (format == Binder.Format.x2E)
                    {
                        byte[] bytes = br.GetBytes(fileOffset, (int)compressedSize);
                        Bytes = DCX.Decompress(bytes, out DCX.Type type);
                        if (type != DCX.Type.DemonsSoulsEDGE)
                            throw null;
                    }
                    else
                    {
                        br.StepIn(fileOffset);
                        Bytes = SFUtil.ReadZlib(br, (int)compressedSize);
                        br.StepOut();
                    }
                }
                else
                {
                    Bytes = br.GetBytes(fileOffset, (int)compressedSize);
                }
            }

            internal void Write(BinaryWriterEx bw, int index, Binder.Format format)
            {
                bw.WriteByte((byte)Flags);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(-1);
                bw.ReserveInt64($"CompressedSize{index}");
                if (Binder.HasUncompressedSize(format))
                    bw.WriteInt64(Bytes.LongLength);
                bw.ReserveInt32($"FileData{index}");
                if (Binder.HasID(format))
                    bw.WriteInt32(ID);
                bw.ReserveInt32($"FileName{index}");
                if (format == Binder.Format.x20)
                    bw.WriteInt64(0);
            }

            /// <summary>
            /// Returns a string containing the ID and name of this file.
            /// </summary>
            public override string ToString()
            {
                if (ID == -1)
                    return $"{Name ?? "<null>"}";
                else
                    return $"{ID} {Name ?? "<null>"}";
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
