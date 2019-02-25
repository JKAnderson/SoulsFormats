using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.DDS;

namespace SoulsFormats
{
    internal static class Headerizer
    {
        public static byte[] Headerize(TPF.Texture texture)
        {
            if (Encoding.ASCII.GetString(texture.Bytes, 0, 4) == "DDS ")
                return texture.Bytes;

            DDS dds = new DDS();
            PIXELFORMAT ddspf = dds.ddspf;

            byte format = texture.Format;
            if (format != 16 && format != 26)
            {
                DDSD dwFlags = DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT;
                if (format == 0 || format == 1 || format == 3 || format == 5)
                    dwFlags |= DDSD.LINEARSIZE;
                else if (format == 9 || format == 10)
                    dwFlags |= DDSD.PITCH;
                dds.dwFlags = dwFlags;

                dds.dwHeight = texture.Header.Height;
                dds.dwWidth = texture.Header.Width;

                if (format == 9 || format == 10)
                    dds.dwPitchOrLinearSize = (texture.Header.Width * 32 + 7) / 8;
                //else if (format == 0 || format == 1)
                //    pitch = Math.Max(1, (texture.Header.Width + 3) / 4) * (8 * 8 * 8);
                //else if (format == 5)
                //    pitch = Math.Max(1, (texture.Header.Width + 3) / 4) * (16 * 16 * 8);

                dds.dwDepth = 1;
                dds.dwMipMapCount = texture.Mipmaps;

                DDPF ddspfdwFlags = 0;
                if (format == 0 || format == 1 || format == 3 || format == 5)
                    ddspfdwFlags |= DDPF.FOURCC;
                else if (format == 9)
                    ddspfdwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
                else if (format == 10)
                    ddspfdwFlags |= DDPF.RGB;
                ddspf.dwFlags = ddspfdwFlags;

                if (format == 0 || format == 1)
                    ddspf.dwFourCC = "DXT1";
                else if (format == 3)
                    ddspf.dwFourCC = "DXT3";
                else if (format == 5)
                    ddspf.dwFourCC = "DXT5";
                else
                    ddspf.dwFourCC = "\0\0\0\0";

                if (format == 9 || format == 10)
                    ddspf.dwRGBBitCount = 32;

                if (format == 9)
                {
                    ddspf.dwRBitMask = 0x00FF0000;
                    ddspf.dwGBitMask = 0x0000FF00;
                    ddspf.dwBBitMask = 0x000000FF;
                    ddspf.dwABitMask = 0xFF000000;
                }
                else if (format == 10)
                {
                    ddspf.dwRBitMask = 0x000000FF;
                    ddspf.dwGBitMask = 0x0000FF00;
                    ddspf.dwBBitMask = 0x00FF0000;
                    ddspf.dwABitMask = 0x00000000;
                }

                DDSCAPS dwCaps = DDSCAPS.TEXTURE;
                if (texture.Cubemap)
                    dwCaps |= DDSCAPS.COMPLEX;
                if (texture.Mipmaps > 1)
                    dwCaps |= DDSCAPS.COMPLEX | DDSCAPS.MIPMAP;
                dds.dwCaps = dwCaps;

                if (texture.Cubemap)
                    dds.dwCaps2 = CUBEMAP_ALLFACES;
            }
            else
            {
                //int a = 0;
            }

            return dds.Write(texture.Bytes);
        }
    }
}
