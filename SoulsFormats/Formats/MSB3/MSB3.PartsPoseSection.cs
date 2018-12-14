using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// A section that seems to contain groups of bone transforms. Might not even be used.
        /// </summary>
        public class PartsPoseSection : Section<PartsPose>
        {
            internal override string Type => "MAPSTUDIO_PARTS_POSE_ST";

            /// <summary>
            /// Parts pose entries in this section.
            /// </summary>
            public List<PartsPose> Poses;

            /// <summary>
            /// Creates a new PartsPoseSection with no entries.
            /// </summary>
            public PartsPoseSection(int unk1 = 0) : base(unk1)
            {
                Poses = new List<PartsPose>();
            }

            /// <summary>
            /// Returns every parts pose in the order they will be written.
            /// </summary>
            public override List<PartsPose> GetEntries()
            {
                return Poses;
            }

            internal override PartsPose ReadEntry(BinaryReaderEx br)
            {
                var pose = new PartsPose(br);
                Poses.Add(pose);
                return pose;
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<PartsPose> entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    entries[i].Write(bw);
                }
            }
        }

        /// <summary>
        /// Unknown; probably represents translations of bones in a model.
        /// </summary>
        public class PartsPose
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk00;

            /// <summary>
            /// Unknown; probably the transform of a single bone.
            /// </summary>
            public List<Member> Members;

            /// <summary>
            /// Creates a new PartsPose with no members.
            /// </summary>
            public PartsPose(short unk00 = 0)
            {
                Unk00 = unk00;
                Members = new List<Member>();
            }

            internal PartsPose(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                short count = br.ReadInt16();
                br.AssertInt32(0);
                br.AssertInt64(0x10);

                Members = new List<Member>(count);
                for (int i = 0; i < count; i++)
                    Members.Add(new Member(br));
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(Unk00);
                bw.WriteInt16((short)Members.Count);
                bw.WriteInt32(0);
                bw.WriteInt64(0x10);

                foreach (var member in Members)
                    member.Write(bw);
            }

            /// <summary>
            /// A member in a parts pose entry; probably corresponds to a bone.
            /// </summary>
            public class Member
            {
                /// <summary>
                /// Unknown; seems to just count up from 0 for each member.
                /// </summary>
                public int ID;

                /// <summary>
                /// Unknown, but probably translation.
                /// </summary>
                public Vector3 Translation;

                /// <summary>
                /// Unknown, but probably rotation.
                /// </summary>
                public Vector3 Rotation;

                /// <summary>
                /// Unknown, but almost certainly scale.
                /// </summary>
                public Vector3 Scale;

                /// <summary>
                /// Creates a new Member with the given ID and default transforms.
                /// </summary>
                public Member(int id)
                {
                    ID = id;
                    Translation = Vector3.Zero;
                    Rotation = Vector3.Zero;
                    Scale = Vector3.One;
                }

                internal Member(BinaryReaderEx br)
                {
                    ID = br.ReadInt32();
                    Translation = br.ReadVector3();
                    Rotation = br.ReadVector3();
                    Scale = br.ReadVector3();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ID);
                    bw.WriteVector3(Translation);
                    bw.WriteVector3(Rotation);
                    bw.WriteVector3(Scale);
                }

                /// <summary>
                /// Returns the ID and transforms of this member.
                /// </summary>
                public override string ToString()
                {
                    return $"{ID} : {Translation} {Rotation} {Scale}";
                }
            }
        }
    }
}
