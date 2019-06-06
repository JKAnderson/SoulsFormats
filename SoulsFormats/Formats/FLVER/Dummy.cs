using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// "Dummy polygons" used for hit detection, particle effect locations, and much more.
        /// </summary>
        public class Dummy
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Vector indicating the dummy point's forward direction.
            /// </summary>
            public Vector3 Forward;

            /// <summary>
            /// Vector indicating the dummy point's upward direction.
            /// </summary>
            public Vector3 Upward;

            /// <summary>
            /// Indicates the type of dummy point this is (hitbox, sfx, etc).
            /// </summary>
            public short ReferenceID;

            /// <summary>
            /// Presumably the index of a bone the dummy points would be listed under in an editor. Not known to mean anything ingame.
            /// </summary>
            public short DummyBoneIndex;

            /// <summary>
            /// Index of the bone that the dummy point follows physically.
            /// </summary>
            public short AttachBoneIndex;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk0C, Unk0D;

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk0E;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Flag1, Flag2;

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk30, Unk34;

            /// <summary>
            /// Creates a new dummy point with default values.
            /// </summary>
            public Dummy()
            {
                Position = Vector3.Zero;
                Forward = Vector3.Zero;
                Upward = Vector3.Zero;
                ReferenceID = 0;
                DummyBoneIndex = -1;
                AttachBoneIndex = -1;
                Unk0C = 0;
                Unk0D = 0;
                Unk0E = 0;
                Flag1 = false;
                Flag2 = false;
                Unk30 = 0;
                Unk34 = 0;
            }

            internal Dummy(BinaryReaderEx br)
            {
                Position = br.ReadVector3();

                Unk0C = br.ReadByte();
                Unk0D = br.ReadByte();
                Unk0E = br.ReadInt16();

                Forward = br.ReadVector3();

                ReferenceID = br.ReadInt16();
                DummyBoneIndex = br.ReadInt16();

                Upward = br.ReadVector3();

                AttachBoneIndex = br.ReadInt16();
                Flag1 = br.ReadBoolean();
                Flag2 = br.ReadBoolean();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Position);

                bw.WriteByte(Unk0C);
                bw.WriteByte(Unk0D);
                bw.WriteInt16(Unk0E);

                bw.WriteVector3(Forward);

                bw.WriteInt16(ReferenceID);
                bw.WriteInt16(DummyBoneIndex);

                bw.WriteVector3(Upward);

                bw.WriteInt16(AttachBoneIndex);
                bw.WriteBoolean(Flag1);
                bw.WriteBoolean(Flag2);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns the dummy point's reference ID.
            /// </summary>
            public override string ToString()
            {
                return $"{ReferenceID}";
            }
        }
    }
}
