using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// A section containing points and volumes for various purposes.
        /// </summary>
        public class PointParam : Param<Region>
        {
            internal override string Type => "POINT_PARAM_ST";

            /// <summary>
            /// General regions in the MSB.
            /// </summary>
            public List<Region.General> General;

            /// <summary>
            /// Unk00 regions in the MSB.
            /// </summary>
            public List<Region.Unk00> Unk00s;

            /// <summary>
            /// InvasionPoints in the MSB.
            /// </summary>
            public List<Region.InvasionPoint> InvasionPoints;

            /// <summary>
            /// EnvironmentMapPoints in the MSB.
            /// </summary>
            public List<Region.EnvironmentMapPoint> EnvironmentMapPoints;

            /// <summary>
            /// Sound regions in the MSB.
            /// </summary>
            public List<Region.Sound> Sounds;

            /// <summary>
            /// SFX regions in the MSB.
            /// </summary>
            public List<Region.SFX> SFX;

            /// <summary>
            /// WindSFX regions in the MSB.
            /// </summary>
            public List<Region.WindSFX> WindSFX;

            /// <summary>
            /// SpawnPoints in the MSB.
            /// </summary>
            public List<Region.SpawnPoint> SpawnPoints;

            /// <summary>
            /// Messages in the MSB.
            /// </summary>
            public List<Region.Message> Messages;

            /// <summary>
            /// WalkRoute points in the MSB.
            /// </summary>
            public List<Region.WalkRoute> WalkRoutes;

            /// <summary>
            /// Unk12 regions in the MSB.
            /// </summary>
            public List<Region.Unk12> Unk12s;

            /// <summary>
            /// WarpPoints in the MSB.
            /// </summary>
            public List<Region.WarpPoint> WarpPoints;

            /// <summary>
            /// ActivationAreas in the MSB.
            /// </summary>
            public List<Region.ActivationArea> ActivationAreas;

            /// <summary>
            /// Event regions in the MSB.
            /// </summary>
            public List<Region.Event> Events;

            /// <summary>
            /// EnvironmentMapEffectBoxes in the MSB.
            /// </summary>
            public List<Region.EnvironmentMapEffectBox> EnvironmentMapEffectBoxes;

            /// <summary>
            /// WindAreas in the MSB.
            /// </summary>
            public List<Region.WindArea> WindAreas;

            /// <summary>
            /// MufflingBoxes in the MSB.
            /// </summary>
            public List<Region.MufflingBox> MufflingBoxes;

            /// <summary>
            /// MufflingPortals in the MSB.
            /// </summary>
            public List<Region.MufflingPortal> MufflingPortals;

            /// <summary>
            /// Creates a new PointParam with no regions.
            /// </summary>
            public PointParam(int unk1 = 3) : base(unk1)
            {
                General = new List<Region.General>();
                Unk00s = new List<Region.Unk00>();
                InvasionPoints = new List<Region.InvasionPoint>();
                EnvironmentMapPoints = new List<Region.EnvironmentMapPoint>();
                Sounds = new List<Region.Sound>();
                SFX = new List<Region.SFX>();
                WindSFX = new List<Region.WindSFX>();
                SpawnPoints = new List<Region.SpawnPoint>();
                Messages = new List<Region.Message>();
                WalkRoutes = new List<Region.WalkRoute>();
                Unk12s = new List<Region.Unk12>();
                WarpPoints = new List<Region.WarpPoint>();
                ActivationAreas = new List<Region.ActivationArea>();
                Events = new List<Region.Event>();
                EnvironmentMapEffectBoxes = new List<Region.EnvironmentMapEffectBox>();
                WindAreas = new List<Region.WindArea>();
                MufflingBoxes = new List<Region.MufflingBox>();
                MufflingPortals = new List<Region.MufflingPortal>();
            }

            /// <summary>
            /// Returns every region in the order they will be written.
            /// </summary>
            public override List<Region> GetEntries()
            {
                return SFUtil.ConcatAll<Region>(
                    General, Unk00s, InvasionPoints, EnvironmentMapPoints, Sounds, SFX, WindSFX, SpawnPoints, Messages,
                    WalkRoutes, Unk12s, WarpPoints, ActivationAreas, Events, EnvironmentMapEffectBoxes, WindAreas, MufflingBoxes, MufflingPortals);
            }

            internal override Region ReadEntry(BinaryReaderEx br)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 0x8);

                switch (type)
                {
                    case RegionType.General:
                        var general = new Region.General(br);
                        General.Add(general);
                        return general;

                    case RegionType.Unk00:
                        var unk00 = new Region.Unk00(br);
                        Unk00s.Add(unk00);
                        return unk00;

                    case RegionType.InvasionPoint:
                        var invasion = new Region.InvasionPoint(br);
                        InvasionPoints.Add(invasion);
                        return invasion;

                    case RegionType.EnvironmentMapPoint:
                        var envMapPoint = new Region.EnvironmentMapPoint(br);
                        EnvironmentMapPoints.Add(envMapPoint);
                        return envMapPoint;

                    case RegionType.Sound:
                        var sound = new Region.Sound(br);
                        Sounds.Add(sound);
                        return sound;

                    case RegionType.SFX:
                        var sfx = new Region.SFX(br);
                        SFX.Add(sfx);
                        return sfx;

                    case RegionType.WindSFX:
                        var windSFX = new Region.WindSFX(br);
                        WindSFX.Add(windSFX);
                        return windSFX;

                    case RegionType.SpawnPoint:
                        var spawnPoint = new Region.SpawnPoint(br);
                        SpawnPoints.Add(spawnPoint);
                        return spawnPoint;

                    case RegionType.Message:
                        var message = new Region.Message(br);
                        Messages.Add(message);
                        return message;

                    case RegionType.WalkRoute:
                        var walkRoute = new Region.WalkRoute(br);
                        WalkRoutes.Add(walkRoute);
                        return walkRoute;

                    case RegionType.Unk12:
                        var unk12 = new Region.Unk12(br);
                        Unk12s.Add(unk12);
                        return unk12;

                    case RegionType.WarpPoint:
                        var warpPoint = new Region.WarpPoint(br);
                        WarpPoints.Add(warpPoint);
                        return warpPoint;

                    case RegionType.ActivationArea:
                        var activationArea = new Region.ActivationArea(br);
                        ActivationAreas.Add(activationArea);
                        return activationArea;

                    case RegionType.Event:
                        var ev = new Region.Event(br);
                        Events.Add(ev);
                        return ev;

                    case RegionType.EnvironmentMapEffectBox:
                        var envMapEffectBox = new Region.EnvironmentMapEffectBox(br);
                        EnvironmentMapEffectBoxes.Add(envMapEffectBox);
                        return envMapEffectBox;

                    case RegionType.WindArea:
                        var windArea = new Region.WindArea(br);
                        WindAreas.Add(windArea);
                        return windArea;

                    case RegionType.MufflingBox:
                        var muffBox = new Region.MufflingBox(br);
                        MufflingBoxes.Add(muffBox);
                        return muffBox;

                    case RegionType.MufflingPortal:
                        var muffPortal = new Region.MufflingPortal(br);
                        MufflingPortals.Add(muffPortal);
                        return muffPortal;

                    default:
                        throw new NotImplementedException($"Unsupported region type: {type}");
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<Region> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    entries[i].Write(bw);
                }
            }

            internal void GetNames(MSB3 msb, Entries entries)
            {
                foreach (Region region in entries.Regions)
                    region.GetNames(msb, entries);
            }

            internal void GetIndices(MSB3 msb, Entries entries)
            {
                foreach (Region region in entries.Regions)
                    region.GetIndices(msb, entries);
            }
        }

        internal enum RegionType : uint
        {
            General = 0xFFFFFFFF,
            Unk00 = 0,
            InvasionPoint = 1,
            EnvironmentMapPoint = 2,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            SpawnPoint = 8,
            Message = 9,
            WalkRoute = 11,
            Unk12 = 12,
            WarpPoint = 13,
            ActivationArea = 14,
            Event = 15,
            EnvironmentMapEffectBox = 17,
            WindArea = 18,
            MufflingBox = 20,
            MufflingPortal = 21,
        }

        /// <summary>
        /// A point or volumetric area used for a variety of purposes.
        /// </summary>
        public abstract class Region : Entry
        {
            internal abstract RegionType Type { get; }

            /// <summary>
            /// The name of this region.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Whether this region has additional type data. The only region type where this actually varies is Sound.
            /// </summary>
            public bool HasTypeData;

            /// <summary>
            /// The ID of this region.
            /// </summary>
            public int ID;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk2, Unk3, Unk4;

            /// <summary>
            /// The shape of this region.
            /// </summary>
            public Shape Shape;

            /// <summary>
            /// Controls whether the event is present in different ceremonies. Maybe only used for Messages?
            /// </summary>
            public uint MapStudioLayer;

            /// <summary>
            /// Center of the region.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation;

            private int ActivationPartIndex;
            /// <summary>
            /// Region is inactive unless this part is drawn; null for always active.
            /// </summary>
            public string ActivationPartName;

            /// <summary>
            /// An ID used to identify this region in event scripts.
            /// </summary>
            public int EventEntityID;

            internal Region(int id, string name, bool hasTypeData)
            {
                ID = id;
                Name = name;
                Position = Vector3.Zero;
                Rotation = Vector3.Zero;
                Shape = new Shape.Point();
                ActivationPartName = null;
                EventEntityID = -1;
                Unk2 = 0;
                Unk3 = 0;
                Unk4 = 0;
                MapStudioLayer = 0;
                HasTypeData = hasTypeData;
            }

            internal Region(Region clone)
            {
                Name = clone.Name;
                ID = clone.ID;
                Position = clone.Position;
                Rotation = clone.Rotation;
                Shape = clone.Shape.Clone();
                ActivationPartName = clone.ActivationPartName;
                EventEntityID = clone.EventEntityID;
                Unk2 = clone.Unk2;
                Unk3 = clone.Unk3;
                Unk4 = clone.Unk4;
                MapStudioLayer = clone.MapStudioLayer;
                HasTypeData = clone.HasTypeData;
            }

            internal Region(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                ID = br.ReadInt32();
                ShapeType shapeType = br.ReadEnum32<ShapeType>();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk2 = br.ReadInt32();

                Name = br.GetUTF16(start + nameOffset);

                long baseDataOffset1 = br.ReadInt64();
                br.StepIn(start + baseDataOffset1);
                Unk3 = br.ReadInt32();
                br.StepOut();

                long baseDataOffset2 = br.AssertInt64(baseDataOffset1 + 4);
                br.StepIn(start + baseDataOffset2);
                Unk4 = br.ReadInt32();

                // These padding asserts are just for my peace of mind
                if (br.Position % 8 != 0)
                    br.AssertInt32(0);
                br.StepOut();

                br.AssertInt32(-1);
                MapStudioLayer = br.ReadUInt32();

                // This will be 0 for points, but that's fine
                long shapeDataOffset = br.ReadInt64();
                br.StepIn(start + shapeDataOffset);
                switch (shapeType)
                {
                    case ShapeType.Point:
                        Shape = new Shape.Point();
                        break;

                    case ShapeType.Circle:
                        Shape = new Shape.Circle(br);
                        break;

                    case ShapeType.Sphere:
                        Shape = new Shape.Sphere(br);
                        break;

                    case ShapeType.Cylinder:
                        Shape = new Shape.Cylinder(br);
                        break;

                    case ShapeType.Box:
                        Shape = new Shape.Box(br);
                        break;

                    default:
                        throw new NotImplementedException($"Unsupported shape type: {shapeType}");
                }
                br.StepOut();

                long baseDataOffset3 = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                br.StepIn(start + baseDataOffset3);
                ActivationPartIndex = br.ReadInt32();
                EventEntityID = br.ReadInt32();

                HasTypeData = typeDataOffset != 0 || Type == RegionType.MufflingBox;
                if (HasTypeData)
                    ReadSpecific(br);

                if (br.Position % 8 != 0)
                    br.AssertInt32(0);

                br.StepOut();
            }

            internal abstract void ReadSpecific(BinaryReaderEx br);

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(ID);
                bw.WriteUInt32((uint)Shape.Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk2);

                bw.ReserveInt64("BaseDataOffset1");
                bw.ReserveInt64("BaseDataOffset2");

                bw.WriteInt32(-1);
                bw.WriteUInt32(MapStudioLayer);

                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("BaseDataOffset3");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(ReambiguateName(Name), true);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset1", bw.Position - start);
                bw.WriteInt32(Unk3);

                bw.FillInt64("BaseDataOffset2", bw.Position - start);
                bw.WriteInt32(Unk4);
                bw.Pad(8);

                Shape.Write(bw, start);

                bw.FillInt64("BaseDataOffset3", bw.Position - start);
                bw.WriteInt32(ActivationPartIndex);
                bw.WriteInt32(EventEntityID);

                if (HasTypeData)
                    WriteSpecific(bw, start);
                else
                    bw.FillInt64("TypeDataOffset", 0);

                bw.Pad(8);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw, long start);

            internal virtual void GetNames(MSB3 msb, Entries entries)
            {
                ActivationPartName = GetName(entries.Parts, ActivationPartIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries)
            {
                ActivationPartIndex = GetIndex(entries.Parts, ActivationPartName);
            }

            /// <summary>
            /// Returns the region type, ID, shape type, and name of this region.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {ID} {Shape.Type} : {Name}";
            }

            /// <summary>
            /// A region type with no type data.
            /// </summary>
            public abstract class SimpleRegion : Region
            {
                internal SimpleRegion(int id, string name) : base(id, name, false) { }

                internal SimpleRegion(SimpleRegion clone) : base(clone) { }

                internal SimpleRegion(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    throw new InvalidOperationException("SimpleRegions should never have type data.");
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    throw new InvalidOperationException("SimpleRegions should never have type data.");
                }
            }

            /// <summary>
            /// Regions for random things.
            /// </summary>
            public class General : SimpleRegion
            {
                internal override RegionType Type => RegionType.General;

                /// <summary>
                /// Creates a new General with the given ID and name.
                /// </summary>
                public General(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new General region with values copied from another.
                /// </summary>
                public General(General clone) : base(clone) { }

                internal General(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; only used 3 times in Catacombs.
            /// </summary>
            public class Unk00 : SimpleRegion
            {
                internal override RegionType Type => RegionType.Unk00;

                /// <summary>
                /// Creates a new Unk00 with the given ID and name.
                /// </summary>
                public Unk00(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new Unk00 with values copied from another.
                /// </summary>
                public Unk00(General clone) : base(clone) { }

                internal Unk00(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A point where other players invade your world.
            /// </summary>
            public class InvasionPoint : Region
            {
                internal override RegionType Type => RegionType.InvasionPoint;

                /// <summary>
                /// Not sure what this does.
                /// </summary>
                public int Priority;

                /// <summary>
                /// Creates a new InvasionPoint with the given ID and name.
                /// </summary>
                public InvasionPoint(int id, string name) : base(id, name, true)
                {
                    Priority = 0;
                }

                /// <summary>
                /// Creates a new InvasionPoint with values copied from another.
                /// </summary>
                public InvasionPoint(InvasionPoint clone) : base(clone)
                {
                    Priority = clone.Priority;
                }

                internal InvasionPoint(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    Priority = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteInt32(Priority);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnvironmentMapPoint : Region
            {
                internal override RegionType Type => RegionType.EnvironmentMapPoint;

                /// <summary>
                /// Unknown. Only ever 1 bit set, so probably flags.
                /// </summary>
                public int UnkFlags;

                /// <summary>
                /// Creates a new EnvironmentMapPoint with the given ID and name.
                /// </summary>
                public EnvironmentMapPoint(int id, string name) : base(id, name, true)
                {
                    UnkFlags = 0;
                }

                /// <summary>
                /// Creates a new EnvironmentMapPoint with values copied from another.
                /// </summary>
                public EnvironmentMapPoint(EnvironmentMapPoint clone) : base(clone)
                {
                    UnkFlags = clone.UnkFlags;
                }

                internal EnvironmentMapPoint(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    UnkFlags = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteInt32(UnkFlags);
                }
            }

            /// <summary>
            /// A region that plays a sound while you're in it.
            /// </summary>
            public class Sound : Region
            {
                /// <summary>
                /// Types of sound that may be in a Sound region.
                /// </summary>
                public enum SndType : uint
                {
                    /// <summary>
                    /// Ambient sounds like wind, creaking, etc.
                    /// </summary>
                    Environment = 0,

                    /// <summary>
                    /// Boss fight music.
                    /// </summary>
                    BGM = 6,

                    /// <summary>
                    /// Character voices.
                    /// </summary>
                    Voice = 7,
                }

                internal override RegionType Type => RegionType.Sound;

                /// <summary>
                /// Type of sound in this region; determines mixing behavior like muffling.
                /// </summary>
                public SndType SoundType;

                /// <summary>
                /// ID of the sound to play in this region, or 0 for child regions.
                /// </summary>
                public int SoundID;

                private int[] ChildRegionIndices;

                /// <summary>
                /// Names of other Sound regions which extend this one.
                /// </summary>
                public string[] ChildRegionNames { get; private set; }

                /// <summary>
                /// Creates a new Sound with the given ID and name.
                /// </summary>
                public Sound(int id, string name) : base(id, name, true)
                {
                    SoundType = SndType.Environment;
                    SoundID = 0;
                    ChildRegionNames = new string[16];
                }

                /// <summary>
                /// Creates a new Sound region with values copied from another.
                /// </summary>
                public Sound(Sound clone) : base(clone)
                {
                    SoundType = clone.SoundType;
                    SoundID = clone.SoundID;
                    ChildRegionNames = (string[])clone.ChildRegionNames.Clone();
                }

                internal Sound(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    SoundType = br.ReadEnum32<SndType>();
                    SoundID = br.ReadInt32();
                    ChildRegionIndices = br.ReadInt32s(16);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteUInt32((uint)SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32s(ChildRegionIndices);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ChildRegionNames = new string[ChildRegionIndices.Length];
                    for (int i = 0; i < ChildRegionIndices.Length; i++)
                        ChildRegionNames[i] = GetName(entries.Regions, ChildRegionIndices[i]);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ChildRegionIndices = new int[ChildRegionNames.Length];
                    for (int i = 0; i < ChildRegionNames.Length; i++)
                        ChildRegionIndices[i] = GetIndex(entries.Regions, ChildRegionNames[i]);
                }
            }

            /// <summary>
            /// A region that plays a special effect.
            /// </summary>
            public class SFX : Region
            {
                internal override RegionType Type => RegionType.SFX;

                /// <summary>
                /// The ID of the .fxr file to play in this region.
                /// </summary>
                public int FFXID;

                /// <summary>
                /// If true, the effect is off by default until enabled by event scripts.
                /// </summary>
                public bool StartDisabled;

                /// <summary>
                /// Creates a new SFX with the given ID and name.
                /// </summary>
                public SFX(int id, string name) : base(id, name, true)
                {
                    FFXID = -1;
                    StartDisabled = false;
                }

                /// <summary>
                /// Creates a new SFX with values copied from another.
                /// </summary>
                public SFX(SFX clone) : base(clone)
                {
                    FFXID = clone.FFXID;
                    StartDisabled = clone.StartDisabled;
                }

                internal SFX(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    FFXID = br.ReadInt32();
                    // These are not additional FFX IDs, I checked
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    StartDisabled = br.AssertInt32(0, 1) == 1;
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteInt32(FFXID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(StartDisabled ? 1 : 0);
                }
            }

            /// <summary>
            /// Unknown exactly what this does.
            /// </summary>
            public class WindSFX : Region
            {
                internal override RegionType Type => RegionType.WindSFX;

                /// <summary>
                /// ID of an .fxr file.
                /// </summary>
                public int FFXID;

                private int WindAreaIndex;
                /// <summary>
                /// Name of a corresponding WindArea region.
                /// </summary>
                public string WindAreaName;

                /// <summary>
                /// Creates a new WindSFX with the given ID and name.
                /// </summary>
                public WindSFX(int id, string name) : base(id, name, true)
                {
                    FFXID = -1;
                    WindAreaName = null;
                }

                /// <summary>
                /// Creates a new WindSFX with values copied from another.
                /// </summary>
                public WindSFX(WindSFX clone) : base(clone)
                {
                    FFXID = clone.FFXID;
                    WindAreaName = clone.WindAreaName;
                }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    FFXID = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    WindAreaIndex = br.ReadInt32();
                    br.AssertSingle(-1);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteInt32(FFXID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(WindAreaIndex);
                    bw.WriteSingle(-1);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WindAreaName = GetName(entries.Regions, WindAreaIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    WindAreaIndex = GetIndex(entries.Regions, WindAreaName);
                }
            }

            /// <summary>
            /// A region where players enter the map.
            /// </summary>
            public class SpawnPoint : Region
            {
                internal override RegionType Type => RegionType.SpawnPoint;

                /// <summary>
                /// Unknown; seems kind of like a region index, but also kind of doesn't.
                /// </summary>
                public int UnkT00;

                /// <summary>
                /// Creates a new SpawnPoint with the given ID and name.
                /// </summary>
                public SpawnPoint(int id, string name) : base(id, name, true)
                {
                    UnkT00 = -1;
                }

                /// <summary>
                /// Creates a new SpawnPoint with values copied from another.
                /// </summary>
                public SpawnPoint(SpawnPoint clone) : base(clone)
                {
                    UnkT00 = clone.UnkT00;
                }

                internal SpawnPoint(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An orange developer message.
            /// </summary>
            public class Message : Region
            {
                internal override RegionType Type => RegionType.Message;

                /// <summary>
                /// ID of the message's text in the FMGs.
                /// </summary>
                public short MessageID;

                /// <summary>
                /// Unknown. Always 0 or 2.
                /// </summary>
                public short UnkT02;

                /// <summary>
                /// Whether the message requires Seek Guidance to appear.
                /// </summary>
                public bool Hidden;

                /// <summary>
                /// Creates a new Message with the given ID and name.
                /// </summary>
                public Message(int id, string name) : base(id, name, true)
                {
                    MessageID = -1;
                    UnkT02 = 0;
                    Hidden = false;
                }

                /// <summary>
                /// Creates a new Message with values copied from another.
                /// </summary>
                public Message(Message clone) : base(clone)
                {
                    MessageID = clone.MessageID;
                    UnkT02 = clone.UnkT02;
                    Hidden = clone.Hidden;
                }

                internal Message(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    MessageID = br.ReadInt16();
                    UnkT02 = br.ReadInt16();
                    Hidden = br.AssertInt32(0, 1) == 1;
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteInt16(MessageID);
                    bw.WriteInt16(UnkT02);
                    bw.WriteInt32(Hidden ? 1 : 0);
                }
            }

            /// <summary>
            /// A point in a WalkRoute.
            /// </summary>
            public class WalkRoute : SimpleRegion
            {
                internal override RegionType Type => RegionType.WalkRoute;

                /// <summary>
                /// Creates a new WalkRoute with the given ID and name.
                /// </summary>
                public WalkRoute(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new WalkRoute with values copied from another.
                /// </summary>
                public WalkRoute(General clone) : base(clone) { }

                internal WalkRoute(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unk12 : SimpleRegion
            {
                internal override RegionType Type => RegionType.Unk12;

                /// <summary>
                /// Creates a new Unk12 with the given ID and name.
                /// </summary>
                public Unk12(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new Unk12 with values copied from another.
                /// </summary>
                public Unk12(General clone) : base(clone) { }

                internal Unk12(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; seems to be used for moving enemies around.
            /// </summary>
            public class WarpPoint : SimpleRegion
            {
                internal override RegionType Type => RegionType.WarpPoint;

                /// <summary>
                /// Creates a new WarpPoint with the given ID and name.
                /// </summary>
                public WarpPoint(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new WarpPoint with values copied from another.
                /// </summary>
                public WarpPoint(General clone) : base(clone) { }

                internal WarpPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Triggers an enemy when entered.
            /// </summary>
            public class ActivationArea : SimpleRegion
            {
                internal override RegionType Type => RegionType.ActivationArea;

                /// <summary>
                /// Creates a new ActivationArea with the given ID and name.
                /// </summary>
                public ActivationArea(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new ActivationArea with values copied from another.
                /// </summary>
                public ActivationArea(General clone) : base(clone) { }

                internal ActivationArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Any kind of region for use with event scripts.
            /// </summary>
            public class Event : SimpleRegion
            {
                internal override RegionType Type => RegionType.Event;

                /// <summary>
                /// Creates a new Event with the given ID and name.
                /// </summary>
                public Event(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new Event with values copied from another.
                /// </summary>
                public Event(General clone) : base(clone) { }

                internal Event(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnvironmentMapEffectBox : Region
            {
                internal override RegionType Type => RegionType.EnvironmentMapEffectBox;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Compare;

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT08;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT09;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0A;

                /// <summary>
                /// Creates a new EnvironmentMapEffectBox with the given ID and name.
                /// </summary>
                public EnvironmentMapEffectBox(int id, string name) : base(id, name, true)
                {
                    UnkT00 = 0;
                    Compare = 0;
                    UnkT08 = false;
                    UnkT09 = 0;
                    UnkT0A = 0;
                }

                /// <summary>
                /// Creates a new EnvironmentMapEffectBox with values copied from another.
                /// </summary>
                public EnvironmentMapEffectBox(EnvironmentMapEffectBox clone) : base(clone)
                {
                    UnkT00 = clone.UnkT00;
                    Compare = clone.Compare;
                    UnkT08 = clone.UnkT08;
                    UnkT09 = clone.UnkT09;
                    UnkT0A = clone.UnkT0A;
                }

                internal EnvironmentMapEffectBox(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    Compare = br.ReadSingle();
                    UnkT08 = br.ReadBoolean();
                    UnkT09 = br.ReadByte();
                    UnkT0A = br.ReadInt16();
                    br.AssertInt32(0); // float (6)
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteSingle(UnkT00);
                    bw.WriteSingle(Compare);
                    bw.WriteBoolean(UnkT08);
                    bw.WriteByte(UnkT09);
                    bw.WriteInt16(UnkT0A);
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

            /// <summary>
            /// Unknown; each WindSFX has a reference to a WindArea.
            /// </summary>
            public class WindArea : SimpleRegion
            {
                internal override RegionType Type => RegionType.WindArea;

                /// <summary>
                /// Creates a new WindArea with the given ID and name.
                /// </summary>
                public WindArea(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new WindArea with values copied from another.
                /// </summary>
                public WindArea(General clone) : base(clone) { }

                internal WindArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Muffles environmental sound while inside it.
            /// </summary>
            public class MufflingBox : Region
            {
                internal override RegionType Type => RegionType.MufflingBox;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00;

                /// <summary>
                /// Creates a new MufflingBox with the given ID and name.
                /// </summary>
                public MufflingBox(int id, string name) : base(id, name, true)
                {
                    UnkT00 = 0;
                }

                /// <summary>
                /// Creates a new MufflingBox with values copied from another.
                /// </summary>
                public MufflingBox(MufflingBox clone) : base(clone)
                {
                    UnkT00 = clone.UnkT00;
                }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", 0);
                    bw.WriteInt32(UnkT00);
                }
            }

            /// <summary>
            /// A region leading into a MufflingBox.
            /// </summary>
            public class MufflingPortal : SimpleRegion
            {
                internal override RegionType Type => RegionType.MufflingPortal;

                /// <summary>
                /// Creates a new MufflingPortal with the given ID and name.
                /// </summary>
                public MufflingPortal(int id, string name) : base(id, name) { }

                /// <summary>
                /// Creates a new MufflingPortal with values copied from another.
                /// </summary>
                public MufflingPortal(General clone) : base(clone) { }

                internal MufflingPortal(BinaryReaderEx br) : base(br) { }
            }
        }

        /// <summary>
        /// Different shapes that regions can take.
        /// </summary>
        public enum ShapeType : uint
        {
            /// <summary>
            /// A single point.
            /// </summary>
            Point = 0,

            /// <summary>
            /// A flat circle with a radius.
            /// </summary>
            Circle = 1,

            /// <summary>
            /// A sphere with a radius.
            /// </summary>
            Sphere = 2,

            /// <summary>
            /// A cylinder with a radius and height.
            /// </summary>
            Cylinder = 3,

            /// <summary>
            /// A flat square that is never used and I haven't bothered implementing.
            /// </summary>
            Square = 4,

            /// <summary>
            /// A rectangular prism with width, depth, and height.
            /// </summary>
            Box = 5,
        }

        /// <summary>
        /// A shape taken by a region.
        /// </summary>
        public abstract class Shape
        {
            /// <summary>
            /// The type of this shape.
            /// </summary>
            public abstract ShapeType Type { get; }

            internal abstract Shape Clone();

            internal abstract void Write(BinaryWriterEx bw, long start);

            /// <summary>
            /// A single point.
            /// </summary>
            public class Point : Shape
            {
                /// <summary>
                /// The type of this shape.
                /// </summary>
                public override ShapeType Type => ShapeType.Point;

                /// <summary>
                /// Creates a new Point.
                /// </summary>
                public Point() { }

                internal override Shape Clone()
                {
                    return new Point();
                }

                internal override void Write(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("ShapeDataOffset", 0);
                }
            }

            /// <summary>
            /// A flat circle.
            /// </summary>
            public class Circle : Shape
            {
                /// <summary>
                /// The type of this shape.
                /// </summary>
                public override ShapeType Type => ShapeType.Circle;

                /// <summary>
                /// The radius of the circle.
                /// </summary>
                public float Radius;

                /// <summary>
                /// Creates a new Circle with radius 1.
                /// </summary>
                public Circle() : this(1) { }

                /// <summary>
                /// Creates a new Circle with the given radius.
                /// </summary>
                public Circle(float radius)
                {
                    Radius = radius;
                }

                /// <summary>
                /// Creates a new Circle with the radius of another.
                /// </summary>
                public Circle(Circle clone) : this(clone.Radius) { }

                internal override Shape Clone()
                {
                    return new Circle(this);
                }

                internal Circle(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                }

                internal override void Write(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    bw.WriteSingle(Radius);
                }
            }

            /// <summary>
            /// A volumetric sphere.
            /// </summary>
            public class Sphere : Shape
            {
                /// <summary>
                /// The type of this shape.
                /// </summary>
                public override ShapeType Type => ShapeType.Sphere;

                /// <summary>
                /// The radius of the sphere.
                /// </summary>
                public float Radius;

                /// <summary>
                /// Creates a new Sphere with radius 1.
                /// </summary>
                public Sphere() : this(1) { }

                /// <summary>
                /// Creates a new Sphere with the given radius.
                /// </summary>
                public Sphere(float radius)
                {
                    Radius = radius;
                }

                /// <summary>
                /// Creates a new Sphere with the radius of another.
                /// </summary>
                public Sphere(Sphere clone) : this(clone.Radius) { }

                internal override Shape Clone()
                {
                    return new Sphere(this);
                }

                internal Sphere(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                }

                internal override void Write(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    bw.WriteSingle(Radius);
                }
            }

            /// <summary>
            /// A volumetric cylinder.
            /// </summary>
            public class Cylinder : Shape
            {
                /// <summary>
                /// The type of this shape.
                /// </summary>
                public override ShapeType Type => ShapeType.Cylinder;

                /// <summary>
                /// The radius of the cylinder.
                /// </summary>
                public float Radius;

                /// <summary>
                /// The height of the cylinder.
                /// </summary>
                public float Height;

                /// <summary>
                /// Creates a new Cylinder with radius and height 1.
                /// </summary>
                public Cylinder() : this(1, 1) { }

                /// <summary>
                /// Creates a new Cylinder with the given dimensions.
                /// </summary>
                public Cylinder(float radius, float height)
                {
                    Radius = radius;
                    Height = height;
                }

                /// <summary>
                /// Creates a new Cylinder with the dimensions of another.
                /// </summary>
                public Cylinder(Cylinder clone) : this(clone.Radius, clone.Height) { }

                internal override Shape Clone()
                {
                    return new Cylinder(this);
                }

                internal Cylinder(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                    Height = br.ReadSingle();
                }

                internal override void Write(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    bw.WriteSingle(Radius);
                    bw.WriteSingle(Height);
                }
            }

            /// <summary>
            /// A rectangular prism.
            /// </summary>
            public class Box : Shape
            {
                /// <summary>
                /// The type of this shape.
                /// </summary>
                public override ShapeType Type => ShapeType.Box;

                /// <summary>
                /// The width of the box.
                /// </summary>
                public float Width;

                /// <summary>
                /// The depth of the box.
                /// </summary>
                public float Depth;

                /// <summary>
                /// The height of the box.
                /// </summary>
                public float Height;

                /// <summary>
                /// Creates a new Box with width, depth, and height 1.
                /// </summary>
                public Box() : this(1, 1, 1) { }

                /// <summary>
                /// Creates a new Box with the given dimensions.
                /// </summary>
                public Box(float width, float depth, float height)
                {
                    Width = width;
                    Depth = depth;
                    Height = height;
                }

                /// <summary>
                /// Creates a new Box with the dimensions of another.
                /// </summary>
                public Box(Box clone) : this(clone.Width, clone.Depth, clone.Height) { }

                internal override Shape Clone()
                {
                    return new Box(this);
                }

                internal Box(BinaryReaderEx br)
                {
                    Width = br.ReadSingle();
                    Depth = br.ReadSingle();
                    Height = br.ReadSingle();
                }

                internal override void Write(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    bw.WriteSingle(Width);
                    bw.WriteSingle(Depth);
                    bw.WriteSingle(Height);
                }
            }
        }
    }
}
