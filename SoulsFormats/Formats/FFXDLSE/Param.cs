using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public abstract class Param : FXSerializable
        {
            internal override string ClassName => "FXSerializableParam";

            internal override int Version => 2;

            internal abstract int Type { get; }

            internal Param(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                br.AssertInt32(Type);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(Type);
            }

            internal static Param Read(BinaryReaderEx br, List<string> classNames)
            {
                // Don't @ me.
                int type = br.GetInt32(br.Position + 0xA);
                switch (type)
                {
                    case 1: return new Param1(br, classNames);
                    case 2: return new Param2(br, classNames);
                    case 5: return new Param5(br, classNames);
                    case 6: return new Param6(br, classNames);
                    case 7: return new Param7(br, classNames);
                    case 9: return new Param9(br, classNames);
                    case 11: return new Param11(br, classNames);
                    case 12: return new Param12(br, classNames);
                    case 13: return new Param13(br, classNames);
                    case 15: return new Param15(br, classNames);
                    case 17: return new Param17(br, classNames);
                    case 18: return new Param18(br, classNames);
                    case 19: return new Param19(br, classNames);
                    case 20: return new Param20(br, classNames);
                    case 21: return new Param21(br, classNames);
                    case 37: return new Param37(br, classNames);
                    case 38: return new Param38(br, classNames);
                    case 40: return new Param40(br, classNames);
                    case 41: return new Param41(br, classNames);
                    case 44: return new Param44(br, classNames);
                    case 45: return new Param45(br, classNames);
                    case 46: return new Param46(br, classNames);
                    case 47: return new Param47(br, classNames);
                    case 59: return new Param59(br, classNames);
                    case 60: return new Param60(br, classNames);
                    case 66: return new Param66(br, classNames);
                    case 68: return new Param68(br, classNames);
                    case 69: return new Param69(br, classNames);
                    case 70: return new Param70(br, classNames);
                    case 71: return new Param71(br, classNames);
                    case 79: return new Param79(br, classNames);
                    case 81: return new Param81(br, classNames);
                    case 82: return new Param82(br, classNames);
                    case 83: return new Param83(br, classNames);
                    case 84: return new Param84(br, classNames);
                    case 85: return new Param85(br, classNames);
                    case 87: return new Param87(br, classNames);

                    default:
                        throw new NotImplementedException($"Unimplemented param type: {type}");
                }
            }
        }

        public class Param1 : Param
        {
            internal override int Type => 1;

            public PrimitiveInt Int { get; set; }

            internal Param1(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Int = new PrimitiveInt(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Int.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Int.Write(bw, classNames);
            }
        }

        public class Param2 : Param
        {
            internal override int Type => 2;

            public List<PrimitiveInt> Ints { get; set; }

            internal Param2(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                Ints = new List<PrimitiveInt>(count);
                for (int i = 0; i < count; i++)
                    Ints.Add(new PrimitiveInt(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (PrimitiveInt primInt in Ints)
                    primInt.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Ints.Count);
                foreach (PrimitiveInt primInt in Ints)
                    primInt.Write(bw, classNames);
            }
        }

        public class Param5 : Param
        {
            internal override int Type => 5;

            public List<TickInt> TickInts { get; set; }

            internal Param5(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickInts = new List<TickInt>(count);
                for (int i = 0; i < count; i++)
                    TickInts.Add(new TickInt(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickInt tickInt in TickInts)
                    tickInt.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickInts.Count);
                foreach (TickInt tickInt in TickInts)
                    tickInt.Write(bw, classNames);
            }
        }

        public class Param6 : Param
        {
            internal override int Type => 6;

            public List<TickInt> TickInts { get; set; }

            internal Param6(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickInts = new List<TickInt>(count);
                for (int i = 0; i < count; i++)
                    TickInts.Add(new TickInt(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickInt tickInt in TickInts)
                    tickInt.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickInts.Count);
                foreach (TickInt tickInt in TickInts)
                    tickInt.Write(bw, classNames);
            }
        }

        public class Param7 : Param
        {
            internal override int Type => 7;

            public PrimitiveFloat Float { get; set; }

            internal Param7(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Float = new PrimitiveFloat(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Float.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Float.Write(bw, classNames);
            }
        }

        public class Param9 : Param
        {
            internal override int Type => 9;

            public List<TickFloat> TickFloats { get; set; }

            internal Param9(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickFloats = new List<TickFloat>(count);
                for (int i = 0; i < count; i++)
                    TickFloats.Add(new TickFloat(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickFloat tickFloat in TickFloats)
                    tickFloat.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickFloats.Count);
                foreach (TickFloat tickFloat in TickFloats)
                    tickFloat.Write(bw, classNames);
            }
        }

        public class Param11 : Param
        {
            internal override int Type => 11;

            public List<TickFloat> TickFloats { get; set; }

            internal Param11(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickFloats = new List<TickFloat>(count);
                for (int i = 0; i < count; i++)
                    TickFloats.Add(new TickFloat(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickFloat tickFloat in TickFloats)
                    tickFloat.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickFloats.Count);
                foreach (TickFloat tickFloat in TickFloats)
                    tickFloat.Write(bw, classNames);
            }
        }

        public class Param12 : Param
        {
            internal override int Type => 12;

            public List<TickFloat> TickFloats { get; set; }

            internal Param12(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickFloats = new List<TickFloat>(count);
                for (int i = 0; i < count; i++)
                    TickFloats.Add(new TickFloat(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickFloat tickFloat in TickFloats)
                    tickFloat.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickFloats.Count);
                foreach (TickFloat tickFloat in TickFloats)
                    tickFloat.Write(bw, classNames);
            }
        }

        public class Param13 : Param
        {
            internal override int Type => 13;

            public List<TickFloat3> TickFloat3s { get; set; }

            internal Param13(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickFloat3s = new List<TickFloat3>(count);
                for (int i = 0; i < count; i++)
                    TickFloat3s.Add(new TickFloat3(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickFloat3 tickFloat3 in TickFloat3s)
                    tickFloat3.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickFloat3s.Count);
                foreach (TickFloat3 tickFloat3 in TickFloat3s)
                    tickFloat3.Write(bw, classNames);
            }
        }

        public class Param15 : Param
        {
            internal override int Type => 15;

            public PrimitiveColor Color { get; set; }

            internal Param15(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Color = new PrimitiveColor(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Color.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Color.Write(bw, classNames);
            }
        }

        public class Param17 : Param
        {
            internal override int Type => 17;

            public List<TickColor> TickColors { get; set; }

            internal Param17(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++)
                    TickColors.Add(new TickColor(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in TickColors)
                    tickColor.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickColors.Count);
                foreach (TickColor tickColor in TickColors)
                    tickColor.Write(bw, classNames);
            }
        }

        public class Param18 : Param
        {
            internal override int Type => 18;

            public List<TickColor> TickColors { get; set; }

            internal Param18(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++)
                    TickColors.Add(new TickColor(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in TickColors)
                    tickColor.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickColors.Count);
                foreach (TickColor tickColor in TickColors)
                    tickColor.Write(bw, classNames);
            }
        }

        public class Param19 : Param
        {
            internal override int Type => 19;

            public List<TickColor> TickColors { get; set; }

            internal Param19(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++)
                    TickColors.Add(new TickColor(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in TickColors)
                    tickColor.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickColors.Count);
                foreach (TickColor tickColor in TickColors)
                    tickColor.Write(bw, classNames);
            }
        }

        public class Param20 : Param
        {
            internal override int Type => 20;

            public List<TickColor> TickColors { get; set; }

            internal Param20(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++)
                    TickColors.Add(new TickColor(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in TickColors)
                    tickColor.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickColors.Count);
                foreach (TickColor tickColor in TickColors)
                    tickColor.Write(bw, classNames);
            }
        }

        public class Param21 : Param
        {
            internal override int Type => 21;

            public List<TickColor3> TickColor3s { get; set; }

            internal Param21(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                TickColor3s = new List<TickColor3>(count);
                for (int i = 0; i < count; i++)
                    TickColor3s.Add(new TickColor3(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (TickColor3 tickColor3 in TickColor3s)
                    tickColor3.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(TickColor3s.Count);
                foreach (TickColor3 tickColor3 in TickColor3s)
                    tickColor3.Write(bw, classNames);
            }
        }

        public class Param37 : Param
        {
            internal override int Type => 37;

            public int Unk04 { get; set; }

            public ParamList ParamList { get; set; }

            internal Param37(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                ParamList = new ParamList(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                ParamList.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                ParamList.Write(bw, classNames);
            }
        }

        public class Param38 : Param
        {
            internal override int Type => 38;

            public int Unk04 { get; set; }

            public ParamList ParamList { get; set; }

            internal Param38(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                ParamList = new ParamList(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                ParamList.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                ParamList.Write(bw, classNames);
            }
        }

        public class Param40 : Param
        {
            internal override int Type => 40;

            public int Unk04 { get; set; }

            internal Param40(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
            }
        }

        public class Param41 : Param
        {
            internal override int Type => 41;

            public int Unk04 { get; set; }

            internal Param41(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
            }
        }

        public class Param44 : Param
        {
            internal override int Type => 44;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param44(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param45 : Param
        {
            internal override int Type => 45;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param45(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param46 : Param
        {
            internal override int Type => 46;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param46(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param47 : Param
        {
            internal override int Type => 47;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param47(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param59 : Param
        {
            internal override int Type => 59;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param59(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param60 : Param
        {
            internal override int Type => 60;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param60(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param66 : Param
        {
            internal override int Type => 66;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param66(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param68 : Param
        {
            internal override int Type => 68;

            public int Unk04 { get; set; }

            internal Param68(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
            }
        }

        public class Param69 : Param
        {
            internal override int Type => 69;

            public int Unk04 { get; set; }

            internal Param69(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
            }
        }

        public class Param70 : Param
        {
            internal override int Type => 70;

            public PrimitiveTick Tick { get; set; }

            internal Param70(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Tick = new PrimitiveTick(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Tick.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Tick.Write(bw, classNames);
            }
        }

        public class Param71 : Param
        {
            internal override int Type => 71;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param71(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Param79 : Param
        {
            internal override int Type => 79;

            public PrimitiveInt Int1 { get; set; }

            public PrimitiveInt Int2 { get; set; }

            internal Param79(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Int1 = new PrimitiveInt(br, classNames);
                Int2 = new PrimitiveInt(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Int1.AddClassNames(classNames);
                Int2.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Int1.Write(bw, classNames);
                Int2.Write(bw, classNames);
            }
        }

        public class Param81 : Param
        {
            internal override int Type => 81;

            public PrimitiveFloat Float1 { get; set; }

            public PrimitiveFloat Float2 { get; set; }

            internal Param81(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Float1 = new PrimitiveFloat(br, classNames);
                Float2 = new PrimitiveFloat(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Float1.AddClassNames(classNames);
                Float2.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Float1.Write(bw, classNames);
                Float2.Write(bw, classNames);
            }
        }

        public class Param82 : Param
        {
            internal override int Type => 82;

            public Param Param { get; set; }

            public PrimitiveFloat Float { get; set; }

            internal Param82(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Param = Param.Read(br, classNames);
                Float = new PrimitiveFloat(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Param.AddClassNames(classNames);
                Float.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Param.Write(bw, classNames);
                Float.Write(bw, classNames);
            }
        }

        public class Param83 : Param
        {
            internal override int Type => 83;

            public PrimitiveColor Color1 { get; set; }

            public PrimitiveColor Color2 { get; set; }

            internal Param83(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Color1 = new PrimitiveColor(br, classNames);
                Color2 = new PrimitiveColor(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Color1.AddClassNames(classNames);
                Color2.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Color1.Write(bw, classNames);
                Color2.Write(bw, classNames);
            }
        }

        public class Param84 : Param
        {
            internal override int Type => 84;

            public Param Param { get; set; }

            public PrimitiveColor Color { get; set; }

            internal Param84(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Param = Param.Read(br, classNames);
                Color = new PrimitiveColor(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Param.AddClassNames(classNames);
                Color.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Param.Write(bw, classNames);
                Color.Write(bw, classNames);
            }
        }

        public class Param85 : Param
        {
            internal override int Type => 85;

            public PrimitiveTick Tick1 { get; set; }

            public PrimitiveTick Tick2 { get; set; }

            internal Param85(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Tick1 = new PrimitiveTick(br, classNames);
                Tick2 = new PrimitiveTick(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Tick1.AddClassNames(classNames);
                Tick2.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                Tick1.Write(bw, classNames);
                Tick2.Write(bw, classNames);
            }
        }

        public class Param87 : Param
        {
            internal override int Type => 87;

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            internal Param87(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class TickInt
        {
            public PrimitiveTick Tick { get; set; }

            public PrimitiveInt Int { get; set; }

            internal TickInt(BinaryReaderEx br, List<string> classNames)
            {
                Tick = new PrimitiveTick(br, classNames);
                Int = new PrimitiveInt(br, classNames);
            }

            internal void AddClassNames(List<string> classNames)
            {
                Tick.AddClassNames(classNames);
                Int.AddClassNames(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                Tick.Write(bw, classNames);
                Int.Write(bw, classNames);
            }
        }

        public class TickFloat
        {
            public PrimitiveTick Tick { get; set; }

            public PrimitiveFloat Float { get; set; }

            internal TickFloat(BinaryReaderEx br, List<string> classNames)
            {
                Tick = new PrimitiveTick(br, classNames);
                Float = new PrimitiveFloat(br, classNames);
            }

            internal void AddClassNames(List<string> classNames)
            {
                Tick.AddClassNames(classNames);
                Float.AddClassNames(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                Tick.Write(bw, classNames);
                Float.Write(bw, classNames);
            }
        }

        public class TickFloat3
        {
            public PrimitiveTick Tick { get; set; }

            public PrimitiveFloat Float1 { get; set; }

            public PrimitiveFloat Float2 { get; set; }

            public PrimitiveFloat Float3 { get; set; }

            internal TickFloat3(BinaryReaderEx br, List<string> classNames)
            {
                Tick = new PrimitiveTick(br, classNames);
                Float1 = new PrimitiveFloat(br, classNames);
                Float2 = new PrimitiveFloat(br, classNames);
                Float3 = new PrimitiveFloat(br, classNames);
            }

            internal void AddClassNames(List<string> classNames)
            {
                Tick.AddClassNames(classNames);
                Float1.AddClassNames(classNames);
                Float2.AddClassNames(classNames);
                Float3.AddClassNames(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                Tick.Write(bw, classNames);
                Float1.Write(bw, classNames);
                Float2.Write(bw, classNames);
                Float3.Write(bw, classNames);
            }
        }

        public class TickColor
        {
            public PrimitiveTick Tick { get; set; }

            public PrimitiveColor Color { get; set; }

            internal TickColor(BinaryReaderEx br, List<string> classNames)
            {
                Tick = new PrimitiveTick(br, classNames);
                Color = new PrimitiveColor(br, classNames);
            }

            internal void AddClassNames(List<string> classNames)
            {
                Tick.AddClassNames(classNames);
                Color.AddClassNames(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                Tick.Write(bw, classNames);
                Color.Write(bw, classNames);
            }
        }

        public class TickColor3
        {
            public PrimitiveTick Tick { get; set; }

            public PrimitiveColor Color1 { get; set; }

            public PrimitiveColor Color2 { get; set; }

            public PrimitiveColor Color3 { get; set; }

            internal TickColor3(BinaryReaderEx br, List<string> classNames)
            {
                Tick = new PrimitiveTick(br, classNames);
                Color1 = new PrimitiveColor(br, classNames);
                Color2 = new PrimitiveColor(br, classNames);
                Color3 = new PrimitiveColor(br, classNames);
            }

            internal void AddClassNames(List<string> classNames)
            {
                Tick.AddClassNames(classNames);
                Color1.AddClassNames(classNames);
                Color2.AddClassNames(classNames);
                Color3.AddClassNames(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                Tick.Write(bw, classNames);
                Color1.Write(bw, classNames);
                Color2.Write(bw, classNames);
                Color3.Write(bw, classNames);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
