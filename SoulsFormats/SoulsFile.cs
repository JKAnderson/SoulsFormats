using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A generic From file supporting transparent DCX reading and writing.
    /// </summary>
    public abstract class SoulsFile<TFormat> where TFormat : SoulsFile<TFormat>, new()
    {
        /// <summary>
        /// The type of DCX compression to be used when writing.
        /// </summary>
        public DCX.Type Compression = DCX.Type.None;

        /// <summary>
        /// Decompresses data and returns a new BinaryReaderEx if necessary.
        /// </summary>
        private static BinaryReaderEx GetDecompressedBR(BinaryReaderEx br, out DCX.Type compression)
        {
            if (DCX.Is(br))
            {
                byte[] bytes = DCX.Decompress(br, out compression);
                return new BinaryReaderEx(false, bytes);
            }
            else
            {
                compression = DCX.Type.None;
                return br;
            }
        }

        /// <summary>
        /// Returns true if the data appears to be a BND4.
        /// </summary>
        internal abstract bool Is(BinaryReaderEx br);

        /// <summary>
        /// Returns true if the bytes appear to be a file of this type.
        /// </summary>
        public static bool Is(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            var dummy = new TFormat();
            return dummy.Is(GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a file of this type.
        /// </summary>
        public static bool Is(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                var dummy = new TFormat();
                return dummy.Is(GetDecompressedBR(br, out _));
            }
        }

        /// <summary>
        /// Loads file data from a BinaryReaderEx.
        /// </summary>
        internal abstract void Read(BinaryReaderEx br);

        /// <summary>
        /// Loads a file from a byte array, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            TFormat file = new TFormat();
            br = GetDecompressedBR(br, out file.Compression);
            file.Read(br);
            return file;
        }

        /// <summary>
        /// Loads a file from the specified path, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                TFormat file = new TFormat();
                br = GetDecompressedBR(br, out file.Compression);
                file.Read(br);
                return file;
            }
        }

        /// <summary>
        /// Writes file data to a BinaryWriterEx.
        /// </summary>
        internal abstract void Write(BinaryWriterEx bw);

        /// <summary>
        /// Writes file data to a BinaryWriterEx, compressing it afterwards if specified.
        /// </summary>
        private void Write(BinaryWriterEx bw, DCX.Type compression)
        {
            if (compression == DCX.Type.None)
            {
                Write(bw);
            }
            else
            {
                BinaryWriterEx bwUncompressed = new BinaryWriterEx(false);
                Write(bwUncompressed);
                byte[] uncompressed = bwUncompressed.FinishBytes();
                DCX.Compress(uncompressed, bw, compression);
            }
        }

        /// <summary>
        /// Writes the file to an array of bytes, automatically compressing it if necessary.
        /// </summary>
        public byte[] Write()
        {
            return Write(Compression);
        }

        /// <summary>
        /// Writes the file to an array of bytes, compressing it as specified.
        /// </summary>
        public byte[] Write(DCX.Type compression)
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw, compression);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Writes the file to the specified path, automatically compressing it if necessary.
        /// </summary>
        public void Write(string path)
        {
            Write(path, Compression);
        }

        /// <summary>
        /// Writes the file to the specified path, compressing it as specified.
        /// </summary>
        public void Write(string path, DCX.Type compression)
        {
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, stream);
                Write(bw, compression);
                bw.Finish();
            }
        }
    }
}
