using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSBS
    {
        public enum RouteType : uint
        {
            MufflingPortalLink = 3,
            MufflingBoxLink = 4,
        }

        public class RouteParam : Param<Route>
        {
            public List<Route.MufflingPortalLink> MufflingPortalLinks { get; set; }

            public List<Route.MufflingBoxLink> MufflingBoxLinks { get; set; }

            public RouteParam() : this(0x23) { }

            public RouteParam(int unk00) : base(unk00, "ROUTE_PARAM_ST")
            {
                MufflingPortalLinks = new List<Route.MufflingPortalLink>();
                MufflingBoxLinks = new List<Route.MufflingBoxLink>();
            }

            internal override Route ReadEntry(BinaryReaderEx br)
            {
                RouteType type = br.GetEnum32<RouteType>(br.Position + 0x10);
                switch (type)
                {
                    case RouteType.MufflingPortalLink:
                        var portalLink = new Route.MufflingPortalLink(br);
                        MufflingPortalLinks.Add(portalLink);
                        return portalLink;

                    case RouteType.MufflingBoxLink:
                        var boxLink = new Route.MufflingBoxLink(br);
                        MufflingBoxLinks.Add(boxLink);
                        return boxLink;

                    default:
                        throw new NotImplementedException($"Unimplemented route type: {type}");
                }
            }

            public override List<Route> GetEntries()
            {
                return SFUtil.ConcatAll<Route>(
                    MufflingPortalLinks, MufflingBoxLinks);
            }
        }

        public abstract class Route : Entry
        {
            public abstract RouteType Type { get; }

            public override string Name { get; set; }

            public int Unk08 { get; set; }

            public int Unk0C { get; set; }

            internal Route(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                br.AssertNull(0x68, false);

                Name = br.GetUTF16(start + nameOffset);
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
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
                return $"\"{Name}\" {Unk08} {Unk0C}";
            }

            public class MufflingPortalLink : Route
            {
                public override RouteType Type => RouteType.MufflingPortalLink;

                internal MufflingPortalLink(BinaryReaderEx br) : base(br) { }
            }

            public class MufflingBoxLink : Route
            {
                public override RouteType Type => RouteType.MufflingBoxLink;

                internal MufflingBoxLink(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
