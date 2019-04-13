using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats
{
    public class EMEVD : SoulsFile<EMEVD>
    {
        public List<Event> Events { get; set; }

        public List<long> LinkedFileOffsets { get; set; }

        public byte[] Strings { get; set; }

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "EVD\0";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("EVD\0");
            br.AssertByte(0);
            br.AssertByte(0xFF);
            br.AssertByte(1);
            br.AssertByte(0xFF);
            br.AssertInt32(0xCD);
            br.ReadInt32(); // File size

            Offsets offsets;
            long eventCount = br.ReadInt64();
            offsets.Events = br.ReadInt64();
            br.ReadInt64(); // Command count
            offsets.Commands = br.ReadInt64();
            br.AssertInt64(0);
            br.ReadInt64(); // Unused offset
            br.ReadInt64(); // Event layer count
            offsets.EventLayers = br.ReadInt64();
            br.ReadInt64(); // Parameter count
            offsets.Parameters = br.ReadInt64();
            long linkedFileCount = br.ReadInt64();
            offsets.LinkedFiles = br.ReadInt64();
            br.ReadInt64(); // Arguments length
            offsets.Arguments = br.ReadInt64();
            long stringLength = br.ReadInt64();
            offsets.Strings = br.ReadInt64();

            br.Position = offsets.Events;
            Events = new List<Event>((int)eventCount);
            for (int i = 0; i < eventCount; i++)
                Events.Add(new Event(br, offsets));

            LinkedFileOffsets = new List<long>(br.GetInt64s(offsets.LinkedFiles, (int)linkedFileCount));
            Strings = br.GetBytes(offsets.Strings, (int)stringLength);
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        internal struct Offsets
        {
            public long Events;
            public long Commands;
            public long EventLayers;
            public long Parameters;
            public long LinkedFiles;
            public long Arguments;
            public long Strings;
        }

        public enum RestBehaviorType : uint
        {
            None = 0,
            Restart = 1,
            End = 2,
        }

        public class Event
        {
            public long ID { get; set; }

            public RestBehaviorType RestBehavior { get; set; }

            public List<Command> Commands { get; set; }

            public List<Parameter> Parameters { get; set; }

            internal Event(BinaryReaderEx br, Offsets offsets)
            {
                ID = br.ReadInt64();
                long commandCount = br.ReadInt64();
                long commandsOffset = br.ReadInt64();
                long parameterCount = br.ReadInt64();
                long parametersOffset = br.ReadInt64();
                RestBehavior = br.ReadEnum32<RestBehaviorType>();
                br.AssertInt32(0);

                br.StepIn(offsets.Commands + commandsOffset);
                {
                    Commands = new List<Command>((int)commandCount);
                    for (int i = 0; i < commandCount; i++)
                        Commands.Add(new Command(br, offsets));
                }
                br.StepOut();

                br.StepIn(offsets.Parameters + parametersOffset);
                {
                    Parameters = new List<Parameter>((int)parameterCount);
                    for (int i = 0; i < parameterCount; i++)
                        Parameters.Add(new Parameter(br));
                }
                br.StepOut();
            }
        }

        public class Command
        {
            public int CommandClass { get; set; }

            public int CommandIndex { get; set; }

            public byte[] Arguments { get; set; }

            public int? EventLayer { get; set; }

            internal Command(BinaryReaderEx br, Offsets offsets)
            {
                CommandClass = br.ReadInt32();
                CommandIndex = br.ReadInt32();
                long argumentsLength = br.ReadInt64();
                long argumentsOffset = br.ReadInt64();
                long eventLayerOffset = br.ReadInt64();

                Arguments = br.GetBytes(offsets.Arguments + argumentsOffset, (int)argumentsLength);

                if (eventLayerOffset != -1)
                {
                    br.StepIn(offsets.EventLayers + eventLayerOffset);
                    {
                        br.AssertInt32(2);
                        EventLayer = br.ReadInt32();
                        br.AssertInt64(0);
                        br.AssertInt64(-1);
                        br.AssertInt64(1);
                    }
                    br.StepOut();
                }
            }
        }

        public class Parameter
        {
            public long CommandIndex { get; set; }

            public long TargetStartByte { get; set; }

            public long SourceStartByte { get; set; }

            public int Length { get; set; }

            public int Unk10 { get; set; }

            internal Parameter(BinaryReaderEx br)
            {
                CommandIndex = br.ReadInt64();
                TargetStartByte = br.ReadInt64();
                SourceStartByte = br.ReadInt64();
                Length = br.ReadInt32();
                Unk10 = br.ReadInt32();
            }
        }
    }
}
