using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A map layout format used in DS2, DS3, and BB.
    /// </summary>
    public partial class MSB64 : SoulsFile<MSB64>
    {
        public ModelSection Models;

        public EventSection Events;

        public PointSection Regions;

        public RouteSection Routes;

        public LayerSection Layers;

        public PartsSection Parts;

        public DummySection PartsPoses;

        public BoneNameSection BoneNames;

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0);
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
                        Models = new ModelSection(br, unk1);
                        entries.Models = Models.Read(br, offsets);
                        break;

                    case "EVENT_PARAM_ST":
                        Events = new EventSection(br, unk1);
                        entries.Events = Events.Read(br, offsets);
                        break;

                    case "POINT_PARAM_ST":
                        Regions = new PointSection(br, unk1);
                        entries.Regions = Regions.Read(br, offsets);
                        break;

                    case "ROUTE_PARAM_ST":
                        Routes = new RouteSection(br, unk1);
                        entries.Routes = Routes.Read(br, offsets);
                        break;

                    case "LAYER_PARAM_ST":
                        Layers = new LayerSection(br, unk1);
                        entries.Layers = Layers.Read(br, offsets);
                        break;

                    case "PARTS_PARAM_ST":
                        Parts = new PartsSection(br, unk1);
                        entries.Parts = Parts.Read(br, offsets);
                        break;

                    case "MAPSTUDIO_PARTS_POSE_ST":
                        PartsPoses = new DummySection(br, unk1, type, offsets);
                        break;

                    case "MAPSTUDIO_BONE_NAME_STRING":
                        BoneNames = new BoneNameSection(br, unk1);
                        entries.BoneNames = BoneNames.Read(br, offsets);
                        break;

                    default:
                        throw new NotImplementedException($"Unimplemented section: {type}");
                }

                nextSectionOffset = br.ReadInt64();
            }

            Events.GetNames(this, entries);
            Parts.GetNames(this, entries);
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
            entries.BoneNames = BoneNames.GetEntries();

            Events.GetIndices(this, entries);
            Parts.GetIndices(this, entries);

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

            PartsPoses.Write(bw);
            bw.Pad(8);
            bw.FillInt64("NextOffset", bw.Position);

            BoneNames.Write(bw, entries.BoneNames);
            bw.FillInt64("NextOffset", 0);
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

        public abstract class Section<T>
        {
            public int Unk1;

            public abstract string Type { get; }
            
            internal Section(BinaryReaderEx br, int unk1)
            {
                Unk1 = unk1;
            }

            internal abstract List<T> GetEntries();

            internal List<T> Read(BinaryReaderEx br, int offsets)
            {
                var entries = new List<T>();
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

            public override string ToString()
            {
                return Type;
            }
        }

        public abstract class Entry
        {
            public abstract string Name { get; set; }
        }

        public class DummySection
        {
            public int Unk1;
            public string Type;
            public List<Dummy> Entries;

            internal DummySection(BinaryReaderEx br, int unk1, string type, int offsets)
            {
                Unk1 = unk1;
                Type = type;

                Entries = new List<Dummy>();
                for (int i = 0; i < offsets; i++)
                {
                    long offset = br.ReadInt64();
                    long next = br.GetInt64(br.Position);
                    Entries.Add(new Dummy(br.GetBytes(offset, (int)(next - offset)), type));
                }
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk1);
                bw.WriteInt32(Entries.Count + 1);
                bw.ReserveInt64("TypeOffset");

                for (int i = 0; i < Entries.Count; i++)
                    bw.ReserveInt64($"Offset{i}");

                bw.ReserveInt64("NextOffset");

                bw.FillInt64("TypeOffset", bw.Position);
                bw.WriteUTF16(Type, true);
                bw.Pad(8);

                for (int i = 0; i < Entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    bw.WriteBytes(Entries[i].Bytes);
                }
            }

            public class Dummy
            {
                public string Name;
                public byte[] Bytes;

                internal Dummy(byte[] bytes, string type)
                {
                    Bytes = bytes;

                    if (type != "MAPSTUDIO_PARTS_POSE_ST")
                    {
                        BinaryReaderEx br = new BinaryReaderEx(false, bytes);
                        long nameOffset = br.ReadInt64();
                        Name = br.GetUTF16(nameOffset);
                    }
                    else
                    {
                        Name = null;
                    }
                }

                public override string ToString()
                {
                    return $"{Name}";
                }
            }
        }
    }
}
