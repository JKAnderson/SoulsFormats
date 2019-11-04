using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class EvaluatableInt : FXSerializable
        {
            internal override string ClassName => "FXSerializableEvaluatable<dl_int32>";

            internal override int Version => 1;

            public int Operator { get; set; }

            public int Type { get; set; }

            public int Literal1 { get; set; }

            public int Literal2 { get; set; }

            public EvaluatableInt Left { get; set; }

            public EvaluatableInt Right { get; set; }

            internal EvaluatableInt(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Operator = br.ReadInt32();
                Type = br.ReadInt32();
                switch (Operator)
                {
                    case 4:
                    case 5:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                        break;

                    case 1:
                        Literal1 = br.ReadInt32();
                        break;

                    case 2:
                    case 3:
                        Literal1 = br.ReadInt32();
                        Literal2 = br.ReadInt32();
                        break;

                    case 20:
                        Left = new EvaluatableInt(br, classNames);
                        break;

                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        Left = new EvaluatableInt(br, classNames);
                        Right = new EvaluatableInt(br, classNames);
                        break;

                    default:
                        throw new NotImplementedException($"Unimplemented operator: {Operator}");
                }
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Left?.AddClassNames(classNames);
                Right?.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(Operator);
                bw.WriteInt32(Type);
                switch (Operator)
                {
                    case 4:
                    case 5:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                        break;

                    case 1:
                        bw.WriteInt32(Literal1);
                        break;

                    case 2:
                    case 3:
                        bw.WriteInt32(Literal1);
                        bw.WriteInt32(Literal2);
                        break;

                    case 20:
                        Left.Write(bw, classNames);
                        break;

                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        Left.Write(bw, classNames);
                        Right.Write(bw, classNames);
                        break;

                    default:
                        throw new NotImplementedException($"Unimplemented operator: {Operator}");
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
