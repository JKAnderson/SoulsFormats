using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SoulsFormats
{
    /// <summary>
    /// A map layout file used in DS1. Extension: .msb
    /// </summary>
    public partial class MSB1 : SoulsFile<MSB1>
    {
        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public ModelParam Models { get; set; }

        /// <summary>
        /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
        /// </summary>
        public EventParam Events { get; set; }

        /// <summary>
        /// Points or areas of space that trigger some sort of behavior.
        /// </summary>
        public PointParam Regions { get; set; }

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public PartsParam Parts { get; set; }

        internal struct Entries
        {
            public List<Model> Models;
            public List<Event> Events;
            public List<Region> Regions;
            public List<Part> Parts;
        }

        /// <summary>
        /// Creates an empty MSB1.
        /// </summary>
        public MSB1()
        {
            Models = new ModelParam();
            Events = new EventParam();
            Regions = new PointParam();
            Parts = new PartsParam();
        }

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            Entries entries;
            Models = new ModelParam();
            entries.Models = Models.Read(br);
            Events = new EventParam();
            entries.Events = Events.Read(br);
            Regions = new PointParam();
            entries.Regions = Regions.Read(br);
            Parts = new PartsParam();
            entries.Parts = Parts.Read(br);

            if (br.Position != 0)
                throw new InvalidDataException("The next param offset of the final param should be 0, but it wasn't.");

            DisambiguateNames(entries.Models);
            DisambiguateNames(entries.Regions);
            DisambiguateNames(entries.Parts);

            foreach (Event evt in entries.Events)
                evt.GetNames(this, entries);
            foreach (Part part in entries.Parts)
                part.GetNames(this, entries);
        }

        internal override void Write(BinaryWriterEx bw)
        {
            Entries entries;
            entries.Models = Models.GetEntries();
            entries.Events = Events.GetEntries();
            entries.Regions = Regions.GetEntries();
            entries.Parts = Parts.GetEntries();

            Models.DiscriminateModels();
            foreach (Model model in entries.Models)
                model.CountInstances(entries.Parts);
            foreach (Event evt in entries.Events)
                evt.GetIndices(this, entries);
            foreach (Part part in entries.Parts)
                part.GetIndices(this, entries);

            bw.BigEndian = false;

            Models.Write(bw, entries.Models);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Events.Write(bw, entries.Events);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Regions.Write(bw, entries.Regions);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Parts.Write(bw, entries.Parts);
            bw.FillInt32("NextParamOffset", 0);
        }

        /// <summary>
        /// A generic group of entries in an MSB.
        /// </summary>
        public abstract class Param<T> where T : Entry
        {
            /// <summary>
            /// A string identifying the type of entries in the param.
            /// </summary>
            internal abstract string Name { get; }

            internal List<T> Read(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                int nameOffset = br.ReadInt32();
                int offsetCount = br.ReadInt32();
                int[] entryOffsets = br.ReadInt32s(offsetCount - 1);
                int nextParamOffset = br.ReadInt32();

                string name = br.GetASCII(nameOffset);
                if (name != Name)
                    throw new InvalidDataException($"Expected param \"{Name}\", got param \"{name}\"");

                var entries = new List<T>(offsetCount - 1);
                foreach (int offset in entryOffsets)
                {
                    br.Position = offset;
                    entries.Add(ReadEntry(br));
                }
                br.Position = nextParamOffset;
                return entries;
            }

            internal abstract T ReadEntry(BinaryReaderEx br);

            internal virtual void Write(BinaryWriterEx bw, List<T> entries)
            {
                bw.WriteInt32(0);
                bw.ReserveInt32("ParamNameOffset");
                bw.WriteInt32(entries.Count + 1);
                for (int i = 0; i < entries.Count; i++)
                    bw.ReserveInt32($"EntryOffset{i}");
                bw.ReserveInt32("NextParamOffset");

                bw.FillInt32("ParamNameOffset", (int)bw.Position);
                bw.WriteASCII(Name, true);
                bw.Pad(4);

                int id = 0;
                Type type = null;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (type != entries[i].GetType())
                    {
                        type = entries[i].GetType();
                        id = 0;
                    }

                    bw.FillInt32($"EntryOffset{i}", (int)bw.Position);
                    entries[i].Write(bw, id);
                    id++;
                }
            }

            /// <summary>
            /// Returns all of the entries in this param, in the order they will be written to the file.
            /// </summary>
            public abstract List<T> GetEntries();

            /// <summary>
            /// Returns the name of the param as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Name}";
            }
        }

        /// <summary>
        /// A generic entry in an MSB param.
        /// </summary>
        public abstract class Entry
        {
            /// <summary>
            /// The name of this entry.
            /// </summary>
            public string Name { get; set; }

            internal abstract void Write(BinaryWriterEx bw, int id);
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
                        entry.Name = $"{name} {{{nameCounts[name]}}}";
                    }
                }
            }
            while (ambiguous);
        }

        private static string ReambiguateName(string name)
        {
            return Regex.Replace(name, @" \{\d+\}", "");
        }

        private static string FindName<T>(List<T> list, int index) where T : Entry
        {
            if (index == -1)
                return null;
            else
                return list[index].Name;
        }

        private static string[] FindNames<T>(List<T> list, int[] indices) where T : Entry
        {
            var names = new string[indices.Length];
            for (int i = 0; i < indices.Length; i++)
                names[i] = FindName(list, indices[i]);
            return names;
        }

        private static int FindIndex<T>(List<T> list, string name) where T : Entry
        {
            if (name == null)
            {
                return -1;
            }
            else
            {
                int result = list.FindIndex(entry => entry.Name == name);
                if (result == -1)
                    throw new KeyNotFoundException($"Name not found: {name}");
                return result;
            }
        }

        private static int[] FindIndices<T>(List<T> list, string[] names) where T : Entry
        {
            var indices = new int[names.Length];
            for (int i = 0; i < names.Length; i++)
                indices[i] = FindIndex(list, names[i]);
            return indices;
        }
    }
}
