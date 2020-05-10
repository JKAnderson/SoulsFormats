using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// A section containing bone name strings. Purpose unknown.
        /// </summary>
        public class MapstudioBoneName : Param<string>
        {
            internal override int Version => 0;
            internal override string Type => "MAPSTUDIO_BONE_NAME_STRING";

            /// <summary>
            /// The bone names in this section.
            /// </summary>
            public List<string> Names { get; set; }

            /// <summary>
            /// Creates a new BoneNameSection with no bone names.
            /// </summary>
            public MapstudioBoneName()
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
                return Names.EchoAdd(br.ReadUTF16());
            }

            internal override void WriteEntry(BinaryWriterEx bw, int id, string entry)
            {
                bw.WriteUTF16(entry, true);
                bw.Pad(8);
            }
        }
    }
}
