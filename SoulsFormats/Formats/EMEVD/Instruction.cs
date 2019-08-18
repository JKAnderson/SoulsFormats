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
        /// A single instruction to be executed by an event.
        /// </summary>
        public class Instruction
        {
            /// <summary>
            /// The bank from which to select the instruction.
            /// </summary>
            public int Bank { get; set; }

            /// <summary>
            /// The ID of this instruction to select from the bank.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Optional layer ID structure.
            /// </summary>
            public EventLayer Layer { get; set; }

            /// <summary>
            /// Arguments provided to the instruction, in raw block of bytes form.
            /// </summary>
            public byte[] Args { get; set; }

            /// <summary>
            /// Creates a new instruction with bank 0, ID 0, no arguments, and no layer.
            /// </summary>
            public Instruction()
            {
                Bank = 0;
                ID = 0;
                Layer = null;
                Args = new byte[0];
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and (optionally) args.
            /// </summary>
            public Instruction(int bank, int id, byte[] args = null)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                Args = args;
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and (optionally) args.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, byte[] args = null)
            {
                Bank = bank;
                ID = id;
                Layer = new EventLayer(layerMask);
                Args = args;
            }

            internal Instruction(BinaryReaderEx br, GameType game, OffsetsContainer offsets)
            {
                Bank = br.ReadInt32();
                ID = br.ReadInt32();

                long argsLength = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
                long argsOffset = br.ReadInt32();

                br.StepIn(offsets.ArgsBlockOffset + argsOffset);
                {
                    Args = br.ReadBytes((int)argsLength);
                }
                br.StepOut();

                if (game != GameType.DS1)
                    br.AssertInt32(0);

                long layerOffset = (game == GameType.DS3) ? br.ReadInt64() : br.ReadInt32();
                if (layerOffset != -1)
                {
                    br.StepIn(offsets.EventLayersOffset + layerOffset);
                    {
                        Layer = new EventLayer(br, game);
                    }
                    br.StepOut();
                }

                if (game != GameType.DS3)
                    br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, GameType game, int i, int j)
            {
                bw.WriteInt32(Bank);
                bw.WriteInt32(ID);

                if (game != GameType.DS1)
                    bw.WriteInt64(Args.Length);
                else
                    bw.WriteInt32(Args.Length);

                if (Args.Length == 0)
                    bw.WriteInt32(-1);
                else
                    bw.ReserveInt32($"InstructionArgsOffset{i}:{j}");

                if (game != GameType.DS1)
                    bw.WriteInt32(0);

                if (game == GameType.DS3)
                {
                    if (Layer == null)
                        bw.WriteInt64(-1);
                    else
                        bw.ReserveInt64($"InstructionLayerOffset{i}:{j}");
                }
                else
                {
                    if (Layer == null)
                        bw.WriteInt32(-1);
                    else
                        bw.ReserveInt32($"InstructionLayerOffset{i}:{j}");
                    bw.WriteInt32(0);
                }

            }
        }
    }
}
