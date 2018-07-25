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

        private string signature;
        private byte format;
        public List<File> Files;

        private BND3(BinaryReaderEx br)
        {
            br.AssertASCII("BND3");
            // FaceGen.fgbnd: 09G17X51
            // Everything else (that I'm checking): 07D7R6\0\0
            signature = br.ReadASCII(8);
            format = br.AssertByte(0x54, 0x74);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);

            int fileCount = br.ReadInt32();
            if (fileCount == 0)
                throw new NotSupportedException("Empty BND :(");
            int fileNameEnd = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);

            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br));
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
            bw.WriteASCII(signature);
            bw.WriteByte(format);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("NameEnd");
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Write(bw, i);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.FillInt32($"FileName{i}", (int)bw.Position);
                bw.WriteShiftJIS(file.Name, true);
            }

            // Do not include padding
            bw.FillInt32($"NameEnd", (int)bw.Position);
            bw.Pad(0x10);

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.FillInt32($"FileData{i}", (int)bw.Position);
                bw.WriteBytes(file.Bytes);
                bw.Pad(0x10);
            }
        }

        public class File
        {
            public string Name;
            public int ID;
            public byte[] Bytes;

            internal File(BinaryReaderEx br)
            {
                br.AssertInt32(0x40);
                int fileSize = br.ReadInt32();
                int fileOffset = br.ReadInt32();
                // This is not the same as the index
                ID = br.ReadInt32();
                int fileNameOffset = br.ReadInt32();
                br.AssertInt32(fileSize);

                Name = br.GetShiftJIS(fileNameOffset);
                Bytes = br.GetBytes(fileOffset, fileSize);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(0x40);
                bw.WriteInt32(Bytes.Length);
                bw.ReserveInt32($"FileData{index}");
                bw.WriteInt32(ID);
                bw.ReserveInt32($"FileName{index}");
                bw.WriteInt32(Bytes.Length);
            }
        }
    }
}
