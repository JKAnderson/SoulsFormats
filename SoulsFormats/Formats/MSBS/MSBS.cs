using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public partial class MSBS : SoulsFile<MSBS>
    {
        public ModelParam Models;

        public EventParam Events;

        public PointParam Regions;

        public RouteParam Routes;

        public PartsParam Parts;

        private EmptyParam Layers, PartsPoses, BoneNames;

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "MSB ";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("MSB ");
            br.AssertInt32(1);
            br.AssertInt32(0x10);
            br.AssertBoolean(false);
            br.AssertBoolean(false);
            br.AssertByte(1);
            br.AssertByte(0xFF);

            Entries entries;
            Models = new ModelParam();
            entries.Models = Models.Read(br);
            Events = new EventParam();
            List<Event> events = Events.Read(br);
            Regions = new PointParam();
            entries.Regions = Regions.Read(br);
            Routes = new RouteParam();
            Routes.Read(br);
            Layers = new EmptyParam("LAYER_PARAM_ST");
            Layers.Read(br);
            Parts = new PartsParam();
            entries.Parts = Parts.Read(br);
            PartsPoses = new EmptyParam("MAPSTUDIO_PARTS_POSE_ST");
            PartsPoses.Read(br);
            BoneNames = new EmptyParam("MAPSTUDIO_BONE_NAME_STRING");
            BoneNames.Read(br);

            if (br.Position != 0)
                throw new InvalidDataException("The next param offset of the final param should be 0, but it wasn't.");

            DisambiguateNames(entries.Models);
            DisambiguateNames(entries.Regions);
            DisambiguateNames(entries.Parts);

            foreach (Event evt in events)
                evt.GetNames(entries);
            foreach (Region region in entries.Regions)
                region.GetNames(entries);
            foreach (Part part in entries.Parts)
                part.GetNames(this, entries);
        }

        internal override void Write(BinaryWriterEx bw)
        {
            Entries entries;
            entries.Models = Models.GetEntries();
            List<Event> events = Events.GetEntries();
            entries.Regions = Regions.GetEntries();
            List<Route> routes = Routes.GetEntries();
            entries.Parts = Parts.GetEntries();

            foreach (Event evt in events)
                evt.GetIndices(entries);
            foreach (Region region in entries.Regions)
                region.GetIndices(entries);
            foreach (Part part in entries.Parts)
                part.GetIndices(this, entries);

            bw.WriteASCII("MSB ");
            bw.WriteInt32(1);
            bw.WriteInt32(0x10);
            bw.WriteBoolean(false);
            bw.WriteBoolean(false);
            bw.WriteByte(1);
            bw.WriteByte(0xFF);

            Models.Write(bw, entries.Models);
            bw.FillInt64("NextParamOffset", bw.Position);
            Events.Write(bw, events);
            bw.FillInt64("NextParamOffset", bw.Position);
            Regions.Write(bw, entries.Regions);
            bw.FillInt64("NextParamOffset", bw.Position);
            Routes.Write(bw, routes);
            bw.FillInt64("NextParamOffset", bw.Position);
            Layers.Write(bw, Layers.GetEntries());
            bw.FillInt64("NextParamOffset", bw.Position);
            Parts.Write(bw, entries.Parts);
            bw.FillInt64("NextParamOffset", bw.Position);
            PartsPoses.Write(bw, Layers.GetEntries());
            bw.FillInt64("NextParamOffset", bw.Position);
            BoneNames.Write(bw, Layers.GetEntries());
            bw.FillInt64("NextParamOffset", 0);
        }

        internal struct Entries
        {
            public List<Model> Models;
            public List<Region> Regions;
            public List<Part> Parts;
        }

        public abstract class Param<T> where T : Entry
        {
            public string Name { get; }

            public int Unk00 { get; set; }

            internal Param(string name)
            {
                Name = name;
            }

            internal List<T> Read(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                int offsetCount = br.ReadInt32();
                long nameOffset = br.ReadInt64();
                long[] entryOffsets = br.ReadInt64s(offsetCount - 1);
                long nextParamOffset = br.ReadInt64();

                string name = br.GetUTF16(nameOffset);
                if (name != Name)
                    throw new InvalidDataException($"Expected param \"{Name}\", got param \"{name}\"");

                var entries = new List<T>(offsetCount - 1);
                foreach (long offset in entryOffsets)
                {
                    br.Position = offset;
                    entries.Add(ReadEntry(br));
                }
                br.Position = nextParamOffset;
                return entries;
            }

            internal abstract T ReadEntry(BinaryReaderEx br);

            internal void Write(BinaryWriterEx bw, List<T> entries)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(entries.Count + 1);
                bw.ReserveInt64("ParamNameOffset");
                for (int i = 0; i < entries.Count; i++)
                    bw.ReserveInt64($"EntryOffset{i}");
                bw.ReserveInt64("NextParamOffset");

                bw.FillInt64("ParamNameOffset", bw.Position);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"EntryOffset{i}", bw.Position);
                    entries[i].Write(bw);
                }
            }

            public abstract List<T> GetEntries();
        }

        public abstract class Entry
        {
            public abstract string Name { get; set; }

            internal abstract void Write(BinaryWriterEx bw);
        }

        private class EmptyParam : Param<Model>
        {
            public EmptyParam(string name) : base(name) { }

            internal override Model ReadEntry(BinaryReaderEx br)
            {
                throw new InvalidDataException($"Expected param \"{Name}\" to be empty, but it wasn't.");
            }

            public override List<Model> GetEntries()
            {
                return new List<Model>();
            }
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
    }
}
