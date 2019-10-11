using System;
using System.Runtime.InteropServices;

namespace SoulsFormats
{
    internal static class Oodle26
    {
        [DllImport("oo2core_6_win64.dll")]
        private static extern uint OodleLZ_GetCompressedBufferSizeNeeded(ulong src_len);

        [DllImport("oo2core_6_win64.dll")]
        private static extern uint OodleLZ_Compress(Codec codec, byte[] src_buf, ulong src_len, byte[] dst_buf, Level level,
            IntPtr opts, ulong offs, ulong unused, IntPtr scratch, ulong scratch_size);

        [DllImport("oo2core_6_win64.dll")]
        private static extern uint OodleLZ_Decompress(byte[] src_buf, ulong src_len, byte[] dst_buf, ulong dst_size,
            int fuzz, int crc, int verbose, IntPtr dst_base, ulong e, IntPtr cb, IntPtr cb_ctx, IntPtr scratch, ulong scratch_size, int threadPhase);

        public static byte[] Compress(byte[] source, Codec codec, Level level)
        {
            uint compressionBound = OodleLZ_GetCompressedBufferSizeNeeded((ulong)source.LongLength);
            byte[] dest = new byte[compressionBound];
            uint destLength = OodleLZ_Compress(codec, source, (ulong)source.LongLength, dest, level, IntPtr.Zero, 0, 0, IntPtr.Zero, 0);
            Array.Resize(ref dest, (int)destLength);
            return dest;
        }

        public static byte[] Decompress(byte[] source, ulong uncompressedSize)
        {
            byte[] dest = new byte[uncompressedSize];
            OodleLZ_Decompress(source, (ulong)source.LongLength, dest, uncompressedSize, 0, 0, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, 0);
            return dest;
        }

        public enum Codec : int
        {
            LZH,
            LZHLW,
            LZNIB,
            None,
            LZB16,
            LZBLW,
            LZA,
            LZNA,
            Kraken,
            Mermaid,
            BitKnit,
            Selkie,
            Akkorokamui
        }

        public enum Level : int
        {
            None,
            SuperFast,
            VeryFast,
            Fast,
            Normal,
            Optimal1,
            Optimal2,
            Optimal3,
            Optimal4,
            Optimal5
        }
    }
}
