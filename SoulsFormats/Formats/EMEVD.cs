using System;
using System.Collections.Generic;

namespace SoulsFormats.Formats
{
    /// <summary>
    /// A collection of event scripts; this class only supports Sekiro, I think?
    /// </summary>
    public class EMEVD : SoulsFile<EMEVD>
    {
        /// <summary>
        /// Events in the file.
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// Offsets to strings that indicate linked emevd files.
        /// </summary>
        public List<long> LinkedFileOffsets { get; set; }

        /// <summary>
        /// Undifferentiated string data block.
        /// </summary>
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

        /// <summary>
        /// Defines the behavior of an event when the player rests at a bonfire.
        /// </summary>
        public enum RestBehaviorType : uint
        {
            /// <summary>
            /// Nothing happens to the event.
            /// </summary>
            None = 0,

            /// <summary>
            /// The event restarts.
            /// </summary>
            Restart = 1,

            /// <summary>
            /// The event is terminated.
            /// </summary>
            End = 2,
        }

        /// <summary>
        /// A set of instructions making up an event scripts.
        /// </summary>
        public class Event
        {
            /// <summary>
            /// The ID of this event.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// The behavior of the event when resting.
            /// </summary>
            public RestBehaviorType RestBehavior { get; set; }

            /// <summary>
            /// Instructions that make up the event.
            /// </summary>
            public List<Command> Commands { get; set; }

            /// <summary>
            /// Parameters that can be passed to the event.
            /// </summary>
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

        /// <summary>
        /// An individual instruction in an event.
        /// </summary>
        public class Command
        {
            /// <summary>
            /// The broad category of commands it's in.
            /// </summary>
            public int CommandClass { get; set; }

            /// <summary>
            /// The specific type of command to run.
            /// </summary>
            public int CommandIndex { get; set; }

            /// <summary>
            /// Undifferentiated argument data for the command.
            /// </summary>
            public byte[] Arguments { get; set; }

            /// <summary>
            /// If not null, determines under which ceremonies the command will run.
            /// </summary>
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

        /// <summary>
        /// A parameter passed to an event.
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// Index of the command that the parameter applies to.
            /// </summary>
            public long CommandIndex { get; set; }

            /// <summary>
            /// Index of the starting byte in the command arguments.
            /// </summary>
            public long TargetStartByte { get; set; }

            /// <summary>
            /// Index of the starting byte in the event parameters.
            /// </summary>
            public long SourceStartByte { get; set; }

            /// <summary>
            /// Length of the data in bytes.
            /// </summary>
            public int Length { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
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
