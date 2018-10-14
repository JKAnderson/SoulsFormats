using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB64
    {
        public class EventSection : Section<Event>
        {
            public override string Type => "EVENT_PARAM_ST";

            public List<Event.Treasure> Treasures;

            public List<Event.Generators> Generators;

            public List<Event.ObjAct> ObjActs;

            public List<Event.MapOffset> MapOffsets;

            public List<Event.Invasion> Invasions;

            public List<Event.WalkRoute> WalkRoutes;

            public List<Event.GroupTour> GroupTours;

            public List<Event.Other> Others;

            internal EventSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generators>();
                ObjActs = new List<Event.ObjAct>();
                MapOffsets = new List<Event.MapOffset>();
                Invasions = new List<Event.Invasion>();
                WalkRoutes = new List<Event.WalkRoute>();
                GroupTours = new List<Event.GroupTour>();
                Others = new List<Event.Other>();
            }

            internal override List<Event> GetEntries()
            {
                return Util.ConcatAll<Event>(
                    Treasures, Generators, ObjActs, MapOffsets, Invasions, WalkRoutes, GroupTours, Others);
            }

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);

                switch (type)
                {
                    case EventType.Treasures:
                        var treasure = new Event.Treasure(br);
                        Treasures.Add(treasure);
                        return treasure;

                    case EventType.Generators:
                        var generator = new Event.Generators(br);
                        Generators.Add(generator);
                        return generator;

                    case EventType.ObjActs:
                        var objAct = new Event.ObjAct(br);
                        ObjActs.Add(objAct);
                        return objAct;

                    case EventType.MapOffset:
                        var mapOffset = new Event.MapOffset(br);
                        MapOffsets.Add(mapOffset);
                        return mapOffset;

                    case EventType.BlackEyeOrbInvasions:
                        var invasion = new Event.Invasion(br);
                        Invasions.Add(invasion);
                        return invasion;

                    case EventType.WalkRoute:
                        var walkRoute = new Event.WalkRoute(br);
                        WalkRoutes.Add(walkRoute);
                        return walkRoute;

                    case EventType.GroupTour:
                        var groupTour = new Event.GroupTour(br);
                        GroupTours.Add(groupTour);
                        return groupTour;

                    case EventType.Other:
                        var other = new Event.Other(br);
                        Others.Add(other);
                        return other;

                    default:
                        throw new NotImplementedException($"Unsupported event type: {type}");
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<Event> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    entries[i].Write(bw);
                }
            }

            internal void GetNames(MSB64 msb, Entries entries)
            {
                foreach (Event ev in entries.Events)
                    ev.GetNames(msb, entries);
            }

            internal void GetIndices(MSB64 msb, Entries entries)
            {
                foreach (Event ev in entries.Events)
                    ev.GetIndices(msb, entries);
            }
        }

        public enum EventType : uint
        {
            Lights = 0x0,
            Sounds = 0x1,
            SFX = 0x2,
            WindSFX = 0x3,
            Treasures = 0x4,
            Generators = 0x5,
            BloodMsg = 0x6,
            ObjActs = 0x7,
            SpawnPoints = 0x8,
            MapOffset = 0x9,
            Navimesh = 0xA,
            Environment = 0xB,
            BlackEyeOrbInvasions = 0xC,
            // Mystery = 0xD,
            WalkRoute = 0xE,
            GroupTour = 0xF,
            Other = 0xFFFFFFFF,
        }

        public abstract class Event
        {
            internal abstract EventType Type { get; }

            public string Name;
            public int EventIndex;
            public int ID;
            private int partIndex;
            public string PartName;
            private int pointIndex;
            public string PointName;
            public int EventEntityID;

            internal Event(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                EventIndex = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                ID = br.ReadInt32();
                br.AssertInt32(0);
                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                Name = br.GetUTF16(start + nameOffset);

                br.StepIn(start + baseDataOffset);
                partIndex = br.ReadInt32();
                pointIndex = br.ReadInt32();
                EventEntityID = br.ReadInt32();
                br.AssertInt32(0);
                br.StepOut();

                br.StepIn(start + typeDataOffset);
                Read(br);
                br.StepOut();
            }

            internal abstract void Read(BinaryReaderEx br);

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventIndex);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(ID);
                bw.WriteInt32(0);
                bw.ReserveInt64("BaseDataOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset", bw.Position - start);
                bw.WriteInt32(partIndex);
                bw.WriteInt32(pointIndex);
                bw.WriteInt32(EventEntityID);
                bw.WriteInt32(0);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteSpecific(bw);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw);

            internal virtual void GetNames(MSB64 msb, Entries entries)
            {
                PartName = GetName(entries.Parts, partIndex);
                PointName = GetName(entries.Regions, pointIndex);
            }

            internal virtual void GetIndices(MSB64 msb, Entries entries)
            {
                partIndex = GetIndex(entries.Parts, PartName);
                pointIndex = GetIndex(entries.Regions, PointName);
            }

            public override string ToString()
            {
                return $"{Type} {ID} : {Name}";
            }

            /// <summary>
            /// A pickuppable item.
            /// </summary>
            public class Treasure : Event
            {
                internal override EventType Type => EventType.Treasures;

                private int partIndex2;
                public string PartName2;
                public int ItemLot1, ItemLot2;

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID;

                /// <summary>
                /// Used for treasures inside chests, exact significance unknown.
                /// </summary>
                public bool IsChest;

                /// <summary>
                /// Used only for Yoel's ashes treasure.
                /// </summary>
                public bool IsYoelUnknown;

                internal Treasure(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    partIndex2 = br.ReadInt32();
                    br.AssertInt32(0);
                    ItemLot1 = br.ReadInt32();
                    ItemLot2 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    PickupAnimID = br.ReadInt32();

                    IsChest = br.ReadBoolean();
                    IsYoelUnknown = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);

                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(partIndex2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ItemLot1);
                    bw.WriteInt32(ItemLot2);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);

                    bw.WriteBoolean(IsChest);
                    bw.WriteBoolean(IsYoelUnknown);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB64 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PartName2 = GetName(entries.Parts, partIndex2);
                }

                internal override void GetIndices(MSB64 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    partIndex2 = GetIndex(entries.Parts, PartName2);
                }
            }

            public class Generators : Event
            {
                internal override EventType Type => EventType.Generators;

                public short MaxNum;
                public short LimitNum;
                public short MinGenNum;
                public short MaxGenNum;
                public float MinInterval;
                public float MaxInterval;
                private int[] spawnPointIndices;
                public string[] SpawnPointNames { get; private set; }
                private int[] spawnPartIndices;
                public string[] SpawnPartNames { get; private set; }

                public int UnkT10;
                public float UnkT14, UnkT18;

                internal Generators(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    MaxNum = br.ReadInt16();
                    LimitNum = br.ReadInt16();
                    MinGenNum = br.ReadInt16();
                    MaxGenNum = br.ReadInt16();
                    MinInterval = br.ReadSingle();
                    MaxInterval = br.ReadSingle();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    spawnPointIndices = br.ReadInt32s(8);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    spawnPartIndices = br.ReadInt32s(32);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt16(MaxNum);
                    bw.WriteInt16(LimitNum);
                    bw.WriteInt16(MinGenNum);
                    bw.WriteInt16(MaxGenNum);
                    bw.WriteSingle(MinInterval);
                    bw.WriteSingle(MaxInterval);
                    bw.WriteInt32(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(spawnPointIndices);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(spawnPartIndices);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB64 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointNames = new string[spawnPointIndices.Length];
                    for (int i = 0; i < spawnPointIndices.Length; i++)
                        SpawnPointNames[i] = GetName(entries.Regions, spawnPointIndices[i]);

                    SpawnPartNames = new string[spawnPartIndices.Length];
                    for (int i = 0; i < spawnPartIndices.Length; i++)
                        SpawnPartNames[i] = GetName(entries.Parts, spawnPartIndices[i]);
                }

                internal override void GetIndices(MSB64 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    spawnPointIndices = new int[SpawnPointNames.Length];
                    for (int i = 0; i < SpawnPointNames.Length; i++)
                        spawnPointIndices[i] = GetIndex(entries.Regions, SpawnPointNames[i]);

                    spawnPartIndices = new int[SpawnPartNames.Length];
                    for (int i = 0; i < SpawnPartNames.Length; i++)
                        spawnPartIndices[i] = GetIndex(entries.Parts, SpawnPartNames[i]);
                }
            }

            public class ObjAct : Event
            {
                internal override EventType Type => EventType.ObjActs;

                public int ObjActEntityID;
                private int partIndex2;
                public string PartName2;
                public int ParameterID;
                public int UnkT10;
                public int EventFlagID;

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    ObjActEntityID = br.ReadInt32();
                    partIndex2 = br.ReadInt32();
                    ParameterID = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    EventFlagID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ObjActEntityID);
                    bw.WriteInt32(partIndex2);
                    bw.WriteInt32(ParameterID);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB64 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PartName2 = GetName(entries.Parts, partIndex2);
                }

                internal override void GetIndices(MSB64 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    partIndex2 = GetIndex(entries.Parts, PartName2);
                }
            }

            public class MapOffset : Event
            {
                internal override EventType Type => EventType.MapOffset;

                public Vector3 Position;

                public float Degree;

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    Position = br.ReadVector3();
                    Degree = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteVector3(Position);
                    bw.WriteSingle(Degree);
                }
            }

            public class Invasion : Event
            {
                internal override EventType Type => EventType.BlackEyeOrbInvasions;

                public int HostEventEntityID;
                public int InvasionEventEntityID;
                public int InvasionRegionIndex;
                public int SoundIDMaybe;
                public int MapEventIDMaybe;
                public int FlagsMaybe;
                public int UnkT18;

                internal Invasion(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    HostEventEntityID = br.ReadInt32();
                    InvasionEventEntityID = br.ReadInt32();
                    InvasionRegionIndex = br.ReadInt32();
                    SoundIDMaybe = br.ReadInt32();
                    MapEventIDMaybe = br.ReadInt32();
                    FlagsMaybe = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(HostEventEntityID);
                    bw.WriteInt32(InvasionEventEntityID);
                    bw.WriteInt32(InvasionRegionIndex);
                    bw.WriteInt32(SoundIDMaybe);
                    bw.WriteInt32(MapEventIDMaybe);
                    bw.WriteInt32(FlagsMaybe);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A simple list of points defining a path for enemies to take.
            /// </summary>
            public class WalkRoute : Event
            {
                internal override EventType Type => EventType.WalkRoute;

                /// <summary>
                /// Unknown; probably some kind of route type.
                /// </summary>
                public int UnkT00;

                private short[] walkPointIndices;
                /// <summary>
                /// List of points in the route.
                /// </summary>
                public string[] WalkPointNames { get; private set; }

                internal WalkRoute(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    UnkT00 = br.AssertInt32(0, 1, 2, 5);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    walkPointIndices = br.ReadInt16s(32);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(walkPointIndices);
                }

                internal override void GetNames(MSB64 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkPointNames = new string[walkPointIndices.Length];
                    for (int i = 0; i < walkPointIndices.Length; i++)
                        WalkPointNames[i] = GetName(entries.Regions, walkPointIndices[i]);
                }

                internal override void GetIndices(MSB64 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    walkPointIndices = new short[WalkPointNames.Length];
                    for (int i = 0; i < WalkPointNames.Length; i++)
                        walkPointIndices[i] = (short)GetIndex(entries.Regions, WalkPointNames[i]);
                }
            }

            public class GroupTour : Event
            {
                internal override EventType Type => EventType.GroupTour;

                public int UnkT00, UnkT04;
                private int[] groupPartsIndices;
                public string[] GroupPartsNames { get; private set; }

                internal GroupTour(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    groupPartsIndices = br.ReadInt32s(32);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(groupPartsIndices);
                }

                internal override void GetNames(MSB64 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartsNames = new string[groupPartsIndices.Length];
                    for (int i = 0; i < groupPartsIndices.Length; i++)
                        GroupPartsNames[i] = GetName(entries.Parts, groupPartsIndices[i]);
                }

                internal override void GetIndices(MSB64 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    groupPartsIndices = new int[GroupPartsNames.Length];
                    for (int i = 0; i < GroupPartsNames.Length; i++)
                        groupPartsIndices[i] = GetIndex(entries.Parts, GroupPartsNames[i]);
                }
            }

            // This type only appears once in one unused MSB so it's hard to draw too many conclusions from it.
            public class Other : Event
            {
                internal override EventType Type => EventType.Other;

                public int SoundTypeMaybe;
                public int SoundIDMaybe;

                internal Other(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    SoundTypeMaybe = br.ReadInt32();
                    SoundIDMaybe = br.ReadInt32();

                    for (int i = 0; i < 16; i++)
                        br.AssertInt32(-1);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundTypeMaybe);
                    bw.WriteInt32(SoundIDMaybe);

                    for (int i = 0; i < 16; i++)
                        bw.WriteInt32(-1);
                }
            }
        }
    }
}
