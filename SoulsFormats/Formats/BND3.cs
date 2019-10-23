using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used before DS2. Extension: .*bnd
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
        public string Version { get; set; }

        /// <summary>
        /// Indicates the format of the BND3.
        /// </summary>
        public Binder.Format Format { get; set; }

        /// <summary>
        /// Write bytes in big-endian order for PS3/X360.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Unknown; usually false.
        /// </summary>
        public bool BitBigEndian { get; set; }

        /// <summary>
        /// Unknown; always 0 except in DeS where it's occasionally 0x80000000 (probably a byte).
        /// </summary>
        public int Unk18 { get; set; }

        /// <summary>
        /// Creates an empty BND3 formatted for DS1.
        /// </summary>
        public BND3()
        {
            Files = new List<BinderFile>();
            Version = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression;
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BND3";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("BND3");
            Version = br.ReadFixStr(8);

            BitBigEndian = br.GetBoolean(0xE);

            Format = Binder.ReadFormat(br, BitBigEndian);
            BigEndian = br.ReadBoolean();
            br.AssertBoolean(BitBigEndian);
            br.AssertByte(0);

            br.BigEndian = BigEndian || Binder.ForceBigEndian(Format);

            int fileCount = br.ReadInt32();
            br.ReadInt32(); // End of file headers, not including padding before data
            Unk18 = br.AssertInt32(0, unchecked((int)0x80000000));
            br.AssertInt32(0);

            Files = new List<BinderFile>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                BinderFileHeader fileHeader = BinderFileHeader.ReadBinder3FileHeader(br, Format, BitBigEndian);
                Files.Add(fileHeader.ReadFileData(br));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian || Binder.ForceBigEndian(Format);

            bw.WriteASCII("BND3");
            bw.WriteFixStr(Version, 8);

            Binder.WriteFormat(bw, BigEndian, Format);
            bw.WriteBoolean(BigEndian);
            bw.WriteBoolean(BitBigEndian);
            bw.WriteByte(0);

            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("FileHeadersEnd");
            bw.WriteInt32(Unk18);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder3FileHeader(Files[i], bw, Format, BitBigEndian, i);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteFileName(Files[i], bw, Format, false, i);

            bw.FillInt32($"FileHeadersEnd", (int)bw.Position);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder3FileData(Files[i], bw, bw, Format, i);
        }
    }
}
