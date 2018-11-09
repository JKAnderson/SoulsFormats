using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    /// <summary>
    /// Controls when different events happen during animations; this specific version used in DS3. Extension: .tae
    /// </summary>
    public partial class TAE3 : SoulsFile<TAE3>
    {
        /// <summary>
        /// ID number of this TAE.
        /// </summary>
        public int ID;

        /// <summary>
        /// Unknown flags.
        /// </summary>
        public byte[] Flags { get; private set; }

        /// <summary>
        /// Unknown .hkt file.
        /// </summary>
        public string SkeletonName;

        /// <summary>
        /// Unknown .sib file.
        /// </summary>
        public string SibName;

        /// <summary>
        /// Animations controlled by this TAE.
        /// </summary>
        public List<Animation> Animations;

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.ReadASCII(4);
            return magic == "TAE ";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("TAE ");
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0xFF);
            int version = br.AssertInt32(0x1000C);
            int fileSize = br.ReadInt32();
            br.AssertInt64(0x40);
            br.AssertInt64(1);
            br.AssertInt64(0x50);
            br.AssertInt64(0x80);
            br.AssertInt64(0x15);
            br.AssertInt64(0);
            Flags = br.ReadBytes(8);
            br.AssertInt64(1);
            ID = br.ReadInt32();
            int animCount = br.ReadInt32();
            long animsOffset = br.ReadInt64();
            long animGroupsOffset = br.ReadInt64();
            br.AssertInt64(0xA0);
            br.AssertInt64(animCount);
            long firstAnimOffset = br.ReadInt64();
            br.AssertInt64(1);
            br.AssertInt64(0x90);
            br.AssertInt32(ID);
            br.AssertInt32(ID);
            br.AssertInt64(0x50);
            br.AssertInt64(0);
            br.AssertInt64(0xB0);
            long skeletonNameOffset = br.ReadInt64();
            long sibNameOffset = br.ReadInt64();
            br.AssertInt64(0);
            br.AssertInt64(0);

            SkeletonName = br.GetUTF16(skeletonNameOffset);
            SibName = br.GetUTF16(sibNameOffset);

            br.StepIn(animsOffset);
            {
                Animations = new List<Animation>(animCount);
                for (int i = 0; i < animCount; i++)
                    Animations.Add(new Animation(br));
            }
            br.StepOut();

            // Don't bother reading anim groups.
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("TAE ");
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0xFF);
            bw.WriteInt32(0x1000C);
            bw.ReserveInt32("FileSize");
            bw.WriteInt64(0x40);
            bw.WriteInt64(1);
            bw.WriteInt64(0x50);
            bw.WriteInt64(0x80);
            bw.WriteInt64(0x15);
            bw.WriteInt64(0);
            bw.WriteBytes(Flags);
            bw.WriteInt64(1);
            bw.WriteInt32(ID);
            bw.WriteInt32(Animations.Count);
            bw.ReserveInt64("AnimsOffset");
            bw.ReserveInt64("AnimGroupsOffset");
            bw.WriteInt64(0xA0);
            bw.WriteInt64(Animations.Count);
            bw.ReserveInt64("FirstAnimOffset");
            bw.WriteInt64(1);
            bw.WriteInt64(0x90);
            bw.WriteInt32(ID);
            bw.WriteInt32(ID);
            bw.WriteInt64(0x50);
            bw.WriteInt64(0);
            bw.WriteInt64(0xB0);
            bw.ReserveInt64("SkeletonName");
            bw.ReserveInt64("SibName");
            bw.WriteInt64(0);
            bw.WriteInt64(0);

            bw.FillInt64("SkeletonName", bw.Position);
            bw.WriteUTF16(SkeletonName, true);
            bw.Pad(0x10);

            bw.FillInt64("SibName", bw.Position);
            bw.WriteUTF16(SibName, true);
            bw.Pad(0x10);

            Animations.Sort((a1, a2) => a1.ID.CompareTo(a2.ID));

            bw.FillInt64("AnimsOffset", bw.Position);
            var animOffsets = new List<long>(Animations.Count);
            for (int i = 0; i < Animations.Count; i++)
            {
                animOffsets.Add(bw.Position);
                Animations[i].WriteHeader(bw, i);
            }

            bw.FillInt64("AnimGroupsOffset", bw.Position);
            bw.ReserveInt64("AnimGroupsCount");
            bw.ReserveInt64("AnimGroupsOffset");
            int groupCount = 0;
            long groupStart = bw.Position;
            for (int i = 0; i < Animations.Count; i++)
            {
                int firstIndex = i;
                bw.WriteInt32((int)Animations[i].ID);
                while (i < Animations.Count - 1 && Animations[i + 1].ID == Animations[i].ID + 1)
                    i++;
                bw.WriteInt32((int)Animations[i].ID);
                bw.WriteInt64(animOffsets[firstIndex]);
                groupCount++;
            }
            bw.FillInt64("AnimGroupsCount", groupCount);
            if (groupCount == 0)
                bw.FillInt64("AnimGroupsOffset", 0);
            else
                bw.FillInt64("AnimGroupsOffset", groupStart);

            bw.FillInt64("FirstAnimOffset", bw.Position);
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].WriteBody(bw, i);

            for (int i = 0; i < Animations.Count; i++)
            {
                Animations[i].WriteAnimFile(bw, i);

                long timeStart = bw.Position;
                Animations[i].WriteTimes(bw, i);

                var eventHeaderOffsets = Animations[i].WriteEventHeaders(bw, i, timeStart);

                Animations[i].WriteEventData(bw, i);

                Animations[i].WriteEventGroupHeaders(bw, i);

                Animations[i].WriteEventGroupData(bw, i, eventHeaderOffsets);
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Controls an individual animation.
        /// </summary>
        public class Animation
        {
            /// <summary>
            /// ID number of this animation.
            /// </summary>
            public long ID;

            /// <summary>
            /// Timed events in this animation.
            /// </summary>
            public List<Event> Events;

            /// <summary>
            /// Unknown groups of events.
            /// </summary>
            public List<EventGroup> EventGroups;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk28;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool AnimFileReference;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int AnimFileUnk18, AnimFileUnk20;

            /// <summary>
            /// Unknown.
            /// </summary>
            public string AnimFileName;

            internal Animation(BinaryReaderEx br)
            {
                ID = br.ReadInt64();
                long offset = br.ReadInt64();
                br.StepIn(offset);
                {
                    long eventHeadersOffset = br.ReadInt64();
                    long eventGroupsOffset = br.ReadInt64();
                    long timesOffset = br.ReadInt64();
                    long animFileOffset = br.ReadInt64();
                    int eventCount = br.ReadInt32();
                    int eventGroupCount = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    br.AssertInt32(0);

                    var eventHeaderOffsets = new List<long>(eventCount);
                    Events = new List<Event>(eventCount);
                    if (eventHeadersOffset != 0)
                    {
                        br.StepIn(eventHeadersOffset);
                        {
                            for (int i = 0; i < eventCount; i++)
                            {
                                eventHeaderOffsets.Add(br.Position);
                                Events.Add(Event.Read(br));
                            }
                        }
                        br.StepOut();
                    }

                    EventGroups = new List<EventGroup>(eventGroupCount);
                    if (eventGroupsOffset != 0)
                    {
                        br.StepIn(eventGroupsOffset);
                        {
                            for (int i = 0; i < eventGroupCount; i++)
                                EventGroups.Add(new EventGroup(br, eventHeaderOffsets));
                        }
                        br.StepOut();
                    }

                    br.StepIn(animFileOffset);
                    {
                        AnimFileReference = br.AssertInt64(0, 1) == 1;
                        br.AssertInt64(br.Position + 8);
                        long animFileNameOffset = br.ReadInt64();
                        AnimFileUnk18 = br.ReadInt32();
                        // TODO
                        AnimFileUnk20 = br.ReadInt32();
                        br.AssertInt64(0);
                        br.AssertInt64(0);

                        if (animFileNameOffset < br.Stream.Length)
                            AnimFileName = br.GetUTF16(animFileNameOffset);
                        else
                            AnimFileName = null;
                    }
                    br.StepOut();
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i)
            {
                bw.WriteInt64(ID);
                bw.ReserveInt64($"AnimationOffset{i}");
            }

            internal void WriteBody(BinaryWriterEx bw, int i)
            {
                bw.FillInt64($"AnimationOffset{i}", bw.Position);
                bw.ReserveInt64($"EventHeadersOffset{i}");
                bw.ReserveInt64($"EventGroupHeadersOffset{i}");
                bw.ReserveInt64($"TimesOffset{i}");
                bw.ReserveInt64($"AnimFileOffset{i}");
                bw.WriteInt32(Events.Count);
                bw.WriteInt32(EventGroups.Count);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(0);
            }

            internal void WriteAnimFile(BinaryWriterEx bw, int i)
            {
                bw.FillInt64($"AnimFileOffset{i}", bw.Position);
                bw.WriteInt64(AnimFileReference ? 1 : 0);
                bw.WriteInt64(bw.Position + 8);
                bw.ReserveInt64("AnimFileNameOffset");
                bw.WriteInt32(AnimFileUnk18);
                bw.WriteInt32(AnimFileUnk20);
                bw.WriteInt64(0);
                bw.WriteInt64(0);

                bw.FillInt64("AnimFileNameOffset", bw.Position);
                if (AnimFileName != null)
                {
                    bw.WriteUTF16(AnimFileName, true);
                    bw.Pad(0x10);
                }
            }

            internal void WriteTimes(BinaryWriterEx bw, int i)
            {
                bw.FillInt64($"TimesOffset{i}", bw.Position);
                for (int j = 0; j < Events.Count; j++)
                    Events[j].WriteTime(bw);
            }

            internal List<long> WriteEventHeaders(BinaryWriterEx bw, int i, long timeStart)
            {
                var eventHeaderOffsets = new List<long>(Events.Count);
                if (Events.Count > 0)
                {
                    bw.FillInt64($"EventHeadersOffset{i}", bw.Position);
                    for (int j = 0; j < Events.Count; j++)
                    {
                        eventHeaderOffsets.Add(bw.Position);
                        Events[j].WriteHeader(bw, i, j, timeStart);
                    }
                }
                else
                {
                    bw.FillInt64($"EventHeadersOffset{i}", 0);
                }
                return eventHeaderOffsets;
            }

            internal void WriteEventData(BinaryWriterEx bw, int i)
            {
                for (int j = 0; j < Events.Count; j++)
                    Events[j].WriteData(bw, i, j);
            }

            internal void WriteEventGroupHeaders(BinaryWriterEx bw, int i)
            {
                if (EventGroups.Count > 0)
                {
                    bw.FillInt64($"EventGroupHeadersOffset{i}", bw.Position);
                    for (int j = 0; j < EventGroups.Count; j++)
                        EventGroups[j].WriteHeader(bw, i, j);
                }
                else
                {
                    bw.FillInt64($"EventGroupHeadersOffset{i}", 0);
                }
            }

            internal void WriteEventGroupData(BinaryWriterEx bw, int i, List<long> eventHeaderOffsets)
            {
                for (int j = 0; j < EventGroups.Count; j++)
                    EventGroups[j].WriteData(bw, i, j, eventHeaderOffsets);
            }
        }

        /// <summary>
        /// A group of events in an animation with an associated EventType that does not necessarily match theirs.
        /// </summary>
        public class EventGroup
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public EventType Type;

            /// <summary>
            /// Indices of events in this group in the parent animation's collection.
            /// </summary>
            public List<int> Indices;

            internal EventGroup(BinaryReaderEx br, List<long> eventHeaderOffsets)
            {
                long entryCount = br.ReadInt64();
                long valuesOffset = br.ReadInt64();
                long typeOffset = br.ReadInt64();
                br.AssertInt64(0);

                br.StepIn(typeOffset);
                {
                    Type = br.ReadEnum64<EventType>();
                    br.AssertInt64(0);
                }
                br.StepOut();

                br.StepIn(valuesOffset);
                {
                    Indices = br.ReadInt32s((int)entryCount).Select(offset => eventHeaderOffsets.FindIndex(headerOffset => headerOffset == offset)).ToList();
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i, int j)
            {
                bw.WriteInt64(Indices.Count);
                bw.ReserveInt64($"EventGroupValuesOffset{i}:{j}");
                bw.ReserveInt64($"EventGroupTypeOffset{i}:{j}");
                bw.WriteInt64(0);
            }

            internal void WriteData(BinaryWriterEx bw, int i, int j, List<long> eventHeaderOffsets)
            {
                bw.FillInt64($"EventGroupTypeOffset{i}:{j}", bw.Position);
                bw.WriteUInt64((ulong)Type);
                bw.WriteInt64(0);

                bw.FillInt64($"EventGroupValuesOffset{i}:{j}", bw.Position);
                for (int k = 0; k < Indices.Count; k++)
                    bw.WriteInt32((int)eventHeaderOffsets[Indices[k]]);
                bw.Pad(0x10);
            }
        }
    }
}
