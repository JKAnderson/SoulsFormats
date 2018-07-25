using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoulsFormats
{
    public class BinaryReaderEx
    {
        private static readonly Encoding ASCII = Encoding.ASCII;
        private static readonly Encoding ShiftJIS = Encoding.GetEncoding("shift-jis");
        private static readonly Encoding UTF16 = Encoding.Unicode;

        private Stream stream;
        private BinaryReader br;

        public bool BigEndian = false;
        public long Position
        {
            get { return stream.Position; }
            set { stream.Position = value; }
        }

        public BinaryReaderEx(bool bigEndian, byte[] input) : this(bigEndian, new MemoryStream(input)) { }

        public BinaryReaderEx(bool bigEndian, Stream stream)
        {
            BigEndian = bigEndian;
            this.stream = stream;
            br = new BinaryReader(stream);
        }

        private byte[] ReadEndian(int length)
        {
            byte[] bytes = br.ReadBytes(length);
            if (BigEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public void Pad(int align)
        {
            if (stream.Position % align > 0)
                stream.Position += align - (stream.Position % align);
        }

        public void Skip(int count)
        {
            stream.Position += count;
        }

        public byte ReadByte()
        {
            return br.ReadByte();
        }

        public byte[] ReadBytes(int length)
        {
            return br.ReadBytes(length);
        }

        public byte GetByte(int offset)
        {
            long pos = stream.Position;
            stream.Position = offset;
            byte result = ReadByte();
            stream.Position = pos;
            return result;
        }

        public byte[] GetBytes(int offset, int length)
        {
            long pos = stream.Position;
            stream.Position = offset;
            byte[] result = ReadBytes(length);
            stream.Position = pos;
            return result;
        }

        public bool ReadBoolean()
        {
            return br.ReadBoolean();
        }

        public short ReadInt16()
        {
            return BitConverter.ToInt16(ReadEndian(2), 0);
        }

        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadEndian(2), 0);
        }

        public short GetInt16(int offset)
        {
            long position = stream.Position;
            stream.Position = offset;
            short result = ReadInt16();
            stream.Position = position;
            return result;
        }

        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadEndian(4), 0);
        }

        public int[] ReadInt32s(int count)
        {
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt32();
            return result;
        }

        public long ReadInt64()
        {
            return BitConverter.ToInt64(ReadEndian(8), 0);
        }

        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadEndian(4), 0);
        }

        public float[] ReadSingles(int count)
        {
            float[] result = new float[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSingle();
            return result;
        }

        public float[] GetSingles(int offset, int count)
        {
            long position = stream.Position;
            stream.Position = offset;
            float[] result = ReadSingles(count);
            stream.Position = position;
            return result;
        }

        private string readChars(Encoding encoding, int length)
        {
            byte[] bytes;
            if (length == 0)
            {
                List<byte> byteList = new List<byte>();
                byte b = ReadByte();
                while (b != 0)
                {
                    byteList.Add(b);
                    b = ReadByte();
                }
                bytes = byteList.ToArray();
            }
            else
            {
                bytes = ReadBytes(length);
            }
            return encoding.GetString(bytes);
        }

        public string ReadASCII(int length = 0)
        {
            return readChars(ASCII, length);
        }

        public string GetASCII(int offset)
        {
            long pos = stream.Position;
            stream.Position = offset;
            string result = ReadASCII();
            stream.Position = pos;
            return result;
        }

        public string ReadShiftJIS(int length = 0)
        {
            return readChars(ShiftJIS, length);
        }

        public string ReadShiftJISLengthPrefixed(byte delimiter)
        {
            int length = ReadInt32();
            string result = "";
            if (length > 0)
                result = readChars(ShiftJIS, length);
            AssertByte(delimiter);
            Pad(4);
            return result;
        }

        public string GetShiftJIS(int offset)
        {
            long pos = stream.Position;
            stream.Position = offset;
            string result = ReadShiftJIS();
            stream.Position = pos;
            return result;
        }

        public string ReadUTF16()
        {
            List<byte> bytes = new List<byte>();
            byte[] pair = ReadBytes(2);
            while (pair[0] != 0 || pair[1] != 0)
            {
                bytes.Add(pair[0]);
                bytes.Add(pair[1]);
                pair = ReadBytes(2);
            }
            return UTF16.GetString(bytes.ToArray());
        }

        public string GetUTF16(int offset)
        {
            long pos = stream.Position;
            stream.Position = offset;
            string result = ReadUTF16();
            stream.Position = pos;
            return result;
        }

        public byte AssertByte(params byte[] values)
        {
            byte b = ReadByte();
            bool valid = false;
            foreach (byte value in values)
                if (b == value)
                    valid = true;

            if (!valid)
            {
                StringBuilder sbValues = new StringBuilder();
                for (int i = 0; i < values.Length; i++)
                {
                    if (i != 0)
                        sbValues.Append(", ");
                    sbValues.Append("0x" + values[i].ToString("X"));
                }
                throw new InvalidDataException(string.Format(
                    "Read byte: 0x{0:X} | Expected byte: {1}", b, sbValues.ToString()));
            }

            return b;
        }

        public void AssertBytes(params byte[] values)
        {
            foreach (byte value in values)
            {
                byte b = ReadByte();
                if (b != value)
                {
                    throw new InvalidDataException(string.Format(
                        "Read byte: 0x{0:X} | Expected byte: 0x{1:X}", b, value));
                }
            }
        }

        public void AssertInt16(short value)
        {
            short s = ReadInt16();
            if (s != value)
            {
                throw new InvalidDataException(string.Format(
                    "Read short: 0x{0:X} | Expected short: 0x{1:X}", s, value));
            }
        }

        public int AssertInt32(params int[] values)
        {
            int i = ReadInt32();
            bool valid = false;
            foreach (int value in values)
                if (i == value)
                    valid = true;

            if (!valid)
            {
                StringBuilder sbValues = new StringBuilder();
                for (int index = 0; i < values.Length; i++)
                {
                    if (index != 0)
                        sbValues.Append(", ");
                    sbValues.Append("0x" + values[index].ToString("X"));
                }
                throw new InvalidDataException(string.Format(
                    "Read byte: 0x{0:X} | Expected byte: {1}", i, sbValues.ToString()));
            }

            return i;
        }

        public void AssertInt64(long value)
        {
            long l = ReadInt64();
            if (l != value)
            {
                throw new InvalidDataException(string.Format(
                    "Read long: 0x{0:X} | Expected long: 0x{1:X}", l, value));
            }
        }

        public void AssertASCII(string value)
        {
            string s = ReadASCII(value.Length);
            if (s != value)
            {
                throw new InvalidDataException(string.Format(
                    "Read string: {0} | Expected string: {1}", s, value));
            }
        }
    }
}
