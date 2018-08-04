using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used in DS2, DS3, and BB. Extension: .*bnd
    /// </summary>
    public class BND4
    {
        #region Public Read
        /// <summary>
        /// Reads an array of bytes as a BND4.
        /// </summary>
        public static BND4 Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new BND4(br);
        }

        /// <summary>
        /// Reads a file as a BND4 using file streams.
        /// </summary>
        public static BND4 Read(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new BND4(br);
            }
        }
        #endregion

        /// <summary>
        /// A timestamp of unknown purpose.
        /// </summary>
        public DateTime Timestamp;

        /// <summary>
        /// The files contained within this BND4.
        /// </summary>
        public List<File> Files;

        private bool unicode;
        private byte flag;
        private byte extended;

        private BND4(BinaryReaderEx br)
        {
            br.AssertASCII("BND4");
            br.AssertInt32(0);
            br.AssertInt32(0x10000);
            int fileCount = br.ReadInt32();
            // Header size
            br.AssertInt64(0x40);
            Timestamp = Util.ParseBNDTimestamp(br.ReadASCII(8));
            // File header size
            br.AssertInt64(0x24);
            long dataStart = br.ReadInt64();
            unicode = br.ReadBoolean();
            flag = br.AssertByte(0x54, 0x74);
            extended = br.AssertByte(0, 4);
            br.AssertByte(0);
            br.AssertInt32(0);
            long hashGroupsOffset = 0;
            if (extended == 4)
                hashGroupsOffset = br.ReadInt64();
            else
                br.AssertInt64(0);

            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, unicode));
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

        #region Public Write
        /// <summary>
        /// Writes a BND4 file as an array of bytes.
        /// </summary>
        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Writes a BND4 file to the specified path using file streams.
        /// </summary>
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
            bw.WriteASCII("BND4");
            bw.WriteInt32(0);
            bw.WriteInt32(0x10000);
            bw.WriteInt32(Files.Count);
            bw.WriteInt64(0x40);
            bw.WriteASCII(Util.UnparseBNDTimestamp(Timestamp));
            bw.WriteInt64(0x24);
            bw.ReserveInt64("DataStart");
            bw.WriteBoolean(unicode);
            bw.WriteByte(flag);
            bw.WriteByte(extended);
            bw.WriteByte(0);
            bw.WriteInt32(0);
            if (extended == 4)
                bw.ReserveInt64("HashGroups");
            else
                bw.WriteInt64(0);

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Write(bw, i);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.FillInt32($"FileName{i}", (int)bw.Position);
                if (unicode)
                    bw.WriteUTF16(file.Name, true);
                else
                    bw.WriteShiftJIS(file.Name, true);
            }

            if (extended == 4)
            {
                uint groupCount = 0;
                for (uint p = (uint)Files.Count / 7; p <= 100000; p++)
                {
                    if (Util.IsPrime(p))
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
                bw.WriteBytes(Files[i].Bytes);
            }
        }

        /// <summary>
        /// A generic file in a BND4 container.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The name of the file, typically a virtual path.
            /// </summary>
            public string Name;

            /// <summary>
            /// The ID number of the file.
            /// </summary>
            public int ID;

            /// <summary>
            /// The raw data of the file.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br, bool unicode)
            {
                br.AssertInt32(0x40);
                br.AssertInt32(-1);
                long fileSize = br.ReadInt64();
                br.AssertInt64(fileSize);
                int fileOffset = br.ReadInt32();
                ID = br.ReadInt32();
                int nameOffset = br.ReadInt32();

                Bytes = br.GetBytes(fileOffset, (int)fileSize);
                if (unicode)
                    Name = br.GetUTF16(nameOffset);
                else
                    Name = br.GetShiftJIS(nameOffset);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(0x40);
                bw.WriteInt32(-1);
                bw.WriteInt64(Bytes.LongLength);
                bw.WriteInt64(Bytes.LongLength);
                bw.ReserveInt32($"FileData{index}");
                bw.WriteInt32(ID);
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
