using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB64
    {
        public class BoneNameSection : Section<string>
        {
            public override string Type => "MAPSTUDIO_BONE_NAME_STRING";

            public List<string> Names;

            internal BoneNameSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                Names = new List<string>();
            }

            internal override List<string> GetEntries()
            {
                return Names;
            }

            internal override string ReadEntry(BinaryReaderEx br)
            {
                var name = br.ReadUTF16();
                Names.Add(name);
                return name;
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<string> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    bw.WriteUTF16(entries[i], true);
                }
            }
        }
    }
}
