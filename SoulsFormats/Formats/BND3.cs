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
        public List<BinderFile> Files { get; set; }

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Timestamp;

        /// <summary>
        /// Indicates the format of the BND3.
        /// </summary>
        public Binder.Format Format { get; set; }

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
            Files = new List<BinderFile>();
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
            if (br.Length < 4)
                return false;

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

            Files = new List<BinderFile>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(ReadFile(br, Format));
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
                WriteFileHeader(Files[i], bw, i, Format);

            if (Binder.HasName(Format))
            {
                for (int i = 0; i < Files.Count; i++)
                    WriteFileName(Files[i], bw, i);
            }

            bw.FillInt32($"HeaderEnd", (int)bw.Position);

            for (int i = 0; i < Files.Count; i++)
                WriteFileData(Files[i], bw, i);
        }

        private static BinderFile ReadFile(BinaryReaderEx br, Binder.Format format)
        {
            Binder.FileFlags flags = br.ReadEnum8<Binder.FileFlags>();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);

            int compressedSize = br.ReadInt32();
            uint fileOffset = br.ReadUInt32();

            int id = -1;
            if (Binder.HasID(format))
            {
                id = br.ReadInt32();
            }

            string name = null;
            if (Binder.HasName(format))
            {
                uint fileNameOffset = br.ReadUInt32();
                name = br.GetShiftJIS(fileNameOffset);
            }

            if (Binder.HasUncompressedSize(format))
            {
                int uncompressedSize = br.ReadInt32();
            }

            byte[] bytes;
            if (Binder.IsCompressed(flags))
            {
                br.StepIn(fileOffset);
                bytes = SFUtil.ReadZlib(br, compressedSize);
                br.StepOut();
            }
            else
            {
                bytes = br.GetBytes(fileOffset, compressedSize);
            }

            return new BinderFile(flags, id, name, bytes);
        }

        private static void WriteFileHeader(BinderFile file, BinaryWriterEx bw, int index, Binder.Format format)
        {
            bw.WriteByte((byte)file.Flags);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.ReserveInt32($"CompressedSize{index}");
            bw.ReserveUInt32($"FileData{index}");

            if (Binder.HasID(format))
                bw.WriteInt32(file.ID);

            if (Binder.HasName(format))
                bw.ReserveUInt32($"FileName{index}");

            if (Binder.HasUncompressedSize(format))
                bw.WriteInt32(file.Bytes.Length);
        }

        private static void WriteFileName(BinderFile file, BinaryWriterEx bw, int index)
        {
            bw.FillUInt32($"FileName{index}", (uint)bw.Position);
            bw.WriteShiftJIS(file.Name, true);
        }

        private static void WriteFileData(BinderFile file, BinaryWriterEx bw, int index)
        {
            if (file.Bytes.Length > 0)
                bw.Pad(0x10);

            bw.FillUInt32($"FileData{index}", (uint)bw.Position);

            int compressedSize = file.Bytes.Length;
            if (Binder.IsCompressed(file.Flags))
            {
                compressedSize = SFUtil.WriteZlib(bw, 0x9C, file.Bytes);
            }
            else
            {
                bw.WriteBytes(file.Bytes);
            }

            bw.FillInt32($"CompressedSize{index}", compressedSize);
        }
    }
}
