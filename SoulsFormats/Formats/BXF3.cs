using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose headered file container used in DS1 and DSR. Extensions: .*bhd (header) and .*bdt (data)
    /// </summary>
    public class BXF3
    {
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
        
        /// <summary>
        /// A timestamp of unknown purpose.
        /// </summary>
        public string BHDTimestamp
        {
            get { return bhdTimestamp; }
            set
            {
                if (value.Length > 8)
                    throw new ArgumentException("Timestamp may not be longer than 8 characters.");
                else
                    bhdTimestamp = value.PadRight(8, '\0');
            }
        }
        private string bhdTimestamp;

        /// <summary>
        /// A timestamp of unknown purpose.
        /// </summary>
        public string BDTTimestamp
        {
            get { return bdtTimestamp; }
            set
            {
                if (value.Length > 8)
                    throw new ArgumentException("Timestamp may not be longer than 8 characters.");
                else
                    bdtTimestamp = value.PadRight(8, '\0');
            }
        }
        private string bdtTimestamp;

        /// <summary>
        /// The files contained within this BXF3.
        /// </summary>
        public List<File> Files;

        private int flag;

        private BXF3(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            BHD3 bhd = new BHD3(bhdReader);
            BHDTimestamp = bhd.Timestamp;
            flag = bhd.Flag;

            bdtReader.AssertASCII("BDF3");
            BDTTimestamp = bdtReader.ReadASCII(8);
            bdtReader.AssertInt32(0);

            Files = new List<File>();
            for (int i = 0; i < bhd.FileHeaders.Count; i++)
            {
                BHD3.FileHeader fileHeader = bhd.FileHeaders[i];
                string name = fileHeader.Name;
                byte[] data = bdtReader.GetBytes(fileHeader.Offset, fileHeader.Size);

                File file = new File
                {
                    Name = name,
                    Bytes = data
                };
                Files.Add(file);
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
            bhdWriter.WriteASCII("BHF3");
            bhdWriter.WriteASCII(BHDTimestamp);
            bhdWriter.WriteInt32(flag);
            bhdWriter.WriteInt32(Files.Count);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);

            bdtWriter.WriteASCII("BDF3");
            bdtWriter.WriteASCII(BDTTimestamp);
            bdtWriter.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bhdWriter.WriteInt32(0x40);
                bhdWriter.WriteInt32(file.Bytes.Length);
                bhdWriter.WriteInt32((int)bdtWriter.Position);
                bhdWriter.WriteInt32(i);
                bhdWriter.ReserveInt32($"FileName{i}");
                bhdWriter.WriteInt32(file.Bytes.Length);

                bdtWriter.WriteBytes(file.Bytes);
                bdtWriter.Pad(0x10);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bhdWriter.FillInt32($"FileName{i}", (int)bhdWriter.Position);
                bhdWriter.WriteShiftJIS(file.Name, true);
            }
        }

        private class BHD3
        {
            public string Timestamp;
            public List<FileHeader> FileHeaders;
            public int Flag;

            public BHD3(BinaryReaderEx br)
            {
                br.AssertASCII("BHF3");
                Timestamp = br.ReadASCII(8);
                Flag = br.ReadInt32();
                if (Flag != 0x54 && Flag != 0x74)
                    throw new NotSupportedException($"Unrecognized BHD flag: 0x{Flag:X}");

                int fileCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                FileHeaders = new List<FileHeader>();
                for (int i = 0; i < fileCount; i++)
                {
                    br.AssertInt32(0x40);
                    int fileSize = br.ReadInt32();
                    int fileOffset = br.ReadInt32();
                    br.AssertInt32(i);
                    int fileNameOffset = br.ReadInt32();
                    br.AssertInt32(fileSize);

                    string name = br.GetShiftJIS(fileNameOffset);
                    FileHeader fileHeader = new FileHeader()
                    {
                        Name = name,
                        Offset = fileOffset,
                        Size = fileSize,
                    };
                    FileHeaders.Add(fileHeader);
                }
            }

            public class FileHeader
            {
                public string Name;
                public int Offset;
                public int Size;
            }
        }

        /// <summary>
        /// A generic file in a BXF3 container.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The name of the file, typically a virtual path.
            /// </summary>
            public string Name;

            /// <summary>
            /// The raw data of the file.
            /// </summary>
            public byte[] Bytes;
        }
    }
}
