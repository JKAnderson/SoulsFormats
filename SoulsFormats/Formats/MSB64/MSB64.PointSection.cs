using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB64
    {
        public class PointSection : Section<Region>
        {
            public override string Type => "POINT_PARAM_ST";

            public List<Region.Point> Points;

            public List<Region.Circle> Circles;

            public List<Region.Sphere> Spheres;

            public List<Region.Cylinder> Cylinders;

            public List<Region.Box> Boxes;

            internal PointSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                Points = new List<Region.Point>();
                Circles = new List<Region.Circle>();
                Spheres = new List<Region.Sphere>();
                Cylinders = new List<Region.Cylinder>();
                Boxes = new List<Region.Box>();
            }

            internal override List<Region> GetEntries()
            {
                List<Region> regions = Util.ConcatAll<Region>(Points, Circles, Spheres, Cylinders, Boxes);
                regions.Sort((r1, r2) =>
                {
                    if (r1.BonusType == r2.BonusType)
                        return r1.ID.CompareTo(r2.ID);
                    else
                        return r1.BonusType.CompareTo(r2.BonusType);
                });
                return regions;
            }

            internal override Region ReadEntry(BinaryReaderEx br)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 0x10);

                switch (type)
                {
                    case RegionType.Point:
                        var point = new Region.Point(br);
                        Points.Add(point);
                        return point;

                    case RegionType.Circle:
                        var circle = new Region.Circle(br);
                        Circles.Add(circle);
                        return circle;

                    case RegionType.Sphere:
                        var sphere = new Region.Sphere(br);
                        Spheres.Add(sphere);
                        return sphere;

                    case RegionType.Cylinder:
                        var cylinder = new Region.Cylinder(br);
                        Cylinders.Add(cylinder);
                        return cylinder;

                    case RegionType.Box:
                        var box = new Region.Box(br);
                        Boxes.Add(box);
                        return box;

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
        }

        public enum RegionType : uint
        {
            Point = 0,
            Circle = 1,
            Sphere = 2,
            Cylinder = 3,
            Square = 4,
            Box = 5,
        }

        public abstract class Region : Entry
        {
            public abstract RegionType Type { get; }

            public override string Name { get; set; }
            public int ID;
            public int BonusType;
            public int Unk2, Unk3, Unk4;
            public uint UnkFlags;
            public Vector3 Position;
            public Vector3 Rotation;
            public int EventEntityID1, EventEntityID2;
            public int[] BonusData;

            internal Region(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                BonusType = br.ReadInt32();
                ID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
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
                if (br.Position % 8 != 0)
                    br.AssertInt32(0);
                br.StepOut();

                br.AssertInt32(-1);
                UnkFlags = br.ReadUInt32();

                long typeDataOffset = br.ReadInt64();
                if (typeDataOffset != 0)
                {
                    br.StepIn(start + typeDataOffset);
                    ReadSpecific(br);
                    br.StepOut();
                }

                long entityIDOffset = br.ReadInt64();
                long bonusDataOffset = br.ReadInt64();

                br.StepIn(start + entityIDOffset);
                EventEntityID1 = br.ReadInt32();
                EventEntityID2 = br.ReadInt32();

                // Offset is 0 but there's still data because MIYAZAKIIIIIIIIII
                if (BonusType == 20)
                    BonusData = br.ReadInt32s(1);
                else if (bonusDataOffset == 0)
                    BonusData = null;
                else if (BonusType == 1)
                    BonusData = br.ReadInt32s(1);
                else if (BonusType == 2)
                    BonusData = br.ReadInt32s(1);
                else if (BonusType == 4)
                    BonusData = br.ReadInt32s(18);
                else if (BonusType == 5)
                    BonusData = br.ReadInt32s(6);
                else if (BonusType == 6)
                    BonusData = br.ReadInt32s(7);
                else if (BonusType == 8)
                    BonusData = br.ReadInt32s(4);
                else if (BonusType == 9)
                    BonusData = br.ReadInt32s(2);
                else if (BonusType == 17)
                    // Possibly 10 padded, can't tell because it's only found in boxes
                    BonusData = br.ReadInt32s(11);
                else
                    throw new InvalidOperationException($"Bonus type {BonusType} should not have any bonus data.");

                if (br.Position % 8 != 0)
                    br.AssertInt32(0);

                br.StepOut();
            }

            internal abstract void ReadSpecific(BinaryReaderEx br);

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(BonusType);
                bw.WriteInt32(ID);
                bw.WriteUInt32((uint)Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk2);

                bw.ReserveInt64("BaseDataOffset1");
                bw.ReserveInt64("BaseDataOffset2");

                bw.WriteInt32(-1);
                bw.WriteUInt32(UnkFlags);

                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("EntityIDOffset");
                bw.ReserveInt64("BonusDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset1", bw.Position - start);
                bw.WriteInt32(Unk3);

                bw.FillInt64("BaseDataOffset2", bw.Position - start);
                bw.WriteInt32(Unk4);
                bw.Pad(8);

                WriteSpecific(bw, start);

                bw.FillInt64("EntityIDOffset", bw.Position - start);
                bw.WriteInt32(EventEntityID1);
                bw.WriteInt32(EventEntityID2);

                if (BonusData == null || BonusType == 20)
                    bw.FillInt64("BonusDataOffset", 0);
                else
                    bw.FillInt64("BonusDataOffset", bw.Position - start);

                if (BonusData != null)
                    bw.WriteInt32s(BonusData);
                bw.Pad(8);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw, long start);

            public override string ToString()
            {
                return $"{BonusType} {Type} {ID} : {Name}";
            }

            public class Point : Region
            {
                public override RegionType Type => RegionType.Point;

                internal Point(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br) { }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            public class Circle : Region
            {
                public override RegionType Type => RegionType.Circle;

                public float Radius;

                internal Circle(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteSingle(Radius);
                }
            }

            public class Sphere : Region
            {
                public override RegionType Type => RegionType.Sphere;

                public float Radius;

                internal Sphere(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteSingle(Radius);
                }
            }

            public class Cylinder : Region
            {
                public override RegionType Type => RegionType.Cylinder;

                public float Radius;
                public float Height;

                internal Cylinder(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                    Height = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteSingle(Radius);
                    bw.WriteSingle(Height);
                }
            }

            public class Box : Region
            {
                public override RegionType Type => RegionType.Box;

                public float Length;
                public float Width;
                public float Height;

                internal Box(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br)
                {
                    Length = br.ReadSingle();
                    Width = br.ReadSingle();
                    Height = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteSingle(Length);
                    bw.WriteSingle(Width);
                    bw.WriteSingle(Height);
                }
            }
        }
    }
}
