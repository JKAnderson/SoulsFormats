using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public partial class MSBS
    {
        public enum RegionType : uint
        {
            Region0 = 0,
            InvasionPoint = 1,
            EnvironmentMapPoint = 2,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            SpawnPoint = 8,
            WalkRoute = 11,
            WarpPoint = 13,
            ActivationArea = 14,
            Event = 15,
            EnvironmentMapEffectBox = 17,
            WindArea = 18,
            MufflingBox = 20,
            MufflingPortal = 21,
            Region23 = 23,
            Region24 = 24,
            PartsGroup = 25,
            AutoDrawGroup = 26,
            Other = 0xFFFFFFFF,
        }

        public class PointParam : Param<Region>
        {
            public List<Region.Region0> Region0s { get; set; }

            public List<Region.InvasionPoint> InvasionPoints { get; set; }

            public List<Region.EnvironmentMapPoint> EnvironmentMapPoints { get; set; }

            public List<Region.Sound> Sounds { get; set; }

            public List<Region.SFX> SFXs { get; set; }

            public List<Region.WindSFX> WindSFXs { get; set; }

            public List<Region.SpawnPoint> SpawnPoints { get; set; }

            public List<Region.WalkRoute> WalkRoutes { get; set; }

            public List<Region.WarpPoint> WarpPoints { get; set; }

            public List<Region.ActivationArea> ActivationAreas { get; set; }

            public List<Region.Event> Events { get; set; }

            public List<Region.EnvironmentMapEffectBox> EnvironmentMapEffectBoxes { get; set; }

            public List<Region.WindArea> WindAreas { get; set; }

            public List<Region.MufflingBox> MufflingBoxes { get; set; }

            public List<Region.MufflingPortal> MufflingPortals { get; set; }

            public List<Region.Region23> Region23s { get; set; }

            public List<Region.Region24> Region24s { get; set; }

            public List<Region.PartsGroup> PartsGroups { get; set; }

            public List<Region.AutoDrawGroup> AutoDrawGroups { get; set; }

            public List<Region.Other> Others { get; set; }

            internal PointParam() : base("POINT_PARAM_ST")
            {
                Region0s = new List<Region.Region0>();
                InvasionPoints = new List<Region.InvasionPoint>();
                EnvironmentMapPoints = new List<Region.EnvironmentMapPoint>();
                Sounds = new List<Region.Sound>();
                SFXs = new List<Region.SFX>();
                WindSFXs = new List<Region.WindSFX>();
                SpawnPoints = new List<Region.SpawnPoint>();
                WalkRoutes = new List<Region.WalkRoute>();
                WarpPoints = new List<Region.WarpPoint>();
                ActivationAreas = new List<Region.ActivationArea>();
                Events = new List<Region.Event>();
                EnvironmentMapEffectBoxes = new List<Region.EnvironmentMapEffectBox>();
                WindAreas = new List<Region.WindArea>();
                MufflingBoxes = new List<Region.MufflingBox>();
                MufflingPortals = new List<Region.MufflingPortal>();
                Region23s = new List<Region.Region23>();
                Region24s = new List<Region.Region24>();
                PartsGroups = new List<Region.PartsGroup>();
                AutoDrawGroups = new List<Region.AutoDrawGroup>();
                Others = new List<Region.Other>();
            }

            internal override Region ReadEntry(BinaryReaderEx br)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                switch (type)
                {
                    case RegionType.Region0:
                        var region0 = new Region.Region0(br);
                        Region0s.Add(region0);
                        return region0;

                    case RegionType.InvasionPoint:
                        var invasionPoint = new Region.InvasionPoint(br);
                        InvasionPoints.Add(invasionPoint);
                        return invasionPoint;

                    case RegionType.EnvironmentMapPoint:
                        var environmentMapPoint = new Region.EnvironmentMapPoint(br);
                        EnvironmentMapPoints.Add(environmentMapPoint);
                        return environmentMapPoint;

                    case RegionType.Sound:
                        var sound = new Region.Sound(br);
                        Sounds.Add(sound);
                        return sound;

                    case RegionType.SFX:
                        var sfx = new Region.SFX(br);
                        SFXs.Add(sfx);
                        return sfx;

                    case RegionType.WindSFX:
                        var windSFX = new Region.WindSFX(br);
                        WindSFXs.Add(windSFX);
                        return windSFX;

                    case RegionType.SpawnPoint:
                        var spawnPoint = new Region.SpawnPoint(br);
                        SpawnPoints.Add(spawnPoint);
                        return spawnPoint;

                    case RegionType.WalkRoute:
                        var walkRoute = new Region.WalkRoute(br);
                        WalkRoutes.Add(walkRoute);
                        return walkRoute;

                    case RegionType.WarpPoint:
                        var warpPoint = new Region.WarpPoint(br);
                        WarpPoints.Add(warpPoint);
                        return warpPoint;

                    case RegionType.ActivationArea:
                        var activationArea = new Region.ActivationArea(br);
                        ActivationAreas.Add(activationArea);
                        return activationArea;

                    case RegionType.Event:
                        var evt = new Region.Event(br);
                        Events.Add(evt);
                        return evt;

                    case RegionType.EnvironmentMapEffectBox:
                        var environmentMapEffectBox = new Region.EnvironmentMapEffectBox(br);
                        EnvironmentMapEffectBoxes.Add(environmentMapEffectBox);
                        return environmentMapEffectBox;

                    case RegionType.WindArea:
                        var windArea = new Region.WindArea(br);
                        WindAreas.Add(windArea);
                        return windArea;

                    case RegionType.MufflingBox:
                        var mufflingBox = new Region.MufflingBox(br);
                        MufflingBoxes.Add(mufflingBox);
                        return mufflingBox;

                    case RegionType.MufflingPortal:
                        var mufflingPortal = new Region.MufflingPortal(br);
                        MufflingPortals.Add(mufflingPortal);
                        return mufflingPortal;

                    case RegionType.Region23:
                        var region23 = new Region.Region23(br);
                        Region23s.Add(region23);
                        return region23;

                    case RegionType.Region24:
                        var region24 = new Region.Region24(br);
                        Region24s.Add(region24);
                        return region24;

                    case RegionType.PartsGroup:
                        var partsGroup = new Region.PartsGroup(br);
                        PartsGroups.Add(partsGroup);
                        return partsGroup;

                    case RegionType.AutoDrawGroup:
                        var autoDrawGroup = new Region.AutoDrawGroup(br);
                        AutoDrawGroups.Add(autoDrawGroup);
                        return autoDrawGroup;

                    case RegionType.Other:
                        var other = new Region.Other(br);
                        Others.Add(other);
                        return other;

                    default:
                        throw new NotImplementedException($"Unimplemented region type: {type}");
                }
            }

            public override List<Region> GetEntries()
            {
                return SFUtil.ConcatAll<Region>(
                    InvasionPoints, EnvironmentMapPoints, Sounds, SFXs, WindSFXs,
                    SpawnPoints, WalkRoutes, WarpPoints, ActivationAreas, Events,
                    Region0s, EnvironmentMapEffectBoxes, WindAreas, MufflingBoxes, MufflingPortals,
                    Region23s, Region24s, PartsGroups, AutoDrawGroups, Others);
            }
        }

        public abstract class Region : Entry
        {
            internal abstract RegionType Type { get; }

            internal abstract bool HasTypeData { get; }

            public override string Name { get; set; }

            public int ID { get; set; }

            public Shape Shape { get; set; }

            public Vector3 Position { get; set; }

            public Vector3 Rotation { get; set; }

            public int Unk2C { get; set; }

            public int MapStudioLayer { get; set; }

            public short[] UnkA { get; set; }

            public short[] UnkB { get; set; }

            public int UnkC00 { get; set; }

            public int UnkC04 { get; set; }

            internal Region(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                ID = br.ReadInt32();
                ShapeType shapeType = br.ReadEnum32<ShapeType>();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk2C = br.ReadInt32();
                long baseDataOffset1 = br.ReadInt64();
                long baseDataOffset2 = br.ReadInt64();
                br.AssertInt32(-1);
                MapStudioLayer = br.ReadInt32();
                long shapeDataOffset = br.ReadInt64();
                long baseDataOffset3 = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                Name = br.GetUTF16(start + nameOffset);
                br.Position = start + baseDataOffset1;
                short countA = br.ReadInt16();
                UnkA = br.ReadInt16s(countA);
                br.Position = start + baseDataOffset2;
                short countB = br.ReadInt16();
                UnkB = br.ReadInt16s(countB);

                br.Position = start + shapeDataOffset;
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

                    case ShapeType.Rect:
                        Shape = new Shape.Rect(br);
                        break;

                    case ShapeType.Box:
                        Shape = new Shape.Box(br);
                        break;

                    case ShapeType.Composite:
                        Shape = new Shape.Composite(br);
                        break;

                    default:
                        throw new NotImplementedException($"Unimplemented shape type: {shapeType}");
                }

                br.Position = start + baseDataOffset3;
                UnkC00 = br.ReadInt32();
                UnkC04 = br.ReadInt32();
                br.Position = start + typeDataOffset;
            }

            internal override void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(ID);
                bw.WriteUInt32((uint)Shape.Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk2C);
                bw.ReserveInt64("BaseDataOffset1");
                bw.ReserveInt64("BaseDataOffset2");
                bw.WriteInt32(-1);
                bw.WriteInt32(MapStudioLayer);
                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("BaseDataOffset3");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset1", bw.Position - start);
                bw.WriteInt16((short)UnkA.Length);
                bw.WriteInt16s(UnkA);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset2", bw.Position - start);
                bw.WriteInt16((short)UnkB.Length);
                bw.WriteInt16s(UnkB);
                bw.Pad(8);

                if (Shape.HasShapeData)
                {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    Shape.WriteShapeData(bw);
                }
                else
                {
                    bw.FillInt64("ShapeDataOffset", 0);
                }

                bw.FillInt64("BaseDataOffset3", bw.Position - start);
                bw.WriteInt32(UnkC00);
                bw.WriteInt32(UnkC04);
                
                if (HasTypeData)
                {
                    if (Type == RegionType.Region23 || Type == RegionType.PartsGroup || Type == RegionType.AutoDrawGroup)
                        bw.Pad(8);

                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
                bw.Pad(8);
            }

            internal virtual void WriteTypeData(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Type data should not be written for regions with no type data.");
            }

            public override string ToString()
            {
                return $"{Type} {Shape.Type} {Name}";
            }

            public class Region0 : Region
            {
                internal override RegionType Type => RegionType.Region0;

                internal override bool HasTypeData => false;

                internal Region0(BinaryReaderEx br) : base(br) { }
            }

            public class InvasionPoint : Region
            {
                internal override RegionType Type => RegionType.InvasionPoint;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                internal InvasionPoint(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                }
            }

            public class EnvironmentMapPoint : Region
            {
                internal override RegionType Type => RegionType.EnvironmentMapPoint;

                internal override bool HasTypeData => true;

                public float UnkT00 { get; set; }

                public int UnkT04 { get; set; }

                public int UnkT0C { get; set; }

                public float UnkT10 { get; set; }

                public float UnkT14 { get; set; }

                public int UnkT18 { get; set; }

                public int UnkT1C { get; set; }

                public int UnkT20 { get; set; }

                public int UnkT24 { get; set; }

                public int UnkT28 { get; set; }

                internal EnvironmentMapPoint(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertNull(0x10, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(-1);
                    bw.WriteNull(0x10, false);
                }
            }

            public class Sound : Region
            {
                internal override RegionType Type => RegionType.Sound;

                internal override bool HasTypeData => true;

                public int SoundType { get; set; }

                public int SoundID { get; set; }

                public int[] ChildRegionIndices { get; set; }

                public int UnkT48 { get; set; }

                internal Sound(BinaryReaderEx br) : base(br)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    ChildRegionIndices = br.ReadInt32s(16);
                    UnkT48 = br.ReadInt32();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32s(ChildRegionIndices);
                    bw.WriteInt32(UnkT48);
                }
            }

            public class SFX : Region
            {
                internal override RegionType Type => RegionType.SFX;

                internal override bool HasTypeData => true;

                public int FFXID { get; set; }

                public int UnkT04 { get; set; }

                public int StartDisabled { get; set; }

                internal SFX(BinaryReaderEx br) : base(br)
                {
                    FFXID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    StartDisabled = br.ReadInt32();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(FFXID);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(StartDisabled);
                }
            }

            public class WindSFX : Region
            {
                internal override RegionType Type => RegionType.WindSFX;

                internal override bool HasTypeData => true;

                public int FFXID { get; set; }

                public int WindAreaIndex { get; set; }

                public float UnkT18 { get; set; }

                internal WindSFX(BinaryReaderEx br) : base(br)
                {
                    FFXID = br.ReadInt32();
                    br.AssertNull(0x10, true);
                    WindAreaIndex = br.ReadInt32();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(FFXID);
                    bw.WriteNull(0x10, true);
                    bw.WriteInt32(WindAreaIndex);
                    bw.WriteSingle(UnkT18);
                    bw.WriteInt32(0);
                }
            }

            public class SpawnPoint : Region
            {
                internal override RegionType Type => RegionType.SpawnPoint;

                internal override bool HasTypeData => true;

                internal SpawnPoint(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class WalkRoute : Region
            {
                internal override RegionType Type => RegionType.WalkRoute;

                internal override bool HasTypeData => false;

                internal WalkRoute(BinaryReaderEx br) : base(br) { }
            }

            public class WarpPoint : Region
            {
                internal override RegionType Type => RegionType.WarpPoint;

                internal override bool HasTypeData => false;

                internal WarpPoint(BinaryReaderEx br) : base(br) { }
            }

            public class ActivationArea : Region
            {
                internal override RegionType Type => RegionType.ActivationArea;

                internal override bool HasTypeData => false;

                internal ActivationArea(BinaryReaderEx br) : base(br) { }
            }

            public class Event : Region
            {
                internal override RegionType Type => RegionType.Event;

                internal override bool HasTypeData => false;

                internal Event(BinaryReaderEx br) : base(br) { }
            }

            public class EnvironmentMapEffectBox : Region
            {
                internal override RegionType Type => RegionType.EnvironmentMapEffectBox;

                internal override bool HasTypeData => true;

                public float UnkT00 { get; set; }

                public float Compare { get; set; }

                public byte UnkT08 { get; set; }

                public byte UnkT09 { get; set; }

                public short UnkT0A { get; set; }

                public int UnkT24 { get; set; }

                public float UnkT28 { get; set; }

                public float UnkT2C { get; set; }

                internal EnvironmentMapEffectBox(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadSingle();
                    Compare = br.ReadSingle();
                    UnkT08 = br.ReadByte();
                    UnkT09 = br.ReadByte();
                    UnkT0A = br.ReadInt16();
                    br.AssertNull(0x18, false);
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadSingle();
                    UnkT2C = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteSingle(Compare);
                    bw.WriteByte(UnkT08);
                    bw.WriteByte(UnkT09);
                    bw.WriteInt16(UnkT0A);
                    bw.WriteNull(0x18, false);
                    bw.WriteInt32(UnkT24);
                    bw.WriteSingle(UnkT28);
                    bw.WriteSingle(UnkT2C);
                    bw.WriteInt32(0);
                }
            }

            public class WindArea : Region
            {
                internal override RegionType Type => RegionType.WindArea;

                internal override bool HasTypeData => false;

                internal WindArea(BinaryReaderEx br) : base(br) { }
            }

            public class MufflingBox : Region
            {
                internal override RegionType Type => RegionType.MufflingBox;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                internal MufflingBox(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                }
            }

            public class MufflingPortal : Region
            {
                internal override RegionType Type => RegionType.MufflingPortal;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                internal MufflingPortal(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                }
            }

            public class Region23 : Region
            {
                internal override RegionType Type => RegionType.Region23;

                internal override bool HasTypeData => true;

                public long UnkT00 { get; set; }

                internal Region23(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt64();
                    br.AssertNull(0x18, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt64(UnkT00);
                    bw.WriteNull(0x18, false);
                }
            }

            public class Region24 : Region
            {
                internal override RegionType Type => RegionType.Region24;

                internal override bool HasTypeData => false;

                internal Region24(BinaryReaderEx br) : base(br) { }
            }

            public class PartsGroup : Region
            {
                internal override RegionType Type => RegionType.PartsGroup;

                internal override bool HasTypeData => true;

                public long UnkT00 { get; set; }

                internal PartsGroup(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt64();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt64(UnkT00);
                }
            }

            public class AutoDrawGroup : Region
            {
                internal override RegionType Type => RegionType.AutoDrawGroup;

                internal override bool HasTypeData => true;

                public long UnkT00 { get; set; }

                internal AutoDrawGroup(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt64();
                    br.AssertNull(0x18, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt64(UnkT00);
                    bw.WriteNull(0x18, false);
                }
            }

            public class Other : Region
            {
                internal override RegionType Type => RegionType.Other;

                internal override bool HasTypeData => false;

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
