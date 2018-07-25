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

        private Stream stream;
        private BinaryWriter bw;
        private Dictionary<string, long> reservations;

        public bool BigEndian = false;
        public long Position
        {
            get { return stream.Position; }
            set { stream.Position = value; }
        }

        public BinaryWriterEx(bool bigEndian) : this(bigEndian, new MemoryStream()) { }

        public BinaryWriterEx(bool bigEndian, Stream stream)
        {
            this.stream = stream;
            bw = new BinaryWriter(stream);
            reservations = new Dictionary<string, long>();
            BigEndian = bigEndian;
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
            MemoryStream ms = (MemoryStream)stream;
            byte[] result = ms.ToArray();
            Finish();
            return result;
        }

        public void Pad(int align)
        {
            while (stream.Position % align > 0)
                WriteByte(0);
        }

        private void WriteEndian(byte[] bytes)
        {
            if (BigEndian)
                Array.Reverse(bytes);
            bw.Write(bytes);
        }

        public void WriteByte(byte value)
        {
            bw.Write(value);
        }

        public void WriteBytes(byte[] bytes)
        {
            bw.Write(bytes);
        }

        public void WriteBoolean(bool value)
        {
            bw.Write(value);
        }

        public void WriteInt16(short value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteInt32(int value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteInt32s(int[] values)
        {
            foreach (int value in values)
                WriteInt32(value);
        }

        public void ReserveInt32(string name)
        {
            if (reservations.ContainsKey(name))
                throw new ArgumentException("Key already reserved: " + name);

            reservations[name] = stream.Position;
            WriteInt32(0);
        }

        public void FillInt32(string name, int value)
        {
            if (!reservations.ContainsKey(name))
                throw new ArgumentException("Key was not reserved: " + name);

            long pos = stream.Position;
            stream.Position = reservations[name];
            WriteInt32(value);
            stream.Position = pos;
        }

        public void WriteInt64(long value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void ReserveInt64(string name)
        {
            if (reservations.ContainsKey(name))
                throw new ArgumentException("Key already reserved: " + name);

            reservations[name] = stream.Position;
            WriteInt64(0);
        }

        public void FillInt64(string name, long value)
        {
            if (!reservations.ContainsKey(name))
                throw new ArgumentException("Key was not reserved: " + name);

            long pos = stream.Position;
            stream.Position = reservations[name];
            WriteInt64(value);
            stream.Position = pos;
        }

        public void WriteSingle(float value)
        {
            WriteEndian(BitConverter.GetBytes(value));
        }

        public void WriteSingles(float[] values)
        {
            foreach (float value in values)
                WriteSingle(value);
        }

        private void WriteChars(string text, Encoding encoding, bool terminate)
        {
            byte[] bytes = encoding.GetBytes(text);
            bw.Write(bytes);
            if (terminate)
                bw.Write((byte)0);
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
            byte[] bytes = UTF16.GetBytes(text);
            bw.Write(bytes);
            if (terminate)
            {
                WriteBytes(new byte[] { 0, 0 });
            }
        }
    }
}
