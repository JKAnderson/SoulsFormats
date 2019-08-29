using System;
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
        /// Returns true if the data appears to be a file of this type.
        /// </summary>
        // This should really be a static method, but interfaces do not allow static inheritance; hence the dummy objects below.
        internal abstract bool Is(BinaryReaderEx br);

        /// <summary>
        /// Returns true if the bytes appear to be a file of this type.
        /// </summary>
        public static bool Is(byte[] bytes)
        {
            if (bytes.Length == 0)
                return false;

            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            var dummy = new TFormat();
            return dummy.Is(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a file of this type.
        /// </summary>
        public static bool Is(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length == 0)
                    return false;

                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                var dummy = new TFormat();
                return dummy.Is(SFUtil.GetDecompressedBR(br, out _));
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
            br = SFUtil.GetDecompressedBR(br, out file.Compression);
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
                br = SFUtil.GetDecompressedBR(br, out file.Compression);
                file.Read(br);
                return file;
            }
        }

        /// <summary>
        /// Checks the object for any fatal problems; Write will throw the returned exception on failure.
        /// </summary>
        public virtual bool Validate(out Exception ex)
        {
            ex = null;
            return true;
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
            if (!Validate(out Exception ex))
                throw ex;

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
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, stream);
                Write(bw, compression);
                bw.Finish();
            }
        }
    }
}
