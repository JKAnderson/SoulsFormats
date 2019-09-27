using System;
using System.Linq;
using System.Text;
using static SoulsFormats.DDS;

namespace SoulsFormats
{
    /* Known TPF texture formats
      0 - DXT1
      1 - DXT1
      3 - DXT3
      5 - DXT5
      6 - B5G5R5A1_UNORM
      9 - B8G8R8A8
     10 - R8G8B8
     16 - A8
     22 - A16B16G16R16f
     23 - DXT5
     24 - DXT1
     25 - DXT1
     33 - DXT5
    100 - BC6H_UF16
    102 - BC7_UNORM
    103 - ATI1
    104 - ATI2
    105 - A8B8G8R8
    106 - BC7_UNORM
    107 - BC7_UNORM
    108 - DXT1
    109 - DXT1
    110 - DXT5
    112 - BC7_UNORM_SRGB
    113 - BC6H_UF16
    */
    internal static class Headerizer
    {
        private static byte[] PitchFormats = { 0, 1, 3, 5, 23, 24, 25, 33, 100, 102, 103, 104, 106, 107, 108, 109, 110, 112, 113 };
        private static byte[] LinearFormats = { 6, 9, 10, 16, 22, 105 };
        private static byte[] FourCCFormats = { 0, 1, 3, 5, 6, 22, 23, 24, 25, 33, 100, 102, 103, 104, 106, 107, 108, 109, 110, 112, 113 };

        public static byte[] Headerize(TPF.Texture texture)
        {
            if (Encoding.ASCII.GetString(texture.Bytes, 0, 4) == "DDS ")
                return texture.Bytes;

            byte format = texture.Format;
            var dds = new DDS();

            dds.dwFlags = DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT;
            if (PitchFormats.Contains(format))
                dds.dwFlags |= DDSD.PITCH;
            else if (LinearFormats.Contains(format))
                dds.dwFlags |= DDSD.LINEARSIZE;

            dds.dwHeight = texture.Header.Height;
            dds.dwWidth = texture.Header.Width;

            if (format == 22)
                dds.dwPitchOrLinearSize = (texture.Header.Width * 64 + 7) / 8;
            else if (format == 9 || format == 105)
                dds.dwPitchOrLinearSize = (texture.Header.Width * 32 + 7) / 8;
            else if (format == 10)
                dds.dwPitchOrLinearSize = (texture.Header.Width * 24 + 7) / 8;
            else if (format == 6)
                dds.dwPitchOrLinearSize = (texture.Header.Width * 16 + 7) / 8;
            else if (format == 16)
                dds.dwPitchOrLinearSize = (texture.Header.Width * 8 + 7) / 8;
            else if (format == 0 || format == 1 || format == 24 || format == 25 || format == 108 || format == 109)
                dds.dwPitchOrLinearSize = Math.Max(1, (texture.Header.Width + 3) / 4) * 8;
            else
                dds.dwPitchOrLinearSize = Math.Max(1, (texture.Header.Width + 3) / 4) * 16;

            dds.dwMipMapCount = texture.Mipmaps;

            dds.dwCaps = DDSCAPS.TEXTURE;
            if (texture.Type == TPF.TexType.Cubemap)
                dds.dwCaps |= DDSCAPS.COMPLEX;
            if (texture.Mipmaps > 1)
                dds.dwCaps |= DDSCAPS.COMPLEX | DDSCAPS.MIPMAP;

            if (texture.Type == TPF.TexType.Cubemap)
                dds.dwCaps2 = CUBEMAP_ALLFACES;
            else if (texture.Type == TPF.TexType.Volume)
                dds.dwCaps2 = DDSCAPS2.VOLUME;

            PIXELFORMAT ddspf = dds.ddspf;

            if (FourCCFormats.Contains(format))
                ddspf.dwFlags = DDPF.FOURCC;
            if (format == 6)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 9)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 10)
                ddspf.dwFlags |= DDPF.RGB;
            else if (format == 16)
                ddspf.dwFlags |= DDPF.ALPHA;
            else if (format == 105)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;

            if (format == 0 || format == 1 || format == 24 || format == 25 || format == 108 || format == 109)
                ddspf.dwFourCC = "DXT1";
            else if (format == 3)
                ddspf.dwFourCC = "DXT3";
            else if (format == 5 || format == 23 || format == 33 || format == 110)
                ddspf.dwFourCC = "DXT5";
            else if (format == 103)
                ddspf.dwFourCC = "ATI1";
            else if (format == 104)
                ddspf.dwFourCC = "ATI2";
            else if (format == 22)
                ddspf.dwFourCC = "q\0\0\0"; // 0x71
            else if (format == 6 || format == 100 || format == 106 || format == 107 || format == 112 || format == 113)
                ddspf.dwFourCC = "DX10";

            if (format == 6)
            {
                ddspf.dwRGBBitCount = 16;
                ddspf.dwRBitMask = 0b01111100_00000000;
                ddspf.dwGBitMask = 0b00000011_11100000;
                ddspf.dwBBitMask = 0b00000000_00011111;
                ddspf.dwABitMask = 0b10000000_00000000;
            }
            else if (format == 9)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
                ddspf.dwABitMask = 0xFF000000;
            }
            else if (format == 10)
            {
                ddspf.dwRGBBitCount = 24;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
            }
            else if (format == 16)
            {
                ddspf.dwRGBBitCount = 8;
                ddspf.dwABitMask = 0x000000FF;
            }
            else if (format == 105)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x000000FF;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x00FF0000;
                ddspf.dwABitMask = 0xFF000000;
            }

            if (format == 6 || format == 100 || format == 102 || format == 106 || format == 107 || format == 112 || format == 113)
            {
                dds.header10 = new HEADER_DXT10();
                dds.header10.dxgiFormat = (DXGI_FORMAT)texture.Header.DXGIFormat;
                if (texture.Type == TPF.TexType.Cubemap)
                    dds.header10.miscFlag = RESOURCE_MISC.TEXTURECUBE;
            }

            return dds.Write(texture.Bytes);
        }
    }
}
