using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoulsFormats
{
    public class BinaryWriterEx
    {
        private static readonly Encoding ASCII = Encoding.ASCII;
        private static readonly Encoding ShiftJIS = Encoding.GetEncoding("shift-jis");
        private static readonly Encoding UTF16 = Encoding.Unicode;

        private BinaryWriter bw;
        private Stack<long> steps;
        private Dictionary<string, long> reservations;

        public bool BigEndian;

        public Stream Stream { get; private set; }

        public long Position
        {
            get { return Stream.Position; }
            set { Stream.Position = value; }
        }

        public BinaryWriterEx(bool bigEndian) : this(bigEndian, new MemoryStream()) { }

        public BinaryWriterEx(bool bigEndian, Stream stream)
        {
            BigEndian = bigEndian;
            steps = new Stack<long>();
            reservations = new Dictionary<string, long>();
            Stream = stream;
            bw = new BinaryWriter(stream);
        }

        private void WriteEndian(byte[] bytes)
        {
            if (BigEndian)
                Array.Reverse(bytes);
            bw.Write(bytes);
        }

        private void Reserve(string name, string typeName, int bytes)
        {
            name += ":" + typeName;
            if (reservations.ContainsKey(name))
                throw new ArgumentException("Key already reserved: " + name);

            reservations[name] = Stream.Position;
            for (int i = 0; i < bytes; i++)
                WriteByte(0xFE);
        }

        private void Fill<T>(Action<T> writeValue, string name, string typeName, T value)
        {
            name += ":" + typeName;
            if (!reservations.ContainsKey(name))
                throw new ArgumentException("Key is not reserved: " + name);

            StepIn(reservations[name]);
            writeValue(value);
            StepOut();
            reservations.Remove(name);
        }

        public void Finish()
        {
            if (reservations.Count > 0)
            {
                throw new InvalidOperationException("Not all reservations filled: " + string.Join(", ", reservations.Keys));
            }
            bw.Close();
        }

        public byte[] FinishBytes()
        {
            MemoryStream ms = (MemoryStream)Stream;
            byte[] result = ms.ToArray();
            Finish();
            return result;
        }

        public void StepIn(long offset)
        {
            steps.Push(Stream.Position);
            Stream.Position = offset;
        }

        public void StepOut()
        {
            if (steps.Count == 0)
                throw new InvalidOperationException("Writer is already stepped all the way out.");

            Stream.Position = steps.Pop();
        }

        public void Pad(int align)
        {
            while (Stream.Position % align > 0)
                WriteByte(0);
        }

        #region Boolean
        public void WriteBoolean(bool value)
        {
            bw.Write(value);
        }

        public void WriteBooleans(IList<bool> values)
        {
            foreach (bool value in values)
                WriteBoolean(value);
        }

        public void ReserveBoolean(string name)
        {
            Reserve(name, "Boolean", 1);
        }

        public void FillBoolean(string name, bool value)
        {
            Fill(WriteBoolean, name, "Boolean", value);
        }
        #endregion

        #region SByte
        public void WriteSByte(sbyte value)
        {
            bw.Write(value);
        }

        public void WriteSBytes(IList<sbyte> values)
        {
            foreach (sbyte value in values)
                WriteSByte(value);
        }

        public void ReserveSByte(string name)
        {
            Reserve(name, "SByte", 1);
        }

        public void FillSByte(string name, sbyte value)
        {
            Fill(WriteSByte, name, "SByte", value);
        }
        #endregion

        #region Byte
        public void WriteByte(byte value)
        {
            bw.Write(value);
        }

        public void WriteBytes(byte[] bytes)
        {
            bw.Write(bytes);
        }

        public void WriteBytes(IList<byte> values)
        {
            foreach (byte value in values)
                WriteByte(value);
        }

        public void ReserveByte(string name)
        {
            Reserve(name, "Byte", 1);
        }

        public void FillByte(string name, byte value)
        {
            Fill(WriteByte, name, "Byte", value);
        }
        #endregion

        #region Int16
        public void WriteInt16(short value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteInt16s(IList<short> values)
        {
            foreach (short value in values)
                WriteInt16(value);
        }

        public void ReserveInt16(string name)
        {
            Reserve(name, "Int16", 2);
        }

        public void FillInt16(string name, short value)
        {
            Fill(WriteInt16, name, "Int16", value);
        }
        #endregion

        #region UInt16
        public void WriteUInt16(ushort value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteUInt16s(IList<ushort> values)
        {
            foreach (ushort value in values)
                WriteUInt16(value);
        }

        public void ReserveUInt16(string name)
        {
            Reserve(name, "UInt16", 2);
        }

        public void FillUInt16(string name, ushort value)
        {
            Fill(WriteUInt16, name, "UInt16", value);
        }
        #endregion

        #region Int32
        public void WriteInt32(int value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteInt32s(IList<int> values)
        {
            foreach (int value in values)
                WriteInt32(value);
        }

        public void ReserveInt32(string name)
        {
            Reserve(name, "Int32", 4);
        }

        public void FillInt32(string name, int value)
        {
            Fill(WriteInt32, name, "Int32", value);
        }
        #endregion

        #region UInt32
        public void WriteUInt32(uint value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteUInt32s(IList<uint> values)
        {
            foreach (uint value in values)
                WriteUInt32(value);
        }

        public void ReserveUInt32(string name)
        {
            Reserve(name, "UInt32", 4);
        }

        public void FillUInt32(string name, uint value)
        {
            Fill(WriteUInt32, name, "UInt32", value);
        }
        #endregion

        #region Int64
        public void WriteInt64(long value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteInt64s(IList<long> values)
        {
            foreach (long value in values)
                WriteInt64(value);
        }

        public void ReserveInt64(string name)
        {
            Reserve(name, "Int64", 8);
        }

        public void FillInt64(string name, long value)
        {
            Fill(WriteInt64, name, "Int64", value);
        }
        #endregion

        #region UInt64
        public void WriteUInt64(ulong value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteUInt64s(IList<ulong> values)
        {
            foreach (ulong value in values)
                WriteUInt64(value);
        }

        public void ReserveUInt64(string name)
        {
            Reserve(name, "UInt64", 8);
        }

        public void FillUInt64(string name, ulong value)
        {
            Fill(WriteUInt64, name, "UInt64", value);
        }
        #endregion

        #region Single
        public void WriteSingle(float value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteSingles(IList<float> values)
        {
            foreach (float value in values)
                WriteSingle(value);
        }

        public void ReserveSingle(string name)
        {
            Reserve(name, "Single", 4);
        }

        public void FillSingle(string name, float value)
        {
            Fill(WriteSingle, name, "Single", value);
        }
        #endregion

        #region Double
        public void WriteDouble(double value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteDoubles(IList<double> values)
        {
            foreach (double value in values)
                WriteDouble(value);
        }

        public void ReserveDouble(string name)
        {
            Reserve(name, "Double", 8);
        }

        public void FillDouble(string name, double value)
        {
            Fill(WriteDouble, name, "Double", value);
        }
        #endregion

        #region String
        private void WriteChars(string text, Encoding encoding, bool terminate)
        {
            if (terminate)
                text += '\0';
            byte[] bytes = encoding.GetBytes(text);
            bw.Write(bytes);
        }

        public void WriteASCII(string text, bool terminate = false)
        {
            WriteChars(text, ASCII, terminate);
        }

        public void WriteShiftJIS(string text, bool terminate = false)
        {
            WriteChars(text, ShiftJIS, terminate);
        }

        public void WriteShiftJISLengthPrefixed(string text, byte delimiter)
        {
            byte[] bytes = ShiftJIS.GetBytes(text);
            WriteInt32(bytes.Length);
            if (bytes.Length > 0)
                WriteBytes(bytes);
            WriteByte(delimiter);
            Pad(4);
        }

        public void WriteUTF16(string text, bool terminate = false)
        {
            WriteChars(text, UTF16, terminate);
        }
        #endregion
    }
}
