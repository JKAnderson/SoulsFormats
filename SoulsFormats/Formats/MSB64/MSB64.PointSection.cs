using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB64
    {
        /// <summary>
        /// A section containing points and region shapes for various purposes.
        /// </summary>
        public class PointSection : Section<Region>
        {
            /// <summary>
            /// The MSB type string for this section.
            /// </summary>
            public override string Type => "POINT_PARAM_ST";

            public List<Region.General> General;
            public List<Region.Unk00> Unk00s;
            public List<Region.InvasionPoint> InvasionPoints;
            public List<Region.EnvironmentMapPoint> EnvironmentMapPoints;
            public List<Region.Sound> Sounds;
            public List<Region.SFX> SFX;
            public List<Region.WindSFX> WindSFX;
            public List<Region.SpawnPoint> SpawnPoints;
            public List<Region.Message> Messages;
            public List<Region.WalkRoute> WalkRoutes;
            public List<Region.Unk12> Unk12s;
            public List<Region.WarpPoint> WarpPoints;
            public List<Region.ActivationArea> ActivationAreas;
            public List<Region.Event> Events;
            public List<Region.EnvironmentMapEffectBox> EnvironmentMapEffectBoxes;
            public List<Region.WindArea> WindAreas;
            public List<Region.MufflingBox> MufflingBoxes;
            public List<Region.MufflingPortal> MufflingPortals;

            internal PointSection(BinaryReaderEx br, int unk1) : base(br, unk1)
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

            public override List<Region> GetEntries()
            {
                return Util.ConcatAll<Region>(
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

            internal void GetNames(MSB64 msb, Entries entries)
            {
                foreach (Region region in entries.Regions)
                    region.GetNames(msb, entries);
            }

            internal void GetIndices(MSB64 msb, Entries entries)
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

        public abstract class Region : Entry
        {
            internal abstract RegionType Type { get; }

            public override string Name { get; set; }

            public bool HasTypeData;

            public int ID;

            public int Unk2, Unk3, Unk4;

            public Shape Shape;

            /// <summary>
            /// Not sure if this is exactly a drawgroup, but it's what makes messages not appear in dark Firelink.
            /// </summary>
            public uint DrawGroup;

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

            public int EventEntityID;

            public Region(Region clone)
            {
                Name = clone.Name;
                ID = clone.ID;
                Position = new Vector3(clone.Position.X, clone.Position.Y, clone.Position.Z);
                Rotation = new Vector3(clone.Rotation.X, clone.Rotation.Y, clone.Rotation.Z);
                Shape = clone.Shape.Clone();
                ActivationPartName = clone.ActivationPartName;
                EventEntityID = clone.EventEntityID;
                Unk2 = clone.Unk2;
                Unk3 = clone.Unk3;
                Unk4 = clone.Unk4;
                DrawGroup = clone.DrawGroup;
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
                DrawGroup = br.ReadUInt32();

                // This will be 0 for points, but that's fine
                long shapeDataOffset = br.ReadInt64();
                br.StepIn(start + shapeDataOffset);
                switch (shapeType)
                {
                    case ShapeType.Point:
                        Shape = new Shape.Point(br);
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
                bw.WriteUInt32(DrawGroup);

                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("BaseDataOffset3");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
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

            internal virtual void GetNames(MSB64 msb, Entries entries)
            {
                ActivationPartName = GetName(entries.Parts, ActivationPartIndex);
            }

            internal virtual void GetIndices(MSB64 msb, Entries entries)
            {
                ActivationPartIndex = GetIndex(entries.Parts, ActivationPartName);
            }

            public override string ToString()
            {
                return $"{Type} {ID} {Shape.Type} : {Name}";
            }

            public abstract class SimpleRegion : Region
            {
                public SimpleRegion(SimpleRegion clone) : base(clone) { }

                internal SimpleRegion(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    throw new NotImplementedException();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    throw new NotImplementedException();
                }
            }

            public class General : SimpleRegion
            {
                internal override RegionType Type => RegionType.General;
                public General(General clone) : base(clone) { }
                internal General(BinaryReaderEx br) : base(br) { }
            }

            public class Unk00 : SimpleRegion
            {
                internal override RegionType Type => RegionType.Unk00;
                public Unk00(General clone) : base(clone) { }
                internal Unk00(BinaryReaderEx br) : base(br) { }
            }

            public class InvasionPoint : Region
            {
                internal override RegionType Type => RegionType.InvasionPoint;

                /// <summary>
                /// Not sure what this does.
                /// </summary>
                public int Priority;

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

            public class EnvironmentMapPoint : Region
            {
                internal override RegionType Type => RegionType.EnvironmentMapPoint;

                /// <summary>
                /// Unknown. Only ever 1 bit set, so probably flags.
                /// </summary>
                public int UnkFlags;

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

            public class Sound : Region
            {
                public enum SndType : uint
                {
                    Environment = 0,
                    BGM = 6,
                    Voice = 7,
                }

                internal override RegionType Type => RegionType.Sound;

                internal Sound(BinaryReaderEx br) : base(br) { }

                public SndType SoundType;

                public int SoundID;

                private int[] ChildRegionIndices;

                public string[] ChildRegionNames { get; private set; }

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

                internal override void GetNames(MSB64 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ChildRegionNames = new string[ChildRegionIndices.Length];
                    for (int i = 0; i < ChildRegionIndices.Length; i++)
                        ChildRegionNames[i] = GetName(entries.Regions, ChildRegionIndices[i]);
                }

                internal override void GetIndices(MSB64 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ChildRegionIndices = new int[ChildRegionNames.Length];
                    for (int i = 0; i < ChildRegionNames.Length; i++)
                        ChildRegionIndices[i] = GetIndex(entries.Regions, ChildRegionNames[i]);
                }
            }

            public class SFX : Region
            {
                internal override RegionType Type => RegionType.SFX;

                public int FFXID;

                public bool StartDisabled;

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

            public class WindSFX : Region
            {
                internal override RegionType Type => RegionType.WindSFX;

                public int FFXID;

                private int WindAreaIndex;

                public string WindAreaName;

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

                internal override void GetNames(MSB64 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WindAreaName = GetName(entries.Regions, WindAreaIndex);
                }

                internal override void GetIndices(MSB64 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    WindAreaIndex = GetIndex(entries.Regions, WindAreaName);
                }
            }

            public class SpawnPoint : Region
            {
                internal override RegionType Type => RegionType.SpawnPoint;

                public int UnkT00;

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

            public class WalkRoute : SimpleRegion
            {
                internal override RegionType Type => RegionType.WalkRoute;
                public WalkRoute(General clone) : base(clone) { }
                internal WalkRoute(BinaryReaderEx br) : base(br) { }
            }

            public class Unk12 : SimpleRegion
            {
                internal override RegionType Type => RegionType.Unk12;
                public Unk12(General clone) : base(clone) { }
                internal Unk12(BinaryReaderEx br) : base(br) { }
            }

            public class WarpPoint : SimpleRegion
            {
                internal override RegionType Type => RegionType.WarpPoint;
                public WarpPoint(General clone) : base(clone) { }
                internal WarpPoint(BinaryReaderEx br) : base(br) { }
            }

            public class ActivationArea : SimpleRegion
            {
                internal override RegionType Type => RegionType.ActivationArea;
                public ActivationArea(General clone) : base(clone) { }
                internal ActivationArea(BinaryReaderEx br) : base(br) { }
            }

            public class Event : SimpleRegion
            {
                internal override RegionType Type => RegionType.Event;
                public Event(General clone) : base(clone) { }
                internal Event(BinaryReaderEx br) : base(br) { }
            }

            public class EnvironmentMapEffectBox : Region
            {
                internal override RegionType Type => RegionType.EnvironmentMapEffectBox;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT08, UnkT0A;

                internal EnvironmentMapEffectBox(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadInt16();
                    UnkT0A = br.ReadInt16();
                    br.AssertInt32(0);
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
                    bw.WriteInt32(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteInt16(UnkT08);
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

            public class WindArea : SimpleRegion
            {
                internal override RegionType Type => RegionType.WindArea;
                public WindArea(General clone) : base(clone) { }
                internal WindArea(BinaryReaderEx br) : base(br) { }
            }

            public class MufflingBox : Region
            {
                internal override RegionType Type => RegionType.MufflingBox;

                public int UnkT00;

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

            public class MufflingPortal : SimpleRegion
            {
                internal override RegionType Type => RegionType.MufflingPortal;
                public MufflingPortal(General clone) : base(clone) { }
                internal MufflingPortal(BinaryReaderEx br) : base(br) { }
            }
        }

        public enum ShapeType : uint
        {
            Point = 0,
            Circle = 1,
            Sphere = 2,
            Cylinder = 3,
            Square = 4,
            Box = 5,
        }

        public abstract class Shape
        {
            public abstract ShapeType Type { get; }

            public abstract Shape Clone();

            internal abstract void Write(BinaryWriterEx bw, long start);

            public class Point : Shape
            {
                public override ShapeType Type => ShapeType.Point;

                public Point() { }

                internal Point(BinaryReaderEx br) { }

                public override Shape Clone()
                {
                    return new Point();
                }

                internal override void Write(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("ShapeDataOffset", 0);
                }
            }

            public class Circle : Shape
            {
                public override ShapeType Type => ShapeType.Circle;

                /// <summary>
                /// The radius of the circle.
                /// </summary>
                public float Radius;

                public Circle() : this(1) { }

                public Circle(float radius)
                {
                    Radius = radius;
                }

                public Circle(Circle clone) : this(clone.Radius) { }

                public override Shape Clone()
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

            public class Sphere : Shape
            {
                public override ShapeType Type => ShapeType.Sphere;

                /// <summary>
                /// The radius of the sphere.
                /// </summary>
                public float Radius;

                public Sphere() : this(1) { }

                public Sphere(float radius)
                {
                    Radius = radius;
                }

                public Sphere(Sphere clone) : this(clone.Radius) { }

                public override Shape Clone()
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

            public class Cylinder : Shape
            {
                public override ShapeType Type => ShapeType.Cylinder;

                /// <summary>
                /// The radius of the cylinder.
                /// </summary>
                public float Radius;

                /// <summary>
                /// The height of the cylinder.
                /// </summary>
                public float Height;

                public Cylinder() : this(1, 1) { }

                public Cylinder(float radius, float height)
                {
                    Radius = radius;
                    Height = height;
                }

                public Cylinder(Cylinder clone) : this(clone.Radius, clone.Height) { }

                public override Shape Clone()
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

            public class Box : Shape
            {
                public override ShapeType Type => ShapeType.Box;

                /// <summary>
                /// The length of the box.
                /// </summary>
                public float Length;

                /// <summary>
                /// The width of the box.
                /// </summary>
                public float Width;

                /// <summary>
                /// The height of the box.
                /// </summary>
                public float Height;

                public Box() : this(1, 1, 1) { }

                public Box(float length, float width, float height)
                {
                    Length = length;
                    Width = width;
                    Height = height;
                }

                public Box(Box clone) : this(clone.Length, clone.Width, clone.Height) { }

                public override Shape Clone()
                {
                    return new Box(this);
                }

                internal Box(BinaryReaderEx br)
                {
                    Length = br.ReadSingle();
                    Width = br.ReadSingle();
                    Height = br.ReadSingle();
                }

                internal override void Write(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    bw.WriteSingle(Length);
                    bw.WriteSingle(Width);
                    bw.WriteSingle(Height);
                }
            }
        }
    }
}
