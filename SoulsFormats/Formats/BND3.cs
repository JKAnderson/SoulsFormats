using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used in DS1, DSR, DeS, and NB. Extension: .*bnd
    /// </summary>
    public class BND3
    {
        #region Public Read
        /// <summary>
        /// Reads an array of bytes as a BND3.
        /// </summary>
        public static BND3 Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new BND3(br);
        }

        /// <summary>
        /// Reads a file as a BND3 using file streams.
        /// </summary>
        public static BND3 Read(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new BND3(br);
            }
        }
        #endregion

        /// <summary>
        /// A timestamp of unknown purpose.
        /// </summary>
        public DateTime Timestamp;

        /// <summary>
        /// Indicates the format of the BND3.
        /// </summary>
        public byte Format;

        /// <summary>
        /// The files contained within this BND3.
        /// </summary>
        public List<File> Files;

        private bool bigEndian, unk1;
        private bool writeNameEnd;
        private int unk2;

        private BND3(BinaryReaderEx br)
        {
            br.AssertASCII("BND3");
            Timestamp = Util.ParseBNDTimestamp(br.ReadASCII(8));

            Format = br.AssertByte(0x0E, 0x2E, 0x54, 0x60, 0x64, 0x70, 0x74, 0xE0, 0xF0);
            bigEndian = br.ReadBoolean();
            unk1 = br.ReadBoolean();
            br.AssertByte(0);

            br.BigEndian = bigEndian || Format == 0xE0 || Format == 0xF0;
            int fileCount = br.ReadInt32();
            int fileNameEnd = br.ReadInt32();
            writeNameEnd = fileNameEnd != 0;
            unk2 = br.ReadInt32();
            br.AssertInt32(0);

            // There are 12 DeS BNDs with 0 count and all file header fields blank except the name offset
            // No idea what to do about it. Ex: chr\0300\c0300.anibnd
            if (fileCount == 0)
                throw new NotImplementedException("Zero-count BND3 is not supported.");

            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, Format));
            }
        }

        #region Public Write
        /// <summary>
        /// Writes a BND3 file as an array of bytes.
        /// </summary>
        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Writes a BND3 file to the specified path using file streams.
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
            bw.WriteASCII("BND3");
            bw.WriteASCII(Util.UnparseBNDTimestamp(Timestamp));
            bw.WriteByte(Format);
            bw.WriteBoolean(bigEndian);
            bw.WriteBoolean(unk1);
            bw.WriteByte(0);

            bw.BigEndian = bigEndian || Format == 0xE0 || Format == 0xF0;
            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("NameEnd");
            bw.WriteInt32(unk2);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Write(bw, i, Format);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.FillInt32($"FileName{i}", (int)bw.Position);
                bw.WriteShiftJIS(file.Name, true);
            }
            bw.FillInt32($"NameEnd", writeNameEnd ? (int)bw.Position : 0);

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                if (file.Bytes.Length > 0)
                    bw.Pad(0x10);

                bw.FillInt32($"FileData{i}", (int)bw.Position);

                byte[] bytes = file.Bytes;
                int compressedSize = bytes.Length;

                if ((file.Flags & 0x80) != 0)
                {
                    compressedSize = Util.WriteZlib(bw, 0x9C, bytes);
                }
                else
                {
                    bw.WriteBytes(bytes);
                }

                bw.FillInt32($"CompressedSize{i}", bytes.Length);
            }
        }

        /// <summary>
        /// A generic file in a BND3 container.
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
            /// Flags indicating whether to compress the file (0x80) and other things we don't understand.
            /// </summary>
            public byte Flags;

            /// <summary>
            /// The raw data of the file.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br, byte format)
            {
                Flags = br.AssertByte(0x02, 0x40, 0xC0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                int compressedSize = br.ReadInt32();
                int fileOffset = br.ReadInt32();
                ID = br.ReadInt32();
                int fileNameOffset = br.ReadInt32();

                int uncompressedSize = compressedSize;
                if (format == 0x2E || format == 0x54 || format == 0x64 || format == 0x74)
                    uncompressedSize = br.ReadInt32();

                Name = br.GetShiftJIS(fileNameOffset);

                // Compressed
                if ((Flags & 0x80) != 0)
                {
                    br.StepIn(fileOffset);
                    Bytes = Util.ReadZlib(br, 0x9C, compressedSize);
                    br.StepOut();
                }
                else
                {
                    Bytes = br.GetBytes(fileOffset, compressedSize);
                }
            }

            internal void Write(BinaryWriterEx bw, int index, byte format)
            {
                bw.WriteByte(Flags);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.ReserveInt32($"CompressedSize{index}");
                bw.ReserveInt32($"FileData{index}");
                bw.WriteInt32(ID);
                bw.ReserveInt32($"FileName{index}");

                if (format == 0x2E || format == 0x54 || format == 0x64 || format == 0x74)
                    bw.WriteInt32(Bytes.Length);
            }
        }
    }
}
