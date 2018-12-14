using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// Events controlling various interactive or dynamic features in the map.
        /// </summary>
        public class EventSection : Section<Event>
        {
            internal override string Type => "EVENT_PARAM_ST";

            /// <summary>
            /// Treasures in the MSB.
            /// </summary>
            public List<Event.Treasure> Treasures;

            /// <summary>
            /// Generators in the MSB.
            /// </summary>
            public List<Event.Generator> Generators;

            /// <summary>
            /// Object actions in the MSB.
            /// </summary>
            public List<Event.ObjAct> ObjActs;

            /// <summary>
            /// Map offsets in the MSB.
            /// </summary>
            public List<Event.MapOffset> MapOffsets;

            /// <summary>
            /// Invasions in the MSB.
            /// </summary>
            public List<Event.Invasion> Invasions;

            /// <summary>
            /// Walk routes in the MSB.
            /// </summary>
            public List<Event.WalkRoute> WalkRoutes;

            /// <summary>
            /// Group tours in the MSB.
            /// </summary>
            public List<Event.GroupTour> GroupTours;

            /// <summary>
            /// Other events in the MSB.
            /// </summary>
            public List<Event.Other> Others;

            /// <summary>
            /// Creates a new EventSection with no events.
            /// </summary>
            public EventSection(int unk1 = 3) : base(unk1)
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                ObjActs = new List<Event.ObjAct>();
                MapOffsets = new List<Event.MapOffset>();
                Invasions = new List<Event.Invasion>();
                WalkRoutes = new List<Event.WalkRoute>();
                GroupTours = new List<Event.GroupTour>();
                Others = new List<Event.Other>();
            }

            /// <summary>
            /// Returns every Event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries()
            {
                return SFUtil.ConcatAll<Event>(
                    Treasures, Generators, ObjActs, MapOffsets, Invasions, WalkRoutes, GroupTours, Others);
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

                    case EventType.PseudoMultiplayer:
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

            internal void GetNames(MSB3 msb, Entries entries)
            {
                foreach (Event ev in entries.Events)
                    ev.GetNames(msb, entries);
            }

            internal void GetIndices(MSB3 msb, Entries entries)
            {
                foreach (Event ev in entries.Events)
                    ev.GetIndices(msb, entries);
            }
        }

        internal enum EventType : uint
        {
            Light = 0x0,
            Sound = 0x1,
            SFX = 0x2,
            WindSFX = 0x3,
            Treasure = 0x4,
            Generator = 0x5,
            Message = 0x6,
            ObjAct = 0x7,
            SpawnPoint = 0x8,
            MapOffset = 0x9,
            Navimesh = 0xA,
            Environment = 0xB,
            PseudoMultiplayer = 0xC,
            // Mystery = 0xD,
            WalkRoute = 0xE,
            GroupTour = 0xF,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// An interactive or dynamic feature of the map.
        /// </summary>
        public abstract class Event : Entry
        {
            internal abstract EventType Type { get; }

            /// <summary>
            /// The name of this event.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EventIndex;

            /// <summary>
            /// The ID of this event.
            /// </summary>
            public int ID;

            private int partIndex;
            /// <summary>
            /// The name of a part the event is attached to.
            /// </summary>
            public string PartName;

            private int pointIndex;
            /// <summary>
            /// The name of a region the event is attached to.
            /// </summary>
            public string PointName;

            /// <summary>
            /// Used to identify the event in event scripts.
            /// </summary>
            public int EventEntityID;

            internal Event(int id, string name)
            {
                ID = id;
                Name = name;
                EventIndex = -1;
                PartName = null;
                PointName = null;
                EventEntityID = -1;
            }

            internal Event(Event clone)
            {
                Name = clone.Name;
                EventIndex = clone.EventIndex;
                ID = clone.ID;
                PartName = clone.PartName;
                PointName = clone.PointName;
                EventEntityID = clone.EventEntityID;
            }

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

            internal virtual void GetNames(MSB3 msb, Entries entries)
            {
                PartName = GetName(entries.Parts, partIndex);
                PointName = GetName(entries.Regions, pointIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries)
            {
                partIndex = GetIndex(entries.Parts, PartName);
                pointIndex = GetIndex(entries.Regions, PointName);
            }

            /// <summary>
            /// Returns the type, ID, and name of this event.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {ID} : {Name}";
            }

            /// <summary>
            /// A pickuppable item.
            /// </summary>
            public class Treasure : Event
            {
                internal override EventType Type => EventType.Treasure;

                private int partIndex2;
                /// <summary>
                /// The part the treasure is attached to.
                /// </summary>
                public string PartName2;

                /// <summary>
                /// IDs in the item lot param given by this treasure.
                /// </summary>
                public int ItemLot1, ItemLot2;

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID;

                /// <summary>
                /// Used for treasures inside chests, exact significance unknown.
                /// </summary>
                public bool InChest;

                /// <summary>
                /// Used only for Yoel's ashes treasure; in DS1, used for corpses in barrels.
                /// </summary>
                public bool StartDisabled;

                /// <summary>
                /// Creates a new Treasure with the given ID and name.
                /// </summary>
                public Treasure(int id, string name) : base(id, name)
                {
                    PartName2 = null;
                    ItemLot1 = -1;
                    ItemLot2 = -1;
                    PickupAnimID = -1;
                    InChest = false;
                    StartDisabled = false;
                }

                /// <summary>
                /// Creates a new Treasure with values copied from another.
                /// </summary>
                public Treasure(Treasure clone) : base(clone)
                {
                    PartName2 = clone.PartName2;
                    ItemLot1 = clone.ItemLot1;
                    ItemLot2 = clone.ItemLot2;
                    PickupAnimID = clone.PickupAnimID;
                    InChest = clone.InChest;
                    StartDisabled = clone.StartDisabled;
                }

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

                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
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
                    bw.WriteInt32(PickupAnimID);

                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PartName2 = GetName(entries.Parts, partIndex2);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    partIndex2 = GetIndex(entries.Parts, PartName2);
                }
            }

            /// <summary>
            /// A continuous enemy spawner.
            /// </summary>
            public class Generator : Event
            {
                internal override EventType Type => EventType.Generator;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MaxNum;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LimitNum;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MinGenNum;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MaxGenNum;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MinInterval;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MaxInterval;

                private int[] spawnPointIndices;
                /// <summary>
                /// Regions that enemies can be spawned at.
                /// </summary>
                public string[] SpawnPointNames { get; private set; }

                private int[] spawnPartIndices;
                /// <summary>
                /// Enemies spawned by this generator.
                /// </summary>
                public string[] SpawnPartNames { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14, UnkT18;

                /// <summary>
                /// Creates a new Generator with the given ID and name.
                /// </summary>
                public Generator(int id, string name) : base(id, name)
                {
                    MaxNum = 0;
                    LimitNum = 0;
                    MinGenNum = 0;
                    MaxGenNum = 0;
                    MinInterval = 0;
                    MaxInterval = 0;
                    UnkT10 = 0;
                    UnkT14 = 0;
                    UnkT18 = 0;
                    SpawnPointNames = new string[8];
                    SpawnPartNames = new string[32];
                }

                /// <summary>
                /// Creates a new Generator with values copied from another.
                /// </summary>
                public Generator(Generator clone) : base(clone)
                {
                    MaxNum = clone.MaxNum;
                    LimitNum = clone.LimitNum;
                    MinGenNum = clone.MinGenNum;
                    MaxGenNum = clone.MaxGenNum;
                    MinInterval = clone.MinInterval;
                    MaxInterval = clone.MaxInterval;
                    UnkT10 = clone.UnkT10;
                    UnkT14 = clone.UnkT14;
                    UnkT18 = clone.UnkT18;
                    SpawnPointNames = (string[])clone.SpawnPointNames.Clone();
                    SpawnPartNames = (string[])clone.SpawnPartNames.Clone();
                }

                internal Generator(BinaryReaderEx br) : base(br) { }

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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointNames = new string[spawnPointIndices.Length];
                    for (int i = 0; i < spawnPointIndices.Length; i++)
                        SpawnPointNames[i] = GetName(entries.Regions, spawnPointIndices[i]);

                    SpawnPartNames = new string[spawnPartIndices.Length];
                    for (int i = 0; i < spawnPartIndices.Length; i++)
                        SpawnPartNames[i] = GetName(entries.Parts, spawnPartIndices[i]);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
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

            /// <summary>
            /// Controls usable objects like levers.
            /// </summary>
            public class ObjAct : Event
            {
                internal override EventType Type => EventType.ObjAct;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ObjActEntityID;

                private int partIndex2;
                /// <summary>
                /// Unknown.
                /// </summary>
                public string PartName2;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ParameterID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagID;

                /// <summary>
                /// Creates a new ObjAct with the given ID and name.
                /// </summary>
                public ObjAct(int id, string name) : base(id, name)
                {
                    ObjActEntityID = -1;
                    PartName2 = null;
                    ParameterID = 0;
                    UnkT10 = 0;
                    EventFlagID = 0;
                }

                /// <summary>
                /// Creates a new ObjAct with values copied from another.
                /// </summary>
                public ObjAct(ObjAct clone) : base(clone)
                {
                    ObjActEntityID = clone.ObjActEntityID;
                    PartName2 = clone.PartName2;
                    ParameterID = clone.ParameterID;
                    UnkT10 = clone.UnkT10;
                    EventFlagID = clone.EventFlagID;
                }

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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PartName2 = GetName(entries.Parts, partIndex2);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    partIndex2 = GetIndex(entries.Parts, PartName2);
                }
            }

            /// <summary>
            /// Moves all of the map pieces when cutscenes are played.
            /// </summary>
            public class MapOffset : Event
            {
                internal override EventType Type => EventType.MapOffset;

                /// <summary>
                /// Position of the map offset.
                /// </summary>
                public Vector3 Position;

                /// <summary>
                /// Rotation of the map offset.
                /// </summary>
                public float Degree;

                /// <summary>
                /// Creates a new MapOffset with the given ID and name.
                /// </summary>
                public MapOffset(int id, string name) : base(id, name)
                {
                    Position = Vector3.Zero;
                    Degree = 0;
                }

                /// <summary>
                /// Creates a new MapOffset with values copied from another.
                /// </summary>
                public MapOffset(MapOffset clone) : base(clone)
                {
                    Position = clone.Position;
                    Degree = clone.Degree;
                }

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

            /// <summary>
            /// A fake multiplayer interaction where the player goes to an NPC's world.
            /// </summary>
            public class Invasion : Event
            {
                internal override EventType Type => EventType.PseudoMultiplayer;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int HostEventEntityID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int InvasionEventEntityID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int InvasionRegionIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int SoundIDMaybe;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MapEventIDMaybe;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int FlagsMaybe;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18;

                /// <summary>
                /// Creates a new Invasion with the given ID and name.
                /// </summary>
                public Invasion(int id, string name) : base(id, name)
                {
                    HostEventEntityID = -1;
                    InvasionEventEntityID = -1;
                    InvasionRegionIndex = -1;
                    SoundIDMaybe = -1;
                    MapEventIDMaybe = -1;
                    FlagsMaybe = 0;
                    UnkT18 = 0;
                }

                /// <summary>
                /// Creates a new Invasion with values copied from another.
                /// </summary>
                public Invasion(Invasion clone) : base(clone)
                {
                    HostEventEntityID = clone.HostEventEntityID;
                    InvasionEventEntityID = clone.InvasionEventEntityID;
                    InvasionRegionIndex = clone.InvasionRegionIndex;
                    SoundIDMaybe = clone.SoundIDMaybe;
                    MapEventIDMaybe = clone.MapEventIDMaybe;
                    FlagsMaybe = clone.FlagsMaybe;
                    UnkT18 = clone.UnkT18;
                }

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

                /// <summary>
                /// Creates a new WalkRoute with the given ID and name.
                /// </summary>
                public WalkRoute(int id, string name) : base(id, name)
                {
                    UnkT00 = 0;
                    WalkPointNames = new string[32];
                }

                /// <summary>
                /// Creates a new WalkRoute with values copied from another.
                /// </summary>
                public WalkRoute(WalkRoute clone) : base(clone)
                {
                    UnkT00 = clone.UnkT00;
                    WalkPointNames = (string[])clone.WalkPointNames.Clone();
                }

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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkPointNames = new string[walkPointIndices.Length];
                    for (int i = 0; i < walkPointIndices.Length; i++)
                        WalkPointNames[i] = GetName(entries.Regions, walkPointIndices[i]);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    walkPointIndices = new short[WalkPointNames.Length];
                    for (int i = 0; i < WalkPointNames.Length; i++)
                        walkPointIndices[i] = (short)GetIndex(entries.Regions, WalkPointNames[i]);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class GroupTour : Event
            {
                internal override EventType Type => EventType.GroupTour;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00, UnkT04;

                private int[] groupPartsIndices;
                /// <summary>
                /// Unknown.
                /// </summary>
                public string[] GroupPartsNames { get; private set; }

                /// <summary>
                /// Creates a new GroupTour with the given ID and name.
                /// </summary>
                public GroupTour(int id, string name) : base(id, name)
                {
                    UnkT00 = 0;
                    UnkT04 = 0;
                    GroupPartsNames = new string[32];
                }

                /// <summary>
                /// Creates a new GroupTour with values copied from another.
                /// </summary>
                public GroupTour(GroupTour clone) : base(clone)
                {
                    UnkT00 = clone.UnkT00;
                    UnkT04 = clone.UnkT04;
                    GroupPartsNames = (string[])clone.GroupPartsNames.Clone();
                }

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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartsNames = new string[groupPartsIndices.Length];
                    for (int i = 0; i < groupPartsIndices.Length; i++)
                        GroupPartsNames[i] = GetName(entries.Parts, groupPartsIndices[i]);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    groupPartsIndices = new int[GroupPartsNames.Length];
                    for (int i = 0; i < GroupPartsNames.Length; i++)
                        groupPartsIndices[i] = GetIndex(entries.Parts, GroupPartsNames[i]);
                }
            }

            /// <summary>
            /// Unknown. Only appears once in one unused MSB so it's hard to draw too many conclusions from it.
            /// </summary>
            public class Other : Event
            {
                internal override EventType Type => EventType.Other;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int SoundTypeMaybe;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int SoundIDMaybe;

                /// <summary>
                /// Creates a new Other with the given ID and name.
                /// </summary>
                public Other(int id, string name) : base(id, name)
                {
                    SoundTypeMaybe = 0;
                    SoundIDMaybe = 0;
                }

                /// <summary>
                /// Creates a new Other with values copied from another.
                /// </summary>
                public Other(Other clone) : base(clone)
                {
                    SoundTypeMaybe = clone.SoundTypeMaybe;
                    SoundIDMaybe = clone.SoundIDMaybe;
                }

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
