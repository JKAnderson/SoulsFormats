using System;

namespace SoulsFormats
{
    public partial class MSBS
    {
        public enum ShapeType : uint
        {
            Point = 0,
            Circle = 1,
            Sphere = 2,
            Cylinder = 3,
            Rect = 4,
            Box = 5,
            Composite = 6,
        }

        public abstract class Shape
        {
            internal abstract ShapeType Type { get; }

            internal abstract bool HasShapeData { get; }

            internal virtual void WriteShapeData(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Shape data should not be written for shapes with no shape data.");
            }

            public class Point : Shape
            {
                internal override ShapeType Type => ShapeType.Point;

                internal override bool HasShapeData => false;
            }

            public class Circle : Shape
            {
                internal override ShapeType Type => ShapeType.Circle;

                internal override bool HasShapeData => true;

                public float Radius { get; set; }

                public Circle() { }

                internal Circle(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Radius);
                }
            }

            public class Sphere : Shape
            {
                internal override ShapeType Type => ShapeType.Sphere;

                internal override bool HasShapeData => true;

                public float Radius { get; set; }

                public Sphere() { }

                internal Sphere(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Radius);
                }
            }

            public class Cylinder : Shape
            {
                internal override ShapeType Type => ShapeType.Cylinder;

                internal override bool HasShapeData => true;

                public float Radius { get; set; }

                public float Height { get; set; }

                public Cylinder() { }

                internal Cylinder(BinaryReaderEx br)
                {
                    Radius = br.ReadSingle();
                    Height = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Radius);
                    bw.WriteSingle(Height);
                }
            }

            public class Rect : Shape
            {
                internal override ShapeType Type => ShapeType.Rect;

                internal override bool HasShapeData => true;

                public float Width { get; set; }

                public float Depth { get; set; }

                public Rect() { }

                internal Rect(BinaryReaderEx br)
                {
                    Width = br.ReadSingle();
                    Depth = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Width);
                    bw.WriteSingle(Depth);
                }
            }

            public class Box : Shape
            {
                internal override ShapeType Type => ShapeType.Box;

                internal override bool HasShapeData => true;

                public float Width { get; set; }

                public float Depth { get; set; }

                public float Height { get; set; }

                public Box() { }

                internal Box(BinaryReaderEx br)
                {
                    Width = br.ReadSingle();
                    Depth = br.ReadSingle();
                    Height = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Width);
                    bw.WriteSingle(Depth);
                    bw.WriteSingle(Height);
                }
            }

            public class Composite : Shape
            {
                internal override ShapeType Type => ShapeType.Composite;

                internal override bool HasShapeData => true;

                public Child[] Children { get; private set; }

                public Composite()
                {
                    Children = new Child[8];
                    for (int i = 0; i < 8; i++)
                        Children[i] = new Child();
                }

                internal Composite(BinaryReaderEx br)
                {
                    Children = new Child[8];
                    for (int i = 0; i < 8; i++)
                        Children[i] = new Child(br);
                }

                internal override void WriteShapeData(BinaryWriterEx bw)
                {
                    for (int i = 0; i < 8; i++)
                        Children[i].Write(bw);
                }

                public class Child
                {
                    public string RegionName { get; set; }
                    private int RegionIndex;

                    public int Unk04 { get; set; }

                    public Child() { }

                    internal Child(BinaryReaderEx br)
                    {
                        RegionIndex = br.ReadInt32();
                        Unk04 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(RegionIndex);
                        bw.WriteInt32(Unk04);
                    }

                    internal void GetNames(Entries entries)
                    {
                        RegionName = GetName(entries.Regions, RegionIndex);
                    }

                    internal void GetIndices(Entries entries)
                    {
                        RegionIndex = GetIndex(entries.Regions, RegionName);
                    }
                }
            }
        }
    }
}
