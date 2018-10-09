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
            public int PointIndex;
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
                PointIndex = br.ReadInt32();
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
                bw.WriteInt32(PointIndex);
                bw.WriteInt32(EventEntityID);
                bw.WriteInt32(0);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteSpecific(bw);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw);

            internal virtual void GetNames(MSB64 msb, Entries entries)
            {
                PartName = GetName(entries.Parts, partIndex);
            }

            internal virtual void GetIndices(MSB64 msb, Entries entries)
            {
                partIndex = GetIndex(entries.Parts, PartName);
            }

            public override string ToString()
            {
                return $"{Type} {ID} : {Name}";
            }

            public class Treasure : Event
            {
                internal override EventType Type => EventType.Treasures;

                private int partIndex2;
                public string PartName2;
                public int ItemLot1, ItemLot2;
                public int Unk3C, Unk40;

                public Treasure(BinaryReaderEx br) : base(br) { }

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
                    Unk3C = br.ReadInt32();
                    Unk40 = br.ReadInt32();
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
                    bw.WriteInt32(Unk3C);
                    bw.WriteInt32(Unk40);
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

                public int UnkT00, UnkT04, UnkT08, UnkT0C, UnkT10, UnkT14, UnkT18;
                public int UnkT30, UnkT34, UnkT38, UnkT3C, UnkT40, UnkT44, UnkT48, UnkT4C;
                public int UnkT60, UnkT64, UnkT68, UnkT6C, UnkT70, UnkT74, UnkT78, UnkT7C, UnkT80, UnkT84, UnkT88, UnkT8C;
                public int UnkT9C, UnkTA0;

                public Generators(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadInt32();
                    UnkT38 = br.ReadInt32();
                    UnkT3C = br.ReadInt32();
                    UnkT40 = br.ReadInt32();
                    UnkT44 = br.ReadInt32();
                    UnkT48 = br.ReadInt32();
                    UnkT4C = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT60 = br.ReadInt32();
                    UnkT64 = br.ReadInt32();
                    UnkT68 = br.ReadInt32();
                    UnkT6C = br.ReadInt32();
                    UnkT70 = br.ReadInt32();
                    UnkT74 = br.ReadInt32();
                    UnkT78 = br.ReadInt32();
                    UnkT7C = br.ReadInt32();
                    UnkT80 = br.ReadInt32();
                    UnkT84 = br.ReadInt32();
                    UnkT88 = br.ReadInt32();
                    UnkT8C = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    UnkT9C = br.ReadInt32();
                    UnkTA0 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
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
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32(UnkT38);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteInt32(UnkT40);
                    bw.WriteInt32(UnkT44);
                    bw.WriteInt32(UnkT48);
                    bw.WriteInt32(UnkT4C);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT60);
                    bw.WriteInt32(UnkT64);
                    bw.WriteInt32(UnkT68);
                    bw.WriteInt32(UnkT6C);
                    bw.WriteInt32(UnkT70);
                    bw.WriteInt32(UnkT74);
                    bw.WriteInt32(UnkT78);
                    bw.WriteInt32(UnkT7C);
                    bw.WriteInt32(UnkT80);
                    bw.WriteInt32(UnkT84);
                    bw.WriteInt32(UnkT88);
                    bw.WriteInt32(UnkT8C);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT9C);
                    bw.WriteInt32(UnkTA0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class ObjAct : Event
            {
                internal override EventType Type => EventType.ObjActs;

                public int ObjActEntityID;
                private int partIndex2;
                public string PartName2;
                public int ParameterID;
                public int Unk1;
                public int EventFlagID;

                public ObjAct(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    ObjActEntityID = br.ReadInt32();
                    partIndex2 = br.ReadInt32();
                    ParameterID = br.ReadInt32();
                    Unk1 = br.ReadInt32();
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
                    bw.WriteInt32(Unk1);
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

                public MapOffset(BinaryReaderEx br) : base(br) { }

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
                public int Unk4;

                public Invasion(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    HostEventEntityID = br.ReadInt32();
                    InvasionEventEntityID = br.ReadInt32();
                    InvasionRegionIndex = br.ReadInt32();
                    SoundIDMaybe = br.ReadInt32();
                    MapEventIDMaybe = br.ReadInt32();
                    FlagsMaybe = br.ReadInt32();
                    Unk4 = br.ReadInt32();
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
                    bw.WriteInt32(Unk4);
                    bw.WriteInt32(0);
                }
            }

            public class WalkRoute : Event
            {
                internal override EventType Type => EventType.WalkRoute;

                public byte[] Unk;

                public WalkRoute(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    Unk = br.ReadBytes(0x50);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Unk);
                }
            }

            public class GroupTour : Event
            {
                internal override EventType Type => EventType.GroupTour;

                public byte[] Unk;

                public GroupTour(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    Unk = br.ReadBytes(0x90);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Unk);
                }
            }

            // This type only appears once in one unused MSB so it's hard to draw too many conclusions from it.
            public class Other : Event
            {
                internal override EventType Type => EventType.Other;

                public int SoundTypeMaybe;
                public int SoundIDMaybe;

                public Other(BinaryReaderEx br) : base(br) { }

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
