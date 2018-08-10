using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats
{
    /// <summary>
    /// A file container used in A.C.E. 3.
    /// </summary>
    public class BND0
    {
        #region Public Read
        /// <summary>
        /// Reads an array of bytes as a BND0.
        /// </summary>
        public static BND0 Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new BND0(br);
        }

        /// <summary>
        /// Reads a file as a BND3 using file streams.
        /// </summary>
        public static BND0 Read(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new BND0(br);
            }
        }
        #endregion

        /// <summary>
        /// The files contained in this BND0.
        /// </summary>
        public List<File> Files;

        private BND0(BinaryReaderEx br)
        {
            br.AssertASCII("BND\0");
            br.AssertInt32(0xF7FF);
            br.AssertInt32(0xD3);
            int unk1 = br.ReadInt32();
            int fileCount = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0x30800);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            Files = new List<File>();
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br));
            }
        }

        /// <summary>
        /// A file in a BND0 container.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The ID number of this file.
            /// </summary>
            public int ID;

            /// <summary>
            /// The raw data of this file.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br)
            {
                int offset = br.ReadInt32();
                int size = br.ReadInt32();
                ID = br.ReadInt32();

                Bytes = br.GetBytes(offset, size);
            }
        }
    }
}
