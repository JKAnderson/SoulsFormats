using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB3
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

            /// <summary>
            /// Creates a new RouteSection with no routes.
            /// </summary>
            public RouteSection(int unk1 = 3) : base(unk1)
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
            public int Unk08, Unk0C;

            /// <summary>
            /// Unknown; seems to always be 4.
            /// </summary>
            public int Unk10;

            /// <summary>
            /// Unknown; seems to just count up from 0.
            /// </summary>
            public int Unk14;

            /// <summary>
            /// Creates a new Route with default values.
            /// </summary>
            public Route()
            {
                Name = "";
                Unk08 = 0;
                Unk0C = 0;
                Unk10 = 4;
                Unk14 = 0;
            }

            internal Route(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();

                for (int i = 0; i < 26; i++)
                    br.AssertInt32(0);

                Name = br.GetUTF16(start + nameOffset);
            }

            internal void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);

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
                return $"{Name} ({Unk08}, {Unk0C}, {Unk10}, {Unk14})";
            }
        }
    }
}
