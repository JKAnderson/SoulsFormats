using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB64
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class PartsPoseSection
        {
            internal string Type = "MAPSTUDIO_PARTS_POSE_ST";

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1;

            /// <summary>
            /// Parts pose data in this section.
            /// </summary>
            public List<byte[]> Entries;

            internal PartsPoseSection(BinaryReaderEx br, int unk1, int offsets)
            {
                Unk1 = unk1;

                Entries = new List<byte[]>();
                for (int i = 0; i < offsets; i++)
                {
                    long offset = br.ReadInt64();
                    long next = br.GetInt64(br.Position);
                    byte[] bytes = br.GetBytes(offset, (int)(next - offset));
                    Entries.Add(bytes);
                }
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk1);
                bw.WriteInt32(Entries.Count + 1);
                bw.ReserveInt64("TypeOffset");

                for (int i = 0; i < Entries.Count; i++)
                    bw.ReserveInt64($"Offset{i}");

                bw.ReserveInt64("NextOffset");

                bw.FillInt64("TypeOffset", bw.Position);
                bw.WriteUTF16(Type, true);
                bw.Pad(8);

                for (int i = 0; i < Entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    bw.WriteBytes(Entries[i]);
                }
            }
        }
    }
}
