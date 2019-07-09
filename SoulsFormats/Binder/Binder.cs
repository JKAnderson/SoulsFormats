using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// Common format information for BND3, BXF3, BND4, and BXF4.
    /// </summary>
    public static class Binder
    {
        /// <summary>
        /// All known format bytes for BND3, BXF3, BND4, or BXF4.
        /// </summary>
        public enum Format : byte
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            x00 = 0x00,
            x06 = 0x06,
            x0A = 0x0A,
            x0C = 0x0C,
            x0E = 0x0E,
            x1C = 0x1C,
            x20 = 0x20,
            x22 = 0x22,
            x24 = 0x24,
            x26 = 0x26,
            x2A = 0x2A,
            x2C = 0x2C,
            x2E = 0x2E,
            x30 = 0x30,
            x3E = 0x3E,
            x40 = 0x40,
            x54 = 0x54,
            x60 = 0x60,
            x64 = 0x64,
            x67 = 0x67,
            x70 = 0x70,
            x74 = 0x74,
            x80 = 0x80,
            x91 = 0x91,
            xA0 = 0xA0,
            xE0 = 0xE0,
            xE4 = 0xE4,
            xF0 = 0xF0,
            xF4 = 0xF4,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
        
        private static HashSet<byte> forceBigEndian = new HashSet<byte>
        {
            0x67, 0x80, 0x91, 0xA0, 0xE0, 0xE4, 0xF0, 0xF4
        };

        /// <summary>
        /// Whether the file is big endian regardless of the big endian indicator.
        /// </summary>
        public static bool ForceBigEndian(Format format)
        {
            return forceBigEndian.Contains((byte)format);
        }
        
        private static HashSet<byte> hasID = new HashSet<byte>
        {
            0x06, 0x0A, 0x0E, 0x22, 0x26, 0x2A, 0x2E, 0x3E, 0x40, 0x54, 0x60, 0x64, 0x67, 0x70, 0x74, 0xE0, 0xE4, 0xF0, 0xF4
        };

        /// <summary>
        /// Whether files have an ID number.
        /// </summary>
        public static bool HasID(Format format)
        {
            return hasID.Contains((byte)format);
        }

        private static HashSet<byte> hasName = new HashSet<byte>
        {
            0x06, 0x0A, 0x0C, 0x0E, 0x1C, 0x20, 0x24, 0x26, 0x2A, 0x2C, 0x2E, 0x30, 0x3E, 0x54, 0x60, 0x64, 0x67, 0x70, 0x74, 0x91, 0xA0, 0xE0, 0xE4, 0xF0, 0xF4
        };

        /// <summary>
        /// Whether files have a name string.
        /// </summary>
        public static bool HasName(Format format)
        {
            return hasName.Contains((byte)format);
        }

        private static HashSet<byte> hasUncompressedSize = new HashSet<byte>
        {
            0x22, 0x24, 0x26, 0x2A, 0x2C, 0x2E, 0x3E, 0x54, 0x64, 0x67, 0x74, 0xE4, 0xF4
        };

        /// <summary>
        /// Whether files include their uncompressed size.
        /// </summary>
        public static bool HasUncompressedSize(Format format)
        {
            return hasUncompressedSize.Contains((byte)format);
        }

        private static HashSet<byte> hasLongOffsets = new HashSet<byte>
        {
            0x1C, 0x3E
        };

        /// <summary>
        /// Whether file headers have a 64-bit offset instead of a 32-bit one.
        /// </summary>
        public static bool HasLongOffsets(Format format)
        {
            return hasLongOffsets.Contains((byte)format);
        }

        /// <summary>
        /// Size of each file header for BND4/BXF4.
        /// </summary>
        public static long FileHeaderSize(Format format)
        {
            return 0x10
                + (HasUncompressedSize(format) ? 8 : 0)
                + (HasLongOffsets(format) ? 8 : 4)
                + (HasID(format) ? 4 : 0)
                + (HasName(format) ? 4 : 0)
                + (format == Format.x20 ? 8 : 0);
        }

        /// <summary>
        /// All known file flag values.
        /// </summary>
        public enum FileFlags : byte
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            x00 = 0x00,
            x02 = 0x02,
            x03 = 0x03,
            x0A = 0x0A,
            x40 = 0x40,
            x50 = 0x50,
            xC0 = 0xC0,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        private static HashSet<byte> isCompressed = new HashSet<byte>
        {
            0x03, 0xC0
        };

        /// <summary>
        /// Whether a file uses integrated compression.
        /// </summary>
        public static bool IsCompressed(FileFlags flags)
        {
            return isCompressed.Contains((byte)flags);
        }
    }
}
