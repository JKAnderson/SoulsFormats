using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public partial class EMEVD : SoulsFile<EMEVD>
    {
        /// <summary>
        /// A perplexing struct with a layer ID and 3 values which never change.
        /// </summary>
        public class EventLayer
        {
            /// <summary>
            /// Which layer(s) this is.
            /// </summary>
            public uint Mask { get; set; }

            /// <summary>
            /// Creates a new EventLayer with Mask of 0.
            /// </summary>
            public EventLayer()
            {
                Mask = 0;
            }

            /// <summary>
            /// Creates a new EventLayer with the specified Mask.
            /// </summary>
            public EventLayer(uint mask)
            {
                Mask = mask;
            }

            internal EventLayer(BinaryReaderEx br, GameType game)
            {
                br.AssertInt32(2);
                Mask = br.ReadUInt32();
                if (game == GameType.DS1)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                    br.AssertInt32(1);
                }
                else
                {
                    br.AssertInt64(0);
                    br.AssertInt64(-1);
                    br.AssertInt64(1);
                }
            }

            internal void Write(BinaryWriterEx bw, GameType game)
            {
                bw.WriteInt32(2);
                bw.WriteUInt32(Mask);
                if (game == GameType.DS1)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(1);
                }
                else
                {
                    bw.WriteInt64(0);
                    bw.WriteInt64(-1);
                    bw.WriteInt64(1);
                }
            }
        }
    }
}
