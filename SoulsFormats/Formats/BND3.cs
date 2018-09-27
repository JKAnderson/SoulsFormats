using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used in DS1, DSR, DeS, and NB. Extension: .*bnd
    /// </summary>
    public class BND3 : SoulsFile<BND3>
    {
        /// <summary>
        /// The files contained within this BND3.
        /// </summary>
        public List<File> Files;

        /// <summary>
        /// A timestamp of unknown purpose.
        /// </summary>
        public string Timestamp
        {
            get { return timestamp; }
            set
            {
                if (value.Length > 8)
                    throw new ArgumentException("Timestamp may not be longer than 8 characters.");
                timestamp = value;
            }
        }
        private string timestamp;

        /// <summary>
        /// Indicates the format of the BND3.
        /// </summary>
        public byte Format;

        /// <summary>
        /// Write bytes in big-endian order for PS3.
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// Unknown; usually false.
        /// </summary>
        public bool Unk1;

        /// <summary>
        /// Unknown; usually 0.
        /// </summary>
        public int Unk2;

        /// <summary>
        /// Creates an empty BND3 formatted for DS1.
        /// </summary>
        public BND3()
        {
            Files = new List<File>();
            Timestamp = Util.UnparseBNDTimestamp(DateTime.Now);
            Format = 0x74;
            BigEndian = false;
            Unk1 = false;
            Unk2 = 0;
        }

        /// <summary>
        /// Returns true if the data appears to be a BND3.
        /// </summary>
        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "BND3";
        }

        /// <summary>
        /// Reads BND3 data from a BinaryReaderEx.
        /// </summary>
        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("BND3");
            Timestamp = br.ReadASCII(8).TrimEnd('\0');

            Format = br.AssertByte(0x0E, 0x2E, 0x40, 0x54, 0x60, 0x64, 0x70, 0x74, 0xE0, 0xF0);
            BigEndian = br.ReadBoolean();
            Unk1 = br.ReadBoolean();
            br.AssertByte(0);

            br.BigEndian = BigEndian || Format == 0xE0 || Format == 0xF0;
            int fileCount = br.ReadInt32();
            // File headers end; sometimes 0, but it's redundant anyways
            br.ReadInt32();
            Unk2 = br.ReadInt32();
            br.AssertInt32(0);

            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, Format));
            }
        }

        /// <summary>
        /// Writes BND3 data to a BinaryWriterEx.
        /// </summary>
        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("BND3");
            bw.WriteASCII(Timestamp.PadRight(8, '\0'));
            bw.WriteByte(Format);
            bw.WriteBoolean(BigEndian);
            bw.WriteBoolean(Unk1);
            bw.WriteByte(0);

            bw.BigEndian = BigEndian || Format == 0xE0 || Format == 0xF0;
            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("HeaderEnd");
            bw.WriteInt32(Unk2);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
                Files[i].WriteHeader(bw, i, Format);

            if (Format != 0x40)
            {
                for (int i = 0; i < Files.Count; i++)
                    Files[i].WriteName(bw, i);
            }

            bw.FillInt32($"HeaderEnd", (int)bw.Position);

            for (int i = 0; i < Files.Count; i++)
                Files[i].WriteData(bw, i, Format);
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

            /// <summary>
            /// Creates a new File with the given information.
            /// </summary>
            public File(int id, string name, byte flags, byte[] bytes)
            {
                ID = id;
                Name = name;
                Flags = flags;
                Bytes = bytes;
            }

            internal File(BinaryReaderEx br, byte format)
            {
                Flags = br.AssertByte(0x02, 0x40, 0xC0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                int compressedSize = br.ReadInt32();
                int fileOffset = br.ReadInt32();
                ID = br.ReadInt32();

                if (format == 0x40)
                {
                    Name = null;
                }
                else
                {
                    int fileNameOffset = br.ReadInt32();
                    Name = br.GetShiftJIS(fileNameOffset);
                }

                int uncompressedSize = compressedSize;
                if (format == 0x2E || format == 0x54 || format == 0x64 || format == 0x74)
                    uncompressedSize = br.ReadInt32();

                // Compressed
                if (Flags == 0xC0)
                {
                    br.StepIn(fileOffset);
                    Bytes = Util.ReadZlib(br, compressedSize);
                    br.StepOut();
                }
                else
                {
                    Bytes = br.GetBytes(fileOffset, compressedSize);
                }
            }

            internal void WriteHeader(BinaryWriterEx bw, int index, byte format)
            {
                bw.WriteByte(Flags);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.ReserveInt32($"CompressedSize{index}");
                bw.ReserveInt32($"FileData{index}");
                bw.WriteInt32(ID);

                if (format != 0x40)
                    bw.ReserveInt32($"FileName{index}");

                if (format == 0x2E || format == 0x54 || format == 0x64 || format == 0x74)
                    bw.WriteInt32(Bytes.Length);
            }

            internal void WriteName(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"FileName{index}", (int)bw.Position);
                bw.WriteShiftJIS(Name, true);
            }

            internal void WriteData(BinaryWriterEx bw, int index, byte format)
            {
                if (Bytes.Length > 0)
                    bw.Pad(0x10);

                bw.FillInt32($"FileData{index}", (int)bw.Position);

                byte[] bytes = Bytes;
                int compressedSize = bytes.Length;

                if (Flags == 0xC0)
                {
                    compressedSize = Util.WriteZlib(bw, 0x9C, bytes);
                }
                else
                {
                    bw.WriteBytes(bytes);
                }

                bw.FillInt32($"CompressedSize{index}", bytes.Length);
            }

            /// <summary>
            /// Returns a string containing the ID and name of this file.
            /// </summary>
            public override string ToString()
            {
                return $"{ID} {Name ?? "<null>"}";
            }
        }
    }
}
