using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB64
    {
        /// <summary>
        /// A section containing routes. Purpose unknown.
        /// </summary>
        public class RouteSection : Section<Route>
        {
            internal override string Type => "ROUTE_PARAM_ST";

            /// <summary>
            /// The routes in this section.
            /// </summary>
            public List<Route> Routes;

            internal RouteSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                Routes = new List<Route>();
            }

            /// <summary>
            /// Returns every route in the order they will be written.
            /// </summary>
            public override List<Route> GetEntries()
            {
                return Routes;
            }

            internal override Route ReadEntry(BinaryReaderEx br)
            {
                var route = new Route(br);
                Routes.Add(route);
                return route;
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<Route> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    entries[i].Write(bw);
                }
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Route
        {
            /// <summary>
            /// The name of this route.
            /// </summary>
            public string Name;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1, Unk2, Unk3, Unk4;

            internal Route(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                Unk1 = br.ReadInt32();
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadInt32();
                Unk4 = br.ReadInt32();

                for (int i = 0; i < 26; i++)
                    br.AssertInt32(0);

                Name = br.GetUTF16(start + nameOffset);
            }

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk1);
                bw.WriteInt32(Unk2);
                bw.WriteInt32(Unk3);
                bw.WriteInt32(Unk4);

                for (int i = 0; i < 26; i++)
                    bw.WriteInt32(0);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);
            }

            /// <summary>
            /// Returns the name and four values of this route.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} ({Unk1}, {Unk2}, {Unk3}, {Unk4})";
            }
        }
    }
}
