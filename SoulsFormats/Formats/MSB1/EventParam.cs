using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB1
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public enum EventType : uint
        {
            Light = 0,
            Sound = 1,
            SFX = 2,
            WindSFX = 3,
            Treasure = 4,
            Generator = 5,
            Message = 6,
            ObjAct = 7,
            SpawnPoint = 8,
            MapOffset = 9,
            Navmesh = 10,
            Environment = 11,
            PseudoMultiplayer = 12,
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        public class EventParam : Param<Event>
        {
            internal override string Name => "EVENT_PARAM_ST";

            public List<Event.Light> Lights { get; set; }

            public List<Event.Sound> Sounds { get; set; }

            public List<Event.SFX> SFXs { get; set; }

            public List<Event.WindSFX> WindSFXs { get; set; }

            public List<Event.Treasure> Treasures { get; set; }

            public List<Event.Generator> Generators { get; set; }

            public List<Event.Message> Messages { get; set; }

            public List<Event.ObjAct> ObjActs { get; set; }

            public List<Event.SpawnPoint> SpawnPoints { get; set; }

            public List<Event.MapOffset> MapOffsets { get; set; }

            public List<Event.Navmesh> Navmeshes { get; set; }

            public List<Event.Environment> Environments { get; set; }

            public List<Event.PseudoMultiplayer> PseudoMultiplayers { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam() : base()
            {
                Lights = new List<Event.Light>();
                Sounds = new List<Event.Sound>();
                SFXs = new List<Event.SFX>();
                WindSFXs = new List<Event.WindSFX>();
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                Messages = new List<Event.Message>();
                ObjActs = new List<Event.ObjAct>();
                SpawnPoints = new List<Event.SpawnPoint>();
                MapOffsets = new List<Event.MapOffset>();
                Navmeshes = new List<Event.Navmesh>();
                Environments = new List<Event.Environment>();
                PseudoMultiplayers = new List<Event.PseudoMultiplayer>();
            }

            public override List<Event> GetEntries()
            {
                return SFUtil.ConcatAll<Event>(
                    Lights, Sounds, SFXs, WindSFXs, Treasures,
                    Generators, Messages, ObjActs, SpawnPoints, MapOffsets,
                    Navmeshes, Environments, PseudoMultiplayers);
            }

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 8);
                switch (type)
                {
                    case EventType.Light:
                        var light = new Event.Light(br);
                        Lights.Add(light);
                        return light;

                    case EventType.Sound:
                        var sound = new Event.Sound(br);
                        Sounds.Add(sound);
                        return sound;

                    case EventType.SFX:
                        var sfx = new Event.SFX(br);
                        SFXs.Add(sfx);
                        return sfx;

                    case EventType.WindSFX:
                        var windSFX = new Event.WindSFX(br);
                        WindSFXs.Add(windSFX);
                        return windSFX;

                    case EventType.Treasure:
                        var treasure = new Event.Treasure(br);
                        Treasures.Add(treasure);
                        return treasure;

                    case EventType.Generator:
                        var generator = new Event.Generator(br);
                        Generators.Add(generator);
                        return generator;

                    case EventType.Message:
                        var message = new Event.Message(br);
                        Messages.Add(message);
                        return message;

                    case EventType.ObjAct:
                        var objAct = new Event.ObjAct(br);
                        ObjActs.Add(objAct);
                        return objAct;

                    case EventType.SpawnPoint:
                        var spawnPoint = new Event.SpawnPoint(br);
                        SpawnPoints.Add(spawnPoint);
                        return spawnPoint;

                    case EventType.MapOffset:
                        var mapOffset = new Event.MapOffset(br);
                        MapOffsets.Add(mapOffset);
                        return mapOffset;

                    case EventType.Navmesh:
                        var navmesh = new Event.Navmesh(br);
                        Navmeshes.Add(navmesh);
                        return navmesh;

                    case EventType.Environment:
                        var environment = new Event.Environment(br);
                        Environments.Add(environment);
                        return environment;

                    case EventType.PseudoMultiplayer:
                        var pseudoMultiplayer = new Event.PseudoMultiplayer(br);
                        PseudoMultiplayers.Add(pseudoMultiplayer);
                        return pseudoMultiplayer;

                    default:
                        throw new NotImplementedException($"Unsupported event type: {type}");
                }
            }
        }

        public abstract class Event : Entry
        {
            public int EventID { get; set; }

            public abstract EventType Type { get; }

            public string PartName { get; set; }
            private int PartIndex;

            public string RegionName { get; set; }
            private int RegionIndex;

            public int EntityID { get; set; }

            internal Event()
            {
                Name = "";
                EventID = -1;
                EntityID = -1;
            }

            internal Event(BinaryReaderEx br)
            {
                long start = br.Position;
                int nameOffset = br.ReadInt32();
                EventID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                int baseDataOffset = br.ReadInt32();
                int typeDataOffset = br.ReadInt32();
                br.AssertInt32(0);

                Name = br.GetShiftJIS(start + nameOffset);

                br.Position = start + baseDataOffset;
                PartIndex = br.ReadInt32();
                RegionIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertInt32(0);

                br.Position = start + typeDataOffset;
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.ReserveInt32("BaseDataOffset");
                bw.ReserveInt32("TypeDataOffset");
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(Name, true);
                bw.Pad(4);

                bw.FillInt32("BaseDataOffset", (int)(bw.Position - start));
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(0);

                bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
            }

            internal virtual void GetNames(MSB1 msb, Entries entries)
            {
                PartName = FindName(entries.Parts, PartIndex);
                RegionName = FindName(entries.Regions, RegionIndex);
            }

            internal virtual void GetIndices(MSB1 msb, Entries entries)
            {
                PartIndex = FindIndex(entries.Parts, PartName);
                RegionIndex = FindIndex(entries.Regions, RegionName);
            }

            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            public class Light : Event
            {
                /// <summary>
                /// EventType.Light
                /// </summary>
                public override EventType Type => EventType.Light;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                public Light() : base() { }

                internal Light(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(UnkT00);
                }
            }

            public class Sound : Event
            {
                /// <summary>
                /// EventType.Sound
                /// </summary>
                public override EventType Type => EventType.Sound;

                public int SoundType { get; set; }

                public int SoundID { get; set; }

                public Sound() : base() { }

                internal Sound(BinaryReaderEx br) : base(br)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                }
            }

            public class SFX : Event
            {
                /// <summary>
                /// EventType.SFX
                /// </summary>
                public override EventType Type => EventType.SFX;

                public int FFXID { get; set; }

                public SFX() : base() { }

                internal SFX(BinaryReaderEx br) : base(br)
                {
                    FFXID = br.ReadInt32();
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(FFXID);
                }
            }

            public class WindSFX : Event
            {
                /// <summary>
                /// EventType.WindSFX
                /// </summary>
                public override EventType Type => EventType.WindSFX;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT3C { get; set; }

                public WindSFX() : base() { }

                internal WindSFX(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    UnkT1C = br.ReadSingle();
                    UnkT20 = br.ReadSingle();
                    UnkT24 = br.ReadSingle();
                    UnkT28 = br.ReadSingle();
                    UnkT2C = br.ReadSingle();
                    UnkT30 = br.ReadSingle();
                    UnkT34 = br.ReadSingle();
                    UnkT38 = br.ReadSingle();
                    UnkT3C = br.ReadSingle();
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteSingle(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteSingle(UnkT1C);
                    bw.WriteSingle(UnkT20);
                    bw.WriteSingle(UnkT24);
                    bw.WriteSingle(UnkT28);
                    bw.WriteSingle(UnkT2C);
                    bw.WriteSingle(UnkT30);
                    bw.WriteSingle(UnkT34);
                    bw.WriteSingle(UnkT38);
                    bw.WriteSingle(UnkT3C);
                }
            }

            public class Treasure : Event
            {
                /// <summary>
                /// EventType.Treasure
                /// </summary>
                public override EventType Type => EventType.Treasure;

                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                /// <summary>
                /// Five ItemLotParam IDs.
                /// </summary>
                public int[] ItemLots { get; private set; }

                public bool InChest { get; set; }

                public bool StartDisabled { get; set; }

                public Treasure() : base()
                {
                    ItemLots = new int[5] { -1, -1, -1, -1, -1 };
                }

                internal Treasure(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt32(0);
                    TreasurePartIndex = br.ReadInt32();
                    ItemLots = new int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        ItemLots[i] = br.ReadInt32();
                        br.AssertInt32(-1);
                    }
                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
                    br.AssertInt16(0);
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TreasurePartIndex);
                    for (int i = 0; i < 5; i++)
                    {
                        bw.WriteInt32(ItemLots[i]);
                        bw.WriteInt32(-1);
                    }
                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteInt16(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    TreasurePartName = FindName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    TreasurePartIndex = FindIndex(entries.Parts, TreasurePartName);
                }
            }

            public class Generator : Event
            {
                /// <summary>
                /// EventType.Generator
                /// </summary>
                public override EventType Type => EventType.Generator;

                public short MaxNum { get; set; }

                public short LimitNum { get; set; }

                public short MinGenNum { get; set; }

                public short MaxGenNum { get; set; }

                public float MinInterval { get; set; }

                public float MaxInterval { get; set; }

                public int InitialSpawnCount { get; set; }

                public string[] SpawnPointNames { get; private set; }
                private int[] SpawnPointIndices;

                public string[] SpawnPartNames { get; private set; }
                private int[] SpawnPartIndices;

                public Generator() : base()
                {
                    SpawnPointNames = new string[4];
                    SpawnPartNames = new string[32];
                }

                internal Generator(BinaryReaderEx br) : base(br)
                {
                    MaxNum = br.ReadInt16();
                    LimitNum = br.ReadInt16();
                    MinGenNum = br.ReadInt16();
                    MaxGenNum = br.ReadInt16();
                    MinInterval = br.ReadSingle();
                    MaxInterval = br.ReadSingle();
                    InitialSpawnCount = br.ReadInt32();
                    br.AssertPattern(0x1C, 0x00);
                    SpawnPointIndices = br.ReadInt32s(4);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertPattern(0x40, 0x00);
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt16(MaxNum);
                    bw.WriteInt16(LimitNum);
                    bw.WriteInt16(MinGenNum);
                    bw.WriteInt16(MaxGenNum);
                    bw.WriteSingle(MinInterval);
                    bw.WriteSingle(MaxInterval);
                    bw.WriteInt32(InitialSpawnCount);
                    bw.WritePattern(0x1C, 0x00);
                    bw.WriteInt32s(SpawnPointIndices);
                    bw.WriteInt32s(SpawnPartIndices);
                    bw.WritePattern(0x40, 0x00);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointNames = FindNames(entries.Regions, SpawnPointIndices);
                    SpawnPartNames = FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnPointIndices = FindIndices(entries.Regions, SpawnPointNames);
                    SpawnPartIndices = FindIndices(entries.Parts, SpawnPartNames);
                }
            }

            public class Message : Event
            {
                /// <summary>
                /// EventType.Message
                /// </summary>
                public override EventType Type => EventType.Message;

                public short MessageID { get; set; }

                public short UnkT02 { get; set; }

                public bool Hidden { get; set; }

                public Message() : base() { }

                internal Message(BinaryReaderEx br) : base(br)
                {
                    MessageID = br.ReadInt16();
                    UnkT02 = br.ReadInt16();
                    Hidden = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt16(MessageID);
                    bw.WriteInt16(UnkT02);
                    bw.WriteBoolean(Hidden);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                }
            }

            public class ObjAct : Event
            {
                /// <summary>
                /// EventType.ObjAct
                /// </summary>
                public override EventType Type => EventType.ObjAct;

                public int ObjActEntityID { get; set; }

                public string ObjActPartName { get; set; }
                private int ObjActPartIndex;

                public short ObjActParamID { get; set; }

                public short UnkT0A { get; set; }

                public int EventFlagID { get; set; }

                public ObjAct() : base()
                {
                    ObjActEntityID = -1;
                    ObjActParamID = -1;
                    EventFlagID = -1;
                }

                internal ObjAct(BinaryReaderEx br) : base(br)
                {
                    ObjActEntityID = br.ReadInt32();
                    ObjActPartIndex = br.ReadInt32();
                    ObjActParamID = br.ReadInt16();
                    UnkT0A = br.ReadInt16();
                    EventFlagID = br.ReadInt32();
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(ObjActEntityID);
                    bw.WriteInt32(ObjActPartIndex);
                    bw.WriteInt16(ObjActParamID);
                    bw.WriteInt16(UnkT0A);
                    bw.WriteInt32(EventFlagID);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjActPartName = FindName(entries.Parts, ObjActPartIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjActPartIndex = FindIndex(entries.Parts, ObjActPartName);
                }
            }

            public class SpawnPoint : Event
            {
                /// <summary>
                /// EventType.SpawnPoint
                /// </summary>
                public override EventType Type => EventType.SpawnPoint;

                public string SpawnPointName { get; set; }
                private int SpawnPointIndex;

                public SpawnPoint() : base() { }

                internal SpawnPoint(BinaryReaderEx br) : base(br)
                {
                    SpawnPointIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(SpawnPointIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointName = FindName(entries.Regions, SpawnPointIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnPointIndex = FindIndex(entries.Regions, SpawnPointName);
                }
            }

            public class MapOffset : Event
            {
                /// <summary>
                /// EventType.MapOffset
                /// </summary>
                public override EventType Type => EventType.MapOffset;

                public Vector3 Position { get; set; }

                public float Degree { get; set; }

                public MapOffset() : base() { }

                internal MapOffset(BinaryReaderEx br) : base(br)
                {
                    Position = br.ReadVector3();
                    Degree = br.ReadSingle();
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteVector3(Position);
                    bw.WriteSingle(Degree);
                }
            }

            public class Navmesh : Event
            {
                /// <summary>
                /// EventType.Navmesh
                /// </summary>
                public override EventType Type => EventType.Navmesh;

                public string NavmeshRegionName { get; set; }
                private int NavmeshRegionIndex;

                public Navmesh() : base() { }

                internal Navmesh(BinaryReaderEx br) : base(br)
                {
                    NavmeshRegionIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(NavmeshRegionIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    NavmeshRegionName = FindName(entries.Regions, NavmeshRegionIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    NavmeshRegionIndex = FindIndex(entries.Regions, NavmeshRegionName);
                }
            }

            public class Environment : Event
            {
                /// <summary>
                /// EventType.Environment
                /// </summary>
                public override EventType Type => EventType.Environment;

                public int UnkT00 { get; set; }

                public float UnkT04 { get; set; }

                public float UnkT08 { get; set; }

                public float UnkT0C { get; set; }

                public float UnkT10 { get; set; }

                public float UnkT14 { get; set; }

                public Environment() : base() { }

                internal Environment(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class PseudoMultiplayer : Event
            {
                /// <summary>
                /// EventType.PseudoMultiplayer
                /// </summary>
                public override EventType Type => EventType.PseudoMultiplayer;

                public int HostEntityID { get; set; }

                public int EventFlagID { get; set; }

                public string SpawnPointName { get; set; }
                private int SpawnPointIndex;

                public PseudoMultiplayer() : base()
                {
                    HostEntityID = -1;
                    EventFlagID = -1;
                }

                internal PseudoMultiplayer(BinaryReaderEx br) : base(br)
                {
                    HostEntityID = br.ReadInt32();
                    EventFlagID = br.ReadInt32();
                    SpawnPointIndex = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int id)
                {
                    base.Write(bw, id);
                    bw.WriteInt32(HostEntityID);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(SpawnPointIndex);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointName = FindName(entries.Regions, SpawnPointIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnPointIndex = FindIndex(entries.Regions, SpawnPointName);
                }
            }
        }
    }
}
