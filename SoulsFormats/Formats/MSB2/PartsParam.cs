using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB2
    {
        public enum PartType : ushort
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            MapPiece = 0,
            Object = 1,
            Collision = 3,
            Navmesh = 4,
            ConnectCollision = 5,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        public class PartsParam : Param<Part>
        {
            internal override string Name => "PARTS_PARAM_ST";
            internal override int Version => 5;

            public List<Part.MapPiece> MapPieces { get; set; }

            public List<Part.Object> Objects { get; set; }

            public List<Part.Collision> Collisions { get; set; }

            public List<Part.Navmesh> Navmeshes { get; set; }

            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            public PartsParam()
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Collisions = new List<Part.Collision>();
                Navmeshes = new List<Part.Navmesh>();
                ConnectCollisions = new List<Part.ConnectCollision>();
            }

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum16<PartType>(br.Position + 8);
                switch (type)
                {
                    case PartType.MapPiece:
                        var mapPiece = new Part.MapPiece(br);
                        MapPieces.Add(mapPiece);
                        return mapPiece;

                    case PartType.Object:
                        var obj = new Part.Object(br);
                        Objects.Add(obj);
                        return obj;

                    case PartType.Collision:
                        var collision = new Part.Collision(br);
                        Collisions.Add(collision);
                        return collision;

                    case PartType.Navmesh:
                        var navmesh = new Part.Navmesh(br);
                        Navmeshes.Add(navmesh);
                        return navmesh;

                    case PartType.ConnectCollision:
                        var connectCollision = new Part.ConnectCollision(br);
                        ConnectCollisions.Add(connectCollision);
                        return connectCollision;

                    default:
                        throw new NotImplementedException($"Unimplemented part type: {type}");
                }
            }

            public override List<Part> GetEntries()
            {
                return SFUtil.ConcatAll<Part>(
                    MapPieces, Objects, Collisions, Navmeshes, ConnectCollisions);
            }
        }

        public abstract class Part : NamedEntry
        {
            public abstract PartType Type { get; }

            public string ModelName { get; set; }
            private int ModelIndex;

            public Vector3 Position { get; set; }

            public Vector3 Rotation { get; set; }

            public Vector3 Scale { get; set; }

            public uint[] DrawGroups { get; private set; }

            public int Unk44 { get; set; }

            public int Unk48 { get; set; }

            public int Unk4C { get; set; }

            public int Unk50 { get; set; }

            public uint[] DispGroups { get; private set; }

            public int Unk64 { get; set; }

            public int Unk68 { get; set; }

            public int Unk6C { get; set; }

            internal Part(string name = "")
            {
                Name = name;
                Scale = Vector3.One;
                DrawGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                DispGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
            }

            internal Part(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                br.AssertUInt16((ushort)Type);
                br.ReadInt16(); // Index
                ModelIndex = br.ReadInt32();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                DrawGroups = br.ReadUInt32s(4);
                Unk44 = br.ReadInt32();
                Unk48 = br.ReadInt32();
                Unk4C = br.ReadInt32();
                Unk50 = br.ReadInt32();
                DispGroups = br.ReadUInt32s(4);
                Unk64 = br.ReadInt32();
                Unk68 = br.ReadInt32();
                Unk6C = br.ReadInt32();
                long typeDataOffset = br.ReadInt64();
                br.AssertInt64(0);

                Name = br.GetUTF16(start + nameOffset);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            internal abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int index)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt16((ushort)Type);
                bw.WriteInt16((short)index);
                bw.WriteInt32(ModelIndex);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteUInt32s(DrawGroups);
                bw.WriteInt32(Unk44);
                bw.WriteInt32(Unk48);
                bw.WriteInt32(Unk4C);
                bw.WriteInt32(Unk50);
                bw.WriteUInt32s(DispGroups);
                bw.WriteInt32(Unk64);
                bw.WriteInt32(Unk68);
                bw.WriteInt32(Unk6C);
                bw.ReserveInt64("TypeDataOffset");
                bw.WriteInt64(0);

                long nameStart = bw.Position;
                bw.FillInt64("NameOffset", nameStart - start);
                bw.WriteUTF16(ReambiguateName(Name), true);
                if (bw.Position - nameStart < 0x20)
                    bw.Position += 0x20 - (bw.Position - nameStart);
                bw.Pad(8);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);
            }

            internal abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB2 msb, Entries entries)
            {
                ModelName = FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(Lookups lookups)
            {
                ModelIndex = FindIndex(lookups.Models, ModelName);
            }

            public class MapPiece : Part
            {
                /// <summary>
                /// PartType.MapPiece
                /// </summary>
                public override PartType Type => PartType.MapPiece;

                public short UnkT00 { get; set; }

                public short UnkT02 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece(string name = "") : base(name) { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt16();
                    UnkT02 = br.ReadInt16();
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkT00);
                    bw.WriteInt16(UnkT02);
                    bw.WriteInt32(0);
                }
            }

            public class Object : Part
            {
                /// <summary>
                /// PartType.Object
                /// </summary>
                public override PartType Type => PartType.Object;

                public int UnkT00 { get; set; }

                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object(string name = "") : base(name) { }

                internal Object(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Collision : Part
            {
                /// <summary>
                /// PartType.Collision
                /// </summary>
                public override PartType Type => PartType.Collision;

                public int UnkT00 { get; set; }

                public int UnkT04 { get; set; }

                public int UnkT08 { get; set; }

                public int UnkT0C { get; set; }

                public short UnkT10 { get; set; }

                public byte UnkT12 { get; set; }

                public byte UnkT13 { get; set; }

                public int UnkT14 { get; set; }

                public int UnkT18 { get; set; }

                public int UnkT1C { get; set; }

                public int UnkT20 { get; set; }

                public short UnkT24 { get; set; }

                public short UnkT26 { get; set; }

                public int UnkT28 { get; set; }

                public short UnkT2C { get; set; }

                public short UnkT2E { get; set; }

                public int UnkT30 { get; set; }

                public short UnkT34 { get; set; }

                public short UnkT36 { get; set; }

                public int UnkT38 { get; set; }

                public int UnkT3C { get; set; }

                public int UnkT40 { get; set; }

                public int UnkT44 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision(string name = "") : base(name) { }

                internal Collision(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt16();
                    UnkT12 = br.ReadByte();
                    UnkT13 = br.ReadByte();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadInt16();
                    UnkT26 = br.ReadInt16();
                    UnkT28 = br.ReadInt32();
                    UnkT2C = br.ReadInt16();
                    UnkT2E = br.ReadInt16();
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadInt16();
                    UnkT36 = br.ReadInt16();
                    UnkT38 = br.ReadInt32();
                    UnkT3C = br.ReadInt32();
                    UnkT40 = br.ReadInt32();
                    UnkT44 = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt16(UnkT10);
                    bw.WriteByte(UnkT12);
                    bw.WriteByte(UnkT13);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt16(UnkT24);
                    bw.WriteInt16(UnkT26);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt16(UnkT2C);
                    bw.WriteInt16(UnkT2E);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt16(UnkT34);
                    bw.WriteInt16(UnkT36);
                    bw.WriteInt32(UnkT38);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteInt32(UnkT40);
                    bw.WriteInt32(UnkT44);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            public class Navmesh : Part
            {
                /// <summary>
                /// PartType.Navmesh
                /// </summary>
                public override PartType Type => PartType.Navmesh;

                public int UnkT00 { get; set; }

                public int UnkT04 { get; set; }

                public int UnkT08 { get; set; }

                public int UnkT0C { get; set; }

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh(string name = "") : base(name) { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            public class ConnectCollision : Part
            {
                /// <summary>
                /// PartType.ConnectCollision
                /// </summary>
                public override PartType Type => PartType.ConnectCollision;

                public string CollisionName { get; set; }
                private int CollisionIndex;

                public byte MapID1 { get; set; }

                public byte MapID2 { get; set; }

                public byte MapID3 { get; set; }

                public byte MapID4 { get; set; }

                public int UnkT08 { get; set; }

                public int UnkT0C { get; set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision(string name = "") : base(name) { }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    CollisionIndex = br.ReadInt32();
                    MapID1 = br.ReadByte();
                    MapID2 = br.ReadByte();
                    MapID3 = br.ReadByte();
                    MapID4 = br.ReadByte();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteByte(MapID1);
                    bw.WriteByte(MapID2);
                    bw.WriteByte(MapID3);
                    bw.WriteByte(MapID4);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                }

                internal override void GetNames(MSB2 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(Lookups lookups)
                {
                    base.GetIndices(lookups);
                    CollisionIndex = FindIndex(lookups.Collisions, CollisionName);
                }
            }
        }
    }
}
