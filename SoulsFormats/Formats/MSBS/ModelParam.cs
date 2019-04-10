using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    public partial class MSBS
    {
        public enum ModelType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
        }

        public class ModelParam : Param<Model>
        {
            public List<Model.MapPiece> MapPieces { get; set; }

            public List<Model.Object> Objects { get; set; }

            public List<Model.Enemy> Enemies { get; set; }

            public List<Model.Player> Players { get; set; }

            public List<Model.Collision> Collisions { get; set; }

            public ModelParam() : this(0x23) { }

            public ModelParam(int unk00) : base(unk00, "MODEL_PARAM_ST")
            {
                MapPieces = new List<Model.MapPiece>();
                Objects = new List<Model.Object>();
                Enemies = new List<Model.Enemy>();
                Players = new List<Model.Player>();
                Collisions = new List<Model.Collision>();
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

                    default:
                        throw new NotImplementedException($"Unimplemented model type: {type}");
                }
            }

            public override List<Model> GetEntries()
            {
                return SFUtil.ConcatAll<Model>(
                    MapPieces, Objects, Enemies, Players, Collisions);
            }
        }

        public abstract class Model : Entry
        {
            public abstract ModelType Type { get; }

            internal abstract bool HasTypeData { get; }

            public override string Name { get; set; }

            public string Placeholder { get; set; }

            internal int InstanceCount;

            public int Unk1C { get; set; }

            internal Model(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                long sibOffset = br.ReadInt64();
                InstanceCount = br.ReadInt32();
                Unk1C = br.ReadInt32();
                long typeDataOffset = br.ReadInt64();

                Name = br.GetUTF16(start + nameOffset);
                Placeholder = br.GetUTF16(start + sibOffset);
                br.Position = start + typeDataOffset;
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.ReserveInt64("SibOffset");
                bw.WriteInt32(InstanceCount);
                bw.WriteInt32(Unk1C);
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(Placeholder, true);
                bw.Pad(8);

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            internal virtual void WriteTypeData(BinaryWriterEx bw)
            {
                throw new InvalidOperationException("Type data should not be written for models with no type data.");
            }

            internal void CountInstances(List<Part> parts)
            {
                InstanceCount = parts.Count(p => p.ModelName == Name);
            }

            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            public class MapPiece : Model
            {
                public override ModelType Type => ModelType.MapPiece;

                internal override bool HasTypeData => true;

                public int UnkT00 { get; set; }

                public float UnkT04 { get; set; }

                public float UnkT08 { get; set; }

                public float UnkT0C { get; set; }

                public float UnkT10 { get; set; }

                public float UnkT14 { get; set; }

                public float UnkT18 { get; set; }

                internal MapPiece(BinaryReaderEx br) : base(br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteInt32(0);
                }
            }

            public class Object : Model
            {
                public override ModelType Type => ModelType.Object;

                internal override bool HasTypeData => false;

                internal Object(BinaryReaderEx br) : base(br) { }
            }

            public class Enemy : Model
            {
                public override ModelType Type => ModelType.Enemy;

                internal override bool HasTypeData => false;

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            public class Player : Model
            {
                public override ModelType Type => ModelType.Player;

                internal override bool HasTypeData => false;

                internal Player(BinaryReaderEx br) : base(br) { }
            }

            public class Collision : Model
            {
                public override ModelType Type => ModelType.Collision;

                internal override bool HasTypeData => false;

                internal Collision(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
