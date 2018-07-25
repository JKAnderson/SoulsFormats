using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoulsFormats
{
    class BND4
    {
        #region Public Read
        public static BND4 Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new BND4(br);
        }

        public static BND4 Read(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new BND4(br);
            }
        }
        #endregion
        
        private string signature;
        private bool unicode;

        private BND4(BinaryReaderEx br)
        {
            br.AssertASCII("BND4");
            br.AssertInt32(0);
            br.AssertInt32(0x10000);
            int fileCount = br.ReadInt32();
            br.AssertInt32(0x40);
            br.AssertInt32(0);
            signature = br.ReadASCII(8);
            // Entry size
            br.AssertInt64(0x24);
            long dataStart = br.ReadInt64();
            unicode = br.ReadBoolean();
            br.AssertByte(0x74);
            br.AssertByte(4);
            br.AssertByte(0);
            br.AssertInt32(0);
            long unkSection1Offset = br.ReadInt64();

        }

        #region Public Write
        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw);
            return bw.FinishBytes();
        }

        public void Write(string path)
        {
            using (FileStream stream = System.IO.File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, stream);
                Write(bw);
                bw.Finish();
            }
        }
        #endregion

        private void Write(BinaryWriterEx bw)
        {

        }
    }
}
