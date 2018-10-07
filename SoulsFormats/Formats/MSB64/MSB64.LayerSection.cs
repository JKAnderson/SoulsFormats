using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB64
    {
        public class LayerSection : Section<Layer>
        {
            public override string Type => "LAYER_PARAM_ST";

            public override List<Layer> Entries => Layers;

            public List<Layer> Layers;

            internal LayerSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                Layers = new List<Layer>();
            }

            internal override Layer ReadEntry(BinaryReaderEx br)
            {
                var layer = new Layer(br);
                Layers.Add(layer);
                return layer;
            }

            internal override void WriteOffsets(BinaryWriterEx bw)
            {
                bw.FillInt32("OffsetCount", Layers.Count + 1);
                for (int i = 0; i < Layers.Count; i++)
                {
                    bw.ReserveInt64($"Offset{i}");
                }
            }

            internal override void WriteData(BinaryWriterEx bw)
            {
                for (int i = 0; i < Layers.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    Layers[i].Write(bw);
                }
            }
        }

        public class Layer
        {
            public string Name;
            public int Unk1, Unk2, Unk3;

            internal Layer(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                Unk1 = br.ReadInt32();
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadInt32();

                Name = br.GetUTF16(start + nameOffset);
            }

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk1);
                bw.WriteInt32(Unk2);
                bw.WriteInt32(Unk3);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);
            }

            public override string ToString()
            {
                return $"{Name} ({Unk1}, {Unk2}, {Unk3})";
            }
        }
    }
}
