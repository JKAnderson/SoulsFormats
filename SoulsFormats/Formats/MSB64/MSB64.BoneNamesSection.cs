using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB64
    {
        /// <summary>
        /// A section containing bone name strings. Purpose unknown.
        /// </summary>
        public class BoneNameSection : Section<string>
        {
            /// <summary>
            /// The MSB type string for this section.
            /// </summary>
            public override string Type => "MAPSTUDIO_BONE_NAME_STRING";

            /// <summary>
            /// The bone names in this section.
            /// </summary>
            public List<string> Names;

            internal BoneNameSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                Names = new List<string>();
            }

            public override List<string> GetEntries()
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
