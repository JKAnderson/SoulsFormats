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
        /// Returns true if the data appears to be a DCX file.
        /// </summary>
        private static bool IsDCX(BinaryReaderEx br)
        {
            br.StepIn(0);
            string magic = br.ReadASCII(4);
            br.StepOut();
            return magic == "DCX\0" || magic == "DCP\0";
        }

        /// <summary>
        /// Loads file data from a BinaryReaderEx.
        /// </summary>
        protected internal abstract void Read(BinaryReaderEx br);

        /// <summary>
        /// Loads file data from a BinaryReaderEx, first decompressing it if necessary.
        /// </summary>
        private static TFormat Read(BinaryReaderEx br, bool isDCX)
        {
            TFormat file = new TFormat();
            if (isDCX)
            {
                byte[] bytes = DCX.Decompress(br, out file.Compression);
                br = new BinaryReaderEx(false, bytes);
            }
            file.Read(br);
            return file;
        }

        /// <summary>
        /// Loads a file from a byte array, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return Read(br, IsDCX(br));
        }

        /// <summary>
        /// Loads a file from the specified path, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return Read(br, IsDCX(br));
            }
        }

        /// <summary>
        /// Writes file data to a BinaryWriterEx.
        /// </summary>
        protected internal abstract void Write(BinaryWriterEx bw);

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
            Write(Compression);
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
