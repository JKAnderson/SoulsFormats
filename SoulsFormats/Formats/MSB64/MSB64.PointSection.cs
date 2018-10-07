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
        //public class PointSection : Section<Region>
        //{
        //    public override string Type => "POINT_PARAM_ST";

        //    public override List<Region> Entries => Util.ConcatAll<Region>(Points, Spheres, Cylinders, Boxes);

        //    public List<Region.Point> Points;

        //    public List<Region.Sphere> Spheres;

        //    public List<Region.Cylinder> Cylinders;

        //    public List<Region.Box> Boxes;

        //    internal PointSection(BinaryReaderEx br, int unk1, int offsets) : base(br, unk1, offsets) { }

        //    internal override void Init()
        //    {
        //        Points = new List<Region.Point>();
        //        Spheres = new List<Region.Sphere>();
        //        Cylinders = new List<Region.Cylinder>();
        //        Boxes = new List<Region.Box>();
        //    }

        //    internal override void Read(BinaryReaderEx br)
        //    {
        //        RegionType type = br.GetEnum32<RegionType>(br.Position + 0x10);

        //        switch (type)
        //        {
        //            case RegionType.Point:
        //                Points.Add(new Region.Point(br));
        //                break;

        //            case RegionType.Sphere:
        //                Spheres.Add(new Region.Sphere(br));
        //                break;

        //            case RegionType.Cylinder:
        //                Cylinders.Add(new Region.Cylinder(br));
        //                break;

        //            case RegionType.Box:
        //                Boxes.Add(new Region.Box(br));
        //                break;

        //            default:
        //                throw new NotImplementedException($"Unsupported region type: {type}");
        //        }
        //    }

        //    internal override void WriteOffsets(BinaryWriterEx bw)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    internal override void WriteData(BinaryWriterEx bw)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public enum RegionType : uint
        //{
        //    Point = 0,
        //    Sphere = 2,
        //    Cylinder = 3,
        //    Box = 5,
        //}

        //public abstract class Region
        //{
        //    public abstract RegionType Type { get; }

        //    public string Name;
        //    public int ID;
        //    public int Unk1;
        //    public int Unk2;
        //    public Vector3 Position;
        //    public Vector3 Rotation;

        //    internal Region(BinaryReaderEx br)
        //    {
        //        long start = br.Position;

        //        long nameOffset = br.ReadInt64();
        //        Unk1 = br.ReadInt32();
        //        ID = br.ReadInt32();
        //        br.AssertUInt32((uint)Type);
        //        Position = br.ReadVector3();
        //        Rotation = br.ReadVector3();
        //        Unk2 = br.ReadInt32();

        //        Name = br.GetUTF16(start + nameOffset);
        //        Read(br, start);
        //    }

        //    internal abstract void Read(BinaryReaderEx br, long start);

        //    public override string ToString()
        //    {
        //        return $"{Unk1} {Type} {ID} : {Name}";
        //    }

        //    public class Point : Region
        //    {
        //        public override RegionType Type => RegionType.Point;

        //        internal Point(BinaryReaderEx br) : base(br) { }

        //        internal override void Read(BinaryReaderEx br, long start)
        //        {
        //            long dataOffset = br.ReadInt64();
        //            br.AssertInt64(dataOffset + 4);
        //            br.AssertInt64(-1);
        //            br.AssertInt64(0);
        //            br.AssertInt64(dataOffset + 0xC);
        //            br.AssertInt64(dataOffset + 0x14);

        //            br.Position = start + dataOffset;
        //            br.AssertInt32(0);
        //        }
        //    }

        //    public class Sphere : Region
        //    {
        //        public override RegionType Type => RegionType.Sphere;

        //        internal Sphere(BinaryReaderEx br) : base(br) { }

        //        internal override void Read(BinaryReaderEx br, long start)
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public class Cylinder : Region
        //    {
        //        public override RegionType Type => RegionType.Cylinder;

        //        internal Cylinder(BinaryReaderEx br) : base(br) { }

        //        internal override void Read(BinaryReaderEx br, long start)
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public class Box : Region
        //    {
        //        public override RegionType Type => RegionType.Box;

        //        internal Box(BinaryReaderEx br) : base(br) { }

        //        internal override void Read(BinaryReaderEx br, long start)
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }
        //}
    }
}
