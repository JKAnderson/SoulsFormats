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
                Long = 7,
            }

            /// <summary>
            /// Packs an enumeration of arg values into a byte array for use in an instruction.
            /// </summary>
            public static byte[] PackArgs(IEnumerable<object> args)
            {
                using (var memStream = new System.IO.MemoryStream())
                {
                    var bw = new BinaryWriterEx(bigEndian: false, memStream);
                    foreach (var arg in args)
                    {
                        var argType = arg.GetType();
                        if (argType == typeof(byte))
                        {
                            bw.WriteByte((byte)arg);
                        }
                        else if (argType == typeof(ushort))
                        {
                            bw.Pad(sizeof(ushort));
                            bw.WriteUInt16((ushort)arg);
                        }
                        else if (argType == typeof(uint))
                        {
                            bw.Pad(sizeof(uint));
                            bw.WriteUInt32((uint)arg);
                        }
                        else if (argType == typeof(sbyte))
                        {
                            bw.WriteSByte((sbyte)arg);
                        }
                        else if (argType == typeof(short))
                        {
                            bw.Pad(sizeof(short));
                            bw.WriteInt16((short)arg);
                        }
                        else if (argType == typeof(int))
                        {
                            bw.Pad(sizeof(int));
                            bw.WriteInt32((int)arg);
                        }
                        else if (argType == typeof(float))
                        {
                            bw.Pad(sizeof(float));
                            bw.WriteSingle((float)arg);
                        }
                        else if (argType == typeof(long))
                        {
                            bw.Pad(sizeof(long));
                            bw.WriteSingle((long)arg);
                        }
                    }
                    return memStream.ToArray();
                }
            }

            /// <summary>
            /// Unpacks an args byte array according to the structure definition provided.
            /// </summary>
            public static List<object> UnpackArgs(byte[] args, IEnumerable<ArgType> argStruct)
            {
                var result = new List<object>();

                using (var memStream = new System.IO.MemoryStream(args))
                {
                    var br = new BinaryReaderEx(bigEndian: false, memStream);

                    foreach (var arg in argStruct)
                    {
                        if (arg == ArgType.Byte)
                        {
                            result.Add(br.ReadByte());
                        }
                        else if (arg == ArgType.UInt16)
                        {
                            br.Pad(sizeof(ushort));
                            result.Add(br.ReadUInt16());
                        }
                        else if (arg == ArgType.UInt32)
                        {
                            br.Pad(sizeof(uint));
                            result.Add(br.ReadUInt32());
                        }
                        else if (arg == ArgType.SByte)
                        {
                            result.Add(br.ReadSByte());
                        }
                        else if (arg == ArgType.Int16)
                        {
                            br.Pad(sizeof(short));
                            result.Add(br.ReadInt16());
                        }
                        else if (arg == ArgType.Int32)
                        {
                            br.Pad(sizeof(int));
                            result.Add(br.ReadInt32());
                        }
                        else if (arg == ArgType.Single)
                        {
                            br.Pad(sizeof(float));
                            result.Add(br.ReadSingle());
                        }
                        else if (arg == ArgType.Long)
                        {
                            br.Pad(sizeof(long));
                            result.Add(br.ReadInt64());
                        }
                    }

                    return result;
                }   
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
                Args = PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and args.
            /// </summary>
            public Instruction(int bank, int id, params object[] args)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                Args = PackArgs(args);
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
                Args = PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and args.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, params object[] args)
            {
                Bank = bank;
                ID = id;
                Layer = new EventLayer(layerMask);
                Args = PackArgs(args);
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
