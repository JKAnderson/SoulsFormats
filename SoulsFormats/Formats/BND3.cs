using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used in DS1, DSR, DeS, and NB. Extension: .*bnd
    /// </summary>
    public class BND3 : SoulsFile<BND3>, IBinder
    {
        /// <summary>
        /// The files contained within this BND3.
        /// </summary>
        public List<File> Files;

        IReadOnlyList<IBinderFile> IBinder.Files => Files;

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Timestamp;

        /// <summary>
        /// Indicates the format of the BND3.
        /// </summary>
        public Binder.Format Format;

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
            Timestamp = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Format = Binder.Format.x74;
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
            Timestamp = br.ReadFixStr(8);

            Format = br.ReadEnum8<Binder.Format>();
            BigEndian = br.ReadBoolean();
            Unk1 = br.ReadBoolean();
            br.AssertByte(0);

            br.BigEndian = BigEndian || Binder.ForceBigEndian(Format);
            int fileCount = br.ReadInt32();
            int fileHeadersEnd = br.ReadInt32();
            Unk2 = br.ReadInt32();
            br.AssertInt32(0);

            Files = new List<File>(fileCount);
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
            bw.WriteFixStr(Timestamp, 8);
            bw.WriteByte((byte)Format);
            bw.WriteBoolean(BigEndian);
            bw.WriteBoolean(Unk1);
            bw.WriteByte(0);

            bw.BigEndian = BigEndian || Binder.ForceBigEndian(Format);
            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("HeaderEnd");
            bw.WriteInt32(Unk2);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
                Files[i].WriteHeader(bw, i, Format);

            if (Binder.HasName(Format))
            {
                for (int i = 0; i < Files.Count; i++)
                    Files[i].WriteName(bw, i);
            }

            bw.FillInt32($"HeaderEnd", (int)bw.Position);

            for (int i = 0; i < Files.Count; i++)
                Files[i].WriteData(bw, i);
        }

        /// <summary>
        /// A generic file in a BND3 container.
        /// </summary>
        public class File : IBinderFile
        {
            /// <summary>
            /// The name of the file, typically a virtual path.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The ID number of the file.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Flags indicating whether to compress the file and other things we don't understand.
            /// </summary>
            public Binder.FileFlags Flags;

            /// <summary>
            /// The raw data of the file.
            /// </summary>
            public byte[] Bytes { get; set; }

            /// <summary>
            /// Creates a new File with the given information.
            /// </summary>
            public File(int id, string name, Binder.FileFlags flags, byte[] bytes)
            {
                ID = id;
                Name = name;
                Flags = flags;
                Bytes = bytes;
            }

            internal File(BinaryReaderEx br, Binder.Format format)
            {
                Flags = br.ReadEnum8<Binder.FileFlags>();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                int compressedSize = br.ReadInt32();
                int fileOffset = br.ReadInt32();

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
                    int fileNameOffset = br.ReadInt32();
                    Name = br.GetShiftJIS(fileNameOffset);
                }
                else
                {
                    Name = null;
                }

                if (Binder.HasUncompressedSize(format))
                {
                    int uncompressedSize = br.ReadInt32();
                }

                // Compressed
                if (Binder.IsCompressed(Flags))
                {
                    br.StepIn(fileOffset);
                    Bytes = SFUtil.ReadZlib(br, compressedSize);
                    br.StepOut();
                }
                else
                {
                    Bytes = br.GetBytes(fileOffset, compressedSize);
                }
            }

            internal void WriteHeader(BinaryWriterEx bw, int index, Binder.Format format)
            {
                bw.WriteByte((byte)Flags);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.ReserveInt32($"CompressedSize{index}");
                bw.ReserveInt32($"FileData{index}");

                if (Binder.HasID(format))
                    bw.WriteInt32(ID);

                if (Binder.HasName(format))
                    bw.ReserveInt32($"FileName{index}");

                if (Binder.HasUncompressedSize(format))
                    bw.WriteInt32(Bytes.Length);
            }

            internal void WriteName(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"FileName{index}", (int)bw.Position);
                bw.WriteShiftJIS(Name, true);
            }

            internal void WriteData(BinaryWriterEx bw, int index)
            {
                if (Bytes.Length > 0)
                    bw.Pad(0x10);

                bw.FillInt32($"FileData{index}", (int)bw.Position);

                byte[] bytes = Bytes;
                int compressedSize = bytes.Length;

                if (Binder.IsCompressed(Flags))
                {
                    compressedSize = SFUtil.WriteZlib(bw, 0x9C, bytes);
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
