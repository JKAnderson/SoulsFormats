namespace SoulsFormats
{
    /// <summary>
    /// Parser for .dds texture file headers.
    /// </summary>
    public class DDS
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int dwFlags;
        public int dwHeight;
        public int dwWidth;
        public int dwPitchOrLinearSize;
        public int dwDepth;
        public int dwMipMapCount;
        public PIXELFORMAT ddspf;
        public int dwCaps;
        public int dwCaps2;
        public HEADER_DXT10 header10;

        /// <summary>
        /// Create a new DDS header with all values 0 and no DX10 header.
        /// </summary>
        public DDS()
        {
            dwFlags = 0;
            dwHeight = 0;
            dwWidth = 0;
            dwPitchOrLinearSize = 0;
            dwDepth = 0;
            dwMipMapCount = 0;
            ddspf = new PIXELFORMAT();
            dwCaps = 0;
            dwCaps2 = 0;
            header10 = null;
        }

        /// <summary>
        /// Read a DDS header from an array of bytes.
        /// </summary>
        public DDS(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            br.AssertASCII("DDS ");
            br.AssertInt32(124);
            dwFlags = br.ReadInt32();
            dwHeight = br.ReadInt32();
            dwWidth = br.ReadInt32();
            dwPitchOrLinearSize = br.ReadInt32();
            dwDepth = br.ReadInt32();
            dwMipMapCount = br.ReadInt32();

            // dwReserved1
            br.Skip(4 * 11);

            ddspf = new PIXELFORMAT(br);
            dwCaps = br.ReadInt32();
            dwCaps2 = br.ReadInt32();

            // dwCaps3, dwCaps4, dwReserved2
            br.Skip(4 * 3);

            if (ddspf.dwFourCC == "DX10")
                header10 = new HEADER_DXT10(br);
            else
                header10 = null;
        }

        /// <summary>
        /// Write a DDS file from this header object and given pixel data.
        /// </summary>
        public byte[] Write(byte[] pixelData)
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            bw.WriteASCII("DDS ");
            bw.WriteInt32(124);
            bw.WriteInt32(dwFlags);
            bw.WriteInt32(dwHeight);
            bw.WriteInt32(dwWidth);
            bw.WriteInt32(dwPitchOrLinearSize);
            bw.WriteInt32(dwDepth);
            bw.WriteInt32(dwMipMapCount);

            for (int i = 0; i < 11; i++)
                bw.WriteInt32(0);

            ddspf.Write(bw);
            bw.WriteInt32(dwCaps);
            bw.WriteInt32(dwCaps2);

            for (int i = 0; i < 3; i++)
                bw.WriteInt32(0);

            if (ddspf.dwFourCC == "DX10")
                header10.Write(bw);

            bw.WriteBytes(pixelData);
            return bw.FinishBytes();
        }

        public class PIXELFORMAT
        {
            public uint dwFlags;
            public string dwFourCC;
            public int dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;

            /// <summary>
            /// Create a new PIXELFORMAT with all values 0 and null FourCC.
            /// </summary>
            public PIXELFORMAT()
            {
                dwFlags = 0;
                dwFourCC = null;
                dwRGBBitCount = 0;
                dwRBitMask = 0;
                dwGBitMask = 0;
                dwBBitMask = 0;
                dwABitMask = 0;
            }

            internal PIXELFORMAT(BinaryReaderEx br)
            {
                br.AssertInt32(32);
                dwFlags = br.ReadUInt32();
                dwFourCC = br.ReadASCII(4);
                dwRGBBitCount = br.ReadInt32();
                dwRBitMask = br.ReadUInt32();
                dwGBitMask = br.ReadUInt32();
                dwBBitMask = br.ReadUInt32();
                dwABitMask = br.ReadUInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(32);
                bw.WriteUInt32(dwFlags);
                // Make sure it's 4 characters
                bw.WriteASCII(dwFourCC.PadRight(4).Substring(0, 4));
                bw.WriteInt32(dwRGBBitCount);
                bw.WriteUInt32(dwRBitMask);
                bw.WriteUInt32(dwGBitMask);
                bw.WriteUInt32(dwBBitMask);
                bw.WriteUInt32(dwABitMask);
            }
        }

        public class HEADER_DXT10
        {
            public uint dxgiFormat;
            public uint resourceDimension;
            public uint miscFlag;
            public uint arraySize;
            public uint miscFlags2;

            /// <summary>
            /// Create a new DX10 header with all values 0.
            /// </summary>
            public HEADER_DXT10()
            {
                dxgiFormat = 0;
                resourceDimension = 0;
                miscFlag = 0;
                arraySize = 0;
                miscFlags2 = 0;
            }

            internal HEADER_DXT10(BinaryReaderEx br)
            {
                dxgiFormat = br.ReadUInt32();
                resourceDimension = br.ReadUInt32();
                miscFlag = br.ReadUInt32();
                arraySize = br.ReadUInt32();
                miscFlags2 = br.ReadUInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(dxgiFormat);
                bw.WriteUInt32(resourceDimension);
                bw.WriteUInt32(miscFlag);
                bw.WriteUInt32(arraySize);
                bw.WriteUInt32(miscFlags2);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
