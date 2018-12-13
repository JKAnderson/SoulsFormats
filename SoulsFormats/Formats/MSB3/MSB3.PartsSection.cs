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
        public class PartsSection : Section<Part>
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

            internal PartsSection(BinaryReaderEx br, int unk1) : base(br, unk1)
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

            internal override void WriteEntries(BinaryWriterEx bw, List<Part> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    entries[i].Write(bw);
                }
            }

            internal void GetNames(MSB3 msb, Entries entries)
            {
                foreach (Part part in entries.Parts)
                    part.GetNames(msb, entries);
            }

            internal void GetIndices(MSB3 msb, Entries entries)
            {
                foreach (Part part in entries.Parts)
                    part.GetIndices(msb, entries);
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

            /// <summary>
            /// The name of this part.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// The placeholder model for this part.
            /// </summary>
            public string Placeholder;

            /// <summary>
            /// The ID of this part, which should be unique but does not appear to be used otherwise.
            /// </summary>
            public int ID;

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
            /// Unknown.
            /// </summary>
            public uint OldDrawGroup1, OldDrawGroup2, OldDrawGroup3, OldDrawGroup4;

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint OldDispGroup1, OldDispGroup2, OldDispGroup3, OldDispGroup4;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkF64, UnkF68, UnkF8C, UnkF90, UnkF94, UnkF98, UnkF9C, UnkFA0, UnkFA4, UnkFA8;

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint DrawGroup1, DrawGroup2, DrawGroup3, DrawGroup4, DrawGroup5, DrawGroup6, DrawGroup7, DrawGroup8;

            /// <summary>
            /// Used to identify the part in event scripts.
            /// </summary>
            public int EventEntityID;

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte OldLightID, OldFogID, OldScatterID, OldLensFlareID;

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte OldLanternID, OldLodParamID, UnkB0E;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool OldIsShadowDest;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool OldIsShadowOnly, OldDrawByReflectCam, OldDrawOnlyReflectCam, OldUseDepthBiasFloat;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool OldDisablePointLightEffect;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkB15, UnkB16, UnkB17;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkB18, UnkB1C, UnkB20, UnkB24, UnkB28, UnkB30, UnkB34, UnkB38;

            private long UnkOffset1Delta, UnkOffset2Delta;

            /// <summary>
            /// Creates a new Part with values copied from another.
            /// </summary>
            public Part(Part clone)
            {
                Name = clone.Name;
                Placeholder = clone.Placeholder;
                ID = clone.ID;
                ModelName = clone.ModelName;
                Position = new Vector3(clone.Position.X, clone.Position.Y, clone.Position.Z);
                Rotation = new Vector3(clone.Rotation.X, clone.Rotation.Y, clone.Rotation.Z);
                Scale = new Vector3(clone.Scale.X, clone.Scale.Y, clone.Scale.Z);
                OldDrawGroup1 = clone.OldDrawGroup1;
                OldDrawGroup2 = clone.OldDrawGroup2;
                OldDrawGroup3 = clone.OldDrawGroup3;
                OldDrawGroup4 = clone.OldDrawGroup4;
                OldDispGroup1 = clone.OldDispGroup1;
                OldDispGroup2 = clone.OldDispGroup2;
                OldDispGroup3 = clone.OldDispGroup3;
                OldDispGroup4 = clone.OldDispGroup4;
                UnkF64 = clone.UnkF64;
                UnkF68 = clone.UnkF68;
                DrawGroup1 = clone.DrawGroup1;
                DrawGroup2 = clone.DrawGroup2;
                DrawGroup3 = clone.DrawGroup3;
                DrawGroup4 = clone.DrawGroup4;
                DrawGroup5 = clone.DrawGroup5;
                DrawGroup6 = clone.DrawGroup6;
                DrawGroup7 = clone.DrawGroup7;
                DrawGroup8 = clone.DrawGroup8;
                UnkF8C = clone.UnkF8C;
                UnkF90 = clone.UnkF90;
                UnkF94 = clone.UnkF94;
                UnkF98 = clone.UnkF98;
                UnkF9C = clone.UnkF9C;
                UnkFA0 = clone.UnkFA0;
                UnkFA4 = clone.UnkFA4;
                UnkFA8 = clone.UnkFA8;
                EventEntityID = clone.EventEntityID;
                OldLightID = clone.OldLightID;
                OldFogID = clone.OldFogID;
                OldScatterID = clone.OldScatterID;
                OldLensFlareID = clone.OldLensFlareID;
                OldLanternID = clone.OldLanternID;
                OldLodParamID = clone.OldLodParamID;
                UnkB0E = clone.UnkB0E;
                OldIsShadowDest = clone.OldIsShadowDest;
                OldIsShadowOnly = clone.OldIsShadowOnly;
                OldDrawByReflectCam = clone.OldDrawByReflectCam;
                OldDrawOnlyReflectCam = clone.OldDrawOnlyReflectCam;
                OldUseDepthBiasFloat = clone.OldUseDepthBiasFloat;
                OldDisablePointLightEffect = clone.OldDisablePointLightEffect;
                UnkB15 = clone.UnkB15;
                UnkB16 = clone.UnkB16;
                UnkB17 = clone.UnkB17;
                UnkB18 = clone.UnkB18;
                UnkB1C = clone.UnkB1C;
                UnkB20 = clone.UnkB20;
                UnkB24 = clone.UnkB24;
                UnkB28 = clone.UnkB28;
                UnkB30 = clone.UnkB30;
                UnkB34 = clone.UnkB34;
                UnkB38 = clone.UnkB38;
                UnkOffset1Delta = clone.UnkOffset1Delta;
                UnkOffset2Delta = clone.UnkOffset2Delta;
            }

            internal Part(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                ID = br.ReadInt32();
                modelIndex = br.ReadInt32();
                br.AssertInt32(0);
                long placeholderOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();

                OldDrawGroup1 = br.ReadUInt32(); // -1
                OldDrawGroup2 = br.ReadUInt32();
                OldDrawGroup3 = br.ReadUInt32();
                OldDrawGroup4 = br.ReadUInt32();
                OldDispGroup1 = br.ReadUInt32();
                OldDispGroup2 = br.ReadUInt32();
                OldDispGroup3 = br.ReadUInt32();
                OldDispGroup4 = br.ReadUInt32();

                UnkF64 = br.ReadInt32();
                UnkF68 = br.ReadInt32();
                DrawGroup1 = br.ReadUInt32();
                DrawGroup2 = br.ReadUInt32();
                DrawGroup3 = br.ReadUInt32();
                DrawGroup4 = br.ReadUInt32();
                DrawGroup5 = br.ReadUInt32();
                DrawGroup6 = br.ReadUInt32();
                DrawGroup7 = br.ReadUInt32();
                DrawGroup8 = br.ReadUInt32();
                UnkF8C = br.ReadInt32();
                UnkF90 = br.ReadInt32();
                UnkF94 = br.ReadInt32();
                UnkF98 = br.ReadInt32();
                UnkF9C = br.ReadInt32();
                UnkFA0 = br.ReadInt32();
                UnkFA4 = br.ReadInt32();
                UnkFA8 = br.ReadInt32();
                br.AssertInt32(0);

                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                UnkOffset1Delta = br.ReadInt64();
                if (UnkOffset1Delta != 0)
                    UnkOffset1Delta -= typeDataOffset;
                UnkOffset2Delta = br.ReadInt64();
                if (UnkOffset2Delta != 0)
                    UnkOffset2Delta -= typeDataOffset;

                Name = br.GetUTF16(start + nameOffset);
                if (placeholderOffset == 0)
                    Placeholder = null;
                else
                    Placeholder = br.GetUTF16(start + placeholderOffset);

                br.StepIn(start + baseDataOffset);
                EventEntityID = br.ReadInt32();

                OldLightID = br.ReadSByte();
                OldFogID = br.ReadSByte();
                OldScatterID = br.ReadSByte();
                OldLensFlareID = br.ReadSByte();

                br.AssertInt32(0);

                OldLanternID = br.ReadSByte();
                OldLodParamID = br.ReadSByte();
                UnkB0E = br.ReadSByte();
                OldIsShadowDest = br.ReadBoolean();

                OldIsShadowOnly = br.ReadBoolean();
                OldDrawByReflectCam = br.ReadBoolean();
                OldDrawOnlyReflectCam = br.ReadBoolean();
                OldUseDepthBiasFloat = br.ReadBoolean();

                OldDisablePointLightEffect = br.ReadBoolean();
                UnkB15 = br.ReadByte();
                UnkB16 = br.ReadByte();
                UnkB17 = br.ReadByte();

                UnkB18 = br.ReadInt32();
                UnkB1C = br.ReadInt32();
                UnkB20 = br.ReadInt32();
                UnkB24 = br.ReadInt32();
                UnkB28 = br.ReadInt32();
                br.AssertInt32(-1);
                UnkB30 = br.ReadInt32();
                UnkB34 = br.ReadInt32();
                UnkB38 = br.ReadInt32();
                br.AssertInt32(0);
                br.StepOut();

                br.StepIn(start + typeDataOffset);
                Read(br);
                br.StepOut();
            }

            internal abstract void Read(BinaryReaderEx br);

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(ID);
                bw.WriteInt32(modelIndex);
                bw.WriteInt32(0);
                bw.ReserveInt64("PlaceholderOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);

                bw.WriteUInt32(OldDrawGroup1);
                bw.WriteUInt32(OldDrawGroup2);
                bw.WriteUInt32(OldDrawGroup3);
                bw.WriteUInt32(OldDrawGroup4);
                bw.WriteUInt32(OldDispGroup1);
                bw.WriteUInt32(OldDispGroup2);
                bw.WriteUInt32(OldDispGroup3);
                bw.WriteUInt32(OldDispGroup4);

                bw.WriteInt32(UnkF64);
                bw.WriteInt32(UnkF68);
                bw.WriteUInt32(DrawGroup1);
                bw.WriteUInt32(DrawGroup2);
                bw.WriteUInt32(DrawGroup3);
                bw.WriteUInt32(DrawGroup4);
                bw.WriteUInt32(DrawGroup5);
                bw.WriteUInt32(DrawGroup6);
                bw.WriteUInt32(DrawGroup7);
                bw.WriteUInt32(DrawGroup8);
                bw.WriteInt32(UnkF8C);
                bw.WriteInt32(UnkF90);
                bw.WriteInt32(UnkF94);
                bw.WriteInt32(UnkF98);
                bw.WriteInt32(UnkF9C);
                bw.WriteInt32(UnkFA0);
                bw.WriteInt32(UnkFA4);
                bw.WriteInt32(UnkFA8);
                bw.WriteInt32(0);

                bw.ReserveInt64("BaseDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("UnkOffset1");
                bw.ReserveInt64("UnkOffset2");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                if (Placeholder == null)
                    bw.FillInt64("PlaceholderOffset", 0);
                else
                {
                    bw.FillInt64("PlaceholderOffset", bw.Position - start);
                    bw.WriteUTF16(Placeholder, true);
                }
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset", bw.Position - start);
                bw.WriteInt32(EventEntityID);

                bw.WriteSByte(OldLightID);
                bw.WriteSByte(OldFogID);
                bw.WriteSByte(OldScatterID);
                bw.WriteSByte(OldLensFlareID);

                bw.WriteInt32(0);

                bw.WriteSByte(OldLanternID);
                bw.WriteSByte(OldLodParamID);
                bw.WriteSByte(UnkB0E);
                bw.WriteBoolean(OldIsShadowDest);

                bw.WriteBoolean(OldIsShadowOnly);
                bw.WriteBoolean(OldDrawByReflectCam);
                bw.WriteBoolean(OldDrawOnlyReflectCam);
                bw.WriteBoolean(OldUseDepthBiasFloat);

                bw.WriteBoolean(OldDisablePointLightEffect);
                bw.WriteByte(UnkB15);
                bw.WriteByte(UnkB16);
                bw.WriteByte(UnkB17);

                bw.WriteInt32(UnkB18);
                bw.WriteInt32(UnkB1C);
                bw.WriteInt32(UnkB20);
                bw.WriteInt32(UnkB24);
                bw.WriteInt32(UnkB28);
                bw.WriteInt32(-1);
                bw.WriteInt32(UnkB30);
                bw.WriteInt32(UnkB34);
                bw.WriteInt32(UnkB38);
                bw.WriteInt32(0);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                if (UnkOffset1Delta == 0)
                    bw.FillInt64("UnkOffset1", 0);
                else
                    bw.FillInt64("UnkOffset1", bw.Position - start + UnkOffset1Delta);

                if (UnkOffset2Delta == 0)
                    bw.FillInt64("UnkOffset2", 0);
                else
                    bw.FillInt64("UnkOffset2", bw.Position - start + UnkOffset2Delta);

                WriteSpecific(bw);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw);

            internal virtual void GetNames(MSB3 msb, Entries entries)
            {
                ModelName = GetName(entries.Models, modelIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries)
            {
                modelIndex = GetIndex(entries.Models, ModelName);
            }

            /// <summary>
            /// Returns the type, ID, and name of this part.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {ID} : {Name}";
            }

            /// <summary>
            /// A static model making up the map.
            /// </summary>
            public class MapPiece : Part
            {
                internal override PartsType Type => PartsType.MapPiece;

                /// <summary>
                /// Controls which value from LightSet in the gparam is used.
                /// </summary>
                public int LightParamID;

                /// <summary>
                /// Controls which value from FogParam in the gparam is used.
                /// </summary>
                public int FogParamID;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10, UnkT14;

                public MapPiece(MapPiece clone) : base(clone)
                {
                    LightParamID = clone.LightParamID;
                    FogParamID = clone.FogParamID;
                    UnkT10 = clone.UnkT10;
                    UnkT14 = clone.UnkT14;
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    LightParamID = br.ReadInt32();
                    FogParamID = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(LightParamID);
                    bw.WriteInt32(FogParamID);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Any dynamic object such as elevators, crates, ladders, etc.
            /// </summary>
            public class Object : Part
            {
                internal override PartsType Type => PartsType.Object;

                private int collisionPartIndex;
                /// <summary>
                /// Unknown.
                /// </summary>
                public string CollisionName;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04, UnkT06, UnkT07, UnkT08, UnkT09, UnkT10;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT02a, UnkT02b, UnkT03a, UnkT03b, UnkT05a, UnkT05b;

                /// <summary>
                /// Creates a new Object with values copied from another.
                /// </summary>
                public Object(Object clone) : base(clone)
                {
                    CollisionName = clone.CollisionName;
                    UnkT02a = clone.UnkT02a;
                    UnkT02b = clone.UnkT02b;
                    UnkT03a = clone.UnkT03a;
                    UnkT03b = clone.UnkT03b;
                    UnkT04 = clone.UnkT04;
                    UnkT05a = clone.UnkT05a;
                    UnkT05b = clone.UnkT05b;
                    UnkT06 = clone.UnkT06;
                    UnkT07 = clone.UnkT07;
                    UnkT08 = clone.UnkT08;
                    UnkT09 = clone.UnkT09;
                    UnkT10 = clone.UnkT10;
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    collisionPartIndex = br.ReadInt32();
                    UnkT02a = br.ReadInt16();
                    UnkT02b = br.ReadInt16();
                    UnkT03a = br.ReadInt16();
                    UnkT03b = br.ReadInt16();
                    UnkT04 = br.ReadInt32();
                    UnkT05a = br.ReadInt16();
                    UnkT05b = br.ReadInt16();
                    UnkT06 = br.ReadInt32();
                    UnkT07 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT09 = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(collisionPartIndex);
                    bw.WriteInt16(UnkT02a);
                    bw.WriteInt16(UnkT02b);
                    bw.WriteInt16(UnkT03a);
                    bw.WriteInt16(UnkT03b);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt16(UnkT05a);
                    bw.WriteInt16(UnkT05b);
                    bw.WriteInt32(UnkT06);
                    bw.WriteInt32(UnkT07);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT09);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

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
                /// Unknown, probably more paramIDs.
                /// </summary>
                public int UnkT04, UnkT07, UnkT08, UnkT09;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT11, UnkT12, UnkT13, UnkT14, UnkT15, UnkT16, UnkT17, UnkT18, UnkT19;

                /// <summary>
                /// Creates a new Enemy with values copied from another.
                /// </summary>
                public Enemy(Enemy clone) : base(clone)
                {
                    ThinkParamID = clone.ThinkParamID;
                    NPCParamID = clone.NPCParamID;
                    TalkID = clone.TalkID;
                    UnkT04 = clone.UnkT04;
                    CharaInitID = clone.CharaInitID;
                    CollisionName = clone.CollisionName;
                    UnkT07 = clone.UnkT07;
                    UnkT08 = clone.UnkT08;
                    UnkT09 = clone.UnkT09;
                    UnkT10 = clone.UnkT10;
                    UnkT11 = clone.UnkT11;
                    UnkT12 = clone.UnkT12;
                    UnkT13 = clone.UnkT13;
                    UnkT14 = clone.UnkT14;
                    UnkT15 = clone.UnkT15;
                    UnkT16 = clone.UnkT16;
                    UnkT17 = clone.UnkT17;
                    UnkT18 = clone.UnkT18;
                    UnkT19 = clone.UnkT19;
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    TalkID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    CharaInitID = br.ReadInt32();
                    collisionPartIndex = br.ReadInt32();
                    UnkT07 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    UnkT08 = br.ReadInt32();
                    br.AssertInt32(-1);
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
                    UnkT09 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT10 = br.ReadSingle();
                    br.AssertInt32(-1);
                    UnkT11 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT12 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT13 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT14 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT15 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT16 = br.ReadInt32();
                    UnkT17 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    UnkT19 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(collisionPartIndex);
                    bw.WriteInt32(UnkT07);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT08);
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
                    bw.WriteInt32(UnkT09);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT10);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT11);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT12);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT13);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT15);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT16);
                    bw.WriteInt32(UnkT17);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT19);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

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

                internal Player(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
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

                internal override PartsType Type => PartsType.Collision;

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
                public int UnkT2C, UnkT34, UnkT50, UnkT54, UnkT58, UnkT5C, UnkT74;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT78;

                internal Collision(BinaryReaderEx br) : base(br) { }

                internal override void Read(BinaryReaderEx br)
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
                    UnkT34 = br.ReadInt32();
                    PlayRegionID = br.ReadInt32();
                    LockCamID1 = br.ReadInt16();
                    LockCamID2 = br.ReadInt16();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT50 = br.ReadInt32();
                    UnkT54 = br.ReadInt32();
                    UnkT58 = br.ReadInt32();
                    UnkT5C = br.ReadInt32();

                    for (int i = 0; i < 19; i++)
                        br.AssertInt32(0);

                    UnkT74 = br.ReadInt32();
                    UnkT78 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
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
                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamID1);
                    bw.WriteInt16(LockCamID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT50);
                    bw.WriteInt32(UnkT54);
                    bw.WriteInt32(UnkT58);
                    bw.WriteInt32(UnkT5C);

                    for (int i = 0; i < 19; i++)
                        bw.WriteInt32(0);

                    bw.WriteInt32(UnkT74);
                    bw.WriteSingle(UnkT78);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

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

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An enemy that is either unused, or used for a cutscene.
            /// </summary>
            public class DummyEnemy : Enemy
            {
                internal override PartsType Type => PartsType.DummyEnemy;

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Determines which collision parts load other maps.
            /// </summary>
            public class ConnectCollision : Part
            {
                internal override PartsType Type => PartsType.ConnectCollision;

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

                internal override void Read(BinaryReaderEx br)
                {
                    collisionIndex = br.ReadInt32();
                    MapID1 = br.ReadByte();
                    MapID2 = br.ReadByte();
                    MapID3 = br.ReadByte();
                    MapID4 = br.ReadByte();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
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
