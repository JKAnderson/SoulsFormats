using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A material definition format used in DS1, DSR, DS2, DS3, DeS, BB, and NB.
    /// </summary>
    public class MTD
    {
        #region Public Read
        /// <summary>
        /// Reads an MTD from an array of bytes.
        /// </summary>
        public static MTD Read(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return new MTD(br);
        }

        /// <summary>
        /// Reads an MTD from the specified path using file streams.
        /// </summary>
        public static MTD Read(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return new MTD(br);
            }
        }
        #endregion

        /// <summary>
        /// Unknown
        /// </summary>
        public string SpxPath;

        /// <summary>
        /// A description of this material's purpose.
        /// </summary>
        public string Description;

        /// <summary>
        /// Values for this material stored in this MTD.
        /// </summary>
        public List<InternalEntry> Internal;

        /// <summary>
        /// Values for this material stored somewhere else.
        /// </summary>
        public List<ExternalEntry> External;

        private int unk1, unk2, unk3, unk4, unk5, unk6, unk7, unk8, unk9, unk10, unk11, unk12;

        private MTD(BinaryReaderEx br)
        {
            br.AssertInt32(0);
            int fileSize = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(3);
            unk1 = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0x1C);
            br.AssertInt32(1);
            br.AssertInt32(2);
            unk2 = br.ReadInt32();
            br.AssertInt32(4);
            br.AssertASCII("MTD ");
            unk3 = br.ReadInt32();
            br.AssertInt32(0x3E8);
            unk4 = br.ReadInt32();
            br.AssertInt32(0);
            int dataSize = br.ReadInt32();
            br.AssertInt32(2);
            br.AssertInt32(4);
            unk5 = br.ReadInt32();
            SpxPath = br.ReadShiftJISLengthPrefixed(0xA3);
            Description = br.ReadShiftJISLengthPrefixed(0x03);
            br.AssertInt32(1);
            br.AssertInt32(0);
            unk6 = br.ReadInt32();
            br.AssertInt32(3);
            br.AssertInt32(4);
            unk7 = br.ReadInt32();
            br.AssertInt32(0);
            unk8 = br.ReadInt32();

            Internal = new List<InternalEntry>();
            int internalEntryCount = br.ReadInt32();
            for (int i = 0; i < internalEntryCount; i++)
                Internal.Add(new InternalEntry(br));

            unk9 = br.ReadInt32();

            External = new List<ExternalEntry>();
            int externalEntryCount = br.ReadInt32();
            for (int i = 0; i < externalEntryCount; i++)
                External.Add(new ExternalEntry(br));

            unk10 = br.ReadInt32();
            br.AssertInt32(0);
            unk11 = br.ReadInt32();
            br.AssertInt32(0);
            unk12 = br.ReadInt32();
            br.AssertInt32(0);
        }

        #region Public Write
        /// <summary>
        /// Writes a TPF file to an array of bytes.
        /// </summary>
        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Writes a TPF file to the specified path using file streams.
        /// </summary>
        public void Write(string path)
        {
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, stream);
                Write(bw);
                bw.Finish();
            }
        }
        #endregion

        private void Write(BinaryWriterEx bw)
        {
            bw.WriteInt32(0);
            bw.ReserveInt32("FileSize");
            bw.WriteInt32(0);
            bw.WriteInt32(3);
            bw.WriteInt32(unk1);
            bw.WriteInt32(0);
            bw.WriteInt32(0x1C);
            bw.WriteInt32(1);
            bw.WriteInt32(2);
            bw.WriteInt32(unk2);
            bw.WriteInt32(4);
            bw.WriteASCII("MTD ");
            bw.WriteInt32(unk3);
            bw.WriteInt32(0x3E8);
            bw.WriteInt32(unk4);
            bw.WriteInt32(0);
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(2);
            bw.WriteInt32(4);
            bw.WriteInt32(unk5);
            bw.WriteShiftJISLengthPrefixed(SpxPath, 0xA3);
            bw.WriteShiftJISLengthPrefixed(Description, 0x03);
            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.WriteInt32(unk6);
            bw.WriteInt32(3);
            bw.WriteInt32(4);
            bw.WriteInt32(unk7);
            bw.WriteInt32(0);
            bw.WriteInt32(unk8);

            bw.WriteInt32(Internal.Count);
            foreach (InternalEntry internalEntry in Internal)
                internalEntry.Write(bw);

            bw.WriteInt32(unk9);

            bw.WriteInt32(External.Count);
            foreach (ExternalEntry externalEntry in External)
                externalEntry.Write(bw);

            bw.WriteInt32(unk10);
            bw.WriteInt32(0);
            bw.WriteInt32(unk11);
            bw.WriteInt32(0);
            bw.WriteInt32(unk12);
            bw.WriteInt32(0);

            int position = (int)bw.Position;
            bw.FillInt32("FileSize", position - 8);
            bw.FillInt32("DataSize", position - 0x4C);
        }

        /// <summary>
        /// A material value stored within its MTD.
        /// </summary>
        public class InternalEntry
        {
            /// <summary>
            /// The name of this value.
            /// </summary>
            public string Name;

            /// <summary>
            /// The type of this value.
            /// </summary>
            public InternalType Type;

            /// <summary>
            /// The value itself.
            /// </summary>
            public object Value;

            private int unk2, unk5, unk6, unk7, unk8, unk9;

            internal InternalEntry(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                unk2 = br.ReadInt32();
                br.AssertInt32(4);
                br.AssertInt32(4);
                unk5 = br.ReadInt32();
                Name = br.ReadShiftJISLengthPrefixed(0xA3);
                string type = br.ReadShiftJISLengthPrefixed(0x04);
                type = char.ToUpper(type[0]) + type.Substring(1);
                Type = (InternalType)Enum.Parse(typeof(InternalType), type);
                br.AssertInt32(1);
                br.AssertInt32(0);
                unk6 = br.ReadInt32();
                unk7 = br.ReadInt32();
                br.AssertInt32(1);
                unk8 = br.ReadInt32();
                unk9 = br.ReadInt32();

                if (Type == InternalType.Int)
                    Value = br.ReadInt32();
                else if (Type == InternalType.Int2)
                    Value = br.ReadInt32s(2);
                else if (Type == InternalType.Bool)
                    Value = br.ReadBoolean();
                else if (Type == InternalType.Float)
                    Value = br.ReadSingle();
                else if (Type == InternalType.Float2)
                    Value = br.ReadSingles(2);
                else if (Type == InternalType.Float3)
                    Value = br.ReadSingles(3);
                else if (Type == InternalType.Float4)
                    Value = br.ReadSingles(4);

                br.AssertByte(4);
                br.Pad(4);
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(unk2);
                bw.WriteInt32(4);
                bw.WriteInt32(4);
                bw.WriteInt32(unk5);
                bw.WriteShiftJISLengthPrefixed(Name, 0xA3);
                bw.WriteShiftJISLengthPrefixed(Type.ToString().ToLower(), 0x04);
                bw.WriteInt32(1);
                bw.WriteInt32(0);
                bw.WriteInt32(unk6);
                bw.WriteInt32(unk7);
                bw.WriteInt32(1);
                bw.WriteInt32(unk8);
                bw.WriteInt32(unk9);

                if (Type == InternalType.Int)
                    bw.WriteInt32((int)Value);
                else if (Type == InternalType.Int2)
                    bw.WriteInt32s((int[])Value);
                else if (Type == InternalType.Bool)
                    bw.WriteBoolean((bool)Value);
                else if (Type == InternalType.Float)
                    bw.WriteSingle((float)Value);
                else if (Type == InternalType.Float2)
                    bw.WriteSingles((float[])Value);
                else if (Type == InternalType.Float3)
                    bw.WriteSingles((float[])Value);
                else if (Type == InternalType.Float4)
                    bw.WriteSingles((float[])Value);

                bw.WriteByte(4);
                bw.Pad(4);
                bw.WriteInt32(0);
            }
        }

        /// <summary>
        /// Value types of internal MTD values.
        /// </summary>
        public enum InternalType
        {
            /// <summary>
            /// A one-byte boolean value.
            /// </summary>
            Bool,

            /// <summary>
            /// A four-byte floating point number.
            /// </summary>
            Float,

            /// <summary>
            /// An array of two four-byte floating point numbers.
            /// </summary>
            Float2,

            /// <summary>
            /// An array of three four-byte floating point numbers.
            /// </summary>
            Float3,

            /// <summary>
            /// An array of four four-byte floating point numbers.
            /// </summary>
            Float4,

            /// <summary>
            /// A four-byte integer.
            /// </summary>
            Int,

            // TODO: verify this
            /// <summary>
            /// An array of two four-byte integers. Only used in DS3.
            /// </summary>
            Int2
        }

        /// <summary>
        /// A material value not stored in the MTD itself.
        /// </summary>
        public class ExternalEntry
        {
            /// <summary>
            /// The name of the value.
            /// </summary>
            public string Name;

            /// <summary>
            /// Unknown
            /// </summary>
            public int ShaderDataIndex;

            private int unk2, unk5, unk6, unk7;

            internal ExternalEntry(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                unk2 = br.ReadInt32();
                br.AssertInt32(0x2000);
                br.AssertInt32(3);
                unk5 = br.ReadInt32();
                Name = br.ReadShiftJISLengthPrefixed(0x35);
                unk6 = br.ReadInt32();
                unk7 = br.ReadInt32();
                ShaderDataIndex = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(unk2);
                bw.WriteInt32(0x2000);
                bw.WriteInt32(3);
                bw.WriteInt32(unk5);
                bw.WriteShiftJISLengthPrefixed(Name, 0x35);
                bw.WriteInt32(unk6);
                bw.WriteInt32(unk7);
                bw.WriteInt32(ShaderDataIndex);
            }
        }

        /// <summary>
        /// The blending mode of the material, used in value g_BlendMode.
        /// </summary>
        public enum BlendMode
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            Normal = 0,
            TexEdge = 1,
            Blend = 2,
            Water = 3,
            Add = 4,
            Sub = 5,
            Mul = 6,
            AddMul = 7,
            SubMul = 8,
            WaterWave = 9,
            LSNormal = 32,
            LSTexEdge = 33,
            LSBlend = 34,
            LSWater = 35,
            LSAdd = 36,
            LSSub = 37,
            LSMul = 38,
            LSAddMul = 39,
            LSSubMul = 40,
            LSWaterWave = 41,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// The lighting type of a material, used in value g_LightingType.
        /// </summary>
        public enum LightingType
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            None = 0,
            HemDirDifSpcx3 = 1,
            HemEnvDifSpc = 3,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}
