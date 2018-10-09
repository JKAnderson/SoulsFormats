using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB64
    {
        public class ModelSection : Section<Model>
        {
            public override string Type => "MODEL_PARAM_ST";

            public List<Model.MapPiece> MapPieces;
            public List<Model.Object> Objects;
            public List<Model.Enemy> Enemies;
            public List<Model.Player> Players;
            public List<Model.Collision> Collisions;
            public List<Model.Other> Others;

            internal ModelSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                MapPieces = new List<Model.MapPiece>();
                Objects = new List<Model.Object>();
                Enemies = new List<Model.Enemy>();
                Players = new List<Model.Player>();
                Collisions = new List<Model.Collision>();
                Others = new List<Model.Other>();
            }

            internal override List<Model> GetEntries()
            {
                return Util.ConcatAll<Model>(
                    MapPieces, Objects, Enemies, Players, Collisions, Others);
            }

            internal override Model ReadEntry(BinaryReaderEx br)
            {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 8);

                switch (type)
                {
                    case ModelType.MapPiece:
                        var mapPiece = new Model.MapPiece(br);
                        MapPieces.Add(mapPiece);
                        return mapPiece;

                    case ModelType.Object:
                        var obj = new Model.Object(br);
                        Objects.Add(obj);
                        return obj;

                    case ModelType.Enemy:
                        var enemy = new Model.Enemy(br);
                        Enemies.Add(enemy);
                        return enemy;

                    case ModelType.Player:
                        var player = new Model.Player(br);
                        Players.Add(player);
                        return player;

                    case ModelType.Collision:
                        var collision = new Model.Collision(br);
                        Collisions.Add(collision);
                        return collision;

                    case ModelType.Other:
                        var other = new Model.Other(br);
                        Others.Add(other);
                        return other;

                    default:
                        throw new NotImplementedException($"Unsupported model type: {type}");
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<Model> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    entries[i].Write(bw);
                }
            }
        }

        public enum ModelType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Item = 3,
            Player = 4,
            Collision = 5,
            Navmesh = 6,
            DummyObject = 7,
            DummyEnemy = 8,
            Other = 0xFFFFFFFF
        }

        public abstract class Model : Entry
        {
            internal abstract ModelType Type { get; }
            public override string Name { get; set; }
            public string Placeholder;
            public int ID;
            public int InstanceCount;

            internal Model(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                ID = br.ReadInt32();
                long placeholderOffset = br.ReadInt64();
                InstanceCount = br.ReadInt32();
                br.AssertInt32(0);

                Name = br.GetUTF16(start + nameOffset);
                Placeholder = br.GetUTF16(start + placeholderOffset);

                ReadSpecific(br, start);
            }

            internal abstract void ReadSpecific(BinaryReaderEx br, long start);

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(ID);
                bw.ReserveInt64("PlaceholderOffset");
                bw.WriteInt32(InstanceCount);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.FillInt64("PlaceholderOffset", bw.Position - start);
                bw.WriteUTF16(Placeholder, true);
                bw.Pad(8);

                WriteSpecific(bw, start);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw, long start);

            public override string ToString()
            {
                return $"{Type} {ID} : {Name}";
            }

            public class MapPiece : Model
            {
                internal override ModelType Type => ModelType.MapPiece;

                public int Unk1;

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br, long start)
                {
                    long unkOffset = br.ReadInt64();
                    br.Position = start + unkOffset;
                    Unk1 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("UnkOffset", bw.Position - start);
                    bw.WriteInt32(Unk1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Object : Model
            {
                internal override ModelType Type => ModelType.Object;

                public int Unk1;

                internal Object(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br, long start)
                {
                    long unkOffset = br.ReadInt64();
                    br.Position = start + unkOffset;
                    Unk1 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("UnkOffset", bw.Position - start);
                    bw.WriteInt32(Unk1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Enemy : Model
            {
                internal override ModelType Type => ModelType.Enemy;

                internal Enemy(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br, long start)
                {
                    br.AssertInt64(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("UnkOffset", 0);
                }
            }

            public class Player : Model
            {
                internal override ModelType Type => ModelType.Player;

                internal Player(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br, long start)
                {
                    br.AssertInt64(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("UnkOffset", 0);
                }
            }

            public class Collision : Model
            {
                internal override ModelType Type => ModelType.Collision;

                internal Collision(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br, long start)
                {
                    br.AssertInt64(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("UnkOffset", 0);
                }
            }

            public class Other : Model
            {
                internal override ModelType Type => ModelType.Other;

                internal Other(BinaryReaderEx br) : base(br) { }

                internal override void ReadSpecific(BinaryReaderEx br, long start)
                {
                    br.AssertInt64(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, long start)
                {
                    bw.FillInt64("UnkOffset", 0);
                }
            }
        }
    }
}
