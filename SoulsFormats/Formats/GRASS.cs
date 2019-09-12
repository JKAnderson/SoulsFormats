using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// Unknown model companion format used in Sekiro. Extension: .grass
    /// </summary>
    public class GRASS : SoulsFile<GRASS>
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Struct1> Struct1s { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Struct2> Struct2s { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Struct3> Struct3s { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Struct4> Struct4s { get; set; }

        /// <summary>
        /// Creates an empty GRASS.
        /// </summary>
        public GRASS()
        {
            Struct1s = new List<Struct1>();
            Struct2s = new List<Struct2>();
            Struct3s = new List<Struct3>();
            Struct4s = new List<Struct4>();
        }

        internal override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0x28)
                return false;

            int version = br.GetInt32(0);
            int headerSize = br.GetInt32(4);
            int size1 = br.GetInt32(8);
            int size2 = br.GetInt32(0x10);
            int size3 = br.GetInt32(0x18);
            int size4 = br.GetInt32(0x20);
            return version == 1 && headerSize == 0x28 && size1 == 0x14 && size2 == 0x24 && size3 == 0x18 && size4 == 0x18;
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertInt32(1);
            br.AssertInt32(0x28);
            br.AssertInt32(0x14);
            int count1 = br.ReadInt32();
            br.AssertInt32(0x24);
            int count2 = br.ReadInt32();
            br.AssertInt32(0x18);
            int count3 = br.ReadInt32();
            br.AssertInt32(0x18);
            int count4 = br.ReadInt32();

            Struct1s = new List<Struct1>(count1);
            for (int i = 0; i < count1; i++)
                Struct1s.Add(new Struct1(br));

            Struct2s = new List<Struct2>(count2);
            for (int i = 0; i < count2; i++)
                Struct2s.Add(new Struct2(br));

            Struct3s = new List<Struct3>(count3);
            for (int i = 0; i < count3; i++)
                Struct3s.Add(new Struct3(br));

            Struct4s = new List<Struct4>(count4);
            for (int i = 0; i < count4; i++)
                Struct4s.Add(new Struct4(br));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteInt32(1);
            bw.WriteInt32(0x28);
            bw.WriteInt32(0x14);
            bw.WriteInt32(Struct1s.Count);
            bw.WriteInt32(0x24);
            bw.WriteInt32(Struct2s.Count);
            bw.WriteInt32(0x18);
            bw.WriteInt32(Struct3s.Count);
            bw.WriteInt32(0x18);
            bw.WriteInt32(Struct4s.Count);

            foreach (Struct1 struct1 in Struct1s)
                struct1.Write(bw);

            foreach (Struct2 struct2 in Struct2s)
                struct2.Write(bw);

            foreach (Struct3 struct3 in Struct3s)
                struct3.Write(bw);

            foreach (Struct4 struct4 in Struct4s)
                struct4.Write(bw);
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Struct1
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Creates a Struct1 with default values.
            /// </summary>
            public Struct1() { }

            internal Struct1(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Struct2
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk00 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk14 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk18 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk1C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk20 { get; set; }

            /// <summary>
            /// Creates a Struct2 with default values.
            /// </summary>
            public Struct2() { }

            internal Struct2(BinaryReaderEx br)
            {
                Unk00 = br.ReadSingle();
                Unk04 = br.ReadSingle();
                Unk08 = br.ReadSingle();
                Unk0C = br.ReadSingle();
                Unk10 = br.ReadSingle();
                Unk14 = br.ReadSingle();
                Unk18 = br.ReadSingle();
                Unk1C = br.ReadSingle();
                Unk20 = br.ReadSingle();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteSingle(Unk00);
                bw.WriteSingle(Unk04);
                bw.WriteSingle(Unk08);
                bw.WriteSingle(Unk0C);
                bw.WriteSingle(Unk10);
                bw.WriteSingle(Unk14);
                bw.WriteSingle(Unk18);
                bw.WriteSingle(Unk1C);
                bw.WriteSingle(Unk20);
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Struct3
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk00 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Creates a Struct3 with default values.
            /// </summary>
            public Struct3() { }

            internal Struct3(BinaryReaderEx br)
            {
                Unk00 = br.ReadSingle();
                Unk04 = br.ReadSingle();
                Unk08 = br.ReadSingle();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteSingle(Unk00);
                bw.WriteSingle(Unk04);
                bw.WriteSingle(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Struct4
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk00 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk14 { get; set; }

            /// <summary>
            /// Creates a Struct4 with default values.
            /// </summary>
            public Struct4() { }

            internal Struct4(BinaryReaderEx br)
            {
                Unk00 = br.ReadSingle();
                Unk04 = br.ReadSingle();
                Unk08 = br.ReadSingle();
                Unk0C = br.ReadSingle();
                Unk10 = br.ReadSingle();
                Unk14 = br.ReadSingle();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteSingle(Unk00);
                bw.WriteSingle(Unk04);
                bw.WriteSingle(Unk08);
                bw.WriteSingle(Unk0C);
                bw.WriteSingle(Unk10);
                bw.WriteSingle(Unk14);
            }
        }
    }
}
