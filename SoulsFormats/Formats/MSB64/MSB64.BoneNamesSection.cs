using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB64
    {
        public class BoneNameSection : Section<string>
        {
            public override string Type => "MAPSTUDIO_BONE_NAME_STRING";

            public override List<string> Entries => Names;

            public List<string> Names;

            internal BoneNameSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                Names = new List<string>();
            }

            internal override string ReadEntry(BinaryReaderEx br)
            {
                var name = br.ReadUTF16();
                Names.Add(name);
                return name;
            }

            internal override void WriteOffsets(BinaryWriterEx bw)
            {
                bw.FillInt32("OffsetCount", Names.Count + 1);
                for (int i = 0; i < Names.Count; i++)
                    bw.ReserveInt64($"Offset{i}");
            }

            internal override void WriteData(BinaryWriterEx bw)
            {
                for (int i = 0; i < Names.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    bw.WriteUTF16(Names[i], true);
                }
            }
        }
    }
}
