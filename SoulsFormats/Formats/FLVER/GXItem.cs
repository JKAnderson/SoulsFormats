using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER
    {
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

            internal static List<GXItem> ReadList(BinaryReaderEx br)
            {
                var items = new List<GXItem>();
                GXItem item;
                do
                {
                    item = new GXItem(br);
                    items.Add(item);
                }
                while (item.ID != int.MaxValue);
                return items;
            }

            internal static void WriteList(BinaryWriterEx bw, List<GXItem> items)
            {
                foreach (GXItem item in items)
                    item.Write(bw);
            }
        }
    }
}
