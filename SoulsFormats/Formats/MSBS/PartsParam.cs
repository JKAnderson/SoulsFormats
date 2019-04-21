using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBS
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public enum PartType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        public class PartsParam : Param<Part>
        {
            public List<Part.MapPiece> MapPieces { get; set; }

            public List<Part.Object> Objects { get; set; }

            public List<Part.Enemy> Enemies { get; set; }

            public List<Part.Player> Players { get; set; }

            public List<Part.Collision> Collisions { get; set; }

            public List<Part.DummyObject> DummyObjects { get; set; }

            public List<Part.DummyEnemy> DummyEnemies { get; set; }

            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            public PartsParam() : this(0x23) { }

            public PartsParam(int unk00) : base(unk00, "PARTS_PARAM_ST")
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Enemies = new List<Part.Enemy>();
                Players = new List<Part.Player>();
                Collisions = new List<Part.Collision>();
                DummyObjects = new List<Part.DummyObject>();
                DummyEnemies = new List<Part.DummyEnemy>();
                ConnectCollisions = new List<Part.ConnectCollision>();
            }

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum32<PartType>(br.Position + 8);
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

                    case PartType.Enemy:
                        var enemy = new Part.Enemy(br);
                        Enemies.Add(enemy);
                        return enemy;

                    case PartType.Player:
                        var player = new Part.Player(br);
                        Players.Add(player);
                        return player;

                    case PartType.Collision:
                        var collision = new Part.Collision(br);
                        Collisions.Add(collision);
                        return collision;

                    case PartType.DummyObject:
                        var dummyObject = new Part.DummyObject(br);
                        DummyObjects.Add(dummyObject);
                        return dummyObject;

                    case PartType.DummyEnemy:
                        var dummyEnemy = new Part.DummyEnemy(br);
                        DummyEnemies.Add(dummyEnemy);
                        return dummyEnemy;

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
                    MapPieces, Objects, Enemies, Players, Collisions,
                    DummyObjects, DummyEnemies, ConnectCollisions);
            }
        }

        public abstract class Part : Entry
        {
            public abstract PartType Type { get; }

            internal abstract bool HasUnk1 { get; }
            internal abstract bool HasUnk2 { get; }
            internal abstract bool HasUnk5 { get; }
            internal abstract bool HasUnk6 { get; }
            internal abstract bool HasUnk7 { get; }

            public string ModelName { get; set; }
            private int ModelIndex;

            public string Placeholder { get; set; }

            public Vector3 Position { get; set; }

            public Vector3 Rotation { get; set; }

            public Vector3 Scale { get; set; }

            public int EntityID { get; set; }

            public byte UnkE04 { get; set; }

            public byte UnkE05 { get; set; }

            public byte UnkE06 { get; set; }

            public byte LanternID { get; set; }

            public byte LodParamID { get; set; }

            public byte UnkE09 { get; set; }

            public bool IsPointLightShadowSrc { get; set; }

            public byte UnkE0B { get; set; }

            public bool IsShadowSrc { get; set; }

            public byte IsStaticShadowSrc { get; set; }

            public byte IsCascade3ShadowSrc { get; set; }

            public byte UnkE0F { get; set; }

            public byte UnkE10 { get; set; }

            public bool IsShadowDest { get; set; }

            public bool IsShadowOnly { get; set; }

            public bool DrawByReflectCam { get; set; }

            public bool DrawOnlyReflectCam { get; set; }

            public byte EnableOnAboveShadow { get; set; }

            public bool DisablePointLightEffect { get; set; }

            public byte UnkE17 { get; set; }

            public int UnkE18 { get; set; }

            public int[] EntityGroupIDs { get; private set; }

            public int UnkE3C { get; set; }

            public int UnkE40 { get; set; }

            internal Part()
            {
                Name = "";
                Placeholder = "";
                Scale = Vector3.One;
                EntityID = -1;
                EntityGroupIDs = new int[8];
                for (int i = 0; i < 8; i++)
                    EntityGroupIDs[i] = -1;
            }

            public Part(Part clone)
            {
                Name = clone.Name;
                ModelName = clone.ModelName;
                Placeholder = clone.Placeholder;
                Position = clone.Position;
                Rotation = clone.Rotation;
                Scale = clone.Scale;
                EntityID = clone.EntityID;
                UnkE04 = clone.UnkE04;
                UnkE05 = clone.UnkE05;
                UnkE06 = clone.UnkE06;
                LanternID = clone.LanternID;
                LodParamID = clone.LodParamID;
                UnkE09 = clone.UnkE09;
                IsPointLightShadowSrc = clone.IsPointLightShadowSrc;
                UnkE0B = clone.UnkE0B;
                IsShadowSrc = clone.IsShadowSrc;
                IsStaticShadowSrc = clone.IsStaticShadowSrc;
                IsCascade3ShadowSrc = clone.IsCascade3ShadowSrc;
                UnkE0F = clone.UnkE0F;
                UnkE10 = clone.UnkE10;
                IsShadowDest = clone.IsShadowDest;
                IsShadowOnly = clone.IsShadowOnly;
                DrawByReflectCam = clone.DrawByReflectCam;
                DrawOnlyReflectCam = clone.DrawOnlyReflectCam;
                EnableOnAboveShadow = clone.EnableOnAboveShadow;
                DisablePointLightEffect = clone.DisablePointLightEffect;
                UnkE17 = clone.UnkE17;
                UnkE18 = clone.UnkE18;
                EntityGroupIDs = (int[])clone.EntityGroupIDs.Clone();
                UnkE3C = clone.UnkE3C;
                UnkE40 = clone.UnkE40;
            }

            internal Part(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                ModelIndex = br.ReadInt32();
                br.AssertInt32(0);
                long sibOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                br.AssertInt32(-1);
                br.AssertInt32(-1);
                br.AssertInt32(0);
                long unkOffset1 = br.ReadInt64();
                long unkOffset2 = br.ReadInt64();
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long unkOffset5 = br.ReadInt64();
                long unkOffset6 = br.ReadInt64();
                long unkOffset7 = br.ReadInt64();
                br.AssertInt64(0);
                br.AssertInt64(0);
                br.AssertInt64(0);

                Name = br.GetUTF16(start + nameOffset);
                Placeholder = br.GetUTF16(start + sibOffset);
                if (HasUnk1)
                {
                    br.Position = start + unkOffset1;
                    ReadUnk1(br);
                }
                if (HasUnk2)
                {
                    br.Position = start + unkOffset2;
                    ReadUnk2(br);
                }

                br.Position = start + entityDataOffset;
                EntityID = br.ReadInt32();
                UnkE04 = br.ReadByte();
                UnkE05 = br.ReadByte();
                UnkE06 = br.ReadByte();
                LanternID = br.ReadByte();
                LodParamID = br.ReadByte();
                UnkE09 = br.ReadByte();
                IsPointLightShadowSrc = br.ReadBoolean();
                UnkE0B = br.ReadByte();
                IsShadowSrc = br.ReadBoolean();
                IsStaticShadowSrc = br.ReadByte();
                IsCascade3ShadowSrc = br.ReadByte();
                UnkE0F = br.ReadByte();
                UnkE10 = br.ReadByte();
                IsShadowDest = br.ReadBoolean();
                IsShadowOnly = br.ReadBoolean();
                DrawByReflectCam = br.ReadBoolean();
                DrawOnlyReflectCam = br.ReadBoolean();
                EnableOnAboveShadow = br.ReadByte();
                DisablePointLightEffect = br.ReadBoolean();
                UnkE17 = br.ReadByte();
                UnkE18 = br.ReadInt32();
                EntityGroupIDs = br.ReadInt32s(8);
                UnkE3C = br.ReadInt32();
                UnkE40 = br.ReadInt32();
                br.AssertNull(0x10, false);

                if (HasUnk5)
                {
                    br.Position = start + unkOffset5;
                    ReadUnk5(br);
                }
                if (HasUnk6)
                {
                    br.Position = start + unkOffset6;
                    ReadUnk6(br);
                }
                if (HasUnk7)
                {
                    br.Position = start + unkOffset7;
                    ReadUnk7(br);
                }
                br.Position = start + typeDataOffset;
            }

            internal virtual void ReadUnk1(BinaryReaderEx br)
            {
                throw new InvalidOperationException("Unk struct 1 should not be read for parts with no unk struct 1.");
            }

            internal virtual void ReadUnk2(BinaryReaderEx br)
            {
                throw new InvalidOperationException("Unk struct 2 should not be read for parts with no unk struct 2.");
            }

            internal virtual void ReadUnk5(BinaryReaderEx br)
            {
                throw new InvalidOperationException("Unk struct 5 should not be read for parts with no unk struct 5.");
            }

            internal virtual void ReadUnk6(BinaryReaderEx br)
            {
                throw new InvalidOperationException("Unk struct 6 should not be read for parts with no unk struct 6.");
            }

            internal virtual void ReadUnk7(BinaryReaderEx br)
            {
                throw new InvalidOperationException("Unk struct 7 should not be read for parts with no unk struct 7.");
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(ModelIndex);
                bw.WriteInt32(0);
                bw.ReserveInt64("SibOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteInt32(-1);
                bw.WriteInt32(-1);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkOffset1");
                bw.ReserveInt64("UnkOffset2");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("UnkOffset5");
                bw.ReserveInt64("UnkOffset6");
                bw.ReserveInt64("UnkOffset7");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(Placeholder, true);
                bw.Pad(8);

                if (HasUnk1)
                {
                    bw.FillInt64("UnkOffset1", bw.Position - start);
                    WriteUnk1(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset1", 0);
                }

                if (HasUnk2)
                {
                    bw.FillInt64("UnkOffset2", bw.Position - start);
                    WriteUnk2(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset2", 0);
                }

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                bw.WriteInt32(EntityID);
                bw.WriteByte(UnkE04);
                bw.WriteByte(UnkE05);
                bw.WriteByte(UnkE06);
                bw.WriteByte(LanternID);
                bw.WriteByte(LodParamID);
                bw.WriteByte(UnkE09);
                bw.WriteBoolean(IsPointLightShadowSrc);
                bw.WriteByte(UnkE0B);
                bw.WriteBoolean(IsShadowSrc);
                bw.WriteByte(IsStaticShadowSrc);
                bw.WriteByte(IsCascade3ShadowSrc);
                bw.WriteByte(UnkE0F);
                bw.WriteByte(UnkE10);
                bw.WriteBoolean(IsShadowDest);
                bw.WriteBoolean(IsShadowOnly);
                bw.WriteBoolean(DrawByReflectCam);
                bw.WriteBoolean(DrawOnlyReflectCam);
                bw.WriteByte(EnableOnAboveShadow);
                bw.WriteBoolean(DisablePointLightEffect);
                bw.WriteByte(UnkE17);
                bw.WriteInt32(UnkE18);
                bw.WriteInt32s(EntityGroupIDs);
                bw.WriteInt32(UnkE3C);
                bw.WriteInt32(UnkE40);
                bw.WriteNull(0x10, false);
                bw.Pad(8);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);

                if (HasUnk5)
                {
                    bw.FillInt64("UnkOffset5", bw.Position - start);
                    WriteUnk5(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset5", 0);
                }

                if (HasUnk6)
                {
                    bw.FillInt64("UnkOffset6", bw.Position - start);
                    WriteUnk6(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset6", 0);
                }

                if (HasUnk7)
                {
                    bw.FillInt64("UnkOffset7", bw.Position - start);
                    WriteUnk7(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset7", 0);
                }
            }

            internal abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void WriteUnk1(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Unk struct 1 should not be written for parts with no unk struct 1.");
            }

            internal virtual void WriteUnk2(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Unk struct 2 should not be written for parts with no unk struct 2.");
            }

            internal virtual void WriteUnk5(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Unk struct 5 should not be written for parts with no unk struct 5.");
            }

            internal virtual void WriteUnk6(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Unk struct 6 should not be written for parts with no unk struct 6.");
            }

            internal virtual void WriteUnk7(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Unk struct 7 should not be written for parts with no unk struct 7.");
            }

            internal virtual void GetNames(MSBS msb, Entries entries)
            {
                ModelName = GetName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSBS msb, Entries entries)
            {
                ModelIndex = GetIndex(entries.Models, ModelName);
            }

            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            public class UnkStruct1
            {
                public uint[] CollisionMask { get; private set; }

                public byte Condition1 { get; set; }

                public byte Condition2 { get; set; }

                public UnkStruct1()
                {
                    CollisionMask = new uint[48];
                    Condition1 = 0;
                    Condition2 = 0;
                }

                public UnkStruct1(UnkStruct1 clone)
                {
                    CollisionMask = (uint[])clone.CollisionMask.Clone();
                    Condition1 = clone.Condition1;
                    Condition2 = clone.Condition2;
                }

                internal UnkStruct1(BinaryReaderEx br)
                {
                    CollisionMask = br.ReadUInt32s(48);
                    Condition1 = br.ReadByte();
                    Condition2 = br.ReadByte();
                    br.AssertInt16(0);
                    br.AssertNull(0xC0, false);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(CollisionMask);
                    bw.WriteByte(Condition1);
                    bw.WriteByte(Condition2);
                    bw.WriteInt16(0);
                    bw.WriteNull(0xC0, false);
                }
            }

            public class UnkStruct2
            {
                public int Condition { get; set; }

                public int[] DispGroups { get; private set; }

                public short Unk24 { get; set; }

                public short Unk26 { get; set; }

                public UnkStruct2()
                {
                    DispGroups = new int[8];
                }

                internal UnkStruct2(BinaryReaderEx br)
                {
                    Condition = br.ReadInt32();
                    DispGroups = br.ReadInt32s(8);
                    Unk24 = br.ReadInt16();
                    Unk26 = br.ReadInt16();
                    br.AssertNull(0x20, false);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Condition);
                    bw.WriteInt32s(DispGroups);
                    bw.WriteInt16(Unk24);
                    bw.WriteInt16(Unk26);
                    bw.WriteNull(0x20, false);
                }
            }

            public class UnkStruct5
            {
                public int Unk00 { get; set; }

                public int Unk04 { get; set; }

                public int Unk08 { get; set; }

                public int Unk0C { get; set; }

                public UnkStruct5() { }

                public UnkStruct5(UnkStruct5 clone)
                {
                    Unk00 = clone.Unk00;
                    Unk04 = clone.Unk04;
                    Unk08 = clone.Unk08;
                    Unk0C = clone.Unk0C;
                }

                internal UnkStruct5(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    br.AssertNull(0x10, false);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteNull(0x10, false);
                }
            }

            public class UnkStruct6
            {
                public int Unk3C { get; set; }

                public float Unk40 { get; set; }

                public UnkStruct6() { }

                internal UnkStruct6(BinaryReaderEx br)
                {
                    br.AssertNull(0x3C, false);
                    Unk3C = br.ReadInt32();
                    Unk40 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteNull(0x3C, false);
                    bw.WriteInt32(Unk3C);
                    bw.WriteSingle(Unk40);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class UnkStruct7
            {
                public int Unk00 { get; set; }

                public int Unk04 { get; set; }

                public int Unk08 { get; set; }

                public int Unk0C { get; set; }

                public int Unk10 { get; set; }

                public int Unk14 { get; set; }

                public UnkStruct7() { }

                internal UnkStruct7(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                }
            }

            public class MapPiece : Part
            {
                public override PartType Type => PartType.MapPiece;

                internal override bool HasUnk1 => true;
                internal override bool HasUnk2 => false;
                internal override bool HasUnk5 => true;
                internal override bool HasUnk6 => false;
                internal override bool HasUnk7 => true;

                public UnkStruct1 Unk1 { get; set; }

                public UnkStruct5 Unk5 { get; set; }

                public UnkStruct7 Unk7 { get; set; }

                public MapPiece() : base()
                {
                    Unk1 = new UnkStruct1();
                    Unk5 = new UnkStruct5();
                    Unk7 = new UnkStruct7();
                }

                internal MapPiece(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void ReadUnk1(BinaryReaderEx br)
                {
                    Unk1 = new UnkStruct1(br);
                }

                internal override void ReadUnk5(BinaryReaderEx br)
                {
                    Unk5 = new UnkStruct5(br);
                }

                internal override void ReadUnk7(BinaryReaderEx br)
                {
                    Unk7 = new UnkStruct7(br);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void WriteUnk1(BinaryWriterEx bw)
                {
                    Unk1.Write(bw);
                }

                internal override void WriteUnk5(BinaryWriterEx bw)
                {
                    Unk5.Write(bw);
                }

                internal override void WriteUnk7(BinaryWriterEx bw)
                {
                    Unk7.Write(bw);
                }
            }

            public class Object : DummyObject
            {
                public override PartType Type => PartType.Object;

                internal override bool HasUnk1 => true;

                public UnkStruct1 Unk1 { get; set; }

                public Object() : base()
                {
                    Unk1 = new UnkStruct1();
                }

                public Object(Object clone) : base(clone)
                {
                    Unk1 = new UnkStruct1(clone.Unk1);
                }

                public Object(DummyObject clone) : base(clone)
                {
                    Unk1 = new UnkStruct1();
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                internal override void ReadUnk1(BinaryReaderEx br)
                {
                    Unk1 = new UnkStruct1(br);
                }

                internal override void WriteUnk1(BinaryWriterEx bw)
                {
                    Unk1.Write(bw);
                }
            }

            public class Enemy : DummyEnemy
            {
                public override PartType Type => PartType.Enemy;

                internal override bool HasUnk1 => true;

                public UnkStruct1 Unk1 { get; set; }

                public Enemy() : base()
                {
                    Unk1 = new UnkStruct1();
                }

                public Enemy(Enemy clone) : base(clone)
                {
                    Unk1 = new UnkStruct1(clone.Unk1);
                }

                public Enemy(DummyEnemy clone) : base(clone)
                {
                    Unk1 = new UnkStruct1();
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

                internal override void ReadUnk1(BinaryReaderEx br)
                {
                    Unk1 = new UnkStruct1(br);
                }

                internal override void WriteUnk1(BinaryWriterEx bw)
                {
                    Unk1.Write(bw);
                }
            }

            public class Player : Part
            {
                public override PartType Type => PartType.Player;

                internal override bool HasUnk1 => false;
                internal override bool HasUnk2 => false;
                internal override bool HasUnk5 => false;
                internal override bool HasUnk6 => false;
                internal override bool HasUnk7 => false;

                public Player() : base() { }

                internal Player(BinaryReaderEx br) : base(br)
                {
                    br.AssertNull(0x10, false);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteNull(0x10, false);
                }
            }

            public class Collision : Part
            {
                public override PartType Type => PartType.Collision;

                internal override bool HasUnk1 => true;
                internal override bool HasUnk2 => true;
                internal override bool HasUnk5 => true;
                internal override bool HasUnk6 => true;
                internal override bool HasUnk7 => false;

                public UnkStruct1 Unk1 { get; set; }

                public UnkStruct2 Unk2 { get; set; }

                public UnkStruct5 Unk5 { get; set; }

                public UnkStruct6 Unk6 { get; set; }

                public byte HitFilterID { get; set; }

                public byte SoundSpaceType { get; set; }

                public float ReflectPlaneHeight { get; set; }

                public short MapNameID { get; set; }

                public bool DisableStart { get; set; }

                public byte UnkT17 { get; set; }

                public int DisableBonfireEntityID { get; set; }

                public byte UnkT24 { get; set; }

                public byte UnkT25 { get; set; }

                public byte UnkT26 { get; set; }

                public byte MapVisibility { get; set; }

                public int PlayRegionID { get; set; }

                public short LockCamParamID { get; set; }

                public int UnkT3C { get; set; }

                public int UnkT40 { get; set; }

                public float UnkT44 { get; set; }

                public float UnkT48 { get; set; }

                public int UnkT4C { get; set; }

                public float UnkT50 { get; set; }

                public float UnkT54 { get; set; }

                public Collision() : base()
                {
                    Unk1 = new UnkStruct1();
                    Unk2 = new UnkStruct2();
                    Unk5 = new UnkStruct5();
                    Unk6 = new UnkStruct6();
                    DisableBonfireEntityID = -1;
                }

                internal Collision(BinaryReaderEx br) : base(br)
                {
                    HitFilterID = br.ReadByte();
                    SoundSpaceType = br.ReadByte();
                    br.AssertInt16(0);
                    ReflectPlaneHeight = br.ReadSingle();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    MapNameID = br.ReadInt16();
                    DisableStart = br.ReadBoolean();
                    UnkT17 = br.ReadByte();
                    DisableBonfireEntityID = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    UnkT24 = br.ReadByte();
                    UnkT25 = br.ReadByte();
                    UnkT26 = br.ReadByte();
                    MapVisibility = br.ReadByte();
                    PlayRegionID = br.ReadInt32();
                    LockCamParamID = br.ReadInt16();
                    br.AssertInt16(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    UnkT3C = br.ReadInt32();
                    UnkT40 = br.ReadInt32();
                    UnkT44 = br.ReadSingle();
                    UnkT48 = br.ReadSingle();
                    UnkT4C = br.ReadInt32();
                    UnkT50 = br.ReadSingle();
                    UnkT54 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void ReadUnk1(BinaryReaderEx br)
                {
                    Unk1 = new UnkStruct1(br);
                }

                internal override void ReadUnk2(BinaryReaderEx br)
                {
                    Unk2 = new UnkStruct2(br);
                }

                internal override void ReadUnk5(BinaryReaderEx br)
                {
                    Unk5 = new UnkStruct5(br);
                }

                internal override void ReadUnk6(BinaryReaderEx br)
                {
                    Unk6 = new UnkStruct6(br);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteByte(SoundSpaceType);
                    bw.WriteInt16(0);
                    bw.WriteSingle(ReflectPlaneHeight);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(MapNameID);
                    bw.WriteBoolean(DisableStart);
                    bw.WriteByte(UnkT17);
                    bw.WriteInt32(DisableBonfireEntityID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteByte(UnkT24);
                    bw.WriteByte(UnkT25);
                    bw.WriteByte(UnkT26);
                    bw.WriteByte(MapVisibility);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamParamID);
                    bw.WriteInt16(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteInt32(UnkT40);
                    bw.WriteSingle(UnkT44);
                    bw.WriteSingle(UnkT48);
                    bw.WriteInt32(UnkT4C);
                    bw.WriteSingle(UnkT50);
                    bw.WriteSingle(UnkT54);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void WriteUnk1(BinaryWriterEx bw)
                {
                    Unk1.Write(bw);
                }

                internal override void WriteUnk2(BinaryWriterEx bw)
                {
                    Unk2.Write(bw);
                }

                internal override void WriteUnk5(BinaryWriterEx bw)
                {
                    Unk5.Write(bw);
                }

                internal override void WriteUnk6(BinaryWriterEx bw)
                {
                    Unk6.Write(bw);
                }
            }

            public class DummyObject : Part
            {
                public override PartType Type => PartType.DummyObject;

                internal override bool HasUnk1 => false;
                internal override bool HasUnk2 => false;
                internal override bool HasUnk5 => true;
                internal override bool HasUnk6 => false;
                internal override bool HasUnk7 => false;

                public UnkStruct5 Unk5 { get; set; }

                public string CollisionPartName1 { get; set; }
                private int CollisionPartIndex1;

                public byte UnkT0C { get; set; }

                public bool EnableObjAnimNetSyncStructure { get; set; }

                public byte UnkT0E { get; set; }

                public bool SetMainObjStructureBooleans { get; set; }

                public short AnimID { get; set; }

                public short UnkT18 { get; set; }

                public short UnkT1A { get; set; }

                public int UnkT20 { get; set; }

                public string CollisionPartName2 { get; set; }
                private int CollisionPartIndex2;

                public DummyObject() : base()
                {
                    Unk5 = new UnkStruct5();
                }

                public DummyObject(DummyObject clone) : base(clone)
                {
                    Unk5 = new UnkStruct5(clone.Unk5);
                    CollisionPartName1 = clone.CollisionPartName1;
                    UnkT0C = clone.UnkT0C;
                    EnableObjAnimNetSyncStructure = clone.EnableObjAnimNetSyncStructure;
                    UnkT0E = clone.UnkT0E;
                    SetMainObjStructureBooleans = clone.SetMainObjStructureBooleans;
                    AnimID = clone.AnimID;
                    UnkT18 = clone.UnkT18;
                    UnkT1A = clone.UnkT1A;
                    UnkT20 = clone.UnkT20;
                    CollisionPartName2 = clone.CollisionPartName2;
                }

                internal DummyObject(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    CollisionPartIndex1 = br.ReadInt32();
                    UnkT0C = br.ReadByte();
                    EnableObjAnimNetSyncStructure = br.ReadBoolean();
                    UnkT0E = br.ReadByte();
                    SetMainObjStructureBooleans = br.ReadBoolean();
                    AnimID = br.ReadInt16();
                    br.AssertInt16(-1);
                    br.AssertInt32(-1);
                    UnkT18 = br.ReadInt16();
                    UnkT1A = br.ReadInt16();
                    br.AssertInt32(-1);
                    UnkT20 = br.ReadInt32();
                    CollisionPartIndex2 = br.ReadInt32();
                }

                internal override void ReadUnk5(BinaryReaderEx br)
                {
                    Unk5 = new UnkStruct5(br);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(CollisionPartIndex1);
                    bw.WriteByte(UnkT0C);
                    bw.WriteBoolean(EnableObjAnimNetSyncStructure);
                    bw.WriteByte(UnkT0E);
                    bw.WriteBoolean(SetMainObjStructureBooleans);
                    bw.WriteInt16(AnimID);
                    bw.WriteInt16(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(UnkT18);
                    bw.WriteInt16(UnkT1A);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(CollisionPartIndex2);
                }

                internal override void WriteUnk5(BinaryWriterEx bw)
                {
                    Unk5.Write(bw);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionPartName1 = GetName(entries.Parts, CollisionPartIndex1);
                    CollisionPartName2 = GetName(entries.Parts, CollisionPartIndex2);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex1 = GetIndex(entries.Parts, CollisionPartName1);
                    CollisionPartIndex2 = GetIndex(entries.Parts, CollisionPartName2);
                }
            }

            public class DummyEnemy : Part
            {
                public override PartType Type => PartType.DummyEnemy;

                internal override bool HasUnk1 => false;
                internal override bool HasUnk2 => false;
                internal override bool HasUnk5 => true;
                internal override bool HasUnk6 => false;
                internal override bool HasUnk7 => false;

                public UnkStruct5 Unk5 { get; set; }

                public int ThinkParamID { get; set; }

                public int NPCParamID { get; set; }

                /// <summary>
                /// Unknown; previously talk ID, now always 0 or 1 except for the Memorial Mob in Senpou.
                /// </summary>
                public int UnkT10 { get; set; }

                public short ChrManipulatorAllocationParameter { get; set; }

                public int CharaInitID { get; set; }

                public string CollisionPartName { get; set; }
                private int CollisionPartIndex;

                public short UnkT20 { get; set; }

                public short UnkT22 { get; set; }

                public int UnkT24 { get; set; }

                public int BackupEventAnimID { get; set; }

                public int EventFlagID { get; set; }

                public int EventFlagCompareState { get; set; }

                public int UnkT48 { get; set; }

                public int UnkT4C { get; set; }

                public int UnkT50 { get; set; }

                public int UnkT78 { get; set; }

                public float UnkT84 { get; set; }

                public DummyEnemy() : base()
                {
                    Unk5 = new UnkStruct5();
                    ThinkParamID = -1;
                    NPCParamID = -1;
                    UnkT10 = -1;
                    CharaInitID = -1;
                    BackupEventAnimID = -1;
                    EventFlagID = -1;
                }

                public DummyEnemy(DummyEnemy clone) : base(clone)
                {
                    Unk5 = new UnkStruct5(clone.Unk5);
                    ThinkParamID = clone.ThinkParamID;
                    NPCParamID = clone.NPCParamID;
                    UnkT10 = clone.UnkT10;
                    ChrManipulatorAllocationParameter = clone.ChrManipulatorAllocationParameter;
                    CharaInitID = clone.CharaInitID;
                    CollisionPartName = clone.CollisionPartName;
                    UnkT20 = clone.UnkT20;
                    UnkT22 = clone.UnkT22;
                    UnkT24 = clone.UnkT24;
                    BackupEventAnimID = clone.BackupEventAnimID;
                    EventFlagID = clone.EventFlagID;
                    EventFlagCompareState = clone.EventFlagCompareState;
                    UnkT48 = clone.UnkT48;
                    UnkT4C = clone.UnkT4C;
                    UnkT50 = clone.UnkT50;
                    UnkT78 = clone.UnkT78;
                    UnkT84 = clone.UnkT84;
                }

                internal DummyEnemy(BinaryReaderEx br) : base(br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    br.AssertInt16(0);
                    ChrManipulatorAllocationParameter = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    UnkT20 = br.ReadInt16();
                    UnkT22 = br.ReadInt16();
                    UnkT24 = br.ReadInt32();
                    br.AssertNull(0x10, true);
                    BackupEventAnimID = br.ReadInt32();
                    br.AssertInt32(-1);
                    EventFlagID = br.ReadInt32();
                    EventFlagCompareState = br.ReadInt32();
                    UnkT48 = br.ReadInt32();
                    UnkT4C = br.ReadInt32();
                    UnkT50 = br.ReadInt32();
                    br.AssertInt32(1);
                    br.AssertInt32(-1);
                    br.AssertInt32(1);
                    br.AssertNull(0x18, false);
                    UnkT78 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT84 = br.ReadSingle();
                    for (int i = 0; i < 5; i++)
                    {
                        br.AssertInt32(-1);
                        br.AssertInt16(-1);
                        br.AssertInt16(0xA);
                    }
                    br.AssertNull(0x10, false);
                }

                internal override void ReadUnk5(BinaryReaderEx br)
                {
                    Unk5 = new UnkStruct5(br);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt16(0);
                    bw.WriteInt16(ChrManipulatorAllocationParameter);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(UnkT20);
                    bw.WriteInt16(UnkT22);
                    bw.WriteInt32(UnkT24);
                    bw.WriteNull(0x10, true);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(EventFlagCompareState);
                    bw.WriteInt32(UnkT48);
                    bw.WriteInt32(UnkT4C);
                    bw.WriteInt32(UnkT50);
                    bw.WriteInt32(1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(1);
                    bw.WriteNull(0x18, false);
                    bw.WriteInt32(UnkT78);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT84);
                    for (int i = 0; i < 5; i++)
                    {
                        bw.WriteInt32(-1);
                        bw.WriteInt16(-1);
                        bw.WriteInt16(0xA);
                    }
                    bw.WriteNull(0x10, false);
                }

                internal override void WriteUnk5(BinaryWriterEx bw)
                {
                    Unk5.Write(bw);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionPartName = GetName(entries.Parts, CollisionPartIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex = GetIndex(entries.Parts, CollisionPartName);
                }
            }

            public class ConnectCollision : Part
            {
                public override PartType Type => PartType.ConnectCollision;

                internal override bool HasUnk1 => false;
                internal override bool HasUnk2 => true;
                internal override bool HasUnk5 => false;
                internal override bool HasUnk6 => false;
                internal override bool HasUnk7 => false;

                public UnkStruct2 Unk2 { get; set; }

                public string CollisionName { get; set; }
                private int CollisionIndex;

                public byte[] MapID { get; private set; }

                public ConnectCollision() : base()
                {
                    Unk2 = new UnkStruct2();
                    MapID = new byte[4];
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br)
                {
                    CollisionIndex = br.ReadInt32();
                    MapID = br.ReadBytes(4);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void ReadUnk2(BinaryReaderEx br)
                {
                    Unk2 = new UnkStruct2(br);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void WriteUnk2(BinaryWriterEx bw)
                {
                    Unk2.Write(bw);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = GetName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = GetIndex(msb.Parts.Collisions, CollisionName);
                }
            }
        }
    }
}
