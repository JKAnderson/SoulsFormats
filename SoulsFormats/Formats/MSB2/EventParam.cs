using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB2
    {
        public enum EventType : ushort
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            Light = 1,
            Shadow = 2,
            Fog = 3,
            BGColor = 4,
            MapOffset = 5,
            Warp = 6,
            CheapMode = 7,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        public class EventParam : Param<Event>
        {
            internal override string Name => "EVENT_PARAM_ST";
            internal override int Version => 5;

            public List<Event.Light> Lights { get; set; }

            public List<Event.Shadow> Shadows { get; set; }

            public List<Event.Fog> Fogs { get; set; }

            public List<Event.BGColor> BGColors { get; set; }

            public List<Event.MapOffset> MapOffsets { get; set; }

            public List<Event.Warp> Warps { get; set; }

            public List<Event.CheapMode> CheapModes { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam()
            {
                Lights = new List<Event.Light>();
                Shadows = new List<Event.Shadow>();
                Fogs = new List<Event.Fog>();
                BGColors = new List<Event.BGColor>();
                MapOffsets = new List<Event.MapOffset>();
                Warps = new List<Event.Warp>();
                CheapModes = new List<Event.CheapMode>();
            }

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum16<EventType>(br.Position + 0xC);
                switch (type)
                {
                    case EventType.Light:
                        var light = new Event.Light(br);
                        Lights.Add(light);
                        return light;

                    case EventType.Shadow:
                        var shadow = new Event.Shadow(br);
                        Shadows.Add(shadow);
                        return shadow;

                    case EventType.Fog:
                        var fog = new Event.Fog(br);
                        Fogs.Add(fog);
                        return fog;

                    case EventType.BGColor:
                        var bgColor = new Event.BGColor(br);
                        BGColors.Add(bgColor);
                        return bgColor;

                    case EventType.MapOffset:
                        var mapOffset = new Event.MapOffset(br);
                        MapOffsets.Add(mapOffset);
                        return mapOffset;

                    case EventType.Warp:
                        var warp = new Event.Warp(br);
                        Warps.Add(warp);
                        return warp;

                    case EventType.CheapMode:
                        var cheapMode = new Event.CheapMode(br);
                        CheapModes.Add(cheapMode);
                        return cheapMode;

                    default:
                        throw new NotImplementedException($"Unimplemented event type: {type}");
                }
            }

            /// <summary>
            /// Returns every Event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries()
            {
                return SFUtil.ConcatAll<Event>(
                    Lights, Shadows, Fogs, BGColors, MapOffsets,
                    Warps, CheapModes);
            }
        }

        public abstract class Event : NamedEntry
        {
            public abstract EventType Type { get; }

            public int EventID { get; set; }

            internal Event(string name = "")
            {
                Name = name;
                EventID = -1;
            }

            internal Event(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                EventID = br.ReadInt32();
                br.AssertUInt16((ushort)Type);
                br.ReadInt16(); // Index
                long typeDataOffset = br.ReadInt64();

                Name = br.GetUTF16(start + nameOffset);
                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            internal abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int index)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteUInt16((ushort)Type);
                bw.WriteInt16((short)index);
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);
            }

            internal abstract void WriteTypeData(BinaryWriterEx bw);

            public class Light : Event
            {
                /// <summary>
                /// EventType.Light
                /// </summary>
                public override EventType Type => EventType.Light;

                public short UnkT00 { get; set; }

                public float UnkT04 { get; set; }

                public float UnkT08 { get; set; }

                public Color ColorT0C { get; set; }

                public Color ColorT10 { get; set; }

                public float UnkT1C { get; set; }

                public float UnkT20 { get; set; }

                public Color ColorT24 { get; set; }

                public Color ColorT28 { get; set; }

                public Color ColorT34 { get; set; }

                public Color ColorT38 { get; set; }

                public Color ColorT3C { get; set; }

                public float UnkT40 { get; set; }

                public int UnkT44 { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light(string name = "") : base(name) { }

                internal Light(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt16();
                    br.AssertInt16(-1);
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    ColorT0C = br.ReadRGBA();
                    ColorT10 = br.ReadRGBA();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT1C = br.ReadSingle();
                    UnkT20 = br.ReadSingle();
                    ColorT24 = br.ReadRGBA();
                    ColorT28 = br.ReadRGBA();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ColorT34 = br.ReadRGBA();
                    ColorT38 = br.ReadRGBA();
                    ColorT3C = br.ReadRGBA();
                    UnkT40 = br.ReadSingle();
                    UnkT44 = br.ReadInt32();
                    br.AssertPattern(0x38, 0x00);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkT00);
                    bw.WriteInt16(-1);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteRGBA(ColorT0C);
                    bw.WriteRGBA(ColorT10);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT1C);
                    bw.WriteSingle(UnkT20);
                    bw.WriteRGBA(ColorT24);
                    bw.WriteRGBA(ColorT28);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteRGBA(ColorT34);
                    bw.WriteRGBA(ColorT38);
                    bw.WriteRGBA(ColorT3C);
                    bw.WriteSingle(UnkT40);
                    bw.WriteInt32(UnkT44);
                    bw.WritePattern(0x38, 0x00);
                }
            }

            public class Shadow : Event
            {
                /// <summary>
                /// EventType.Shadow
                /// </summary>
                public override EventType Type => EventType.Shadow;

                public int UnkT00 { get; set; }

                public float UnkT04 { get; set; }

                public float UnkT08 { get; set; }

                public float UnkT0C { get; set; }

                public int UnkT10 { get; set; }

                public Color ColorT14 { get; set; }

                public float UnkT18 { get; set; }

                public int UnkT1C { get; set; }

                public float UnkT20 { get; set; }

                public Color ColorT24 { get; set; }

                /// <summary>
                /// Creates a Shadow with default values.
                /// </summary>
                public Shadow(string name = "") : base(name) { }

                internal Shadow(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadInt32();
                    ColorT14 = br.ReadRGBA();
                    UnkT18 = br.ReadSingle();
                    UnkT1C = br.ReadInt32();
                    UnkT20 = br.ReadSingle();
                    ColorT24 = br.ReadRGBA();
                    br.AssertPattern(0x18, 0x00);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteRGBA(ColorT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteSingle(UnkT20);
                    bw.WriteRGBA(ColorT24);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            public class Fog : Event
            {
                /// <summary>
                /// EventType.Fog
                /// </summary>
                public override EventType Type => EventType.Fog;

                public int UnkT00 { get; set; }

                public Color ColorT04 { get; set; }

                public float UnkT08 { get; set; }

                public float UnkT0C { get; set; }

                public float UnkT10 { get; set; }

                public int UnkT14 { get; set; }

                /// <summary>
                /// Creates a Fog with default values.
                /// </summary>
                public Fog(string name = "") : base(name) { }

                internal Fog(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    ColorT04 = br.ReadRGBA();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteRGBA(ColorT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            public class BGColor : Event
            {
                /// <summary>
                /// EventType.BGColor
                /// </summary>
                public override EventType Type => EventType.BGColor;

                public Color Color { get; set; }

                /// <summary>
                /// Creates a BGColor with default values.
                /// </summary>
                public BGColor(string name = "") : base(name) { }

                internal BGColor(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    Color = br.ReadRGBA();
                    br.AssertPattern(0x24, 0x00);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteRGBA(Color);
                    bw.WritePattern(0x24, 0x00);
                }
            }

            public class MapOffset : Event
            {
                /// <summary>
                /// EventType.MapOffset
                /// </summary>
                public override EventType Type => EventType.MapOffset;

                public Vector3 Translation { get; set; }

                /// <summary>
                /// Creates a MapOffset with default values.
                /// </summary>
                public MapOffset(string name = "") : base(name) { }

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    Translation = br.ReadVector3();
                    br.AssertInt32(0); // Degree
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteVector3(Translation);
                    bw.WriteInt32(0);
                }
            }

            public class Warp : Event
            {
                /// <summary>
                /// EventType.Warp
                /// </summary>
                public override EventType Type => EventType.Warp;

                public int UnkT00 { get; set; }

                public Vector3 Position { get; set; }

                /// <summary>
                /// Creates a Warp with default values.
                /// </summary>
                public Warp(string name = "") : base(name) { }

                internal Warp(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    Position = br.ReadVector3();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteVector3(Position);
                }
            }

            public class CheapMode : Event
            {
                /// <summary>
                /// EventType.CheapMode
                /// </summary>
                public override EventType Type => EventType.CheapMode;

                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a CheapMode with default values.
                /// </summary>
                public CheapMode(string name = "") : base(name) { }

                internal CheapMode(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertPattern(0xC, 0x00);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WritePattern(0xC, 0x00);
                }
            }
        }
    }
}
