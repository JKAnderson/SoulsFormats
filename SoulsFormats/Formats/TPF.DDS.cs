namespace SoulsFormats
{
    public partial class TPF : SoulsFile<TPF>
    {
        private class DDS
        {
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

            public class PIXELFORMAT
            {
                public uint dwFlags;
                public string dwFourCC;
                public int dwRGBBitCount;
                public uint dwRBitMask;
                public uint dwGBitMask;
                public uint dwBBitMask;
                public uint dwABitMask;

                public PIXELFORMAT(BinaryReaderEx br)
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
            }

            public class HEADER_DXT10
            {
                public uint dxgiFormat;
                public uint resourceDimension;
                public uint miscFlag;
                public uint arraySize;
                public uint miscFlags2;

                public HEADER_DXT10(BinaryReaderEx br)
                {
                    dxgiFormat = br.ReadUInt32();
                    resourceDimension = br.ReadUInt32();
                    miscFlag = br.ReadUInt32();
                    arraySize = br.ReadUInt32();
                    miscFlags2 = br.ReadUInt32();
                }
            }
        }
    }
}
