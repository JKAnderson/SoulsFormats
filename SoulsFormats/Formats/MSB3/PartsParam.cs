using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// Instances of various "things" in this MSB.
        /// </summary>
        public class PartsParam : Param<Part>
        {
            internal override string Type => "PARTS_PARAM_ST";

            /// <summary>
            /// Map pieces in the MSB.
            /// </summary>
            public List<Part.MapPiece> MapPieces;

            /// <summary>
            /// Objects in the MSB.
            /// </summary>
            public List<Part.Object> Objects;

            /// <summary>
            /// Enemies in the MSB.
            /// </summary>
            public List<Part.Enemy> Enemies;

            /// <summary>
            /// Players in the MSB.
            /// </summary>
            public List<Part.Player> Players;

            /// <summary>
            /// Collisions in the MSB.
            /// </summary>
            public List<Part.Collision> Collisions;

            /// <summary>
            /// Dummy objects in the MSB.
            /// </summary>
            public List<Part.DummyObject> DummyObjects;

            /// <summary>
            /// Dummy enemies in the MSB.
            /// </summary>
            public List<Part.DummyEnemy> DummyEnemies;

            /// <summary>
            /// Connect collisions in the MSB.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions;

            /// <summary>
            /// Creates a new PartsParam with no parts.
            /// </summary>
            public PartsParam(int unk1 = 3) : base(unk1)
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

            /// <summary>
            /// Returns every part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries()
            {
                return SFUtil.ConcatAll<Part>(
                    MapPieces, Objects, Enemies, Players, Collisions, DummyObjects, DummyEnemies, ConnectCollisions);
            }

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartsType type = br.GetEnum32<PartsType>(br.Position + 8);

                switch (type)
                {
                    case PartsType.MapPiece:
                        var mapPiece = new Part.MapPiece(br);
                        MapPieces.Add(mapPiece);
                        return mapPiece;

                    case PartsType.Object:
                        var obj = new Part.Object(br);
                        Objects.Add(obj);
                        return obj;

                    case PartsType.Enemy:
                        var enemy = new Part.Enemy(br);
                        Enemies.Add(enemy);
                        return enemy;

                    case PartsType.Player:
                        var player = new Part.Player(br);
                        Players.Add(player);
                        return player;

                    case PartsType.Collision:
                        var collision = new Part.Collision(br);
                        Collisions.Add(collision);
                        return collision;

                    case PartsType.DummyObject:
                        var dummyObj = new Part.DummyObject(br);
                        DummyObjects.Add(dummyObj);
                        return dummyObj;

                    case PartsType.DummyEnemy:
                        var dummyEne = new Part.DummyEnemy(br);
                        DummyEnemies.Add(dummyEne);
                        return dummyEne;

                    case PartsType.ConnectCollision:
                        var connectColl = new Part.ConnectCollision(br);
                        ConnectCollisions.Add(connectColl);
                        return connectColl;

                    default:
                        throw new NotImplementedException($"Unsupported part type: {type}");
                }
            }

            internal override void WriteEntry(BinaryWriterEx bw, int id, Part entry)
            {
                entry.Write(bw, id);
            }
        }

        internal enum PartsType : uint
        {
            MapPiece = 0x0,
            Object = 0x1,
            Enemy = 0x2,
            Item = 0x3,
            Player = 0x4,
            Collision = 0x5,
            NPCWander = 0x6,
            Protoboss = 0x7,
            Navmesh = 0x8,
            DummyObject = 0x9,
            DummyEnemy = 0xA,
            ConnectCollision = 0xB,
        }

        /// <summary>
        /// Any instance of some "thing" in a map.
        /// </summary>
        public abstract class Part : Entry
        {
            internal abstract PartsType Type { get; }

            internal abstract bool HasUnk3 { get; }
            internal abstract bool HasUnk4 { get; }

            /// <summary>
            /// The name of this part.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// The placeholder model for this part.
            /// </summary>
            public string Placeholder;

            private int modelIndex;
            /// <summary>
            /// The name of this part's model.
            /// </summary>
            public string ModelName;

            /// <summary>
            /// The center of the part.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The rotation of the part.
            /// </summary>
            public Vector3 Rotation;

            /// <summary>
            /// The scale of the part, which only really works right for map pieces.
            /// </summary>
            public Vector3 Scale;

            /// <summary>
            /// Unknown; related to which parts do or don't appear in different ceremonies.
            /// </summary>
            public int MapStudioLayer;

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] DrawGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] DispGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] BackreadGroups { get; private set; }

            /// <summary>
            /// Used to identify the part in event scripts.
            /// </summary>
            public int EventEntityID;

            /// <summary>
            /// Used to identify multiple parts with the same ID in event scripts.
            /// </summary>
            public int[] EventEntityGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte OldLightID, OldFogID, OldScatterID, OldLensFlareID;

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte LanternID, LodParamID, UnkB0E;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool PointLightShadowSource, ShadowSource, ShadowDest, IsShadowOnly, DrawByReflectCam,
                DrawOnlyReflectCam, UseDepthBiasFloat, DisablePointLightEffect, UnkB17;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkB18;

            internal Part(string name)
            {
                Name = name;
                Scale = Vector3.One;
                DrawGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                DispGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                BackreadGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                EventEntityID = -1;
                EventEntityGroups = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
            }

            internal Part(Part clone)
            {
                Name = clone.Name;
                Placeholder = clone.Placeholder;
                ModelName = clone.ModelName;
                Position = clone.Position;
                Rotation = clone.Rotation;
                Scale = clone.Scale;
                MapStudioLayer = clone.MapStudioLayer;
                DrawGroups = (uint[])clone.DrawGroups.Clone();
                DispGroups = (uint[])clone.DispGroups.Clone();
                BackreadGroups = (uint[])clone.BackreadGroups.Clone();
                EventEntityID = clone.EventEntityID;
                OldLightID = clone.OldLightID;
                OldFogID = clone.OldFogID;
                OldScatterID = clone.OldScatterID;
                OldLensFlareID = clone.OldLensFlareID;
                LanternID = clone.LanternID;
                LodParamID = clone.LodParamID;
                UnkB0E = clone.UnkB0E;
                PointLightShadowSource = clone.PointLightShadowSource;
                ShadowSource = clone.ShadowSource;
                ShadowDest = clone.ShadowDest;
                IsShadowOnly = clone.IsShadowOnly;
                DrawByReflectCam = clone.DrawByReflectCam;
                DrawOnlyReflectCam = clone.DrawOnlyReflectCam;
                UseDepthBiasFloat = clone.UseDepthBiasFloat;
                DisablePointLightEffect = clone.DisablePointLightEffect;
                UnkB17 = clone.UnkB17;
                UnkB18 = clone.UnkB18;
                EventEntityGroups = (int[])clone.EventEntityGroups.Clone();
            }

            internal Part(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                modelIndex = br.ReadInt32();
                br.AssertInt32(0);
                long placeholderOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();

                br.AssertInt32(-1);
                MapStudioLayer = br.ReadInt32();
                DrawGroups = br.ReadUInt32s(8);
                DispGroups = br.ReadUInt32s(8);
                BackreadGroups = br.ReadUInt32s(8);
                br.AssertInt32(0);

                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long unkOffset3 = br.ReadInt64();
                long unkOffset4 = br.ReadInt64();

                Name = br.GetUTF16(start + nameOffset);
                Placeholder = br.GetUTF16(start + placeholderOffset);

                br.Position = start + baseDataOffset;
                EventEntityID = br.ReadInt32();
                OldLightID = br.ReadSByte();
                OldFogID = br.ReadSByte();
                OldScatterID = br.ReadSByte();
                OldLensFlareID = br.ReadSByte();
                br.AssertInt32(0);
                LanternID = br.ReadSByte();
                LodParamID = br.ReadSByte();
                UnkB0E = br.ReadSByte();
                PointLightShadowSource = br.ReadBoolean();
                ShadowSource = br.ReadBoolean();
                ShadowDest = br.ReadBoolean();
                IsShadowOnly = br.ReadBoolean();
                DrawByReflectCam = br.ReadBoolean();
                DrawOnlyReflectCam = br.ReadBoolean();
                UseDepthBiasFloat = br.ReadBoolean();
                DisablePointLightEffect = br.ReadBoolean();
                UnkB17 = br.ReadBoolean();
                UnkB18 = br.ReadInt32();
                EventEntityGroups = br.ReadInt32s(8);
                br.AssertInt32(0);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);

                if (HasUnk3)
                {
                    br.Position = start + unkOffset3;
                    ReadUnk3(br);
                }

                if (HasUnk4)
                {
                    br.Position = start + unkOffset4;
                    ReadUnk4(br);
                }
            }

            internal abstract void ReadTypeData(BinaryReaderEx br);

            internal virtual void ReadUnk3(BinaryReaderEx br)
            {
                throw new InvalidOperationException("Unk struct 3 should not be read for parts with no unk struct 3.");
            }

            internal virtual void ReadUnk4(BinaryReaderEx br)
            {
                throw new InvalidOperationException("Unk struct 4 should not be read for parts with no unk struct 4.");
            }

            internal void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(modelIndex);
                bw.WriteInt32(0);
                bw.ReserveInt64("PlaceholderOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);

                bw.WriteInt32(-1);
                bw.WriteInt32(MapStudioLayer);
                bw.WriteUInt32s(DrawGroups);
                bw.WriteUInt32s(DispGroups);
                bw.WriteUInt32s(BackreadGroups);
                bw.WriteInt32(0);

                bw.ReserveInt64("BaseDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("UnkOffset3");
                bw.ReserveInt64("UnkOffset4");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(ReambiguateName(Name), true);
                bw.FillInt64("PlaceholderOffset", bw.Position - start);
                bw.WriteUTF16(Placeholder, true);
                // This is purely here for byte-perfect writes because From is nasty
                if (Placeholder == "")
                    bw.WriteNull(0x24, false);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset", bw.Position - start);
                bw.WriteInt32(EventEntityID);

                bw.WriteSByte(OldLightID);
                bw.WriteSByte(OldFogID);
                bw.WriteSByte(OldScatterID);
                bw.WriteSByte(OldLensFlareID);

                bw.WriteInt32(0);

                bw.WriteSByte(LanternID);
                bw.WriteSByte(LodParamID);
                bw.WriteSByte(UnkB0E);
                bw.WriteBoolean(PointLightShadowSource);

                bw.WriteBoolean(ShadowSource);
                bw.WriteBoolean(ShadowDest);
                bw.WriteBoolean(IsShadowOnly);
                bw.WriteBoolean(DrawByReflectCam);

                bw.WriteBoolean(DrawOnlyReflectCam);
                bw.WriteBoolean(UseDepthBiasFloat);
                bw.WriteBoolean(DisablePointLightEffect);
                bw.WriteBoolean(UnkB17);

                bw.WriteInt32(UnkB18);
                bw.WriteInt32s(EventEntityGroups);
                bw.WriteInt32(0);
                bw.Pad(8);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);

                if (HasUnk3)
                {
                    bw.FillInt64("UnkOffset3", bw.Position - start);
                    WriteUnk3(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset3", 0);
                }

                if (HasUnk4)
                {
                    bw.FillInt64("UnkOffset4", bw.Position - start);
                    WriteUnk4(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset4", 0);
                }
            }

            internal abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void WriteUnk3(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Unk struct 3 should not be written for parts with no unk struct 3.");
            }

            internal virtual void WriteUnk4(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Unk struct 4 should not be written for parts with no unk struct 4.");
            }

            internal virtual void GetNames(MSB3 msb, Entries entries)
            {
                ModelName = GetName(entries.Models, modelIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries)
            {
                modelIndex = GetIndex(entries.Models, ModelName);
            }

            /// <summary>
            /// Returns the type and name of this part.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} : {Name}";
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct3
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Creates an UnkStruct3 with default values.
                /// </summary>
                public UnkStruct3() { }

                /// <summary>
                /// Clones an existing UnkStruct3.
                /// </summary>
                public UnkStruct3(UnkStruct3 clone)
                {
                    Unk00 = clone.Unk00;
                    Unk04 = clone.Unk04;
                    Unk08 = clone.Unk08;
                    Unk0C = clone.Unk0C;
                }

                internal UnkStruct3(BinaryReaderEx br)
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

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct4
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk40 { get; set; }

                /// <summary>
                /// Creates an UnkStruct4 with default values.
                /// </summary>
                public UnkStruct4() { }

                /// <summary>
                /// Clones an existing UnkStruct4.
                /// </summary>
                public UnkStruct4(UnkStruct4 clone)
                {
                    Unk3C = clone.Unk3C;
                    Unk40 = clone.Unk40;
                }

                internal UnkStruct4(BinaryReaderEx br)
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

            /// <summary>
            /// A static model making up the map.
            /// </summary>
            public class MapPiece : Part
            {
                internal override PartsType Type => PartsType.MapPiece;

                internal override bool HasUnk3 => true;
                internal override bool HasUnk4 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct3 Unk3;

                /// <summary>
                /// Creates a new MapPiece with the given name.
                /// </summary>
                public MapPiece(string name) : base(name)
                {
                    Unk3 = new UnkStruct3();
                }

                /// <summary>
                /// Creates a new MapPiece with values copied from another.
                /// </summary>
                public MapPiece(MapPiece clone) : base(clone)
                {
                    Unk3 = new UnkStruct3(clone.Unk3);
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void ReadUnk3(BinaryReaderEx br) => Unk3 = new UnkStruct3(br);

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void WriteUnk3(BinaryWriterEx bw) => Unk3.Write(bw);
            }

            /// <summary>
            /// Any dynamic object such as elevators, crates, ladders, etc.
            /// </summary>
            public class Object : Part
            {
                internal override PartsType Type => PartsType.Object;

                internal override bool HasUnk3 => true;
                internal override bool HasUnk4 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct3 Unk3;

                private int collisionPartIndex;
                /// <summary>
                /// Unknown.
                /// </summary>
                public string CollisionName;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0C, UnkT0E, UnkT10, UnkT12, UnkT14, UnkT16, UnkT18, UnkT1A, UnkT1C, UnkT1E;

                /// <summary>
                /// Creates a new Object with the given name.
                /// </summary>
                public Object(string name) : base(name)
                {
                    Unk3 = new UnkStruct3();
                }

                /// <summary>
                /// Creates a new Object with values copied from another.
                /// </summary>
                public Object(Object clone) : base(clone)
                {
                    Unk3 = new UnkStruct3(clone.Unk3);
                    CollisionName = clone.CollisionName;
                    UnkT0C = clone.UnkT0C;
                    UnkT0E = clone.UnkT0E;
                    UnkT10 = clone.UnkT10;
                    UnkT12 = clone.UnkT12;
                    UnkT14 = clone.UnkT14;
                    UnkT16 = clone.UnkT16;
                    UnkT18 = clone.UnkT18;
                    UnkT1A = clone.UnkT1A;
                    UnkT1C = clone.UnkT1C;
                    UnkT1E = clone.UnkT1E;
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    collisionPartIndex = br.ReadInt32();
                    UnkT0C = br.ReadInt16();
                    UnkT0E = br.ReadInt16();
                    UnkT10 = br.ReadInt16();
                    UnkT12 = br.ReadInt16();
                    UnkT14 = br.ReadInt16();
                    UnkT16 = br.ReadInt16();
                    UnkT18 = br.ReadInt16();
                    UnkT1A = br.ReadInt16();
                    UnkT1C = br.ReadInt16();
                    UnkT1E = br.ReadInt16();
                }

                internal override void ReadUnk3(BinaryReaderEx br) => Unk3 = new UnkStruct3(br);

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(collisionPartIndex);
                    bw.WriteInt16(UnkT0C);
                    bw.WriteInt16(UnkT0E);
                    bw.WriteInt16(UnkT10);
                    bw.WriteInt16(UnkT12);
                    bw.WriteInt16(UnkT14);
                    bw.WriteInt16(UnkT16);
                    bw.WriteInt16(UnkT18);
                    bw.WriteInt16(UnkT1A);
                    bw.WriteInt16(UnkT1C);
                    bw.WriteInt16(UnkT1E);
                }

                internal override void WriteUnk3(BinaryWriterEx bw) => Unk3.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = GetName(entries.Parts, collisionPartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    collisionPartIndex = GetIndex(entries.Parts, CollisionName);
                }
            }

            /// <summary>
            /// Any non-player character, not necessarily hostile.
            /// </summary>
            public class Enemy : Part
            {
                internal override PartsType Type => PartsType.Enemy;

                internal override bool HasUnk3 => true;
                internal override bool HasUnk4 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct3 Unk3;

                private int collisionPartIndex;
                /// <summary>
                /// Unknown.
                /// </summary>
                public string CollisionName;

                /// <summary>
                /// Controls enemy AI.
                /// </summary>
                public int ThinkParamID;

                /// <summary>
                /// Controls enemy stats.
                /// </summary>
                public int NPCParamID;

                /// <summary>
                /// Controls enemy speech.
                /// </summary>
                public int TalkID;

                /// <summary>
                /// Controls enemy equipment.
                /// </summary>
                public int CharaInitID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT04, ChrManipulatorAllocationParameter;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20, BackupEventAnimID, UnkT78;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT84;

                /// <summary>
                /// Creates a new Enemy with the given name.
                /// </summary>
                public Enemy(string name) : base(name)
                {
                    Unk3 = new UnkStruct3();
                }

                /// <summary>
                /// Creates a new Enemy with values copied from another.
                /// </summary>
                public Enemy(Enemy clone) : base(clone)
                {
                    Unk3 = new UnkStruct3(clone.Unk3);
                    ThinkParamID = clone.ThinkParamID;
                    NPCParamID = clone.NPCParamID;
                    TalkID = clone.TalkID;
                    UnkT04 = clone.UnkT04;
                    ChrManipulatorAllocationParameter = clone.ChrManipulatorAllocationParameter;
                    CharaInitID = clone.CharaInitID;
                    CollisionName = clone.CollisionName;
                    UnkT20 = clone.UnkT20;
                    BackupEventAnimID = clone.BackupEventAnimID;
                    UnkT78 = clone.UnkT78;
                    UnkT84 = clone.UnkT84;
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    TalkID = br.ReadInt32();
                    UnkT04 = br.ReadInt16();
                    ChrManipulatorAllocationParameter = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    collisionPartIndex = br.ReadInt32();
                    UnkT20 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    BackupEventAnimID = br.ReadInt32();
                    br.AssertInt32(-1); // BackupThrowAnimID
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
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
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void ReadUnk3(BinaryReaderEx br) => Unk3 = new UnkStruct3(br);

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteInt16(UnkT04);
                    bw.WriteInt16(ChrManipulatorAllocationParameter);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(collisionPartIndex);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
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
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void WriteUnk3(BinaryWriterEx bw) => Unk3.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = GetName(entries.Parts, collisionPartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    collisionPartIndex = GetIndex(entries.Parts, CollisionName);
                }
            }

            /// <summary>
            /// Unknown exactly what this is for.
            /// </summary>
            public class Player : Part
            {
                internal override PartsType Type => PartsType.Player;

                internal override bool HasUnk3 => false;
                internal override bool HasUnk4 => false;

                /// <summary>
                /// Creates a new Player with the given name.
                /// </summary>
                public Player(string name) : base(name) { }

                /// <summary>
                /// Creates a new Player with values copied from another.
                /// </summary>
                public Player(Player clone) : base(clone) { }

                internal Player(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An invisible collision mesh, also used for death planes.
            /// </summary>
            public class Collision : Part
            {
                /// <summary>
                /// Amount of reverb to apply to sounds.
                /// </summary>
                public enum SoundSpace : byte
                {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    NoReverb = 0,
                    SmallReverbA = 1,
                    SmallReverbB = 2,
                    MiddleReverbA = 3,
                    MiddleReverbB = 4,
                    LargeReverbA = 5,
                    LargeReverbB = 6,
                    ExtraLargeReverbA = 7,
                    ExtraLargeReverbB = 8,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public enum MapVisiblity : byte
                {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    Good = 0,
                    Dark = 1,
                    PitchDark = 2,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                internal override PartsType Type => PartsType.Collision;

                internal override bool HasUnk3 => true;
                internal override bool HasUnk4 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct3 Unk3;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct4 Unk4;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte HitFilterID;

                /// <summary>
                /// Modifies sounds while the player is touching this collision.
                /// </summary>
                public SoundSpace SoundSpaceType;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short EnvLightMapSpotIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MapNameID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart;

                /// <summary>
                /// Disables a bonfire with this entity ID when an enemy is touching this collision.
                /// </summary>
                public int DisableBonfireEntityID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlayRegionID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LockCamID1, LockCamID2;

                private int UnkHitIndex;
                /// <summary>
                /// Unknown. Always refers to another collision part.
                /// </summary>
                public string UnkHitName;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT2C;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT34, UnkT35, UnkT36;

                /// <summary>
                /// Unknown.
                /// </summary>
                public MapVisiblity MapVisType;

                /// <summary>
                /// Creates a new Collision with the given name.
                /// </summary>
                public Collision(string name) : base(name)
                {
                    Unk3 = new UnkStruct3();
                    Unk4 = new UnkStruct4();
                    SoundSpaceType = SoundSpace.NoReverb;
                    MapNameID = -1;
                    DisableStart = false;
                    DisableBonfireEntityID = -1;
                    MapVisType = MapVisiblity.Good;
                    PlayRegionID = -1;
                }

                /// <summary>
                /// Creates a new Collision with values copied from another.
                /// </summary>
                public Collision(Collision clone) : base(clone)
                {
                    Unk3 = new UnkStruct3(clone.Unk3);
                    Unk4 = new UnkStruct4(clone.Unk4);
                    HitFilterID = clone.HitFilterID;
                    SoundSpaceType = clone.SoundSpaceType;
                    EnvLightMapSpotIndex = clone.EnvLightMapSpotIndex;
                    ReflectPlaneHeight = clone.ReflectPlaneHeight;
                    MapNameID = clone.MapNameID;
                    DisableStart = clone.DisableStart;
                    DisableBonfireEntityID = clone.DisableBonfireEntityID;
                    UnkT2C = clone.UnkT2C;
                    UnkHitName = clone.UnkHitName;
                    UnkT34 = clone.UnkT34;
                    UnkT35 = clone.UnkT35;
                    UnkT36 = clone.UnkT36;
                    MapVisType = clone.MapVisType;
                    PlayRegionID = clone.PlayRegionID;
                    LockCamID1 = clone.LockCamID1;
                    LockCamID2 = clone.LockCamID2;
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte();
                    SoundSpaceType = br.ReadEnum8<SoundSpace>();
                    EnvLightMapSpotIndex = br.ReadInt16();
                    ReflectPlaneHeight = br.ReadSingle();
                    br.AssertInt32(0); // Navmesh Group (4)
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(-1); // Vagrant Entity ID (3)
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    MapNameID = br.ReadInt16();
                    DisableStart = br.AssertInt16(0, 1) == 1;
                    DisableBonfireEntityID = br.ReadInt32();
                    UnkT2C = br.ReadInt32();
                    UnkHitIndex = br.ReadInt32();
                    UnkT34 = br.ReadByte();
                    UnkT35 = br.ReadByte();
                    UnkT36 = br.ReadByte();
                    MapVisType = br.ReadEnum8<MapVisiblity>();
                    PlayRegionID = br.ReadInt32();
                    LockCamID1 = br.ReadInt16();
                    LockCamID2 = br.ReadInt16();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void ReadUnk3(BinaryReaderEx br) => Unk3 = new UnkStruct3(br);
                internal override void ReadUnk4(BinaryReaderEx br) => Unk4 = new UnkStruct4(br);

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteByte((byte)SoundSpaceType);
                    bw.WriteInt16(EnvLightMapSpotIndex);
                    bw.WriteSingle(ReflectPlaneHeight);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(MapNameID);
                    bw.WriteInt16((short)(DisableStart ? 1 : 0));
                    bw.WriteInt32(DisableBonfireEntityID);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(UnkHitIndex);
                    bw.WriteByte(UnkT34);
                    bw.WriteByte(UnkT35);
                    bw.WriteByte(UnkT36);
                    bw.WriteByte((byte)MapVisType);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamID1);
                    bw.WriteInt16(LockCamID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void WriteUnk3(BinaryWriterEx bw) => Unk3.Write(bw);
                internal override void WriteUnk4(BinaryWriterEx bw) => Unk4.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    UnkHitName = GetName(entries.Parts, UnkHitIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    UnkHitIndex = GetIndex(entries.Parts, UnkHitName);
                }
            }

            /// <summary>
            /// An object that is either unused, or used for a cutscene.
            /// </summary>
            public class DummyObject : Object
            {
                internal override PartsType Type => PartsType.DummyObject;

                /// <summary>
                /// Creates a new DummyObject with the given name.
                /// </summary>
                public DummyObject(string name) : base(name) { }

                /// <summary>
                /// Creates a new DummyObject with values copied from another.
                /// </summary>
                public DummyObject(DummyObject clone) : base(clone) { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An enemy that is either unused, or used for a cutscene.
            /// </summary>
            public class DummyEnemy : Enemy
            {
                internal override PartsType Type => PartsType.DummyEnemy;

                /// <summary>
                /// Creates a new DummyEnemy with the given name.
                /// </summary>
                public DummyEnemy(string name) : base(name) { }

                /// <summary>
                /// Creates a new DummyEnemy with values copied from another.
                /// </summary>
                public DummyEnemy(DummyEnemy clone) : base(clone) { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Determines which collision parts load other maps.
            /// </summary>
            public class ConnectCollision : Part
            {
                internal override PartsType Type => PartsType.ConnectCollision;

                internal override bool HasUnk3 => false;
                internal override bool HasUnk4 => false;

                private int collisionIndex;
                /// <summary>
                /// The name of the associated collision part.
                /// </summary>
                public string CollisionName;

                /// <summary>
                /// A map ID in format mXX_XX_XX_XX.
                /// </summary>
                public byte MapID1, MapID2, MapID3, MapID4;

                /// <summary>
                /// Creates a new ConnectCollision with the given name.
                /// </summary>
                public ConnectCollision(string name) : base(name)
                {
                    CollisionName = null;
                    MapID1 = 0;
                    MapID2 = 0;
                    MapID3 = 0;
                    MapID4 = 0;
                }

                /// <summary>
                /// Creates a new ConnectCollision with values copied from another.
                /// </summary>
                public ConnectCollision(ConnectCollision clone) : base(clone)
                {
                    CollisionName = clone.CollisionName;
                    MapID1 = clone.MapID1;
                    MapID2 = clone.MapID2;
                    MapID3 = clone.MapID3;
                    MapID4 = clone.MapID4;
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                internal override void ReadTypeData(BinaryReaderEx br)
                {
                    collisionIndex = br.ReadInt32();
                    MapID1 = br.ReadByte();
                    MapID2 = br.ReadByte();
                    MapID3 = br.ReadByte();
                    MapID4 = br.ReadByte();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(collisionIndex);
                    bw.WriteByte(MapID1);
                    bw.WriteByte(MapID2);
                    bw.WriteByte(MapID3);
                    bw.WriteByte(MapID4);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = GetName(msb.Parts.Collisions, collisionIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    collisionIndex = GetIndex(msb.Parts.Collisions, CollisionName);
                }
            }
        }
    }
}
