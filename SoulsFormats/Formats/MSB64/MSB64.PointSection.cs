using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
                return Util.ConcatAll<Region>(
                    Points, Circles, Spheres, Cylinders, Boxes);
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

        public abstract class Region
        {
            public abstract RegionType Type { get; }

            public string Name;
            public int ID;
            public int Unk1, Unk2, Unk3, Unk4, Unk5, Unk6;
            public ulong UnkFlags;
            public Vector3 Position;
            public Vector3 Rotation;
            public int EventEntityID;

            internal Region(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                Unk1 = br.ReadInt32();
                ID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk2 = br.ReadInt32();

                Name = br.GetUTF16(start + nameOffset);

                long baseDataOffset1 = br.ReadInt64();
                br.StepIn(start + baseDataOffset1);
                Unk3 = br.ReadInt32();
                Unk4 = br.ReadInt32();
                br.StepOut();

                long baseDataOffset2 = br.AssertInt64(baseDataOffset1 + 4);
                br.StepIn(start + baseDataOffset2);
                Unk5 = br.ReadInt32();
                if (br.Position % 8 != 0)
                    br.AssertInt32(0);
                br.StepOut();

                UnkFlags = br.ReadUInt64();
                Read(br, start);
            }

            internal abstract void Read(BinaryReaderEx br, long start);

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk1);
                bw.WriteInt32(ID);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk2);

                bw.ReserveInt64("DataOffset1");
                bw.ReserveInt64("DataOffset2");
                bw.ReserveInt64("DataOffset3");
                bw.ReserveInt64("DataOffset4");
                bw.ReserveInt64("DataOffset5");
                bw.ReserveInt64("DataOffset6");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                bw.FillInt64("DataOffset1", bw.Position - start);
                WriteSpecific(bw, start);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw, long start);

            public override string ToString()
            {
                return $"{Unk1} {Type} {ID} : {Name}";
            }

            public class Point : Region
            {
                public override RegionType Type => RegionType.Point;

                public int UnkT18;

                internal Point(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br, long start)
                {
                    //br.AssertInt64(0);
                    //br.AssertInt64(dataOffset + 8);
                    //br.AssertInt64(dataOffset + 0x10);

                    //br.Position = start + dataOffset;
                    //br.AssertInt32(0);
                    //br.AssertInt32(0);
                    //EventEntityID = br.ReadInt32();
                    //br.AssertInt32(-1);
                    //UnkT18 = br.ReadInt32();
                    //br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    throw new NotImplementedException();
                }
            }

            public class Circle : Region
            {
                public override RegionType Type => RegionType.Circle;

                internal Circle(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br, long start)
                {
                    //throw new NotImplementedException();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    throw new NotImplementedException();
                }
            }

            public class Sphere : Region
            {
                public override RegionType Type => RegionType.Sphere;

                public float Radius;

                internal Sphere(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br, long start)
                {
                    long typeDataOffset = br.ReadInt64();
                    br.AssertInt64(typeDataOffset + 4);
                    br.AssertInt64(0, typeDataOffset + 0xC);

                    br.Position = start + typeDataOffset;
                    Radius = br.ReadSingle();
                    br.AssertInt32(-1);
                    br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    throw new NotImplementedException();
                }
            }

            public class Cylinder : Region
            {
                public override RegionType Type => RegionType.Cylinder;

                internal Cylinder(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br, long start)
                {
                    //throw new NotImplementedException();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    throw new NotImplementedException();
                }
            }

            public class Box : Region
            {
                public override RegionType Type => RegionType.Box;

                internal Box(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br, long start)
                {
                    //throw new NotImplementedException();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
