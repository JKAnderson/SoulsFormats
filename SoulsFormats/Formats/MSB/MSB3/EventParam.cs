using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
    {
        internal enum EventType : uint
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
            Navimesh = 10,
            Environment = 11,
            PseudoMultiplayer = 12,
            Unk0D = 13,
            WalkRoute = 14,
            GroupTour = 15,
            Unk10 = 16,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Events controlling various interactive or dynamic features in the map.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            internal override int Version => 3;
            internal override string Type => "EVENT_PARAM_ST";

            /// <summary>
            /// Treasures in the MSB.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Generators in the MSB.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// Object actions in the MSB.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Map offsets in the MSB.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Pseudo multiplayer events in the MSB.
            /// </summary>
            public List<Event.PseudoMultiplayer> PseudoMultiplayers { get; set; }

            /// <summary>
            /// Walk routes in the MSB.
            /// </summary>
            public List<Event.WalkRoute> WalkRoutes { get; set; }

            /// <summary>
            /// Group tours in the MSB.
            /// </summary>
            public List<Event.GroupTour> GroupTours { get; set; }

            /// <summary>
            /// Other events in the MSB.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates a new EventParam with no events.
            /// </summary>
            public EventParam()
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                ObjActs = new List<Event.ObjAct>();
                MapOffsets = new List<Event.MapOffset>();
                PseudoMultiplayers = new List<Event.PseudoMultiplayer>();
                WalkRoutes = new List<Event.WalkRoute>();
                GroupTours = new List<Event.GroupTour>();
                Others = new List<Event.Other>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Treasure e: Treasures.Add(e); break;
                    case Event.Generator e: Generators.Add(e); break;
                    case Event.ObjAct e: ObjActs.Add(e); break;
                    case Event.MapOffset e: MapOffsets.Add(e); break;
                    case Event.PseudoMultiplayer e: PseudoMultiplayers.Add(e); break;
                    case Event.WalkRoute e: WalkRoutes.Add(e); break;
                    case Event.GroupTour e: GroupTours.Add(e); break;
                    case Event.Other e: Others.Add(e); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
                }
                return evnt;
            }
            IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => Add((Event)item);

            /// <summary>
            /// Returns every Event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries()
            {
                return SFUtil.ConcatAll<Event>(
                    Treasures, Generators, ObjActs, MapOffsets, PseudoMultiplayers,
                    WalkRoutes, GroupTours, Others);
            }
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                switch (type)
                {
                    case EventType.Treasure:
                        return Treasures.EchoAdd(new Event.Treasure(br));

                    case EventType.Generator:
                        return Generators.EchoAdd(new Event.Generator(br));

                    case EventType.ObjAct:
                        return ObjActs.EchoAdd(new Event.ObjAct(br));

                    case EventType.MapOffset:
                        return MapOffsets.EchoAdd(new Event.MapOffset(br));

                    case EventType.PseudoMultiplayer:
                        return PseudoMultiplayers.EchoAdd(new Event.PseudoMultiplayer(br));

                    case EventType.WalkRoute:
                        return WalkRoutes.EchoAdd(new Event.WalkRoute(br));

                    case EventType.GroupTour:
                        return GroupTours.EchoAdd(new Event.GroupTour(br));

                    case EventType.Other:
                        return Others.EchoAdd(new Event.Other(br));

                    default:
                        throw new NotImplementedException($"Unsupported event type: {type}");
                }
            }
        }

        /// <summary>
        /// An interactive or dynamic feature of the map.
        /// </summary>
        public abstract class Event : NamedEntry, IMsbEvent
        {
            internal abstract EventType Type { get; }

            /// <summary>
            /// The name of this event.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// The name of a part the event is attached to.
            /// </summary>
            public string PartName { get; set; }
            private int PartIndex;

            /// <summary>
            /// The name of a region the event is attached to.
            /// </summary>
            public string PointName { get; set; }
            private int PointIndex;

            /// <summary>
            /// Used to identify the event in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            private protected Event(string name)
            {
                Name = name;
                EventID = -1;
                EntityID = -1;
            }

            private protected Event(Event clone)
            {
                Name = clone.Name;
                EventID = clone.EventID;
                PartName = clone.PartName;
                PointName = clone.PointName;
                EntityID = clone.EntityID;
            }

            private protected Event(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                EventID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                br.AssertInt32(0);
                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (baseDataOffset == 0)
                    throw new InvalidDataException($"{nameof(baseDataOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + baseDataOffset;
                PartIndex = br.ReadInt32();
                PointIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertInt32(0);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventID);
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
                bw.WriteInt32(PointIndex);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(0);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB3 msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                PointName = MSB.FindName(entries.Regions, PointIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries)
            {
                PartIndex = MSB.FindIndex(entries.Parts, PartName);
                PointIndex = MSB.FindIndex(entries.Regions, PointName);
            }

            /// <summary>
            /// Returns the type and name of this event.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} : {Name}";
            }

            /// <summary>
            /// A pickuppable item.
            /// </summary>
            public class Treasure : Event
            {
                internal override EventType Type => EventType.Treasure;

                /// <summary>
                /// The part the treasure is attached to.
                /// </summary>
                public string PartName2 { get; set; }
                private int PartIndex2;

                /// <summary>
                /// First item lot given by this treasure.
                /// </summary>
                public int ItemLot1 { get; set; }

                /// <summary>
                /// Second item lot given by this treasure; rarely used.
                /// </summary>
                public int ItemLot2 { get; set; }

                /// <summary>
                /// Unknown; always -1 in vanilla.
                /// </summary>
                public int ActionButtonParamID { get; set; }

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID { get; set; }

                /// <summary>
                /// Used for treasures inside chests, exact significance unknown.
                /// </summary>
                public bool InChest { get; set; }

                /// <summary>
                /// Used only for Yoel's ashes treasure; in DS1, used for corpses in barrels.
                /// </summary>
                public bool StartDisabled { get; set; }

                /// <summary>
                /// Creates a Treasure with default values.
                /// </summary>
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}")
                {
                    ItemLot1 = -1;
                    ItemLot2 = -1;
                    ActionButtonParamID = -1;
                    PickupAnimID = -1;
                }

                /// <summary>
                /// Creates a new Treasure with values copied from another.
                /// </summary>
                public Treasure(Treasure clone) : base(clone)
                {
                    PartName2 = clone.PartName2;
                    ItemLot1 = clone.ItemLot1;
                    ItemLot2 = clone.ItemLot2;
                    ActionButtonParamID = clone.ActionButtonParamID;
                    PickupAnimID = clone.PickupAnimID;
                    InChest = clone.InChest;
                    StartDisabled = clone.StartDisabled;
                }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    PartIndex2 = br.ReadInt32();
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
                    ActionButtonParamID = br.ReadInt32();
                    PickupAnimID = br.ReadInt32();

                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);

                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(PartIndex2);
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
                    bw.WriteInt32(ActionButtonParamID);
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
                    PartName2 = MSB.FindName(entries.Parts, PartIndex2);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    PartIndex2 = MSB.FindIndex(entries.Parts, PartName2);
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
                public short MaxNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LimitNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MinGenNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MaxGenNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MinInterval { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MaxInterval { get; set; }

                /// <summary>
                /// Regions that enemies can be spawned at.
                /// </summary>
                public string[] SpawnPointNames { get; private set; }
                private int[] SpawnPointIndices;

                /// <summary>
                /// Enemies spawned by this generator.
                /// </summary>
                public string[] SpawnPartNames { get; private set; }
                private int[] SpawnPartIndices;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int SessionCondition { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Creates a Generator with default values.
                /// </summary>
                public Generator() : base($"{nameof(Event)}: {nameof(Generator)}")
                {
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
                    SessionCondition = clone.SessionCondition;
                    UnkT14 = clone.UnkT14;
                    UnkT18 = clone.UnkT18;
                    SpawnPointNames = (string[])clone.SpawnPointNames.Clone();
                    SpawnPartNames = (string[])clone.SpawnPartNames.Clone();
                }

                internal Generator(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
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
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    SpawnPointIndices = br.ReadInt32s(8);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
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
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(SpawnPointIndices);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(SpawnPartIndices);
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
                    SpawnPointNames = MSB.FindNames(entries.Regions, SpawnPointIndices);
                    SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnPointIndices = MSB.FindIndices(entries.Regions, SpawnPointNames);
                    SpawnPartIndices = MSB.FindIndices(entries.Parts, SpawnPartNames);
                }
            }

            /// <summary>
            /// Controls usable objects like levers.
            /// </summary>
            public class ObjAct : Event
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public enum ObjActState : byte
                {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    OneState = 0,
                    DoorState = 1,
                    OneLoopState = 2,
                    OneLoopState2 = 3,
                    DoorState2 = 4,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                internal override EventType Type => EventType.ObjAct;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ObjActEntityID { get; set; }

                /// <summary>
                /// The object which is being interacted with.
                /// </summary>
                public string ObjActPartName { get; set; }
                private int ObjActPartIndex;

                /// <summary>
                /// ID in ObjActParam that configures this ObjAct.
                /// </summary>
                public int ObjActParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public ObjActState ObjActStateType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Creates an ObjAct with default values.
                /// </summary>
                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}")
                {
                    ObjActEntityID = -1;
                    ObjActStateType = ObjActState.OneState;
                }

                /// <summary>
                /// Creates a new ObjAct with values copied from another.
                /// </summary>
                public ObjAct(ObjAct clone) : base(clone)
                {
                    ObjActEntityID = clone.ObjActEntityID;
                    ObjActPartName = clone.ObjActPartName;
                    ObjActParamID = clone.ObjActParamID;
                    ObjActStateType = clone.ObjActStateType;
                    EventFlagID = clone.EventFlagID;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ObjActEntityID = br.ReadInt32();
                    ObjActPartIndex = br.ReadInt32();
                    ObjActParamID = br.ReadInt32();

                    ObjActStateType = br.ReadEnum8<ObjActState>();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);

                    EventFlagID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ObjActEntityID);
                    bw.WriteInt32(ObjActPartIndex);
                    bw.WriteInt32(ObjActParamID);

                    bw.WriteByte((byte)ObjActStateType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjActPartName = MSB.FindName(entries.Parts, ObjActPartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjActPartIndex = MSB.FindIndex(entries.Parts, ObjActPartName);
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
                public Vector3 Position { get; set; }

                /// <summary>
                /// Rotation of the map offset.
                /// </summary>
                public float Degree { get; set; }

                /// <summary>
                /// Creates a MapOffset with default values.
                /// </summary>
                public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

                /// <summary>
                /// Creates a new MapOffset with values copied from another.
                /// </summary>
                public MapOffset(MapOffset clone) : base(clone)
                {
                    Position = clone.Position;
                    Degree = clone.Degree;
                }

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Position = br.ReadVector3();
                    Degree = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteVector3(Position);
                    bw.WriteSingle(Degree);
                }
            }

            /// <summary>
            /// A fake multiplayer interaction where the player goes to an NPC's world.
            /// </summary>
            public class PseudoMultiplayer : Event
            {
                internal override EventType Type => EventType.PseudoMultiplayer;

                /// <summary>
                /// The NPC whose world you're entering.
                /// </summary>
                public int HostEntityID { get; set; }

                /// <summary>
                /// Set when inside the event's region, unset when outside it.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// ID of a goods item that is used to trigger the event.
                /// </summary>
                public int ActivateGoodsID { get; set; }

                /// <summary>
                /// Unknown; possibly a sound ID.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Unknown; possibly a map event ID.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Unknown; possibly flags.
                /// </summary>
                public int UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Creates a new Invasion with the given name.
                /// </summary>
                public PseudoMultiplayer() : base($"{nameof(Event)}: {nameof(PseudoMultiplayer)}")
                {
                    HostEntityID = -1;
                    EventFlagID = -1;
                    ActivateGoodsID = -1;
                    UnkT0C = -1;
                    UnkT10 = -1;
                }

                /// <summary>
                /// Creates an Invasion with default values.
                /// </summary>
                public PseudoMultiplayer(PseudoMultiplayer clone) : base(clone)
                {
                    HostEntityID = clone.HostEntityID;
                    EventFlagID = clone.EventFlagID;
                    ActivateGoodsID = clone.ActivateGoodsID;
                    UnkT0C = clone.UnkT0C;
                    UnkT10 = clone.UnkT10;
                    UnkT14 = clone.UnkT14;
                    UnkT18 = clone.UnkT18;
                }

                internal PseudoMultiplayer(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HostEntityID = br.ReadInt32();
                    EventFlagID = br.ReadInt32();
                    ActivateGoodsID = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(HostEntityID);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(ActivateGoodsID);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
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
                public int UnkT00 { get; set; }

                /// <summary>
                /// List of points in the route.
                /// </summary>
                public string[] WalkPointNames { get; private set; }
                private short[] WalkPointIndices;

                /// <summary>
                /// Creates a WalkRoute with default values.
                /// </summary>
                public WalkRoute() : base($"{nameof(Event)}: {nameof(WalkRoute)}")
                {
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

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.AssertInt32(0, 1, 2, 5);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    WalkPointIndices = br.ReadInt16s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(WalkPointIndices);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkPointNames = new string[WalkPointIndices.Length];
                    for (int i = 0; i < WalkPointIndices.Length; i++)
                        WalkPointNames[i] = MSB.FindName(entries.Regions, WalkPointIndices[i]);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    WalkPointIndices = new short[WalkPointNames.Length];
                    for (int i = 0; i < WalkPointNames.Length; i++)
                        WalkPointIndices[i] = (short)MSB.FindIndex(entries.Regions, WalkPointNames[i]);
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
                public int PlatoonIDScriptActivate { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int State { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public string[] GroupPartsNames { get; private set; }
                private int[] GroupPartsIndices;

                /// <summary>
                /// Creates a GroupTour with default values.
                /// </summary>
                public GroupTour() : base($"{nameof(Event)}: {nameof(GroupTour)}")
                {
                    GroupPartsNames = new string[32];
                }

                /// <summary>
                /// Creates a new GroupTour with values copied from another.
                /// </summary>
                public GroupTour(GroupTour clone) : base(clone)
                {
                    PlatoonIDScriptActivate = clone.PlatoonIDScriptActivate;
                    State = clone.State;
                    GroupPartsNames = (string[])clone.GroupPartsNames.Clone();
                }

                internal GroupTour(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlatoonIDScriptActivate = br.ReadInt32();
                    State = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    GroupPartsIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlatoonIDScriptActivate);
                    bw.WriteInt32(State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(GroupPartsIndices);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartsNames = MSB.FindNames(entries.Parts, GroupPartsIndices);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    GroupPartsIndices = MSB.FindIndices(entries.Parts, GroupPartsNames);
                }
            }

            /// <summary>
            /// Unknown. Only appears once in one unused MSB so it's hard to draw too many conclusions from it.
            /// </summary>
            public class Other : Event
            {
                internal override EventType Type => EventType.Other;

                /// <summary>
                /// Unknown; possibly a sound type.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown; possibly a sound ID.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

                /// <summary>
                /// Creates a new Other with values copied from another.
                /// </summary>
                public Other(Other clone) : base(clone)
                {
                    UnkT00 = clone.UnkT00;
                    UnkT04 = clone.UnkT04;
                }

                internal Other(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertPattern(0x40, 0xFF);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WritePattern(0x40, 0xFF);
                }
            }
        }
    }
}
