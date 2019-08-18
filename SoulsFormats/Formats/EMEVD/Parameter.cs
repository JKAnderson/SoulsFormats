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
        /// An instruction given to the game which tells it to subsitute arg bytes in a particular instruction with ones defined here.
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// The index into the event's instruction list for which to apply the parameter subsitution.
            /// </summary>
            public long InstructionIndex { get; set; }

            /// <summary>
            /// Index of the starting byte in the instruction's arguments.
            /// </summary>
            public long TargetStartByte { get; set; }

            /// <summary>
            /// Index of the starting byte in the event's parameters.
            /// </summary>
            public long SourceStartByte { get; set; }

            /// <summary>
            /// Amount of bytes to copy to the target instruction's arguments.
            /// </summary>
            public long ByteCount { get; set; }

            /// <summary>
            /// Creates a new Parameter with default values.
            /// </summary>
            public Parameter()
            {
                InstructionIndex = 0;
                TargetStartByte = 0;
                SourceStartByte = 0;
                ByteCount = 0;
            }

            /// <summary>
            /// Creates a Parameter with the specified values.
            /// </summary>
            public Parameter(long instrIndex, long targetStartByte, long srcStartByte, long byteCount)
            {
                InstructionIndex = instrIndex;
                TargetStartByte = targetStartByte;
                SourceStartByte = srcStartByte;
                ByteCount = byteCount;
            }

            internal Parameter(BinaryReaderEx br, GameType game)
            {
                InstructionIndex = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
                TargetStartByte = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
                SourceStartByte = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
                ByteCount = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();

                if (game == GameType.DS1)
                    br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, GameType game)
            {
                if (game != GameType.DS1)
                {
                    bw.WriteInt64(InstructionIndex);
                    bw.WriteInt64(TargetStartByte);
                    bw.WriteInt64(SourceStartByte);
                    bw.WriteInt64(ByteCount);
                }
                else
                {
                    bw.WriteInt32((int)InstructionIndex);
                    bw.WriteInt32((int)TargetStartByte);
                    bw.WriteInt32((int)SourceStartByte);
                    bw.WriteInt32((int)ByteCount);
                    bw.WriteInt32(0);
                }
            }
        }
    }
}
