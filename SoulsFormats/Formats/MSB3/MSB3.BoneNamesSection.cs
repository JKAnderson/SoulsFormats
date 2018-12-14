using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// A section containing bone name strings. Purpose unknown.
        /// </summary>
        public class BoneNameSection : Section<string>
        {
            internal override string Type => "MAPSTUDIO_BONE_NAME_STRING";

            /// <summary>
            /// The bone names in this section.
            /// </summary>
            public List<string> Names;

            /// <summary>
            /// Creates a new BoneNameSection with no bone names.
            /// </summary>
            public BoneNameSection(int unk1 = 0) : base(unk1)
            {
                Names = new List<string>();
            }

            /// <summary>
            /// Returns every bone name in the order they will be written.
            /// </summary>
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
