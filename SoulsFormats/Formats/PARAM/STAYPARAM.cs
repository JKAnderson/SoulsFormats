using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A system configuration file to set the properties for various game mechanics used in DS3.
    /// </summary>
    public class STAYPARAM : SoulsFile<STAYPARAM>
    {
        /// <summary>
        /// The rows of this stayparam; must be loaded with STAYPARAM.ApplyParamdef() before fields can be read/written.
        /// </summary>
        public List<Row> Rows { get; set; }

        /// <summary>
        /// The current applied STAYPARAMDEF.
        /// </summary>
        public STAYPARAMDEF AppliedParamdef { get; private set;  }

        /// <summary>
        /// A copy of the data in the STAYPARAM, to be parsed after applying a STAYPARAMDEF.
        /// </summary>
        private BinaryReaderEx _fieldReader;
        
        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            byte[] copy = br.GetBytes(0, (int) br.Stream.Length);
            _fieldReader = new BinaryReaderEx(false, copy);
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            if (AppliedParamdef == null)
                throw new InvalidOperationException("Params cannot be written without applying a paramdef.");

            foreach (var field in Rows)
            {
                field.Write(bw);
            }

            if (bw.Length < 256)
            {
                bw.Pad(256);
            }
        }

        /// <summary>
        /// Interprets data according to the given stayparamdef and stores it for later writing.
        /// </summary>
        public void ApplyParamdef(STAYPARAMDEF def)
        {
            Rows = new List<Row>(def.Fields.Count);
            
            foreach (var field in def.Fields)
            {
                if (_fieldReader != null)
                {
                    Rows.Add(new Row(_fieldReader, field));
                }
                else
                {
                    Rows.Add(new Row(field));
                }
            }

            AppliedParamdef = def;
        }

        /// <summary>
        /// Returns the row with the given display name, or null if not found.
        /// </summary>
        public Row this[string name] => Rows.Find(row => row.Field.DisplayName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        
        
        /// <summary>
        /// An entry of a stayparam that contains a single value and is associated with a STAYPARAMDEF.Field.
        /// </summary>
        public class Row
        {
            /// <summary>
            /// The editor settings for the this row.
            /// </summary>
            public STAYPARAMDEF.Field Field { get; set; }

            /// <summary>
            /// The current value of the field in the stayparam.
            /// </summary>
            public object Value { get; set; }

            internal Row(STAYPARAMDEF.Field field)
            {
                Field = field;
                Value = field.Default;
            }
            
            internal Row(BinaryReaderEx br, STAYPARAMDEF.Field field)
            {
                Field = field;

                if (Field.Type == STAYPARAMDEF.FieldType.s8)
                {
                    Value = br.ReadByte();
                }
                else if (Field.Type == STAYPARAMDEF.FieldType.s16)
                {
                    Value = br.ReadInt16();
                }
                else if (Field.Type == STAYPARAMDEF.FieldType.s32)
                {
                    Value = br.ReadInt32();
                }
                else if (Field.Type == STAYPARAMDEF.FieldType.f32)
                {
                    Value = br.ReadSingle();
                }
            }

            internal void Write(BinaryWriterEx bw)
            {
                if (Field.Type == STAYPARAMDEF.FieldType.s8)
                {
                    bw.WriteByte(Convert.ToByte(Value));
                }
                else if (Field.Type == STAYPARAMDEF.FieldType.s16)
                {
                    bw.WriteInt16(Convert.ToInt16(Value));
                }
                else if (Field.Type == STAYPARAMDEF.FieldType.s32)
                {
                    bw.WriteInt32(Convert.ToInt32(Value));
                }
                else if (Field.Type == STAYPARAMDEF.FieldType.f32)
                {
                    bw.WriteSingle(Convert.ToSingle(Value));
                }
            }
        }
    }
}