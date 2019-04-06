using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSBS
    {
        public class RouteParam : Param<Route>
        {
            public List<Route> Routes { get; set; }

            internal RouteParam() : base("ROUTE_PARAM_ST")
            {
                Routes = new List<Route>();
            }

            internal override Route ReadEntry(BinaryReaderEx br)
            {
                var route = new Route(br);
                Routes.Add(route);
                return route;
            }

            public override List<Route> GetEntries()
            {
                return Routes;
            }
        }

        public class Route : Entry
        {
            public override string Name { get; set; }

            public int Unk08 { get; set; }

            public int Unk0C { get; set; }

            public int Unk10 { get; set; }

            public int Unk14 { get; set; }

            internal Route(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                br.AssertNull(0x68, false);

                Name = br.GetUTF16(start + nameOffset);
            }

            internal override void Write(BinaryWriterEx bw)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteNull(0x68, false);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);
            }

            /// <summary>
            /// Returns the name and values associated with the route as a string.
            /// </summary>
            public override string ToString()
            {
                return $"\"{Name}\" {Unk08} {Unk0C} {Unk10} {Unk14}";
            }
        }
    }
}
