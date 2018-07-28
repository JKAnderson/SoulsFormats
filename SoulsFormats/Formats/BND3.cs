using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public class BND3
    {
        #region Public Read
        public static BND3 Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new BND3(br);
        }

        public static BND3 Read(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new BND3(br);
            }
        }
        #endregion

        public string Signature;
        private byte format;
        private bool bigEndian, unk1;
        private int unk2;
        public List<File> Files;

        private BND3(BinaryReaderEx br)
        {
            br.AssertASCII("BND3");
            Signature = br.ReadASCII(8);
            format = br.AssertByte(0xE, 0x2E, 0x54, 0x70, 0x74);
            bigEndian = br.ReadBoolean();
            unk1 = br.ReadBoolean();
            br.AssertByte(0);

            br.BigEndian = bigEndian;
            int fileCount = br.ReadInt32();
            int fileNameEnd = br.ReadInt32();
            unk2 = br.ReadInt32();
            br.AssertInt32(0);

            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, format));
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
            bw.WriteASCII("BND3");
            bw.WriteASCII(Signature);
            bw.WriteByte(format);
            bw.WriteBoolean(bigEndian);
            bw.WriteBoolean(unk1);
            bw.WriteByte(0);

            bw.BigEndian = bigEndian;
            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("NameEnd");
            bw.WriteInt32(unk2);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Write(bw, i, format);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.FillInt32($"FileName{i}", (int)bw.Position);
                bw.WriteShiftJIS(file.Name, true);
            }
            bw.FillInt32($"NameEnd", (int)bw.Position);

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                if (file.Bytes.LongLength > 0)
                    bw.Pad(0x10);

                bw.FillInt32($"FileData{i}", (int)bw.Position);
                bw.WriteBytes(file.Bytes);
            }
        }

        public class File
        {
            public string Name;
            public int ID;
            public byte[] Bytes;

            internal File(BinaryReaderEx br, byte format)
            {
                if (format == 0xE || format == 0x2E)
                    br.AssertInt32(0x2000000);
                else
                    br.AssertInt32(0x40);

                int fileSize = br.ReadInt32();
                int fileOffset = br.ReadInt32();
                // This is not the same as the index
                ID = br.ReadInt32();
                int fileNameOffset = br.ReadInt32();

                if (format == 0x2E || format == 0x54 || format == 0x74)
                    br.AssertInt32(fileSize);

                Name = br.GetShiftJIS(fileNameOffset);
                Bytes = br.GetBytes(fileOffset, fileSize);
            }

            internal void Write(BinaryWriterEx bw, int index, byte format)
            {
                if (format == 0xE || format == 0x2E)
                    bw.WriteInt32(0x2000000);
                else
                    bw.WriteInt32(0x40);

                bw.WriteInt32(Bytes.Length);
                bw.ReserveInt32($"FileData{index}");
                bw.WriteInt32(ID);
                bw.ReserveInt32($"FileName{index}");

                if (format == 0x2E || format == 0x54 || format == 0x74)
                    bw.WriteInt32(Bytes.Length);
            }
        }
    }
}
