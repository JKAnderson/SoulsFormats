using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A material definition format used in DS1, DSR, DS2, DS3, DeS, BB, and NB.
    /// </summary>
    public class MTD : SoulsFile<MTD>
    {
        /// <summary>
        /// A path to the shader source file, which also determines which compiled shader to use for this material.
        /// </summary>
        public string ShaderPath { get; set; }

        /// <summary>
        /// A description of this material's purpose.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Values determining material properties.
        /// </summary>
        public List<Param> Params;

        /// <summary>
        /// Texture types required by the material shader.
        /// </summary>
        public List<Texture> Textures;

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0x2C, 4);
            return magic == "MTD ";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertInt32(0);
            int fileSize = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(3);
            AssertMarker(br, 0x01);
            br.AssertInt32(0);
            br.AssertInt32(0x1C);
            br.AssertInt32(1);
            br.AssertInt32(2);
            AssertMarker(br, 0xB0);
            br.AssertInt32(4);
            br.AssertASCII("MTD ");
            AssertMarker(br, 0x34);
            br.AssertInt32(0x3E8);
            AssertMarker(br, 0x01);
            br.AssertInt32(0);
            int dataSize = br.ReadInt32();
            br.AssertInt32(2);
            br.AssertInt32(4);
            AssertMarker(br, 0xA3);

            ShaderPath = br.ReadShiftJISLengthPrefixed(0xA3);
            Description = br.ReadShiftJISLengthPrefixed(0x03);

            br.AssertInt32(1);
            br.AssertInt32(0);
            int paramSize = br.ReadInt32();
            br.AssertInt32(3);
            br.AssertInt32(4);
            AssertMarker(br, 0xA3);
            br.AssertInt32(0);
            AssertMarker(br, 0x03);

            int paramCount = br.ReadInt32();
            Params = new List<Param>(paramCount);
            for (int i = 0; i < paramCount; i++)
                Params.Add(new Param(br));

            AssertMarker(br, 0x03);

            int textureCount = br.ReadInt32();
            Textures = new List<Texture>(textureCount);
            for (int i = 0; i < textureCount; i++)
                Textures.Add(new Texture(br));

            AssertMarker(br, 0x04);
            br.AssertInt32(0);
            AssertMarker(br, 0x04);
            br.AssertInt32(0);
            AssertMarker(br, 0x04);
            br.AssertInt32(0);
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteInt32(0);
            bw.ReserveInt32("FileSize");
            int fileStart = (int)bw.Position;
            bw.WriteInt32(0);
            bw.WriteInt32(3);
            WriteMarker(bw, 0x01);
            bw.WriteInt32(0);
            bw.WriteInt32(0x1C);
            bw.WriteInt32(1);
            bw.WriteInt32(2);
            WriteMarker(bw, 0xB0);
            bw.WriteInt32(4);
            bw.WriteASCII("MTD ");
            WriteMarker(bw, 0x34);
            bw.WriteInt32(0x3E8);
            WriteMarker(bw, 0x01);
            bw.WriteInt32(0);
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(2);
            bw.WriteInt32(4);
            int dataStart = (int)bw.Position;
            WriteMarker(bw, 0xA3);

            bw.WriteShiftJISLengthPrefixed(ShaderPath, 0xA3);
            bw.WriteShiftJISLengthPrefixed(Description, 0x03);

            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.ReserveInt32("ParamSize");
            bw.WriteInt32(3);
            bw.WriteInt32(4);
            WriteMarker(bw, 0xA3);
            bw.WriteInt32(0);
            int paramStart = (int)bw.Position;
            WriteMarker(bw, 0x03);

            bw.WriteInt32(Params.Count);
            foreach (Param internalEntry in Params)
                internalEntry.Write(bw);

            WriteMarker(bw, 0x03);

            bw.WriteInt32(Textures.Count);
            foreach (Texture externalEntry in Textures)
                externalEntry.Write(bw);

            WriteMarker(bw, 0x04);
            bw.WriteInt32(0);
            WriteMarker(bw, 0x04);
            bw.WriteInt32(0);
            WriteMarker(bw, 0x04);
            bw.WriteInt32(0);

            int position = (int)bw.Position;
            bw.FillInt32("FileSize", position - fileStart);
            bw.FillInt32("DataSize", position - dataStart);
            bw.FillInt32("ParamSize", position - paramStart);
        }

        /// <summary>
        /// A value defining the material's properties.
        /// </summary>
        public class Param
        {
            /// <summary>
            /// The name of the param.
            /// </summary>
            public string Name;

            /// <summary>
            /// The type of this value.
            /// </summary>
            public ParamType Type;

            /// <summary>
            /// The value itself.
            /// </summary>
            public object Value;

            /// <summary>
            /// Unknown; often seems like the size of this struct, but varies.
            /// </summary>
            public int Unk04;

            /// <summary>
            /// Unknown; definitely one of those markers judging by the 3 bytes of garbage,
            /// but the actual marker byte can be 0xC0, 0xC5, or 0xCA.
            /// </summary>
            public int UnkMarker;

            internal Param(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                Unk04 = br.ReadInt32();
                br.AssertInt32(4);
                br.AssertInt32(4);
                AssertMarker(br, 0xA3);
                Name = br.ReadShiftJISLengthPrefixed(0xA3);
                string type = br.ReadShiftJISLengthPrefixed(0x04);
                Type = (ParamType)Enum.Parse(typeof(ParamType), type, true);
                br.AssertInt32(1);
                br.AssertInt32(0);
                int valueSize = br.ReadInt32();

                if (Type == ParamType.Bool)
                    br.AssertByte(0);
                else if (Type == ParamType.Int || Type == ParamType.Int2)
                    br.AssertByte(1);
                else if (Type == ParamType.Float || Type == ParamType.Float2 || Type == ParamType.Float3 || Type == ParamType.Float4)
                    br.AssertByte(2);
                br.AssertByte(0x10);
                br.AssertByte(0);
                br.AssertByte(0);

                br.AssertInt32(1);
                UnkMarker = br.ReadInt32();

                if (Type == ParamType.Bool || Type == ParamType.Float || Type == ParamType.Int)
                    br.AssertInt32(1);
                else if (Type == ParamType.Float2 || Type == ParamType.Int2)
                    br.AssertInt32(2);
                else if (Type == ParamType.Float3)
                    br.AssertInt32(3);
                else if (Type == ParamType.Float4)
                    br.AssertInt32(4);

                if (Type == ParamType.Int)
                    Value = br.ReadInt32();
                else if (Type == ParamType.Int2)
                    Value = br.ReadInt32s(2);
                else if (Type == ParamType.Bool)
                    Value = br.ReadBoolean();
                else if (Type == ParamType.Float)
                    Value = br.ReadSingle();
                else if (Type == ParamType.Float2)
                    Value = br.ReadSingles(2);
                else if (Type == ParamType.Float3)
                    Value = br.ReadSingles(3);
                else if (Type == ParamType.Float4)
                    Value = br.ReadSingles(4);

                br.AssertByte(4);
                br.Pad(4);
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(4);
                bw.WriteInt32(4);
                WriteMarker(bw, 0xA3);
                bw.WriteShiftJISLengthPrefixed(Name, 0xA3);
                bw.WriteShiftJISLengthPrefixed(Type.ToString().ToLower(), 0x04);
                bw.WriteInt32(1);
                bw.WriteInt32(0);

                bw.ReserveInt32("ValueSize");
                int valueStart = (int)bw.Position;

                if (Type == ParamType.Bool)
                    bw.WriteByte(0);
                else if (Type == ParamType.Int || Type == ParamType.Int2)
                    bw.WriteByte(1);
                else if (Type == ParamType.Float || Type == ParamType.Float2 || Type == ParamType.Float3 || Type == ParamType.Float4)
                    bw.WriteByte(2);
                bw.WriteByte(0x10);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(1);
                bw.WriteInt32(UnkMarker);

                if (Type == ParamType.Bool || Type == ParamType.Float || Type == ParamType.Int)
                    bw.WriteInt32(1);
                else if (Type == ParamType.Float2 || Type == ParamType.Int2)
                    bw.WriteInt32(2);
                else if (Type == ParamType.Float3)
                    bw.WriteInt32(3);
                else if (Type == ParamType.Float4)
                    bw.WriteInt32(4);

                if (Type == ParamType.Int)
                    bw.WriteInt32((int)Value);
                else if (Type == ParamType.Int2)
                    bw.WriteInt32s((int[])Value);
                else if (Type == ParamType.Bool)
                    bw.WriteBoolean((bool)Value);
                else if (Type == ParamType.Float)
                    bw.WriteSingle((float)Value);
                else if (Type == ParamType.Float2)
                    bw.WriteSingles((float[])Value);
                else if (Type == ParamType.Float3)
                    bw.WriteSingles((float[])Value);
                else if (Type == ParamType.Float4)
                    bw.WriteSingles((float[])Value);

                bw.FillInt32("ValueSize", (int)bw.Position - valueStart);
                bw.WriteByte(4);
                bw.Pad(4);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns the name and value of the param.
            /// </summary>
            public override string ToString()
            {
                if (Type == ParamType.Float2 || Type == ParamType.Float3 || Type == ParamType.Float4)
                    return $"{Name} = {{{string.Join(", ", (float[])Value)}}}";
                else if (Type == ParamType.Int2)
                    return $"{Name} = {{{string.Join(", ", (int[])Value)}}}";
                else
                    return $"{Name} = {Value}";
            }
        }

        /// <summary>
        /// Value types of MTD params.
        /// </summary>
        public enum ParamType
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

            /// <summary>
            /// An array of two four-byte integers.
            /// </summary>
            Int2
        }

        /// <summary>
        /// Texture types used by the material, filled in in each FLVER.
        /// </summary>
        public class Texture
        {
            /// <summary>
            /// The type of texture (g_Diffuse, g_Specular, etc).
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Unknown; often seems like the size of this struct, but varies.
            /// </summary>
            public int Unk04;

            /// <summary>
            /// Seems to be one more than the index of the corresponding UV in the FLVER.
            /// </summary>
            public int UVNumber;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int ShaderDataIndex;

            internal Texture(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                Unk04 = br.ReadInt32();
                br.AssertInt32(0x2000);
                br.AssertInt32(3);
                AssertMarker(br, 0xA3);
                Type = br.ReadShiftJISLengthPrefixed(0x35);
                UVNumber = br.ReadInt32();
                AssertMarker(br, 0x35);
                ShaderDataIndex = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(0x2000);
                bw.WriteInt32(3);
                WriteMarker(bw, 0xA3);
                bw.WriteShiftJISLengthPrefixed(Type, 0x35);
                bw.WriteInt32(UVNumber);
                WriteMarker(bw, 0x35);
                bw.WriteInt32(ShaderDataIndex);
            }

            /// <summary>
            /// Returns the type of the texture.
            /// </summary>
            public override string ToString()
            {
                return Type;
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

        /// <summary>
        /// Reads weird markers throughout the file that start with the same byte and then
        /// have what is almost definitely just uninitialized memory for the next three.
        /// </summary>
        private static void AssertMarker(BinaryReaderEx br, byte start)
        {
            br.AssertByte(start);
            br.Skip(3);
        }

        /// <summary>
        /// Writes the given byte and three zeroes.
        /// </summary>
        private static void WriteMarker(BinaryWriterEx bw, byte start)
        {
            bw.WriteByte(start);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
        }
    }
}
