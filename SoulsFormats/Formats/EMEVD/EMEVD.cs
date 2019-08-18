using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    /// <summary>
    /// A list of game area logic events, each with a script.
    /// </summary>
    public partial class EMEVD : SoulsFile<EMEVD>
    {
        /// <summary>
        /// Defines the game for which the EMEVD file was made.
        /// </summary>
        public enum GameType
        {
            /// <summary>
            /// Dark Souls: Prepare to Die Edition and Dark Souls Remastered
            /// </summary>
            DS1,
            /// <summary>
            /// Bloodborne
            /// </summary>
            BB,
            /// <summary>
            /// Dark Souls III
            /// </summary>
            DS3,
        }

        internal class OffsetsContainer
        {
            public long EventsOffset;
            public long InstructionsOffset;
            public long EventLayersOffset;
            public long ParametersOffset;
            public long LinkedFilesOffset;
            public long ArgsBlockOffset;
            public long StringsBlockOffset;
        }

        /// <summary>
        /// Which game this EMEVD file is for.
        /// </summary>
        public GameType Game { get; set; }

        /// <summary>
        /// List of events in this EMEVD.
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// List of indices in the string table which correspond to linked file names used in Bloodborne and Dark Souls III.
        /// </summary>
        public List<int> LinkedFileStringIndices { get; set; }

        /// <summary>
        /// List of strings indexed by instruction args and linked files.
        /// </summary>
        public List<string> StringTable { get; set; }

        internal override bool Is(BinaryReaderEx br)
        {
            return br.ReadASCII(4) == "EVD\0";
        }

        /// <summary>
        /// Creates a new EMEVD for DS1 with no events.
        /// </summary>
        public EMEVD()
        {
            Game = GameType.DS1;
            Events = new List<Event>();
            LinkedFileStringIndices = new List<int>();
            StringTable = new List<string>();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("EVD\0");

            uint versionA = br.AssertUInt32(0, 0xFF00, 0x1FF00);
            uint versionB = br.AssertUInt32(0xCC, 0xCD);

            if (versionA == 0 && versionB == 0xCC)
                Game = GameType.DS1;
            else if (versionA == 0xFF00 && versionB == 0xCC)
                Game = GameType.BB;
            else if (versionA == 0x1FF00 && versionB == 0xCD)
                Game = GameType.DS3;
            else
                throw new System.IO.InvalidDataException($"Invalid pair of version values in EMEVD header: 0x{versionA:X8}, 0x{versionB:X8}.");

            if (Game == GameType.BB)
                br.AssertInt64(br.Length);
            else
                br.AssertInt32((int)br.Length);

            var offsets = new OffsetsContainer();

            long eventsCount = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            offsets.EventsOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long instructionsCount = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            offsets.InstructionsOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long dummiesCount = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long dummiesOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long eventLayersCount = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            offsets.EventLayersOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long parametersCount = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            offsets.ParametersOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long linkedFilesCount = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            offsets.LinkedFilesOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long argsBlockSize = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            offsets.ArgsBlockOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            long stringsBlockSize = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
            offsets.StringsBlockOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();

            if (Game == GameType.DS1)
                br.AssertInt32(0);

            br.StepIn(offsets.EventsOffset);
            {
                for (int i = 0; i < eventsCount; i++)
                    Events.Add(new Event(br, Game, offsets));
            }
            br.StepOut();

            var stringOffsets = new List<long>();

            br.StepIn(offsets.StringsBlockOffset);
            {
                while (br.Position < offsets.StringsBlockOffset + stringsBlockSize)
                {
                    long strOffset = br.Position - offsets.StringsBlockOffset;
                    stringOffsets.Add(strOffset);
                    var str = br.ReadUTF16();
                    StringTable.Add(str);
                }
            }
            br.StepOut();

            br.StepIn(offsets.LinkedFilesOffset);
            {
                for (int i = 0; i < linkedFilesCount; i++)
                {
                    long strOffset = (Game != GameType.DS1) ? br.ReadInt64() : br.ReadInt32();
                    LinkedFileStringIndices.Add(stringOffsets.IndexOf(strOffset));
                }
                
            }
            br.StepOut();
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("EVD\0");

            if (Game == GameType.DS1)
            {
                bw.WriteUInt32(0);
                bw.WriteUInt32(0xCC);
            }
            else if (Game == GameType.BB)
            {
                bw.WriteUInt32(0xFF00);
                bw.WriteUInt32(0xCC);
            }
            else if (Game == GameType.DS3)
            {
                bw.WriteUInt32(0x1FF00);
                bw.WriteUInt32(0xCD);
            }

            void ReserveIntW(string name, bool? isWide = null)
            {
                if (isWide ?? Game != GameType.DS1)
                    bw.ReserveInt64(name);
                else
                    bw.ReserveInt32(name);
            }

            void FillIntW(string name, long value, bool? isWide = null)
            {
                if (isWide ?? Game != GameType.DS1)
                    bw.FillInt64(name, value);
                else
                    bw.FillInt32(name, (int)value);
            }

            void WriteIntW(long value, bool? isWide = null)
            {
                if (isWide ?? Game != GameType.DS1)
                    bw.WriteInt64(value);
                else
                    bw.WriteInt32((int)value);
            }

            if (Game == GameType.BB)
                bw.ReserveInt64("FileLength");
            else
                bw.ReserveInt32("FileLength");

            WriteIntW(Events.Count);
            ReserveIntW("EventsOffset");
            ReserveIntW("InstructionsCount");
            ReserveIntW("InstructionsOffset");

            //Dummy count. Always empty.
            WriteIntW(0);
            // Same as EventLayersOffset because it's always empty.
            ReserveIntW("DummiesOffset");
            ReserveIntW("EventLayersCount");
            ReserveIntW("EventLayersOffset");
            ReserveIntW("ParametersCount");
            ReserveIntW("ParametersOffset");
            WriteIntW(LinkedFileStringIndices.Count);
            ReserveIntW("LinkedFilesOffset");
            ReserveIntW("ArgsBlockSize");
            ReserveIntW("ArgsBlockOffset");
            ReserveIntW("StringsBlockSize");
            ReserveIntW("StringsBlockOffset");

            if (Game == GameType.DS1)
            {
                bw.WriteInt32(0);
            }

            // Events
            FillIntW("EventsOffset", bw.Position);
            for (int i = 0; i < Events.Count; i++)
            {
                Events[i].Write(bw, Game, i);
            }

            // Instructions
            long instructionsOffset = bw.Position;
            FillIntW("InstructionsOffset", instructionsOffset);
            long instructionsCount = 0;
            for (int i = 0; i < Events.Count; i++)
            {
                FillIntW($"EventInstructionsOffset{i}", bw.Position - instructionsOffset);
                for (int j = 0; j < Events[i].Instructions.Count; j++)
                {
                    Events[i].Instructions[j].Write(bw, Game, i, j);
                }
                instructionsCount += Events[i].Instructions.Count;
            }
            FillIntW("InstructionsCount", instructionsCount);

            // Dummies
            FillIntW("DummiesOffset", bw.Position);

            // EventLayers
            long eventLayersOffset = bw.Position;
            FillIntW("EventLayersOffset", eventLayersOffset);
            long eventLayersCount = 0;
            for (int i = 0; i < Events.Count; i++)
            {
                for (int j = 0; j < Events[i].Instructions.Count; j++)
                {
                    if (Events[i].Instructions[j].Layer != null)
                    {
                        if (Game == GameType.DS3)
                            bw.FillInt64($"InstructionLayerOffset{i}:{j}", bw.Position - eventLayersOffset);
                        else
                            bw.FillInt32($"InstructionLayerOffset{i}:{j}", (int)(bw.Position - eventLayersOffset));
                        Events[i].Instructions[j].Layer.Write(bw, Game);
                        eventLayersCount++;
                    }
                }
            }
            FillIntW("EventLayersCount", eventLayersCount);

            // Args
            long argsBlockOffset = bw.Position;
            FillIntW("ArgsBlockOffset", argsBlockOffset);
            for (int i = 0; i < Events.Count; i++)
            {
                for (int j = 0; j < Events[i].Instructions.Count; j++)
                {
                    if (Events[i].Instructions[j].Args.Length > 0)
                    {
                        bw.FillInt32($"InstructionArgsOffset{i}:{j}", (int)(bw.Position - argsBlockOffset));
                        bw.WriteBytes(Events[i].Instructions[j].Args);
                        //bw.Pad(4);
                    }
                }
            }

            if (Game == GameType.DS1)
            {
                //bw.WriteInt32(0);
                while (((bw.Position - argsBlockOffset) % 16) > 0)
                {
                    bw.WriteByte(0);
                }
            }
            else
            {
                bw.Pad(16);
            }
            
            FillIntW("ArgsBlockSize", bw.Position - argsBlockOffset);

            // Parameters
            long parametersOffset = bw.Position;
            FillIntW("ParametersOffset", parametersOffset);
            long parametersCount = 0;
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].Parameters.Count > 0)
                {
                    if (Game == GameType.DS3)
                        bw.FillInt64($"EventParametersOffset{i}", bw.Position - parametersOffset);
                    else
                        bw.FillInt32($"EventParametersOffset{i}", (int)(bw.Position - parametersOffset));
                    for (int j = 0; j < Events[i].Parameters.Count; j++)
                    {
                        Events[i].Parameters[j].Write(bw, Game);
                    }
                    parametersCount += Events[i].Parameters.Count;
                }
            }
            FillIntW("ParametersCount", parametersCount);

            // Linked Files
            FillIntW("LinkedFilesOffset", bw.Position);
            for (int i = 0; i < LinkedFileStringIndices.Count; i++)
            {
                ReserveIntW($"LinkedFileStringOffset{i}");
            }

            // Strings
            FillIntW("StringsBlockOffset", bw.Position);
            long stringsStartOffset = bw.Position;
            List<long> stringTableOffsets = new List<long>();
            for (int i = 0; i < StringTable.Count; i++)
            {
                stringTableOffsets.Add(bw.Position - stringsStartOffset);
                bw.WriteUTF16(StringTable[i], terminate: true);
            }
            FillIntW("StringsBlockSize", bw.Position - stringsStartOffset);

            // Linked Files - Second Pass
            for (int i = 0; i < LinkedFileStringIndices.Count; i++)
            {
                FillIntW($"LinkedFileStringOffset{i}", stringTableOffsets[LinkedFileStringIndices[i]]);
            }

            if (Game == GameType.BB)
                bw.FillInt64("FileLength", bw.Position);
            else
                bw.FillInt32("FileLength", (int)bw.Position);
        }
    }
}
