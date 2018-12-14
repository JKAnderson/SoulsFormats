using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A map layout format used in DS3.
    /// </summary>
    public partial class MSB3 : SoulsFile<MSB3>
    {
        /// <summary>
        /// Models in this MSB.
        /// </summary>
        public ModelSection Models;

        /// <summary>
        /// Events in this MSB.
        /// </summary>
        public EventSection Events;

        /// <summary>
        /// Regions in this MSB.
        /// </summary>
        public PointSection Regions;

        /// <summary>
        /// Routes in this MSB.
        /// </summary>
        public RouteSection Routes;

        /// <summary>
        /// Layers in this MSB.
        /// </summary>
        public LayerSection Layers;

        /// <summary>
        /// Parts in this MSB.
        /// </summary>
        public PartsSection Parts;

        /// <summary>
        /// PartsPose data in this MSB.
        /// </summary>
        public PartsPoseSection PartsPoses;

        /// <summary>
        /// Bone names in this MSB.
        /// </summary>
        public BoneNameSection BoneNames;

        /// <summary>
        /// Creates a new MSB3 with all sections empty.
        /// </summary>
        public MSB3()
        {
            Models = new ModelSection();
            Events = new EventSection();
            Regions = new PointSection();
            Routes = new RouteSection();
            Layers = new LayerSection();
            Parts = new PartsSection();
            PartsPoses = new PartsPoseSection();
            BoneNames = new BoneNameSection();
        }

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "MSB ";
        }

        internal struct Entries
        {
            public List<Model> Models;
            public List<Event> Events;
            public List<Region> Regions;
            public List<Route> Routes;
            public List<Layer> Layers;
            public List<Part> Parts;
            public List<PartsPose> PartsPoses;
            public List<string> BoneNames;
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("MSB ");
            br.AssertInt32(1);
            // Header size/data start
            br.AssertInt32(0x10);

            // Probably bytes, just guessing
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(1);
            br.AssertByte(0xFF);

            Entries entries = default;

            long nextSectionOffset = br.Position;
            while (nextSectionOffset != 0)
            {
                br.Position = nextSectionOffset;

                int unk1 = br.ReadInt32();
                int offsets = br.ReadInt32() - 1;
                long typeOffset = br.ReadInt64();
                string type = br.GetUTF16(typeOffset);

                switch (type)
                {
                    case "MODEL_PARAM_ST":
                        Models = new ModelSection(unk1);
                        entries.Models = Models.Read(br, offsets);
                        break;

                    case "EVENT_PARAM_ST":
                        Events = new EventSection(unk1);
                        entries.Events = Events.Read(br, offsets);
                        break;

                    case "POINT_PARAM_ST":
                        Regions = new PointSection(unk1);
                        entries.Regions = Regions.Read(br, offsets);
                        break;

                    case "ROUTE_PARAM_ST":
                        Routes = new RouteSection(unk1);
                        entries.Routes = Routes.Read(br, offsets);
                        break;

                    case "LAYER_PARAM_ST":
                        Layers = new LayerSection(unk1);
                        entries.Layers = Layers.Read(br, offsets);
                        break;

                    case "PARTS_PARAM_ST":
                        Parts = new PartsSection(unk1);
                        entries.Parts = Parts.Read(br, offsets);
                        break;

                    case "MAPSTUDIO_PARTS_POSE_ST":
                        PartsPoses = new PartsPoseSection(unk1);
                        entries.PartsPoses = PartsPoses.Read(br, offsets);
                        break;

                    case "MAPSTUDIO_BONE_NAME_STRING":
                        BoneNames = new BoneNameSection(unk1);
                        entries.BoneNames = BoneNames.Read(br, offsets);
                        break;

                    default:
                        throw new NotImplementedException($"Unimplemented section: {type}");
                }

                nextSectionOffset = br.ReadInt64();
            }

            DisambiguateNames(entries.Events);
            DisambiguateNames(entries.Models);
            DisambiguateNames(entries.Parts);
            DisambiguateNames(entries.Regions);

            Events.GetNames(this, entries);
            Parts.GetNames(this, entries);
            Regions.GetNames(this, entries);
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            Entries entries;
            entries.Models = Models.GetEntries();
            entries.Events = Events.GetEntries();
            entries.Regions = Regions.GetEntries();
            entries.Routes = Routes.GetEntries();
            entries.Layers = Layers.GetEntries();
            entries.Parts = Parts.GetEntries();
            entries.PartsPoses = PartsPoses.GetEntries();
            entries.BoneNames = BoneNames.GetEntries();

            Models.CountInstances(entries);
            Events.GetIndices(this, entries);
            Parts.GetIndices(this, entries);
            Regions.GetIndices(this, entries);

            bw.WriteASCII("MSB ");
            bw.WriteInt32(1);
            bw.WriteInt32(0x10);

            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(1);
            bw.WriteByte(0xFF);

            Models.Write(bw, entries.Models);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            Events.Write(bw, entries.Events);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            Regions.Write(bw, entries.Regions);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            Routes.Write(bw, entries.Routes);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            Layers.Write(bw, entries.Layers);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            Parts.Write(bw, entries.Parts);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            PartsPoses.Write(bw, entries.PartsPoses);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            BoneNames.Write(bw, entries.BoneNames);
            bw.FillInt64("NextOffset", 0);
        }

        private static void DisambiguateNames<T>(List<T> entries) where T : Entry
        {
            bool ambiguous;
            do
            {
                ambiguous = false;
                var nameCounts = new Dictionary<string, int>();
                foreach (Entry entry in entries)
                {
                    string name = entry.Name;
                    if (!nameCounts.ContainsKey(name))
                    {
                        nameCounts[name] = 1;
                    }
                    else
                    {
                        ambiguous = true;
                        nameCounts[name]++;
                        entry.Name = $"{name} ({nameCounts[name]})";
                    }
                }
            }
            while (ambiguous);
        }

        private static string GetName<T>(List<T> list, int index) where T : Entry
        {
            if (index == -1)
                return null;
            else
                return list[index].Name;
        }

        private static int GetIndex<T>(List<T> list, string name) where T : Entry
        {
            if (name == null)
                return -1;
            else
            {
                int result = list.FindIndex(entry => entry.Name == name);
                if (result == -1)
                    throw new KeyNotFoundException("No items found in list.");
                return result;
            }
        }

        /// <summary>
        /// A generic MSB section containing a list of entries.
        /// </summary>
        public abstract class Section<T>
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1;

            internal abstract string Type { get; }

            internal Section(int unk1)
            {
                Unk1 = unk1;
            }

            /// <summary>
            /// Returns every entry in this section in the order they will be written.
            /// </summary>
            public abstract List<T> GetEntries();

            internal List<T> Read(BinaryReaderEx br, int offsets)
            {
                var entries = new List<T>(offsets);
                for (int i = 0; i < offsets; i++)
                {
                    long offset = br.ReadInt64();
                    br.StepIn(offset);
                    entries.Add(ReadEntry(br));
                    br.StepOut();
                }
                return entries;
            }

            internal abstract T ReadEntry(BinaryReaderEx br);

            internal void Write(BinaryWriterEx bw, List<T> entries)
            {
                bw.WriteInt32(Unk1);
                bw.WriteInt32(entries.Count + 1);
                bw.ReserveInt64("TypeOffset");
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.ReserveInt64($"Offset{i}");
                }
                bw.ReserveInt64("NextOffset");

                bw.FillInt64("TypeOffset", bw.Position);
                bw.WriteUTF16(Type, true);
                bw.Pad(8);
                WriteEntries(bw, entries);
            }

            internal abstract void WriteEntries(BinaryWriterEx bw, List<T> entries);

            /// <summary>
            /// Returns the type string, unknown value and number of entries in this section.
            /// </summary>
            public override string ToString()
            {
                return $"{Type}:{Unk1}[{GetEntries().Count}]";
            }
        }

        /// <summary>
        /// A generic entry in an MSB section.
        /// </summary>
        public abstract class Entry
        {
            /// <summary>
            /// The name of this entry.
            /// </summary>
            public abstract string Name { get; set; }
        }
    }
}
