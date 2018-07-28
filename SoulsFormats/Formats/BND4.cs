using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public class BND4
    {
        #region Public Read
        public static BND4 Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new BND4(br);
        }

        public static BND4 Read(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new BND4(br);
            }
        }
        #endregion

        public List<File> Files;
        public List<PathHash> PathHashes;
        public List<HashGroup> HashGroups;
        public string Signature;
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
            Signature = br.ReadASCII(8);
            // File header size
            br.AssertInt64(0x24);
            long dataStart = br.ReadInt64();
            unicode = br.ReadBoolean();
            flag = br.AssertByte(0x54, 0x74);
            extended = br.AssertByte(0, 4);
            br.AssertByte(0);
            br.AssertInt32(0);
            long unkSection1Offset = 0;
            if (extended == 4)
                unkSection1Offset = br.ReadInt64();
            else
                br.AssertInt64(0);

            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, unicode));
            }

            if (extended == 4)
            {
                br.Position = unkSection1Offset;
                long unkSection2Offset = br.ReadInt64();
                int unkEntry1Count = br.ReadInt32();
                // Probably 4 bytes
                br.AssertInt32(0x00080810);

                PathHashes = new List<PathHash>();
                for (int i = 0; i < unkEntry1Count; i++)
                {
                    PathHashes.Add(new PathHash(br));
                }

                br.Position = unkSection2Offset;
                HashGroups = new List<HashGroup>();
                for (int i = 0; i < fileCount; i++)
                {
                    HashGroups.Add(new HashGroup(br));
                }
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
            bw.WriteASCII("BND4");
            bw.WriteInt32(0);
            bw.WriteInt32(0x10000);
            bw.WriteInt32(Files.Count);
            bw.WriteInt64(0x40);
            bw.WriteASCII(Signature);
            bw.WriteInt64(0x24);
            bw.ReserveInt64("DataStart");
            bw.WriteBoolean(unicode);
            bw.WriteByte(flag);
            bw.WriteByte(extended);
            bw.WriteByte(0);
            bw.WriteInt32(0);
            if (extended == 4)
                bw.ReserveInt64("UnkSection1");
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
                bw.Pad(0x8);
                bw.FillInt64("UnkSection1", bw.Position);
                bw.ReserveInt64("UnkSection2");
                bw.WriteInt32(PathHashes.Count);
                bw.WriteInt32(0x00080810);

                foreach (PathHash unkEntry1 in PathHashes)
                {
                    unkEntry1.Write(bw);
                }

                // No padding after section 1
                bw.FillInt64("UnkSection2", bw.Position);
                foreach (HashGroup unkEntry2 in HashGroups)
                {
                    unkEntry2.Write(bw);
                }
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

        public class File
        {
            public string Name;
            public int ID;
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

        public class PathHash
        {
            public int Index;
            public uint Hash;

            internal PathHash(BinaryReaderEx br)
            {
                Hash = br.ReadUInt32();
                Index = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(Hash);
                bw.WriteInt32(Index);
            }
        }

        public class HashGroup
        {
            public int Index, Length;

            internal HashGroup(BinaryReaderEx br)
            {
                Length = br.ReadInt32();
                Index = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Length);
                bw.WriteInt32(Index);
            }
        }
    }
}
