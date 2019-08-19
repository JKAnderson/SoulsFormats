using System;
using System.Collections.Generic;
using System.IO;

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
            /// Value type of an argument.
            /// </summary>
            public enum ArgType
            {
                /// <summary>
                /// Unsigned 8-bit integer.
                /// </summary>
                Byte = 0,

                /// <summary>
                /// Unsigned 16-bit integer.
                /// </summary>
                UInt16 = 1,

                /// <summary>
                /// Unsigned 32-bit integer.
                /// </summary>
                UInt32 = 2,

                /// <summary>
                /// Signed 8-bit integer.
                /// </summary>
                SByte = 3,

                /// <summary>
                /// Signed 16-bit integer.
                /// </summary>
                Int16 = 4,

                /// <summary>
                /// Signed 32-bit integer.
                /// </summary>
                Int32 = 5,

                /// <summary>
                /// 32-bit floating point number.
                /// </summary>
                Single = 6,

                /// <summary>
                /// 64-bit integer (not sure if signed or unsigned), usually used for string table offsets.
                /// </summary>
                Int64 = 7,
            }

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
            /// Creates a new Instruction with the specified bank and ID with no args.
            /// </summary>
            public Instruction(int bank, int id)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                Args = new byte[0];
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and args bytes.
            /// </summary>
            public Instruction(int bank, int id, byte[] args)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                Args = args;
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and args.
            /// </summary>
            public Instruction(int bank, int id, IEnumerable<object> args)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and args.
            /// </summary>
            public Instruction(int bank, int id, params object[] args)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and args bytes.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, byte[] args)
            {
                Bank = bank;
                ID = id;
                Layer = new EventLayer(layerMask);
                Args = args;
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and args.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, IEnumerable<object> args)
            {
                Bank = bank;
                ID = id;
                Layer = new EventLayer(layerMask);
                PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and args.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, params object[] args)
            {
                Bank = bank;
                ID = id;
                Layer = new EventLayer(layerMask);
                PackArgs(args);
            }

            internal Instruction(BinaryReaderEx br, GameType game, OffsetsContainer offsets)
            {
                Bank = br.ReadInt32();
                ID = br.ReadInt32();

                long argsLength = ReadIntW(br, game != GameType.DS1);
                long argsOffset = br.ReadInt32();

                if (game != GameType.DS1)
                    br.AssertInt32(0);

                if (game != GameType.DS3)
                    br.AssertInt32(0);

                br.StepIn(offsets.ArgsBlockOffset + argsOffset);
                {
                    Args = br.ReadBytes((int)argsLength);
                }
                br.StepOut();

                long layerOffset = ReadIntW(br, game != GameType.DS1);
                if (layerOffset != -1)
                {
                    br.StepIn(offsets.EventLayersOffset + layerOffset);
                    {
                        Layer = new EventLayer(br, game);
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, GameType game, int eventIndex, int instructionIndex)
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
                    bw.ReserveInt32($"InstructionArgsOffset{eventIndex}:{instructionIndex}");

                if (game != GameType.DS1)
                    bw.WriteInt32(0);

                if (game == GameType.DS3)
                {
                    if (Layer == null)
                        bw.WriteInt64(-1);
                    else
                        bw.ReserveInt64($"InstructionLayerOffset{eventIndex}:{instructionIndex}");
                }
                else
                {
                    if (Layer == null)
                        bw.WriteInt32(-1);
                    else
                        bw.ReserveInt32($"InstructionLayerOffset{eventIndex}:{instructionIndex}");
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Packs an enumeration of arg values into a byte array for use in an instruction.
            /// </summary>
            public void PackArgs(IEnumerable<object> args)
            {
                using (var memStream = new MemoryStream())
                {
                    var bw = new BinaryWriterEx(bigEndian: false, memStream);
                    foreach (object arg in args)
                    {
                        switch (arg)
                        {
                            case byte ub:
                                bw.WriteByte(ub); break;
                            case ushort us:
                                bw.Pad(2);
                                bw.WriteUInt16(us); break;
                            case uint ui:
                                bw.Pad(4);
                                bw.WriteUInt32(ui); break;
                            case sbyte sb:
                                bw.WriteSByte(sb); break;
                            case short ss:
                                bw.Pad(2);
                                bw.WriteInt16(ss); break;
                            case int si:
                                bw.Pad(4);
                                bw.WriteInt32(si); break;
                            case float f:
                                bw.Pad(4);
                                bw.WriteSingle(f); break;
                            case long sl:
                                bw.Pad(8);
                                bw.WriteInt64(sl); break;

                            default:
                                throw new NotSupportedException($"Unsupported argument type: {arg.GetType()}");
                        }
                    }
                    Args = bw.FinishBytes();
                }
            }

            /// <summary>
            /// Unpacks an args byte array according to the structure definition provided.
            /// </summary>
            public List<object> UnpackArgs(IEnumerable<ArgType> argStruct)
            {
                var result = new List<object>();

                using (var memStream = new MemoryStream(Args))
                {
                    var br = new BinaryReaderEx(bigEndian: false, memStream);

                    foreach (ArgType arg in argStruct)
                    {
                        switch (arg)
                        {
                            case ArgType.Byte:
                                result.Add(br.ReadByte()); break;
                            case ArgType.UInt16:
                                br.Pad(2);
                                result.Add(br.ReadUInt16()); break;
                            case ArgType.UInt32:
                                br.Pad(4);
                                result.Add(br.ReadUInt32()); break;
                            case ArgType.SByte:
                                result.Add(br.ReadSByte()); break;
                            case ArgType.Int16:
                                br.Pad(2);
                                result.Add(br.ReadInt16()); break;
                            case ArgType.Int32:
                                br.Pad(4);
                                result.Add(br.ReadInt32()); break;
                            case ArgType.Single:
                                br.Pad(4);
                                result.Add(br.ReadSingle()); break;
                            case ArgType.Int64:
                                br.Pad(8);
                                result.Add(br.ReadInt64()); break;

                            default:
                                throw new NotImplementedException($"Unimplemented argument type: {arg}");
                        }
                    }
                }

                return result;
            }
        }
    }
}
