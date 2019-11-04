using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class PrimitiveInt : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<dl_int32>";

            internal override int Version => 1;

            public int Value { get; set; }

            internal PrimitiveInt(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Value = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(Value);
            }
        }

        public class PrimitiveFloat : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<dl_float32>";

            internal override int Version => 1;

            public float Value { get; set; }

            internal PrimitiveFloat(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Value = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteSingle(Value);
            }
        }

        public class PrimitiveTick : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<FXTick>";

            internal override int Version => 1;

            public float Value { get; set; }

            internal PrimitiveTick(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Value = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteSingle(Value);
            }
        }

        public class PrimitiveColor : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<FXColorRGBA>";

            internal override int Version => 1;

            public float R { get; set; }

            public float G { get; set; }

            public float B { get; set; }

            public float A { get; set; }

            internal PrimitiveColor(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                R = br.ReadSingle();
                G = br.ReadSingle();
                B = br.ReadSingle();
                A = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteSingle(R);
                bw.WriteSingle(G);
                bw.WriteSingle(B);
                bw.WriteSingle(A);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
