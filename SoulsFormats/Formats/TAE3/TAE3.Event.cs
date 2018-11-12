using System;

namespace SoulsFormats
{
    public partial class TAE3
    {
        /// <summary>
        /// Determines the behavior of an event and what data it contains.
        /// </summary>
        public enum EventType : ulong
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            Unk000 = 000,
            Unk001 = 001,
            Unk002 = 002,
            Unk005 = 005,
            Unk016 = 016,
            Unk017 = 017,
            Unk024 = 024,
            Unk032 = 032,
            Unk033 = 033,
            Unk034 = 034,
            Unk035 = 035,
            Unk064 = 064,
            Unk065 = 065,
            Unk066 = 066,
            Unk067 = 067,
            Unk096 = 096,
            Unk110 = 110,
            Unk112 = 112,
            Unk113 = 113,
            Unk114 = 114,
            Unk115 = 115,
            Unk116 = 116,
            Unk117 = 117,
            Unk118 = 118,
            Unk119 = 119,
            Unk120 = 120,
            Unk121 = 121,
            PlaySound = 128,
            Unk129 = 129,
            Unk130 = 130,
            Unk131 = 131,
            Unk132 = 132,
            Unk136 = 136,
            Unk137 = 137,
            Unk138 = 138,
            Unk144 = 144,
            Unk145 = 145,
            Unk150 = 150,
            Unk151 = 151,
            Unk161 = 161,
            Unk192 = 192,
            Unk193 = 193,
            Unk194 = 194,
            Unk224 = 224,
            Unk225 = 225,
            Unk226 = 226,
            Unk227 = 227,
            Unk228 = 228,
            Unk229 = 229,
            Unk231 = 231,
            Unk232 = 232,
            Unk233 = 233,
            Unk236 = 236,
            Unk237 = 237,
            Unk300 = 300,
            Unk301 = 301,
            Unk302 = 302,
            Unk303 = 303,
            Unk304 = 304,
            Unk306 = 306,
            Unk307 = 307,
            Unk308 = 308,
            Unk310 = 310,
            Unk311 = 311,
            Unk312 = 312,
            Unk317 = 317,
            Unk320 = 320,
            Unk330 = 330,
            Unk331 = 331,
            Unk332 = 332,
            Unk401 = 401,
            Unk500 = 500,
            Unk510 = 510,
            Unk520 = 520,
            Unk522 = 522,
            Unk600 = 600,
            Unk601 = 601,
            Unk603 = 603,
            Unk605 = 605,
            Unk606 = 606,
            Unk700 = 700,
            Unk703 = 703,
            Unk705 = 705,
            Unk707 = 707,
            HideWeapon = 710,
            Unk711 = 711,
            Unk712 = 712,
            Unk713 = 713,
            Unk714 = 714,
            Unk715 = 715,
            Unk720 = 720,
            Unk730 = 730,
            Unk740 = 740,
            Unk760 = 760,
            Unk770 = 770,
            Unk771 = 771,
            Unk772 = 772,
            Unk781 = 781,
            Unk782 = 782,
            Unk785 = 785,
            Unk786 = 786,
            Unk790 = 790,
            Unk791 = 791,
            Unk792 = 792,
            Unk793 = 793,
            Unk794 = 794,
            Unk795 = 795,
            Unk796 = 796,
            Unk797 = 797,
            Unk798 = 798,
            Unk799 = 799,
            Unk800 = 800,
            Unk900 = 900,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// An action or effect triggered at a certain time during an animation.
        /// </summary>
        public abstract class Event
        {
            /// <summary>
            /// The type of this event.
            /// </summary>
            public readonly EventType Type;

            /// <summary>
            /// When the event begins.
            /// </summary>
            public float StartTime;

            /// <summary>
            /// When the event ends.
            /// </summary>
            public float EndTime;

            internal Event(EventType type, float startTime, float endTime)
            {
                Type = type;
                StartTime = startTime;
                EndTime = endTime;
            }

            internal void WriteTime(BinaryWriterEx bw)
            {
                bw.WriteSingle(StartTime);
                bw.WriteSingle(EndTime);
            }

            internal void WriteHeader(BinaryWriterEx bw, int i, int j, long timeStart)
            {
                bw.WriteInt64(timeStart + j * 8);
                bw.WriteInt64(timeStart + j * 8 + 4);
                bw.ReserveInt64($"EventDataOffset{i}:{j}");
            }

            internal void WriteData(BinaryWriterEx bw, int i, int j)
            {
                bw.FillInt64($"EventDataOffset{i}:{j}", bw.Position);
                bw.WriteUInt64((ulong)Type);
                bw.WriteInt64(bw.Position + 8);
                WriteSpecific(bw);
                bw.Pad(0x10);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw);

            /// <summary>
            /// Returns the start time, end time, and type of the event.
            /// </summary>
            public override string ToString()
            {
                return $"{StartTime:F3} - {EndTime:F3} {Type}";
            }

            internal static Event Read(BinaryReaderEx br)
            {
                long startTimeOffset = br.ReadInt64();
                long endTimeOffset = br.ReadInt64();
                long eventDataOffset = br.ReadInt64();
                float startTime = br.GetSingle(startTimeOffset);
                float endTime = br.GetSingle(endTimeOffset);

                Event result;
                br.StepIn(eventDataOffset);
                {
                    EventType type = br.ReadEnum64<EventType>();
                    br.AssertInt64(br.Position + 8);
                    switch (type)
                    {
                        case EventType.Unk000: result = new Unk000(type, startTime, endTime, br); break;
                        case EventType.Unk001: result = new Unk001(type, startTime, endTime, br); break;
                        case EventType.Unk002: result = new Unk002(type, startTime, endTime, br); break;
                        case EventType.Unk005: result = new Unk005(type, startTime, endTime, br); break;
                        case EventType.Unk016: result = new Unk016(type, startTime, endTime, br); break;
                        case EventType.Unk017: result = new Unk017(type, startTime, endTime, br); break;
                        case EventType.Unk024: result = new Unk024(type, startTime, endTime, br); break;
                        case EventType.Unk032: result = new Unk032(type, startTime, endTime, br); break;
                        case EventType.Unk033: result = new Unk033(type, startTime, endTime, br); break;
                        case EventType.Unk034: result = new Unk034(type, startTime, endTime, br); break;
                        case EventType.Unk035: result = new Unk035(type, startTime, endTime, br); break;
                        case EventType.Unk064: result = new Unk064(type, startTime, endTime, br); break;
                        case EventType.Unk065: result = new Unk065(type, startTime, endTime, br); break;
                        case EventType.Unk066: result = new Unk066(type, startTime, endTime, br); break;
                        case EventType.Unk067: result = new Unk067(type, startTime, endTime, br); break;
                        case EventType.Unk096: result = new Unk096(type, startTime, endTime, br); break;
                        case EventType.Unk110: result = new Unk110(type, startTime, endTime, br); break;
                        case EventType.Unk112: result = new Unk112(type, startTime, endTime, br); break;
                        case EventType.Unk113: result = new Unk113(type, startTime, endTime, br); break;
                        case EventType.Unk114: result = new Unk114(type, startTime, endTime, br); break;
                        case EventType.Unk115: result = new Unk115(type, startTime, endTime, br); break;
                        case EventType.Unk116: result = new Unk116(type, startTime, endTime, br); break;
                        case EventType.Unk117: result = new Unk117(type, startTime, endTime, br); break;
                        case EventType.Unk118: result = new Unk118(type, startTime, endTime, br); break;
                        case EventType.Unk119: result = new Unk119(type, startTime, endTime, br); break;
                        case EventType.Unk120: result = new Unk120(type, startTime, endTime, br); break;
                        case EventType.Unk121: result = new Unk121(type, startTime, endTime, br); break;
                        case EventType.PlaySound: result = new PlaySound(type, startTime, endTime, br); break;
                        case EventType.Unk129: result = new Unk129(type, startTime, endTime, br); break;
                        case EventType.Unk130: result = new Unk130(type, startTime, endTime, br); break;
                        case EventType.Unk131: result = new Unk131(type, startTime, endTime, br); break;
                        case EventType.Unk132: result = new Unk132(type, startTime, endTime, br); break;
                        case EventType.Unk137: result = new Unk137(type, startTime, endTime, br); break;
                        case EventType.Unk138: result = new Unk138(type, startTime, endTime, br); break;
                        case EventType.Unk144: result = new Unk144(type, startTime, endTime, br); break;
                        case EventType.Unk145: result = new Unk145(type, startTime, endTime, br); break;
                        case EventType.Unk150: result = new Unk150(type, startTime, endTime, br); break;
                        case EventType.Unk151: result = new Unk151(type, startTime, endTime, br); break;
                        case EventType.Unk161: result = new Unk161(type, startTime, endTime, br); break;
                        case EventType.Unk193: result = new Unk193(type, startTime, endTime, br); break;
                        case EventType.Unk194: result = new Unk194(type, startTime, endTime, br); break;
                        case EventType.Unk224: result = new Unk224(type, startTime, endTime, br); break;
                        case EventType.Unk225: result = new Unk225(type, startTime, endTime, br); break;
                        case EventType.Unk226: result = new Unk226(type, startTime, endTime, br); break;
                        case EventType.Unk227: result = new Unk227(type, startTime, endTime, br); break;
                        case EventType.Unk228: result = new Unk228(type, startTime, endTime, br); break;
                        case EventType.Unk229: result = new Unk229(type, startTime, endTime, br); break;
                        case EventType.Unk231: result = new Unk231(type, startTime, endTime, br); break;
                        case EventType.Unk232: result = new Unk232(type, startTime, endTime, br); break;
                        case EventType.Unk233: result = new Unk233(type, startTime, endTime, br); break;
                        case EventType.Unk236: result = new Unk236(type, startTime, endTime, br); break;
                        case EventType.Unk237: result = new Unk237(type, startTime, endTime, br); break;
                        case EventType.Unk300: result = new Unk300(type, startTime, endTime, br); break;
                        case EventType.Unk301: result = new Unk301(type, startTime, endTime, br); break;
                        case EventType.Unk302: result = new Unk302(type, startTime, endTime, br); break;
                        case EventType.Unk303: result = new Unk303(type, startTime, endTime, br); break;
                        case EventType.Unk304: result = new Unk304(type, startTime, endTime, br); break;
                        case EventType.Unk307: result = new Unk307(type, startTime, endTime, br); break;
                        case EventType.Unk308: result = new Unk308(type, startTime, endTime, br); break;
                        case EventType.Unk310: result = new Unk310(type, startTime, endTime, br); break;
                        case EventType.Unk311: result = new Unk311(type, startTime, endTime, br); break;
                        case EventType.Unk312: result = new Unk312(type, startTime, endTime, br); break;
                        case EventType.Unk320: result = new Unk320(type, startTime, endTime, br); break;
                        case EventType.Unk330: result = new Unk330(type, startTime, endTime, br); break;
                        case EventType.Unk331: result = new Unk331(type, startTime, endTime, br); break;
                        case EventType.Unk332: result = new Unk332(type, startTime, endTime, br); break;
                        case EventType.Unk401: result = new Unk401(type, startTime, endTime, br); break;
                        case EventType.Unk500: result = new Unk500(type, startTime, endTime, br); break;
                        case EventType.Unk510: result = new Unk510(type, startTime, endTime, br); break;
                        case EventType.Unk520: result = new Unk520(type, startTime, endTime, br); break;
                        case EventType.Unk522: result = new Unk522(type, startTime, endTime, br); break;
                        case EventType.Unk600: result = new Unk600(type, startTime, endTime, br); break;
                        case EventType.Unk601: result = new Unk601(type, startTime, endTime, br); break;
                        case EventType.Unk603: result = new Unk603(type, startTime, endTime, br); break;
                        case EventType.Unk605: result = new Unk605(type, startTime, endTime, br); break;
                        case EventType.Unk606: result = new Unk606(type, startTime, endTime, br); break;
                        case EventType.Unk700: result = new Unk700(type, startTime, endTime, br); break;
                        case EventType.Unk703: result = new Unk703(type, startTime, endTime, br); break;
                        case EventType.Unk705: result = new Unk705(type, startTime, endTime, br); break;
                        case EventType.Unk707: result = new Unk707(type, startTime, endTime, br); break;
                        case EventType.HideWeapon: result = new HideWeapon(type, startTime, endTime, br); break;
                        case EventType.Unk711: result = new Unk711(type, startTime, endTime, br); break;
                        case EventType.Unk712: result = new Unk712(type, startTime, endTime, br); break;
                        case EventType.Unk713: result = new Unk713(type, startTime, endTime, br); break;
                        case EventType.Unk714: result = new Unk714(type, startTime, endTime, br); break;
                        case EventType.Unk715: result = new Unk715(type, startTime, endTime, br); break;
                        case EventType.Unk720: result = new Unk720(type, startTime, endTime, br); break;
                        case EventType.Unk730: result = new Unk730(type, startTime, endTime, br); break;
                        case EventType.Unk740: result = new Unk740(type, startTime, endTime, br); break;
                        case EventType.Unk760: result = new Unk760(type, startTime, endTime, br); break;
                        case EventType.Unk770: result = new Unk770(type, startTime, endTime, br); break;
                        case EventType.Unk771: result = new Unk771(type, startTime, endTime, br); break;
                        case EventType.Unk772: result = new Unk772(type, startTime, endTime, br); break;
                        case EventType.Unk781: result = new Unk781(type, startTime, endTime, br); break;
                        case EventType.Unk782: result = new Unk782(type, startTime, endTime, br); break;
                        case EventType.Unk785: result = new Unk785(type, startTime, endTime, br); break;
                        case EventType.Unk786: result = new Unk786(type, startTime, endTime, br); break;
                        case EventType.Unk790: result = new Unk790(type, startTime, endTime, br); break;
                        case EventType.Unk791: result = new Unk791(type, startTime, endTime, br); break;
                        case EventType.Unk792: result = new Unk792(type, startTime, endTime, br); break;
                        case EventType.Unk793: result = new Unk793(type, startTime, endTime, br); break;
                        case EventType.Unk794: result = new Unk794(type, startTime, endTime, br); break;
                        case EventType.Unk795: result = new Unk795(type, startTime, endTime, br); break;
                        case EventType.Unk796: result = new Unk796(type, startTime, endTime, br); break;
                        case EventType.Unk797: result = new Unk797(type, startTime, endTime, br); break;
                        case EventType.Unk798: result = new Unk798(type, startTime, endTime, br); break;
                        case EventType.Unk799: result = new Unk799(type, startTime, endTime, br); break;
                        case EventType.Unk800: result = new Unk800(type, startTime, endTime, br); break;

                        default:
                            throw new NotImplementedException();
                    }
                }
                br.StepOut();

                return result;
            }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public class Unk000 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                public Unk000(float startTime, float endTime, int unk00, int unk04, int unk08, int unk0C) : base(EventType.Unk000, startTime, endTime)
                {
                    Unk00 = unk00;
                    Unk04 = unk04;
                    Unk08 = unk08;
                    Unk0C = unk0C;
                }

                internal Unk000(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk001 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk001(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk002 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;
                public short Unk10, Unk12;

                public Unk002(float startTime, float endTime, int unk00, int unk04, int unk08, int unk0C, short unk10, short unk12) : base(EventType.Unk002, startTime, endTime)
                {
                    Unk00 = unk00;
                    Unk04 = unk04;
                    Unk08 = unk08;
                    Unk0C = unk0C;
                    Unk10 = unk10;
                    Unk12 = unk12;
                }

                internal Unk002(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt16();
                    Unk12 = br.AssertInt16(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt16(Unk10);
                    bw.WriteInt16(Unk12);
                    bw.WriteInt32(0);
                }
            }

            public class Unk005 : Event
            {
                public int Unk00, Unk04;

                internal Unk005(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk016 : Event
            {
                internal Unk016(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime) { }

                internal override void WriteSpecific(BinaryWriterEx bw) { }
            }

            public class Unk017 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk017(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk024 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk024(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk032 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk032(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk033 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk033(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk034 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk034(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk035 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk035(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk064 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk064(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk065 : Event
            {
                public int Unk00;
                public short Unk04, Unk06;
                public int Unk08, Unk0C;

                internal Unk065(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt16();
                    Unk06 = br.AssertInt16(-1);
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt16(Unk04);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk066 : Event
            {
                public int Unk00;

                public Unk066(float startTime, float endTime, int unk00) : base(EventType.Unk066, startTime, endTime)
                {
                    Unk00 = unk00;
                }

                internal Unk066(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class Unk067 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk067(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk096 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk096(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk110 : Event
            {
                public int Unk00, Unk04;

                internal Unk110(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk112 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk112(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk113 : Event
            {
                public int Unk00, Unk04;

                internal Unk113(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk114 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C;

                internal Unk114(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.AssertInt32(0);
                    Unk18 = br.ReadInt32();
                    Unk1C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk115 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C;

                internal Unk115(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.AssertInt32(0);
                    Unk18 = br.AssertInt32(0);
                    Unk1C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk116 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk116(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk117 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk117(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(-1);
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk118 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk118(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk119 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk119(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk120 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C, Unk20, Unk24, Unk28, Unk2C, Unk30, Unk34, Unk38, Unk3C;

                internal Unk120(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(Unk34);
                    bw.WriteInt32(Unk38);
                    bw.WriteInt32(Unk3C);
                }
            }

            public class Unk121 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk121(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class PlaySound : Event
            {
                public int SoundType, SoundID;

                internal PlaySound(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk129 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14;

                internal Unk129(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                }
            }

            public class Unk130 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14;

                internal Unk130(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(-1);
                    Unk10 = br.AssertInt32(0);
                    Unk14 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                }
            }

            public class Unk131 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk131(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk132 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk132(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk137 : Event
            {
                public int Unk00, Unk04;

                internal Unk137(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk138 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk138(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk144 : Event
            {
                public int Unk00, Unk0C;
                public float Unk04, Unk08;

                internal Unk144(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk145 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk145(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk150 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk150(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk151 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk151(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk161 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk161(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk193 : Event
            {
                public float Unk00, Unk04;

                internal Unk193(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(Unk04);
                }
            }

            public class Unk194 : Event
            {
                public float Unk00, Unk08;
                public int Unk04, Unk0C;

                internal Unk194(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.ReadSingle();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk224 : Event
            {
                public float Unk00;
                public int Unk04;

                internal Unk224(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk225 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk225(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk226 : Event
            {
                public int Unk00, Unk04;

                internal Unk226(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk227 : Event
            {
                public int Unk00, Unk04;

                internal Unk227(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk228 : Event
            {
                public float Unk00, Unk04;
                public int Unk08, Unk0C;

                internal Unk228(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk229 : Event
            {
                public int Unk00, Unk04;

                internal Unk229(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk231 : Event
            {
                public int Unk00, Unk04;

                internal Unk231(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk232 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk232(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk233 : Event
            {
                public int Unk00, Unk10, Unk14, Unk18, Unk1C;
                public short Unk04, Unk06, Unk08, Unk0A, Unk0C, Unk0E;

                internal Unk233(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt16();
                    Unk06 = br.ReadInt16();
                    Unk08 = br.ReadInt16();
                    Unk0A = br.ReadInt16();
                    Unk0C = br.ReadInt16();
                    Unk0E = br.ReadInt16();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt16(Unk04);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt16(Unk08);
                    bw.WriteInt16(Unk0A);
                    bw.WriteInt16(Unk0C);
                    bw.WriteInt16(Unk0E);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk236 : Event
            {
                public float Unk00, Unk04;
                public int Unk08, Unk0C;

                internal Unk236(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk237 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk237(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk300 : Event
            {
                public int Unk00, Unk0C;
                public float Unk04, Unk08;

                internal Unk300(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk301 : Event
            {
                public int Unk00, Unk04;

                internal Unk301(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk302 : Event
            {
                public int Unk00, Unk04;

                internal Unk302(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk303 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk303(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk304 : Event
            {
                public int Unk00, Unk04;

                internal Unk304(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk307 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk307(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk308 : Event
            {
                public float Unk00;
                public int Unk04, Unk08, Unk0C;

                internal Unk308(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk310 : Event
            {
                public int Unk00, Unk04;

                internal Unk310(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk311 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk311(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk312 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C;

                internal Unk312(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk320 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk320(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk330 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk330(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk331 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk331(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk332 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk332(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk401 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk401(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk500 : Event
            {
                public int Unk00, Unk04;

                internal Unk500(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk510 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk510(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk520 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk520(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk522 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk522(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk600 : Event
            {
                public int Unk00, Unk04;

                internal Unk600(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk601 : Event
            {
                public int Unk00, Unk0C;
                public float Unk04, Unk08;

                internal Unk601(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk603 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk603(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk605 : Event
            {
                public int Unk00, Unk04, Unk10, Unk14, Unk18, Unk1C;
                public float Unk08, Unk0C;

                internal Unk605(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadSingle();
                    Unk10 = br.AssertInt32(0);
                    Unk14 = br.AssertInt32(0);
                    Unk18 = br.AssertInt32(0);
                    Unk1C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteSingle(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk606 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk606(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk700 : Event
            {
                public float Unk00, Unk04, Unk08, Unk0C;
                public int Unk10;
                // 6 - head turn
                public byte Unk14;
                public float Unk18, Unk1C, Unk20, Unk24;

                internal Unk700(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadSingle();
                    Unk10 = br.ReadInt32();

                    Unk14 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);

                    Unk18 = br.ReadSingle();
                    Unk1C = br.ReadSingle();
                    Unk20 = br.ReadSingle();
                    Unk24 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteSingle(Unk0C);
                    bw.WriteInt32(Unk10);

                    bw.WriteByte(Unk14);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteSingle(Unk18);
                    bw.WriteSingle(Unk1C);
                    bw.WriteSingle(Unk20);
                    bw.WriteSingle(Unk24);
                }
            }

            public class Unk703 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk703(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk705 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk705(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk707 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk707(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class HideWeapon : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal HideWeapon(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk711 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C;

                internal Unk711(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.AssertInt32(0);
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk712 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C;

                internal Unk712(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.AssertInt32(0);
                    Unk18 = br.AssertInt32(0);
                    Unk1C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk713 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C;

                internal Unk713(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.ReadInt32();
                    Unk10 = br.AssertInt32(0);
                    Unk14 = br.AssertInt32(0);
                    Unk18 = br.AssertInt32(0);
                    Unk1C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk714 : Event
            {
                public int Unk00, Unk04;

                internal Unk714(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk715 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18, Unk1C;

                internal Unk715(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                    Unk10 = br.AssertInt32(0);
                    Unk14 = br.AssertInt32(0);
                    Unk18 = br.AssertInt32(0);
                    Unk1C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            public class Unk720 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk720(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk730 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk730(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk740 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk740(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk760 : Event
            {
                public int Unk00;
                public float Unk04, Unk08, Unk0C, Unk10, Unk14;

                internal Unk760(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadSingle();
                    Unk10 = br.ReadSingle();
                    Unk14 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteSingle(Unk0C);
                    bw.WriteSingle(Unk10);
                    bw.WriteSingle(Unk14);
                }
            }

            public class Unk770 : Event
            {
                public int Unk00, Unk08, Unk0C;
                public float Unk04;

                internal Unk770(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk771 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk771(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk772 : Event
            {
                public int Unk00, Unk08, Unk0C;
                public float Unk04;

                internal Unk772(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk781 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk781(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk782 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk782(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk785 : Event
            {
                public float Unk00;
                public int Unk04, Unk08, Unk0C;

                internal Unk785(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk786 : Event
            {
                public float Unk00;
                public int Unk04;

                internal Unk786(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk790 : Event
            {
                public int Unk00, Unk04;

                internal Unk790(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk791 : Event
            {
                public int Unk00, Unk04;

                internal Unk791(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk792 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk792(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk793 : Event
            {
                public int Unk00, Unk04;

                internal Unk793(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk794 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk794(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk795 : Event
            {
                public int Unk00;
                public float Unk04;

                internal Unk795(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                }
            }

            public class Unk796 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk796(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk797 : Event
            {
                public int Unk00, Unk04;

                internal Unk797(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk798 : Event
            {
                public float Unk00;
                public int Unk04, Unk08, Unk0C;

                internal Unk798(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk799 : Event
            {
                public int Unk00, Unk04, Unk08, Unk0C;

                internal Unk799(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.AssertInt32(0);
                    Unk04 = br.AssertInt32(0);
                    Unk08 = br.AssertInt32(0);
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk800 : Event
            {
                public float Unk00, Unk04, Unk08;
                public int Unk0C;

                internal Unk800(EventType type, float startTime, float endTime, BinaryReaderEx br) : base(type, startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}
