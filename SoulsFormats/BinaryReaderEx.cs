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

        private BinaryReader br;
        private Stack<long> steps;

        public bool BigEndian;

        public Stream Stream { get; private set; }

        public long Position
        {
            get { return Stream.Position; }
            set { Stream.Position = value; }
        }

        public BinaryReaderEx(bool bigEndian, byte[] input) : this(bigEndian, new MemoryStream(input)) { }

        public BinaryReaderEx(bool bigEndian, Stream stream)
        {
            BigEndian = bigEndian;
            steps = new Stack<long>();
            Stream = stream;
            br = new BinaryReader(stream);
        }

        private byte[] ReadEndian(int length)
        {
            byte[] bytes = br.ReadBytes(length);
            if (BigEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        private T[] ReadValues<T>(Func<T> readValue, int count)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = readValue();
            return result;
        }

        private T GetValue<T>(Func<T> readValue, long offset)
        {
            StepIn(offset);
            T result = readValue();
            StepOut();
            return result;
        }

        private T[] GetValues<T>(Func<int, T[]> readValues, long offset, int count)
        {
            StepIn(offset);
            T[] result = readValues(count);
            StepOut();
            return result;
        }

        private T AssertValue<T>(Func<T> readValue, string typeName, string valueFormat, T[] options) where T : IEquatable<T>
        {
            T value = readValue();
            bool valid = false;
            foreach (T option in options)
                if (value.Equals(option))
                    valid = true;

            if (!valid)
            {
                string strValue = string.Format(valueFormat, value);

                List<string> strOptions = new List<string>();
                foreach (T option in options)
                    strOptions.Add(string.Format(valueFormat, option));

                throw new InvalidDataException(string.Format(
                    "Read {0}: {1} | Expected {0}: {2}", typeName, strValue, string.Join(", ", strOptions)));
            }

            return value;
        }

        public void StepIn(long offset)
        {
            steps.Push(Stream.Position);
            Stream.Position = offset;
        }

        public void StepOut()
        {
            if (steps.Count == 0)
                throw new InvalidOperationException("Reader is already stepped all the way out.");

            Stream.Position = steps.Pop();
        }

        public void Pad(int align)
        {
            if (Stream.Position % align > 0)
                Stream.Position += align - (Stream.Position % align);
        }

        public void Skip(int count)
        {
            Stream.Position += count;
        }

        #region Boolean
        public bool ReadBoolean()
        {
            return br.ReadBoolean();
        }

        public bool[] ReadBooleans(int count)
        {
            return ReadValues(ReadBoolean, count);
        }

        public bool GetBoolean(long offset)
        {
            return GetValue(ReadBoolean, offset);
        }

        public bool[] GetBooleans(long offset, int count)
        {
            return GetValues(ReadBooleans, offset, count);
        }

        public void AssertBoolean(bool option)
        {
            bool value = ReadBoolean();
            if (value != option)
            {
                throw new InvalidDataException(string.Format(
                    "Read Boolean: {0} | Expected Boolean: {1}", value, option));
            }
        }
        #endregion

        #region SByte
        public sbyte ReadSByte()
        {
            return br.ReadSByte();
        }

        public sbyte[] ReadSBytes(int count)
        {
            return ReadValues(ReadSByte, count);
        }

        public sbyte GetSByte(long offset)
        {
            return GetValue(ReadSByte, offset);
        }

        public sbyte[] GetSBytes(long offset, int count)
        {
            return GetValues(ReadSBytes, offset, count);
        }

        public sbyte AssertSByte(params sbyte[] options)
        {
            return AssertValue(ReadSByte, "SByte", "0x{0:X}", options);
        }
        #endregion

        #region Byte
        public byte ReadByte()
        {
            return br.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return br.ReadBytes(count);
        }

        public byte GetByte(long offset)
        {
            return GetValue(ReadByte, offset);
        }

        public byte[] GetBytes(long offset, int count)
        {
            StepIn(offset);
            byte[] result = ReadBytes(count);
            StepOut();
            return result;
        }

        public byte AssertByte(params byte[] options)
        {
            return AssertValue(ReadByte, "Byte", "0x{0:X}", options);
        }
        #endregion

        #region Int16
        public short ReadInt16()
        {
            return BitConverter.ToInt16(ReadEndian(2), 0);
        }

        public short[] ReadInt16s(int count)
        {
            return ReadValues(ReadInt16, count);
        }

        public short GetInt16(long offset)
        {
            return GetValue(ReadInt16, offset);
        }

        public short[] GetInt16s(long offset, int count)
        {
            return GetValues(ReadInt16s, offset, count);
        }

        public short AssertInt16(params short[] options)
        {
            return AssertValue(ReadInt16, "Int16", "0x{0:X}", options);
        }
        #endregion

        #region UInt16
        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadEndian(2), 0);
        }

        public ushort[] ReadUInt16s(int count)
        {
            return ReadValues(ReadUInt16, count);
        }

        public ushort GetUInt16(long offset)
        {
            return GetValue(ReadUInt16, offset);
        }

        public ushort[] GetUInt16s(long offset, int count)
        {
            return GetValues(ReadUInt16s, offset, count);
        }

        public ushort AssertUInt16(params ushort[] options)
        {
            return AssertValue(ReadUInt16, "UInt16", "0x{0:X}", options);
        }
        #endregion

        #region Int32
        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadEndian(4), 0);
        }

        public int[] ReadInt32s(int count)
        {
            return ReadValues(ReadInt32, count);
        }

        public int GetInt32(long offset)
        {
            return GetValue(ReadInt32, offset);
        }

        public int[] GetInt32s(long offset, int count)
        {
            return GetValues(ReadInt32s, offset, count);
        }

        public int AssertInt32(params int[] options)
        {
            return AssertValue(ReadInt32, "Int32", "0x{0:X}", options);
        }
        #endregion

        #region UInt32
        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadEndian(4), 0);
        }

        public uint[] ReadUInt32s(int count)
        {
            return ReadValues(ReadUInt32, count);
        }

        public uint GetUInt32(long offset)
        {
            return GetValue(ReadUInt32, offset);
        }

        public uint[] GetUInt32s(long offset, int count)
        {
            return GetValues(ReadUInt32s, offset, count);
        }

        public uint AssertUInt32(params uint[] options)
        {
            return AssertValue(ReadUInt32, "UInt32", "0x{0:X}", options);
        }
        #endregion

        #region Int64
        public long ReadInt64()
        {
            return BitConverter.ToInt64(ReadEndian(8), 0);
        }

        public long[] ReadInt64s(int count)
        {
            return ReadValues(ReadInt64, count);
        }

        public long GetInt64(long offset)
        {
            return GetValue(ReadInt64, offset);
        }

        public long[] GetInt64s(long offset, int count)
        {
            return GetValues(ReadInt64s, offset, count);
        }

        public long AssertInt64(params long[] options)
        {
            return AssertValue(ReadInt64, "Int64", "0x{0:X}", options);
        }
        #endregion

        #region UInt64
        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(ReadEndian(8), 0);
        }

        public ulong[] ReadUInt64s(int count)
        {
            return ReadValues(ReadUInt64, count);
        }

        public ulong GetUInt64(long offset)
        {
            return GetValue(ReadUInt64, offset);
        }

        public ulong[] GetUInt64s(long offset, int count)
        {
            return GetValues(ReadUInt64s, offset, count);
        }

        public ulong AssertUInt64(params ulong[] options)
        {
            return AssertValue(ReadUInt64, "UInt64", "0x{0:X}", options);
        }
        #endregion

        #region Single
        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadEndian(4), 0);
        }

        public float[] ReadSingles(int count)
        {
            return ReadValues(ReadSingle, count);
        }

        public float GetSingle(long offset)
        {
            return GetValue(ReadSingle, offset);
        }

        public float[] GetSingles(long offset, int count)
        {
            return GetValues(ReadSingles, offset, count);
        }

        public float AssertSingle(params float[] options)
        {
            return AssertValue(ReadSingle, "Single", "0x{0:X}", options);
        }
        #endregion

        #region Double
        public double ReadDouble()
        {
            return BitConverter.ToDouble(ReadEndian(8), 0);
        }

        public double[] ReadDoubles(int count)
        {
            return ReadValues(ReadDouble, count);
        }

        public double GetDouble(long offset)
        {
            return GetValue(ReadDouble, offset);
        }

        public double[] GetDoubles(long offset, int count)
        {
            return GetValues(ReadDoubles, offset, count);
        }

        public double AssertDouble(params double[] options)
        {
            return AssertValue(ReadDouble, "Double", "0x{0:X}", options);
        }
        #endregion

        #region String
        private string ReadChars(Encoding encoding, int length)
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
            return ReadChars(ASCII, length);
        }

        public string GetASCII(long offset, int length = 0)
        {
            StepIn(offset);
            string result = ReadASCII(length);
            StepOut();
            return result;
        }

        public void AssertASCII(string value)
        {
            string s = ReadASCII(value.Length);
            if (s != value)
            {
                throw new InvalidDataException(string.Format(
                    "Read ASCII: {0} | Expected ASCII: {1}", s, value));
            }
        }

        public string ReadShiftJIS(int length = 0)
        {
            return ReadChars(ShiftJIS, length);
        }

        public string ReadShiftJISLengthPrefixed(byte delimiter)
        {
            int length = ReadInt32();
            string result = "";
            if (length > 0)
                result = ReadChars(ShiftJIS, length);
            AssertByte(delimiter);
            Pad(4);
            return result;
        }

        public string GetShiftJIS(long offset)
        {
            StepIn(offset);
            string result = ReadShiftJIS();
            StepOut();
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

        public string GetUTF16(long offset)
        {
            StepIn(offset);
            string result = ReadUTF16();
            StepOut();
            return result;
        }
        #endregion
    }
}
