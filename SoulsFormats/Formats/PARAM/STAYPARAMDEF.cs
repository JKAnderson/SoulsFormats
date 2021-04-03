using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A companion format to stayparams that describes the field associated with each value. Extension: .stayparamdef
    /// </summary>
    public class STAYPARAMDEF : SoulsFile<STAYPARAMDEF>
    {
        /// <summary>
        /// Fields for each value in the STAYPARAM, in order they appear.
        /// </summary>
        public List<Field> Fields { get; set; }

        /// <summary>
        /// Creates a new empty STAYPARAMDEF.
        /// </summary>
        public STAYPARAMDEF()
        {
            Fields = new List<Field>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            int fieldCount = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.Skip(4 * fieldCount);

            Fields = new List<Field>(fieldCount);

            for (int i = 0; i < fieldCount; i++)
            {
                Fields.Add(new Field(br));
            }

            foreach (var field in Fields)
            {
                field.ReadStrings(br);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.WriteInt32(Fields.Count);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            for (int i = 0; i < Fields.Count; i++)
            {
                bw.WriteInt32(0);
            }

            foreach (var field in Fields)
            {
                field.WriteInfo(bw);
            }

            foreach (var field in Fields)
            {
                field.WriteStrings(bw);
            }
        }

        /// <summary>
        /// The data type associated with a Field.
        /// </summary>
        public enum FieldType : UInt32
        {
            s8 = 0,
            s16 = 2,
            s32 = 4,
            f32 = 6
        }

        /// <summary>
        /// The display and numerical properties of a value in a stayparam.
        /// </summary>
        public class Field
        {
            /// <summary>
            /// Name to display in the editor.
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// Type of the value stored by this field.
            /// </summary>
            public FieldType Type { get; set; }

            /// <summary>
            /// Printf-style format string to apply to the value in the editor.
            /// </summary>
            public string DisplayFormat { get; set; }

            /// <summary>
            /// Default value for new rows.
            /// </summary>
            public object Default { get; set; }

            /// <summary>
            /// Minimum valid value.
            /// </summary>
            public object Minimum { get; set; }

            /// <summary>
            /// Maximum valid value.
            /// </summary>
            public object Maximum { get; set; }

            /// <summary>
            /// Amount of increase or decrease per step when scrolling in the editor.
            /// </summary>
            public object Increment { get; set; }

            internal Field(BinaryReaderEx br)
            {
                Type = br.ReadEnum32<FieldType>();
                br.AssertInt64(0);
                if (Type == FieldType.s8)
                {
                    Default = br.ReadByte();
                    Increment = br.ReadByte();
                    Minimum = br.ReadByte();
                    Maximum = br.ReadByte();
                }
                else if (Type == FieldType.s16)
                {
                    Default = br.ReadInt16();
                    Increment = br.ReadInt16();
                    Minimum = br.ReadInt16();
                    Maximum = br.ReadInt16();
                }
                else if (Type == FieldType.s32)
                {
                    Default = br.ReadInt32();
                    Increment = br.ReadInt32();
                    Minimum = br.ReadInt32();
                    Maximum = br.ReadInt32();
                }
                else if (Type == FieldType.f32)
                {
                    Default = br.ReadSingle();
                    Increment = br.ReadSingle();
                    Minimum = br.ReadSingle();
                    Maximum = br.ReadSingle();
                }
            }

            internal void ReadStrings(BinaryReaderEx br)
            {
                DisplayName = br.ReadASCII();
                DisplayFormat = br.ReadASCII();
            }

            internal void WriteInfo(BinaryWriterEx bw)
            {
                bw.WriteInt32((int) Type);
                bw.WriteInt64(0);

                if (Type == FieldType.s8)
                {
                    bw.WriteByte(Convert.ToByte(Default));
                    bw.WriteByte(Convert.ToByte(Increment));
                    bw.WriteByte(Convert.ToByte(Minimum));
                    bw.WriteByte(Convert.ToByte(Maximum));
                }
                else if (Type == FieldType.s16)
                {
                    bw.WriteInt16(Convert.ToInt16(Default));
                    bw.WriteInt16(Convert.ToInt16(Increment));
                    bw.WriteInt16(Convert.ToInt16(Minimum));
                    bw.WriteInt16(Convert.ToInt16(Maximum));
                }
                else if (Type == FieldType.s32)
                {
                    bw.WriteInt32(Convert.ToInt32(Default));
                    bw.WriteInt32(Convert.ToInt32(Increment));
                    bw.WriteInt32(Convert.ToInt32(Minimum));
                    bw.WriteInt32(Convert.ToInt32(Maximum));
                }
                else if (Type == FieldType.f32)
                {
                    bw.WriteSingle(Convert.ToSingle(Default));
                    bw.WriteSingle(Convert.ToSingle(Increment));
                    bw.WriteSingle(Convert.ToSingle(Minimum));
                    bw.WriteSingle(Convert.ToSingle(Maximum));
                }
            }

            internal void WriteStrings(BinaryWriterEx bw)
            {
                bw.WriteASCII(DisplayName);
                bw.WriteASCII(DisplayFormat);
            }
        }
    }
}