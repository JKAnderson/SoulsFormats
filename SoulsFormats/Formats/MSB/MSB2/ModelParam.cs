using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB2
    {
        internal enum ModelType : ushort
        {
            MapPiece = 0,
            Object = 1,
            Collision = 3,
            Navmesh = 4,
        }

        /// <summary>
        /// Models available for parts in the map to use.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel>
        {
            internal override int Version => 5;
            internal override string Name => "MODEL_PARAM_ST";

            /// <summary>
            /// Models for static visual elements.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Models for dynamic or interactible elements.
            /// </summary>
            public List<Model.Object> Objects { get; set; }

            /// <summary>
            /// Models for invisible but physical surfaces.
            /// </summary>
            public List<Model.Collision> Collisions { get; set; }

            /// <summary>
            /// Models for AI navigation.
            /// </summary>
            public List<Model.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Creates an empty ModelParam.
            /// </summary>
            public ModelParam()
            {
                MapPieces = new List<Model.MapPiece>();
                Objects = new List<Model.Object>();
                Collisions = new List<Model.Collision>();
                Navmeshes = new List<Model.Navmesh>();
            }
            
            /// <summary>
            /// Returns every Model in the order they'll be written.
            /// </summary>
            public override List<Model> GetEntries()
            {
                return SFUtil.ConcatAll<Model>(
                    MapPieces, Objects, Collisions, Navmeshes);
            }
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => GetEntries();

            /// <summary>
            /// Adds a model to the appropriate list for its type; returns the model.
            /// </summary>
            public Model Add(Model model)
            {
                switch (model)
                {
                    case Model.MapPiece m: MapPieces.Add(m); break;
                    case Model.Object m: Objects.Add(m); break;
                    case Model.Collision m: Collisions.Add(m); break;
                    case Model.Navmesh m: Navmeshes.Add(m); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
                }
                return model;
            }
            IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => Add((Model)item);

            internal override Model ReadEntry(BinaryReaderEx br)
            {
                ModelType type = br.GetEnum16<ModelType>(br.Position + 8);
                switch (type)
                {
                    case ModelType.MapPiece:
                        return MapPieces.EchoAdd(new Model.MapPiece(br));

                    case ModelType.Object:
                        return Objects.EchoAdd(new Model.Object(br));

                    case ModelType.Collision:
                        return Collisions.EchoAdd(new Model.Collision(br));

                    case ModelType.Navmesh:
                        return Navmeshes.EchoAdd(new Model.Navmesh(br));

                    default:
                        throw new NotImplementedException($"Unimplemented model type: {type}");
                }
            }
        }

        /// <summary>
        /// A model file available for parts to reference.
        /// </summary>
        public abstract class Model : NamedEntry, IMsbModel
        {
            private protected abstract ModelType Type { get; }
            private protected abstract bool HasTypeData { get; }

            private protected Model(string name)
            {
                Name = name;
            }

            private protected Model(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                br.AssertUInt16((ushort)Type);
                br.ReadInt16(); // ID
                br.AssertInt32(0);
                long typeDataOffset = br.ReadInt64();
                br.AssertInt64(0);

                Name = br.GetUTF16(start + nameOffset);

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt16((ushort)Type);
                bw.WriteInt16((short)id);
                bw.WriteInt32(0);
                bw.ReserveInt64("TypeDataOffset");
                bw.WriteInt64(0);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.Pad(8);

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                }
                if (Type == ModelType.Object)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    bw.WriteInt64(0);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            /// <summary>
            /// Returns a string representation of the model.
            /// </summary>
            public override string ToString()
            {
                return $"{Name}";
            }

            /// <summary>
            /// A model for a static piece of visual map geometry.
            /// </summary>
            public class MapPiece : Model
            {
                private protected override ModelType Type => ModelType.MapPiece;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a dynamic or interactible part.
            /// </summary>
            public class Object : Model
            {
                private protected override ModelType Type => ModelType.Object;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXX_XXXX") { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A model for a static piece of physical map geometry.
            /// </summary>
            public class Collision : Model
            {
                private protected override ModelType Type => ModelType.Collision;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXX_XXXX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for an AI navigation mesh.
            /// </summary>
            public class Navmesh : Model
            {
                private protected override ModelType Type => ModelType.Navmesh;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXX_XXXX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
