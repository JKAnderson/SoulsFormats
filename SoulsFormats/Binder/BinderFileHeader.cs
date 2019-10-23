namespace SoulsFormats
{
    internal class BinderFileHeader
    {
        public Binder.FileFlags Flags;
        public long CompressedSize;
        public long DataOffset;
        public int ID;
        public string Name;
        public long UncompressedSize;

        private BinderFileHeader(Binder.FileFlags flags, long compressedSize, long dataOffset, int id, string name, long uncompressedSize)
        {
            Flags = flags;
            CompressedSize = compressedSize;
            DataOffset = dataOffset;
            ID = id;
            Name = name;
            UncompressedSize = uncompressedSize;
        }

        public static BinderFileHeader ReadBinder3FileHeader(BinaryReaderEx br, Binder.Format format, bool bitBigEndian)
        {
            Binder.FileFlags flags = Binder.ReadFileFlags(br, bitBigEndian, format);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);

            int compressedSize = br.ReadInt32();

            long dataOffset;
            if (Binder.HasLongOffsets(format))
                dataOffset = br.ReadInt64();
            else
                dataOffset = br.ReadUInt32();

            int id = -1;
            if (Binder.HasIDs(format))
                id = br.ReadInt32();

            string name = null;
            if (Binder.HasNames(format))
            {
                int nameOffset = br.ReadInt32();
                name = br.GetShiftJIS(nameOffset);
            }

            int uncompressedSize = -1;
            if (Binder.HasCompression(format))
                uncompressedSize = br.ReadInt32();

            return new BinderFileHeader(flags, compressedSize, dataOffset, id, name, uncompressedSize);
        }

        public static BinderFileHeader ReadBinder4FileHeader(BinaryReaderEx br, Binder.Format format, bool bitBigEndian, bool unicode)
        {
            Binder.FileFlags flags = Binder.ReadFileFlags(br, bitBigEndian, format);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertInt32(-1);

            long compressedSize = br.ReadInt64();

            long uncompressedSize = -1;
            if (Binder.HasCompression(format))
                uncompressedSize = br.ReadInt64();

            long dataOffset;
            if (Binder.HasLongOffsets(format))
                dataOffset = br.ReadInt64();
            else
                dataOffset = br.ReadUInt32();

            int id = -1;
            if (Binder.HasIDs(format))
                id = br.ReadInt32();

            string name = null;
            if (Binder.HasNames(format))
            {
                uint nameOffset = br.ReadUInt32();
                if (unicode)
                    name = br.GetUTF16(nameOffset);
                else
                    name = br.GetShiftJIS(nameOffset);
            }

            return new BinderFileHeader(flags, compressedSize, dataOffset, id, name, uncompressedSize);
        }

        public BinderFile ReadFileData(BinaryReaderEx br)
        {
            byte[] bytes;
            DCX.Type compressionType = DCX.Type.Unknown;
            if (Binder.IsCompressed(Flags))
            {
                bytes = br.GetBytes(DataOffset, (int)CompressedSize);
                bytes = DCX.Decompress(bytes, out compressionType);
            }
            else
            {
                bytes = br.GetBytes(DataOffset, (int)CompressedSize);
            }

            return new BinderFile(Flags, ID, Name, bytes)
            {
                CompressionType = compressionType,
            };
        }

        public static void WriteBinder3FileHeader(BinderFile file, BinaryWriterEx bw, Binder.Format format, bool bitBigEndian, int index)
        {
            Binder.WriteFileFlags(bw, bitBigEndian, format, file.Flags);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.ReserveInt32($"FileCompressedSize{index}");

            if (Binder.HasLongOffsets(format))
                bw.ReserveInt64($"FileDataOffset{index}");
            else
                bw.ReserveUInt32($"FileDataOffset{index}");

            if (Binder.HasIDs(format))
                bw.WriteInt32(file.ID);

            if (Binder.HasNames(format))
                bw.ReserveInt32($"FileNameOffset{index}");

            if (Binder.HasCompression(format))
                bw.WriteInt32(file.Bytes.Length);
        }

        public static void WriteBinder3FileData(BinderFile file, BinaryWriterEx bw, Binder.Format format, int index)
        {
            if (file.Bytes.Length > 0)
                bw.Pad(0x10);

            long dataOffset = bw.Position;
            int compressedSize;
            if (Binder.IsCompressed(file.Flags))
            {
                byte[] compressed = DCX.Compress(file.Bytes, file.CompressionType);
                compressedSize = compressed.Length;
                bw.WriteBytes(compressed);
            }
            else
            {
                compressedSize = file.Bytes.Length;
                bw.WriteBytes(file.Bytes);
            }

            bw.FillInt32($"FileCompressedSize{index}", compressedSize);

            if (Binder.HasLongOffsets(format))
                bw.FillInt64($"FileDataOffset{index}", dataOffset);
            else
                bw.FillUInt32($"FileDataOffset{index}", (uint)dataOffset);
        }

        public static void WriteBinder4FileHeader(BinderFile file, BinaryWriterEx bw, Binder.Format format, bool bitBigEndian, int index)
        {
            Binder.WriteFileFlags(bw, bitBigEndian, format, file.Flags);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(-1);

            bw.ReserveInt64($"FileCompressedSize{index}");

            if (Binder.HasCompression(format))
                bw.WriteInt64(file.Bytes.LongLength);

            if (Binder.HasLongOffsets(format))
                bw.ReserveInt64($"FileDataOffset{index}");
            else
                bw.ReserveUInt32($"FileDataOffset{index}");

            if (Binder.HasIDs(format))
                bw.WriteInt32(file.ID);

            if (Binder.HasNames(format))
                bw.ReserveInt32($"FileNameOffset{index}");
        }

        public static void WriteBinder4FileData(BinderFile file, BinaryWriterEx bw, Binder.Format format, int index)
        {
            if (file.Bytes.LongLength > 0)
                bw.Pad(0x10);

            long dataOffset = bw.Position;
            long compressedSize;
            if (Binder.IsCompressed(file.Flags))
            {
                if (file.CompressionType == DCX.Type.Unknown)
                {
                    compressedSize = SFUtil.WriteZlib(bw, 0x9C, file.Bytes);
                }
                else
                {
                    byte[] compressed = DCX.Compress(file.Bytes, file.CompressionType);
                    compressedSize = compressed.LongLength;
                    bw.WriteBytes(compressed);
                }
            }
            else
            {
                compressedSize = file.Bytes.LongLength;
                bw.WriteBytes(file.Bytes);
            }

            bw.FillInt64($"FileCompressedSize{index}", compressedSize);

            if (Binder.HasLongOffsets(format))
                bw.FillInt64($"FileDataOffset{index}", dataOffset);
            else
                bw.FillUInt32($"FileDataOffset{index}", (uint)dataOffset);
        }

        public static void WriteFileName(BinderFile file, BinaryWriterEx bw, Binder.Format format, bool unicode, int index)
        {
            if (Binder.HasNames(format))
            {
                bw.FillInt32($"FileNameOffset{index}", (int)bw.Position);
                if (unicode)
                    bw.WriteUTF16(file.Name, true);
                else
                    bw.WriteShiftJIS(file.Name, true);
            }
        }
    }
}
