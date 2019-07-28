using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB2
    {
        private class MapstudioPartsPose : Param<PartPose>
        {
            internal override string Name => "MAPSTUDIO_PARTS_POSE_ST";
            internal override int Version => 0;

            public List<PartPose> Poses { get; set; }

            public MapstudioPartsPose()
            {
                Poses = new List<PartPose>();
            }

            internal override PartPose ReadEntry(BinaryReaderEx br)
            {
                var pose = new PartPose(br);
                Poses.Add(pose);
                return pose;
            }

            public override List<PartPose> GetEntries()
            {
                return Poses;
            }
        }

        public class PartPose : Entry
        {
            public string PartName { get; set; }
            private short PartIndex;

            public List<Bone> Bones { get; set; }

            public PartPose()
            {
                Bones = new List<Bone>();
            }

            internal PartPose(BinaryReaderEx br)
            {
                PartIndex = br.ReadInt16();
                short boneCount = br.ReadInt16();
                br.AssertInt32(0);
                br.AssertInt64(0x10); // Bones offset

                Bones = new List<Bone>(boneCount);
                for (int i = 0; i < boneCount; i++)
                    Bones.Add(new Bone(br));
            }

            internal override void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt16(PartIndex);
                bw.WriteInt16((short)Bones.Count);
                bw.WriteInt32(0);
                bw.WriteInt64(0x10);

                foreach (Bone bone in Bones)
                    bone.Write(bw);
            }

            internal void GetNames(Entries entries)
            {
                PartName = FindName(entries.Parts, PartIndex);
                foreach (Bone bone in Bones)
                    bone.GetNames(entries);
            }

            internal void GetIndices(Lookups lookups, Entries entries)
            {
                PartIndex = (short)FindIndex(lookups.Parts, PartName);
                foreach (Bone bone in Bones)
                    bone.GetIndices(lookups, entries);
            }

            public class Bone
            {
                public string Name { get; set; }
                private int NameIndex;

                public Vector3 Translation { get; set; }

                public Vector3 Rotation { get; set; }

                public Vector3 Scale { get; set; }

                public Bone(string name = "")
                {
                    Name = name;
                    Scale = Vector3.One;
                }

                internal Bone(BinaryReaderEx br)
                {
                    NameIndex = br.ReadInt32();
                    Translation = br.ReadVector3();
                    Rotation = br.ReadVector3();
                    Scale = br.ReadVector3();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(NameIndex);
                    bw.WriteVector3(Translation);
                    bw.WriteVector3(Rotation);
                    bw.WriteVector3(Scale);
                }

                internal void GetNames(Entries entries)
                {
                    Name = FindName(entries.BoneNames, NameIndex);
                }

                internal void GetIndices(Lookups lookups, Entries entries)
                {
                    if (!lookups.BoneNames.ContainsKey(Name))
                    {
                        lookups.BoneNames[Name] = entries.BoneNames.Count;
                        entries.BoneNames.Add(new BoneName(Name));
                    }
                    NameIndex = FindIndex(lookups.BoneNames, Name);
                }
            }
        }
    }
}
