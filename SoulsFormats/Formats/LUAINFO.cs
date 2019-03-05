using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A list of AI goals for Lua scripts.
    /// </summary>
    public class LUAINFO : SoulsFile<LUAINFO>
    {
        /// <summary>
        /// If true, write as big endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// If true, write with 64-bit offsets and UTF-16 strings.
        /// </summary>
        public bool LongFormat { get; set; }

        /// <summary>
        /// AI goals for a luabnd.
        /// </summary>
        public List<Goal> Goals { get; set; }

        /// <summary>
        /// Creates an empty LUAINFO formatted for PC DS1.
        /// </summary>
        public LUAINFO() : this(false, false) { }

        /// <summary>
        /// Creates an empty LUAINFO with the specified format.
        /// </summary>
        public LUAINFO(bool bigEndian, bool longFormat)
        {
            BigEndian = bigEndian;
            LongFormat = longFormat;
            Goals = new List<Goal>();
        }

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "LUAI";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("LUAI");
            BigEndian = br.AssertInt32(1, 0x1000000) == 0x10000000;
            br.BigEndian = BigEndian;
            int goalCount = br.ReadInt32();
            br.AssertInt32(0);

            if (goalCount <= 2)
                throw new NotSupportedException("LUAINFO with less than 2 goals will ruin my long format heuristic.");
            LongFormat = br.GetInt32(0x24) == 0;

            Goals = new List<Goal>(goalCount);
            for (int i = 0; i < goalCount; i++)
                Goals.Add(new Goal(br, LongFormat));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.WriteASCII("LUAI");
            bw.WriteInt32(1);
            bw.WriteInt32(Goals.Count);
            bw.WriteInt32(0);

            for (int i = 0; i < Goals.Count; i++)
                Goals[i].Write(bw, LongFormat, i);

            for (int i = 0; i < Goals.Count; i++)
                Goals[i].WriteStrings(bw, LongFormat, i);
        }

        /// <summary>
        /// Goal information for AI scripts.
        /// </summary>
        public class Goal
        {
            /// <summary>
            /// ID of this goal.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Name of this goal.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Whether to trigger a battle interrupt.
            /// </summary>
            public bool BattleInterrupt { get; set; }

            /// <summary>
            /// Whether to trigger a logic interrupt.
            /// </summary>
            public bool LogicInterrupt { get; set; }

            /// <summary>
            /// Function name of the logic interrupt, or null if not present.
            /// </summary>
            public string LogicInterruptName { get; set; }

            /// <summary>
            /// Creates a new Goal with the specified values.
            /// </summary>
            public Goal(int id, string name, bool battleInterrupt, bool logicInterrupt, string logicInterruptName = null)
            {
                ID = id;
                Name = name;
                BattleInterrupt = battleInterrupt;
                LogicInterrupt = logicInterrupt;
                LogicInterruptName = logicInterruptName;
            }

            internal Goal(BinaryReaderEx br, bool longFormat)
            {
                ID = br.ReadInt32();
                if (longFormat)
                {
                    BattleInterrupt = br.ReadBoolean();
                    LogicInterrupt = br.ReadBoolean();
                    br.AssertInt16(0);
                    long nameOffset = br.ReadInt64();
                    long interruptNameOffset = br.ReadInt64();

                    Name = br.GetUTF16(nameOffset);
                    if (interruptNameOffset == 0)
                        LogicInterruptName = null;
                    else
                        LogicInterruptName = br.GetUTF16(interruptNameOffset);
                }
                else
                {
                    uint nameOffset = br.ReadUInt32();
                    uint interruptNameOffset = br.ReadUInt32();
                    BattleInterrupt = br.ReadBoolean();
                    LogicInterrupt = br.ReadBoolean();
                    br.AssertInt16(0);

                    Name = br.GetShiftJIS(nameOffset);
                    if (interruptNameOffset == 0)
                        LogicInterruptName = null;
                    else
                        LogicInterruptName = br.GetShiftJIS(interruptNameOffset);
                }
            }

            internal void Write(BinaryWriterEx bw, bool longFormat, int index)
            {
                bw.WriteInt32(ID);
                if (longFormat)
                {
                    bw.WriteBoolean(BattleInterrupt);
                    bw.WriteBoolean(LogicInterrupt);
                    bw.WriteInt16(0);
                    bw.ReserveInt64($"NameOffset{index}");
                    bw.ReserveInt64($"LogicInterruptNameOffset{index}");
                }
                else
                {
                    bw.ReserveUInt32($"NameOffset{index}");
                    bw.ReserveUInt32($"LogicInterruptNameOffset{index}");
                    bw.WriteBoolean(BattleInterrupt);
                    bw.WriteBoolean(LogicInterrupt);
                    bw.WriteInt16(0);
                }
            }

            internal void WriteStrings(BinaryWriterEx bw, bool longFormat, int index)
            {
                if (longFormat)
                {
                    bw.FillInt64($"NameOffset{index}", bw.Position);
                    bw.WriteUTF16(Name, true);
                    if (LogicInterruptName == null)
                    {
                        bw.FillInt64($"LogicInterruptNameOffset{index}", 0);
                    }
                    else
                    {
                        bw.FillInt64($"LogicInterruptNameOffset{index}", bw.Position);
                        bw.WriteUTF16(LogicInterruptName, true);
                    }
                }
                else
                {
                    bw.FillUInt32($"NameOffset{index}", (uint)bw.Position);
                    bw.WriteShiftJIS(Name, true);
                    if (LogicInterruptName == null)
                    {
                        bw.FillUInt32($"LogicInterruptNameOffset{index}", 0);
                    }
                    else
                    {
                        bw.FillUInt32($"LogicInterruptNameOffset{index}", (uint)bw.Position);
                        bw.WriteShiftJIS(LogicInterruptName, true);
                    }
                }
            }
        }
    }
}
