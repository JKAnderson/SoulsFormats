using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBS
    {
        public enum EventType : uint
        {
            Treasure = 4,
            Generator = 5,
            ObjAct = 7,
            MapOffset = 9,
            WalkRoute = 14,
            GroupTour = 15,
            Event17 = 17,
            Event18 = 18,
            Event20 = 20,
            Event21 = 21,
            PartsGroup = 22,
            Event23 = 23,
            AutoDrawGroup = 24,
            Other = 0xFFFFFFFF,
        }

        public class EventParam : Param<Event>
        {
            public List<Event.Treasure> Treasures { get; set; }

            public List<Event.Generator> Generators { get; set; }

            public List<Event.ObjAct> ObjActs { get; set; }

            public List<Event.MapOffset> MapOffsets { get; set; }

            public List<Event.WalkRoute> WalkRoutes { get; set; }

            public List<Event.GroupTour> GroupTours { get; set; }

            public List<Event.Event17> Event17s { get; set; }

            public List<Event.Event18> Event18s { get; set; }

            public List<Event.Event20> Event20s { get; set; }

            public List<Event.Event21> Event21s { get; set; }

            public List<Event.PartsGroup> PartsGroups { get; set; }

            public List<Event.Event23> Event23s { get; set; }

            public List<Event.AutoDrawGroup> AutoDrawGroups { get; set; }

            public List<Event.Other> Others { get; set; }

            public EventParam() : this(0x23) { }

            public EventParam(int unk00) : base(unk00, "EVENT_PARAM_ST")
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                ObjActs = new List<Event.ObjAct>();
                MapOffsets = new List<Event.MapOffset>();
                WalkRoutes = new List<Event.WalkRoute>();
                GroupTours = new List<Event.GroupTour>();
                Event17s = new List<Event.Event17>();
                Event18s = new List<Event.Event18>();
                Event20s = new List<Event.Event20>();
                Event21s = new List<Event.Event21>();
                PartsGroups = new List<Event.PartsGroup>();
                Event23s = new List<Event.Event23>();
                AutoDrawGroups = new List<Event.AutoDrawGroup>();
                Others = new List<Event.Other>();
            }

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                switch (type)
                {
                    case EventType.Treasure:
                        var treasure = new Event.Treasure(br);
                        Treasures.Add(treasure);
                        return treasure;

                    case EventType.Generator:
                        var generator = new Event.Generator(br);
                        Generators.Add(generator);
                        return generator;

                    case EventType.ObjAct:
                        var objAct = new Event.ObjAct(br);
                        ObjActs.Add(objAct);
                        return objAct;

                    case EventType.MapOffset:
                        var mapOffset = new Event.MapOffset(br);
                        MapOffsets.Add(mapOffset);
                        return mapOffset;

                    case EventType.WalkRoute:
                        var walkRoute = new Event.WalkRoute(br);
                        WalkRoutes.Add(walkRoute);
                        return walkRoute;

                    case EventType.GroupTour:
                        var groupTour = new Event.GroupTour(br);
                        GroupTours.Add(groupTour);
                        return groupTour;

                    case EventType.Event17:
                        var event17 = new Event.Event17(br);
                        Event17s.Add(event17);
                        return event17;

                    case EventType.Event18:
                        var event18 = new Event.Event18(br);
                        Event18s.Add(event18);
                        return event18;

                    case EventType.Event20:
                        var event20 = new Event.Event20(br);
                        Event20s.Add(event20);
                        return event20;

                    case EventType.Event21:
                        var event21 = new Event.Event21(br);
                        Event21s.Add(event21);
                        return event21;

                    case EventType.PartsGroup:
                        var partsGroup = new Event.PartsGroup(br);
                        PartsGroups.Add(partsGroup);
                        return partsGroup;

                    case EventType.Event23:
                        var event23 = new Event.Event23(br);
                        Event23s.Add(event23);
                        return event23;

                    case EventType.AutoDrawGroup:
                        var autoDrawGroup = new Event.AutoDrawGroup(br);
                        AutoDrawGroups.Add(autoDrawGroup);
                        return autoDrawGroup;

                    case EventType.Other:
                        var other = new Event.Other(br);
                        Others.Add(other);
                        return other;

                    default:
                        throw new NotImplementedException($"Unimplemented model type: {type}");
                }
            }

            public override List<Event> GetEntries()
            {
                return SFUtil.ConcatAll<Event>(
                    Treasures, Generators, ObjActs, MapOffsets, WalkRoutes,
                    GroupTours, Event17s, Event18s, Event20s, Event21s,
                    PartsGroups, Event23s, AutoDrawGroups, Others);
            }
        }

        public abstract class Event : Entry
        {
            public abstract EventType Type { get; }

            internal abstract bool HasTypeData { get; }

            public override string Name { get; set; }

            public int EventIndex { get; set; }

            public string PartName { get; set; }
            private int PartIndex;

            public string RegionName { get; set; }
            private int RegionIndex;

            public int EntityID { get; set; }

            public Event()
            {
                Name = "";
                EventIndex = -1;
                EntityID = -1;
            }

            internal Event(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                EventIndex = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                br.AssertInt32(0);
                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                Name = br.GetUTF16(start + nameOffset);
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
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventIndex);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(0);
                bw.ReserveInt64("BaseDataOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset", bw.Position - start);
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(0);

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            internal virtual void WriteTypeData(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Type data should not be written for events with no type data.");
            }

            internal virtual void GetNames(Entries entries)
            {
                PartName = GetName(entries.Parts, PartIndex);
                RegionName = GetName(entries.Regions, RegionIndex);
            }

            internal virtual void GetIndices(Entries entries)
            {
                PartIndex = GetIndex(entries.Parts, PartName);
                RegionIndex = GetIndex(entries.Regions, RegionName);
            }

            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            public class Treasure : Event
            {
                public override EventType Type => EventType.Treasure;

                internal override bool HasTypeData => true;

                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                public int ItemLotID { get; set; }

                public int ActionButtonID { get; set; }

                public int PickupAnimID { get; set; }

                public bool InChest { get; set; }

                public bool StartDisabled { get; set; }

                public Treasure() : base()
                {
                    ItemLotID = -1;
                    ActionButtonID = -1;
                    PickupAnimID = -1;
                }

                internal Treasure(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    TreasurePartIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    ItemLotID = br.ReadInt32();
                    br.AssertNull(0x24, true);
                    ActionButtonID = br.ReadInt32();
                    PickupAnimID = br.ReadInt32();
                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ItemLotID);
                    bw.WriteNull(0x24, true);
                    bw.WriteInt32(ActionButtonID);
                    bw.WriteInt32(PickupAnimID);
                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    TreasurePartName = GetName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    TreasurePartIndex = GetIndex(entries.Parts, TreasurePartName);
                }
            }

            public class Generator : Event
            {
                public override EventType Type => EventType.Generator;

                internal override bool HasTypeData => true;

                public short MaxNum { get; set; }

                public short LimitNum { get; set; }

                public short MinGenNum { get; set; }

                public short MaxGenNum { get; set; }

                public float MinInterval { get; set; }

                public float MaxInterval { get; set; }

                public int SessionCondition { get; set; }

                public float UnkT14 { get; set; }

                public float UnkT18 { get; set; }

                public string[] SpawnRegionNames { get; private set; }
                private int[] SpawnRegionIndices;

                public string[] SpawnPartNames { get; private set; }
                private int[] SpawnPartIndices;

                public Generator() : base()
                {
                    SpawnRegionNames = new string[8];
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
                    SessionCondition = br.ReadInt32();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertNull(0x14, false);
                    SpawnRegionIndices = br.ReadInt32s(8);
                    br.AssertNull(0x10, false);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertNull(0x20, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(MaxNum);
                    bw.WriteInt16(LimitNum);
                    bw.WriteInt16(MinGenNum);
                    bw.WriteInt16(MaxGenNum);
                    bw.WriteSingle(MinInterval);
                    bw.WriteSingle(MaxInterval);
                    bw.WriteInt32(SessionCondition);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteNull(0x14, false);
                    bw.WriteInt32s(SpawnRegionIndices);
                    bw.WriteNull(0x10, false);
                    bw.WriteInt32s(SpawnPartIndices);
                    bw.WriteNull(0x20, false);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    SpawnRegionNames = new string[SpawnRegionIndices.Length];
                    for (int i = 0; i < SpawnRegionIndices.Length; i++)
                        SpawnRegionNames[i] = GetName(entries.Regions, SpawnRegionIndices[i]);

                    SpawnPartNames = new string[SpawnPartIndices.Length];
                    for (int i = 0; i < SpawnPartIndices.Length; i++)
                        SpawnPartNames[i] = GetName(entries.Parts, SpawnPartIndices[i]);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    SpawnRegionIndices = new int[SpawnRegionNames.Length];
                    for (int i = 0; i < SpawnRegionNames.Length; i++)
                        SpawnRegionIndices[i] = GetIndex(entries.Regions, SpawnRegionNames[i]);

                    SpawnPartIndices = new int[SpawnPartNames.Length];
                    for (int i = 0; i < SpawnPartNames.Length; i++)
                        SpawnPartIndices[i] = GetIndex(entries.Parts, SpawnPartNames[i]);
                }
            }

            public class ObjAct : Event
            {
                public override EventType Type => EventType.ObjAct;

                internal override bool HasTypeData => true;

                public int ObjActEntityID { get; set; }

                public string ObjActPartName { get; set; }
                private int ObjActPartIndex;

                public int ObjActID { get; set; }

                public byte StateType { get; set; }

                public int EventFlagID { get; set; }

                public ObjAct() : base()
                {
                    ObjActEntityID = -1;
                    ObjActID = -1;
                    EventFlagID = -1;
                }

                internal ObjAct(BinaryReaderEx br) : base(br)
                {
                    ObjActEntityID = br.ReadInt32();
                    ObjActPartIndex = br.ReadInt32();
                    ObjActID = br.ReadInt32();
                    StateType = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                    EventFlagID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ObjActEntityID);
                    bw.WriteInt32(ObjActPartIndex);
                    bw.WriteInt32(ObjActID);
                    bw.WriteByte(StateType);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    ObjActPartName = GetName(entries.Parts, ObjActPartIndex);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    ObjActPartIndex = GetIndex(entries.Parts, ObjActPartName);
                }
            }

            public class MapOffset : Event
            {
                public override EventType Type => EventType.MapOffset;

                internal override bool HasTypeData => true;

                public Vector3 Position { get; set; }

                public float Degree { get; set; }

                public MapOffset() : base() { }

                internal MapOffset(BinaryReaderEx br) : base(br)
                {
                    Position = br.ReadVector3();
                    Degree = br.ReadSingle();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteVector3(Position);
                    bw.WriteSingle(Degree);
                }
            }

            public class WalkRoute : Event
            {
                public override EventType Type => EventType.WalkRoute;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                public string[] WalkRegionNames { get; private set; }
                private short[] WalkRegionIndices;

                public WREntry[] WREntries { get; set; }

                public WalkRoute() : base()
                {
                    WalkRegionNames = new string[32];
                    WREntries = new WREntry[5];
                    for (int i = 0; i < 5; i++)
                        WREntries[i] = new WREntry();
                }

                internal WalkRoute(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    WalkRegionIndices = br.ReadInt16s(32);
                    WREntries = new WREntry[5];
                    for (int i = 0; i < 5; i++)
                        WREntries[i] = new WREntry(br);
                    br.AssertNull(0x14, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(WalkRegionIndices);
                    for (int i = 0; i < 5; i++)
                        WREntries[i].Write(bw);
                    bw.WriteNull(0x14, false);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    WalkRegionNames = new string[WalkRegionIndices.Length];
                    for (int i = 0; i < WalkRegionIndices.Length; i++)
                        WalkRegionNames[i] = GetName(entries.Regions, WalkRegionIndices[i]);

                    foreach (WREntry wrEntry in WREntries)
                        wrEntry.GetNames(entries);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    WalkRegionIndices = new short[WalkRegionNames.Length];
                    for (int i = 0; i < WalkRegionNames.Length; i++)
                        WalkRegionIndices[i] = (short)GetIndex(entries.Regions, WalkRegionNames[i]);

                    foreach (WREntry wrEntry in WREntries)
                        wrEntry.GetIndices(entries);
                }

                public class WREntry
                {
                    public string RegionName { get; set; }
                    private short RegionIndex;

                    public int Unk04 { get; set; }

                    public int Unk08 { get; set; }

                    public WREntry() { }

                    internal WREntry(BinaryReaderEx br)
                    {
                        RegionIndex = br.ReadInt16();
                        br.AssertInt16(0);
                        Unk04 = br.ReadInt32();
                        Unk08 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(RegionIndex);
                        bw.WriteInt16(0);
                        bw.WriteInt32(Unk04);
                        bw.WriteInt32(Unk08);
                    }

                    internal void GetNames(Entries entries)
                    {
                        RegionName = GetName(entries.Regions, RegionIndex);
                    }

                    internal void GetIndices(Entries entries)
                    {
                        RegionIndex = (short)GetIndex(entries.Regions, RegionName);
                    }
                }
            }

            public class GroupTour : Event
            {
                public override EventType Type => EventType.GroupTour;

                internal override bool HasTypeData => true;

                public int PlatoonIDScriptActive { get; set; }

                public int State { get; set; }

                public string[] GroupPartNames { get; private set; }
                private int[] GroupPartIndices;

                public GroupTour() : base()
                {
                    GroupPartNames = new string[32];
                }

                internal GroupTour(BinaryReaderEx br) : base(br)
                {
                    PlatoonIDScriptActive = br.ReadInt32();
                    State = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    GroupPartIndices = br.ReadInt32s(32);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlatoonIDScriptActive);
                    bw.WriteInt32(State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(GroupPartIndices);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    GroupPartNames = new string[GroupPartIndices.Length];
                    for (int i = 0; i < GroupPartIndices.Length; i++)
                        GroupPartNames[i] = GetName(entries.Parts, GroupPartIndices[i]);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    GroupPartIndices = new int[GroupPartNames.Length];
                    for (int i = 0; i < GroupPartNames.Length; i++)
                        GroupPartIndices[i] = GetIndex(entries.Parts, GroupPartNames[i]);
                }
            }

            public class Event17 : Event
            {
                public override EventType Type => EventType.Event17;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                public Event17() : base() { }

                internal Event17(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertNull(0x1C, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteNull(0x1C, false);
                }
            }

            public class Event18 : Event
            {
                public override EventType Type => EventType.Event18;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                public Event18() : base() { }

                internal Event18(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertNull(0x1C, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteNull(0x1C, false);
                }
            }

            public class Event20 : Event
            {
                public override EventType Type => EventType.Event20;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                public short UnkT04 { get; set; }

                public short UnkT06 { get; set; }

                public Event20() : base() { }

                internal Event20(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt16();
                    UnkT06 = br.ReadInt16();
                    br.AssertNull(0x18, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt16(UnkT04);
                    bw.WriteInt16(UnkT06);
                    bw.WriteNull(0x18, false);
                }
            }

            public class Event21 : Event
            {
                public override EventType Type => EventType.Event21;

                internal override bool HasTypeData => true;

                public string[] Event21PartNames { get; private set; }
                private int[] Event21PartIndices;

                public Event21() : base()
                {
                    Event21PartNames = new string[32];
                }

                internal Event21(BinaryReaderEx br) : base(br)
                {
                    Event21PartIndices = br.ReadInt32s(32);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32s(Event21PartIndices);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    Event21PartNames = new string[Event21PartIndices.Length];
                    for (int i = 0; i < Event21PartIndices.Length; i++)
                        Event21PartNames[i] = GetName(entries.Parts, Event21PartIndices[i]);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    Event21PartIndices = new int[Event21PartNames.Length];
                    for (int i = 0; i < Event21PartNames.Length; i++)
                        Event21PartIndices[i] = GetIndex(entries.Parts, Event21PartNames[i]);
                }
            }

            public class PartsGroup : Event
            {
                public override EventType Type => EventType.PartsGroup;

                internal override bool HasTypeData => false;

                public PartsGroup() : base() { }

                internal PartsGroup(BinaryReaderEx br) : base(br) { }
            }

            public class Event23 : Event
            {
                public override EventType Type => EventType.Event23;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                public int[] UnkT04 { get; private set; }

                public int[] UnkT24 { get; private set; }

                public short UnkT44 { get; set; }

                public short UnkT46 { get; set; }

                public int UnkT48 { get; set; }

                public Event23() : base()
                {
                    UnkT04 = new int[8];
                    UnkT24 = new int[8];
                }

                internal Event23(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32s(8);
                    UnkT24 = br.ReadInt32s(8);
                    UnkT44 = br.ReadInt16();
                    UnkT46 = br.ReadInt16();
                    UnkT48 = br.ReadInt32();
                    br.AssertNull(0x34, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32s(UnkT04);
                    bw.WriteInt32s(UnkT24);
                    bw.WriteInt16(UnkT44);
                    bw.WriteInt16(UnkT46);
                    bw.WriteInt32(UnkT48);
                    bw.WriteNull(0x34, false);
                }
            }

            public class AutoDrawGroup : Event
            {
                public override EventType Type => EventType.AutoDrawGroup;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                public int UnkT04 { get; set; }

                public AutoDrawGroup() : base() { }

                internal AutoDrawGroup(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertNull(0x18, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteNull(0x18, false);
                }
            }

            public class Other : Event
            {
                public override EventType Type => EventType.Other;

                internal override bool HasTypeData => false;

                public Other() : base() { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
