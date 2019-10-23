using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used since DS2. Extension: .*bnd
    /// </summary>
    public class BND4 : SoulsFile<BND4>, IBinder
    {
        /// <summary>
        /// The files contained within this BND4.
        /// </summary>
        public List<BinderFile> Files { get; set; }

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Indicates the format of this BND4.
        /// </summary>
        public Binder.Format Format { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk04 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk05 { get; set; }

        /// <summary>
        /// Whether to write in big-endian format or not (little-endian).
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Controls ordering of flag bits.
        /// </summary>
        public bool BitBigEndian { get; set; }

        /// <summary>
        /// Whether to encode filenames as UTF-8 or Shift JIS.
        /// </summary>
        public bool Unicode { get; set; }

        /// <summary>
        /// Indicates presence of filename hash table.
        /// </summary>
        public byte Extended { get; set; }

        /// <summary>
        /// Creates an empty BND4 formatted for DS3.
        /// </summary>
        public BND4()
        {
            Files = new List<BinderFile>();
            Version = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression;
            Unicode = true;
            Extended = 4;
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BND4";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("BND4");

            Unk04 = br.ReadBoolean();
            Unk05 = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);

            br.AssertByte(0);
            BigEndian = br.ReadBoolean();
            BitBigEndian = !br.ReadBoolean();
            br.AssertByte(0);

            br.BigEndian = BigEndian;

            int fileCount = br.ReadInt32();
            br.AssertInt64(0x40); // Header size
            Version = br.ReadFixStr(8);
            long fileHeaderSize = br.ReadInt64();
            br.ReadInt64(); // Headers end (includes hash table)

            Unicode = br.ReadBoolean();
            Format = Binder.ReadFormat(br, BitBigEndian);
            Extended = br.AssertByte(0, 1, 4, 0x80);
            br.AssertByte(0);

            br.AssertInt32(0);

            if (Extended == 4)
            {
                long hashTableOffset = br.ReadInt64();
                br.StepIn(hashTableOffset);
                BinderHashTable.Assert(br);
                br.StepOut();
            }
            else
            {
                br.AssertInt64(0);
            }

            if (fileHeaderSize != Binder.GetBND4FileHeaderSize(Format))
                throw new FormatException($"File header size for format {Format} is expected to be 0x{Binder.GetBND4FileHeaderSize(Format):X}, but was 0x{fileHeaderSize:X}");

            Files = new List<BinderFile>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                BinderFileHeader fileHeader = BinderFileHeader.ReadBinder4FileHeader(br, Format, BitBigEndian, Unicode);
                Files.Add(fileHeader.ReadFileData(br));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;

            bw.WriteASCII("BND4");

            bw.WriteBoolean(Unk04);
            bw.WriteBoolean(Unk05);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteByte(0);
            bw.WriteBoolean(BigEndian);
            bw.WriteBoolean(!BitBigEndian);
            bw.WriteByte(0);

            bw.WriteInt32(Files.Count);
            bw.WriteInt64(0x40);
            bw.WriteFixStr(Version, 8);
            bw.WriteInt64(Binder.GetBND4FileHeaderSize(Format));
            bw.ReserveInt64("HeadersEnd");

            bw.WriteBoolean(Unicode);
            Binder.WriteFormat(bw, BitBigEndian, Format);
            bw.WriteByte(Extended);
            bw.WriteByte(0);

            bw.WriteInt32(0);
            bw.ReserveInt64("HashTableOffset");

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder4FileHeader(Files[i], bw, Format, BitBigEndian, i);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteFileName(Files[i], bw, Format, Unicode, i);

            if (Extended == 4)
            {
                bw.Pad(0x8);
                bw.FillInt64("HashTableOffset", bw.Position);
                BinderHashTable.Write(bw, Files);
            }
            else
            {
                bw.FillInt64("HashTableOffset", 0);
            }

            bw.FillInt64("HeadersEnd", bw.Position);

            for (int i = 0; i < Files.Count; i++)
                BinderFileHeader.WriteBinder4FileData(Files[i], bw, bw, Format, i);
        }
    }
}
