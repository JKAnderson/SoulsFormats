using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SoulsFormats
{
    /// <summary>
    /// The header file of the dvdbnd container format used to package all game files with hashed filenames.
    /// </summary>
    public class BHD5
    {
        /// <summary>
        /// A salt used to calculate SHA hashes for file data.
        /// </summary>
        public string Salt { get; }

        /// <summary>
        /// Collections of files grouped by their hash value for faster lookup.
        /// </summary>
        public List<Bucket> Buckets { get; }

        /// <summary>
        /// Read a dvdbnd header from the given stream, formatted for the given game. Must already be decrypted, if applicable.
        /// </summary>
        public static BHD5 Read(Stream bhdStream, Game game)
        {
            var br = new BinaryReaderEx(false, bhdStream);
            return new BHD5(br, game);
        }

        private BHD5(BinaryReaderEx br, Game game)
        {
            br.AssertASCII("BHD5");
            br.BigEndian = br.AssertSByte(0, -1) == 0;
            br.AssertByte(0, 1);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertInt32(1);
            int fileSize = br.ReadInt32();
            int bucketCount = br.ReadInt32();
            int bucketsOffset = br.ReadInt32();

            Salt = null;
            if (game == Game.DarkSouls2 || game == Game.DarkSouls3 || game == Game.Sekiro)
            {
                int saltLength = br.ReadInt32();
                Salt = br.ReadASCII(saltLength);
            }

            Buckets = new List<Bucket>(bucketCount);
            for (int i = 0; i < bucketCount; i++)
                Buckets.Add(new Bucket(br, game));
        }

        /// <summary>
        /// Indicates the format of a dvdbnd.
        /// </summary>
        public enum Game
        {
            /// <summary>
            /// Dark Souls 1, both PC and console versions.
            /// </summary>
            DarkSouls1,

            /// <summary>
            /// Dark Souls 2 and Scholar of the First Sin on PC.
            /// </summary>
            DarkSouls2,

            /// <summary>
            /// Dark Souls 3 on PC.
            /// </summary>
            DarkSouls3,

            /// <summary>
            /// Sekiro on PC.
            /// </summary>
            Sekiro,
        }

        /// <summary>
        /// A collection of files grouped by their hash.
        /// </summary>
        public class Bucket : List<FileHeader>
        {
            internal Bucket(BinaryReaderEx br, Game game) : base()
            {
                int fileHeaderCount = br.ReadInt32();
                int fileHeadersOffset = br.ReadInt32();

                br.StepIn(fileHeadersOffset);
                {
                    for (int i = 0; i < fileHeaderCount; i++)
                        Add(new FileHeader(br, game));
                }
                br.StepOut();
            }
        }

        /// <summary>
        /// Information about an individual file in the dvdbnd.
        /// </summary>
        public class FileHeader
        {
            /// <summary>
            /// Hash of the full file path using From's algorithm found in SFUtil.FromPathHash.
            /// </summary>
            public uint FileNameHash { get; }

            /// <summary>
            /// Full size of the file data in the BDT.
            /// </summary>
            public int PaddedFileSize { get; }

            /// <summary>
            /// File size after decryption; only included in DS3.
            /// </summary>
            public long UnpaddedFileSize { get; }

            /// <summary>
            /// Beginning of file data in the BDT.
            /// </summary>
            public long FileOffset { get; }

            /// <summary>
            /// Hashing information for this file.
            /// </summary>
            public SHAHash SHAHash { get; }

            /// <summary>
            /// Encryption information for this file.
            /// </summary>
            public AESKey AESKey { get; }

            internal FileHeader(BinaryReaderEx br, Game game)
            {
                FileNameHash = br.ReadUInt32();
                PaddedFileSize = br.ReadInt32();
                FileOffset = br.ReadInt64();

                SHAHash = null;
                AESKey = null;
                if (game == Game.DarkSouls2 || game == Game.DarkSouls3 || game == Game.Sekiro)
                {
                    long shaHashOffset = br.ReadInt64();
                    long aesKeyOffset = br.ReadInt64();

                    if (shaHashOffset != 0)
                    {
                        br.StepIn(shaHashOffset);
                        {
                            SHAHash = new SHAHash(br);
                        }
                        br.StepOut();
                    }

                    if (aesKeyOffset != 0)
                    {
                        br.StepIn(aesKeyOffset);
                        {
                            AESKey = new AESKey(br);
                        }
                        br.StepOut();
                    }
                }

                UnpaddedFileSize = -1;
                if (game == Game.DarkSouls3 || game == Game.Sekiro)
                {
                    UnpaddedFileSize = br.ReadInt64();
                }
            }

            /// <summary>
            /// Read and decrypt (if necessary) file data from the BDT.
            /// </summary>
            public byte[] ReadFile(FileStream bdtStream)
            {
                byte[] bytes = new byte[PaddedFileSize];
                bdtStream.Position = FileOffset;
                bdtStream.Read(bytes, 0, PaddedFileSize);
                AESKey?.Decrypt(bytes);
                return bytes;
            }
        }

        /// <summary>
        /// Hash information for a file in the dvdbnd.
        /// </summary>
        public class SHAHash
        {
            /// <summary>
            /// 32-byte salted SHA hash.
            /// </summary>
            public byte[] Hash { get; }

            /// <summary>
            /// Hashed sections of the file.
            /// </summary>
            public List<Range> Ranges { get; }

            internal SHAHash(BinaryReaderEx br)
            {
                Hash = br.ReadBytes(32);
                int rangeCount = br.ReadInt32();
                Ranges = new List<Range>(rangeCount);
                for (int i = 0; i < rangeCount; i++)
                    Ranges.Add(new Range(br));
            }
        }

        /// <summary>
        /// Encryption information for a file in the dvdbnd.
        /// </summary>
        public class AESKey
        {
            private static AesManaged AES = new AesManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128 };

            /// <summary>
            /// 16-byte encryption key.
            /// </summary>
            public byte[] Key { get; }

            /// <summary>
            /// Encrypted sections of the file.
            /// </summary>
            public List<Range> Ranges { get; }

            internal AESKey(BinaryReaderEx br)
            {
                Key = br.ReadBytes(16);
                int rangeCount = br.ReadInt32();
                Ranges = new List<Range>(rangeCount);
                for (int i = 0; i < rangeCount; i++)
                    Ranges.Add(new Range(br));
            }

            /// <summary>
            /// Decrypt file data in-place.
            /// </summary>
            public void Decrypt(byte[] bytes)
            {
                using (ICryptoTransform decryptor = AES.CreateDecryptor(Key, new byte[16]))
                {
                    foreach (Range range in Ranges.Where(r => r.StartOffset != -1 && r.EndOffset != -1 && r.StartOffset != r.EndOffset))
                    {
                        int start = (int)range.StartOffset;
                        int count = (int)(range.EndOffset - range.StartOffset);
                        decryptor.TransformBlock(bytes, start, count, bytes, start);
                    }
                }
            }
        }

        /// <summary>
        /// Indicates a hashed or encrypted section of a file.
        /// </summary>
        public struct Range
        {
            /// <summary>
            /// The beginning of the range, inclusive.
            /// </summary>
            public long StartOffset;

            /// <summary>
            /// The end of the range, exclusive.
            /// </summary>
            public long EndOffset;

            internal Range(BinaryReaderEx br)
            {
                StartOffset = br.ReadInt64();
                EndOffset = br.ReadInt64();
            }
        }
    }
}
