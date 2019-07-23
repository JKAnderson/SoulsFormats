using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// A collection of items that set various material properties.
        /// </summary>
        public class GXList : List<GXItem>
        {
            /// <summary>
            /// The length in bytes of the terminator data block; most likely not important, but varies in original files.
            /// </summary>
            public int TerminatorLength { get; set; }

            /// <summary>
            /// Creates an empty GXList.
            /// </summary>
            public GXList() : base() { }

            internal GXList(BinaryReaderEx br) : base()
            {
                while (br.GetInt32(br.Position) != int.MaxValue)
                    Add(new GXItem(br));

                br.AssertInt32(int.MaxValue);
                br.AssertInt32(100);
                TerminatorLength = br.ReadInt32() - 0xC;
                br.AssertPattern(TerminatorLength, 0x00);
            }

            internal void Write(BinaryWriterEx bw)
            {
                foreach (GXItem item in this)
                    item.Write(bw);

                bw.WriteInt32(int.MaxValue);
                bw.WriteInt32(100);
                bw.WriteInt32(TerminatorLength + 0xC);
                bw.WritePattern(TerminatorLength, 0x00);
            }
        }

        /// <summary>
        /// Unknown; some kind of graphics parameters used by materials.
        /// </summary>
        public class GXItem
        {
            /// <summary>
            /// In DS2, ID is just a number; in other games, it's 4 ASCII characters.
            /// </summary>
            public uint ID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Creates a GXItem with default values.
            /// </summary>
            public GXItem()
            {
                Data = new byte[0];
            }

            /// <summary>
            /// Creates a GXItem with the given values.
            /// </summary>
            public GXItem(uint id, int unk04, byte[] data)
            {
                ID = id;
                Unk04 = unk04;
                Data = data;
            }

            internal GXItem(BinaryReaderEx br)
            {
                ID = br.ReadUInt32();
                Unk04 = br.ReadInt32();
                int length = br.ReadInt32();
                Data = br.ReadBytes(length - 0xC);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(ID);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Data.Length + 0xC);
                bw.WriteBytes(Data);
            }
        }
    }
}
