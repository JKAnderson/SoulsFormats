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
        /// An event containing instructions to be executed.
        /// </summary>
        public class Event
        {
            /// <summary>
            /// Defines the behavior of the event when resting.
            /// </summary>
            public enum RestBehaviorType : uint
            {
                /// <summary>
                /// No effect upon resting.
                /// </summary>
                Default = 0,
                /// <summary>
                /// Event restarts upon resting.
                /// </summary>
                Restart = 1,
                /// <summary>
                /// Event is terminated upon resting.
                /// </summary>
                End = 2,
            }

            /// <summary>
            /// The ID of the event.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// Behavior of this event when resting.
            /// </summary>
            public RestBehaviorType RestBehavior { get; set; }
            /// <summary>
            /// Instructions to execute for this event.
            /// </summary>
            public List<Instruction> Instructions { get; set; }
            /// <summary>
            /// Parameters to be passed to this event.
            /// </summary>
            public List<Parameter> Parameters { get; set; }

            /// <summary>
            /// Creates a new empty event with default ID and rest behavior.
            /// </summary>
            public Event()
            {
                ID = 0;
                RestBehavior = RestBehaviorType.Default;
                Instructions = new List<Instruction>();
                Parameters = new List<Parameter>();
            }

            /// <summary>
            /// Creates a new empty event with specified ID and rest behavior.
            /// </summary>
            public Event(long id, RestBehaviorType restBehavior)
            {
                ID = id;
                RestBehavior = restBehavior;
                Instructions = new List<Instruction>();
                Parameters = new List<Parameter>();
            }

            internal Event(BinaryReaderEx br, GameType game, OffsetsContainer offsets)
                : this()
            {
                ID = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();

                long instructionCount = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
                long instructionOffset = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();

                br.StepIn(offsets.InstructionsOffset + instructionOffset);
                {
                    for (int i = 0; i < instructionCount; i++)
                    {
                        Instructions.Add(new Instruction(br, game, offsets));
                    }
                }
                br.StepOut();

                long parametersCount = (game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
                long parametersOffset = -1;

                if (game == GameType.DS1)
                {
                    parametersOffset = br.ReadInt32();
                }
                else if (game == GameType.BB)
                {
                    parametersOffset = br.ReadInt32();
                    br.AssertInt32(0);
                }
                else if (game == GameType.DS3)
                {
                    parametersOffset = br.ReadInt64();
                }
                else
                {
                    throw new NotImplementedException("Sekiro \"futureproof\".");
                }

                if (parametersOffset >= 0)
                {
                    br.StepIn(offsets.ParametersOffset + parametersOffset);
                    {
                        for (int i = 0; i < parametersCount; i++)
                        {
                            Parameters.Add(new Parameter(br, game));
                        }
                    }
                    br.StepOut();
                }

                RestBehavior = br.ReadEnum32<RestBehaviorType>();

                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, GameType game, int i)
            {
                if (game != GameType.DS1)
                {
                    bw.WriteInt64(ID);
                    bw.WriteInt64(Instructions.Count);
                    bw.ReserveInt64($"EventInstructionsOffset{i}");
                    bw.WriteInt64(Parameters.Count);
                    if (game == GameType.BB)
                    {
                        if (Parameters.Count > 0)
                            bw.ReserveInt32($"EventParametersOffset{i}");
                        else
                            bw.WriteInt32(-1);
                        bw.WriteInt32(0);
                    }
                    else
                    {
                        if (Parameters.Count > 0)
                            bw.ReserveInt64($"EventParametersOffset{i}");
                        else
                            bw.WriteInt64(-1);
                    }

                }
                else
                {
                    bw.WriteInt32((int)ID);
                    bw.WriteInt32(Instructions.Count);
                    bw.ReserveInt32($"EventInstructionsOffset{i}");
                    bw.WriteInt32(Parameters.Count);
                    if (Parameters.Count > 0)
                        bw.ReserveInt32($"EventParametersOffset{i}");
                    else
                        bw.WriteInt32(-1);
                }

                if (game == GameType.BB)
                    bw.WriteInt32(0);

                bw.WriteInt32((int)RestBehavior);

                bw.WriteInt32(0);
            }
        }
    }
}
