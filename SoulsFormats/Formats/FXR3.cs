using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public class FXR3 : SoulsFile<FXR3>
    {
        public int ID;

        public Section1 Section1Tree;

        public Section4 Section4Tree;

        internal override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "FXR\0";
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("FXR\0");
            br.AssertInt32(0x40000);
            br.AssertInt32(1);
            ID = br.ReadInt32();

            int section1Offset = br.ReadInt32();
            int section1Count = br.AssertInt32(1);
            int section2Offset = br.ReadInt32();
            int section2Count = br.ReadInt32();
            int section3Offset = br.ReadInt32();
            int section3Count = br.ReadInt32();
            int section4Offset = br.ReadInt32();
            int section4Count = br.ReadInt32();
            int section5Offset = br.ReadInt32();
            int section5Count = br.ReadInt32();
            int section6Offset = br.ReadInt32();
            int section6Count = br.ReadInt32();
            int section7Offset = br.ReadInt32();
            int section7Count = br.ReadInt32();
            int section8Offset = br.ReadInt32();
            int section8Count = br.ReadInt32();
            int section9Offset = br.ReadInt32();
            int section9Count = br.ReadInt32();
            int section10Offset = br.ReadInt32();
            int section10Count = br.ReadInt32();
            int section11Offset = br.ReadInt32();
            int section11Count = br.ReadInt32();

            br.AssertInt32(1);
            br.AssertInt32(0);

            br.Position = section1Offset;
            Section1Tree = new Section1(br);

            br.Position = section2Offset;
            var section2s = new Dictionary<int, Section2>(section2Count);
            for (int i = 0; i < section2Count; i++)
                section2s[(int)br.Position] = new Section2(br);

            br.Position = section3Offset;
            var section3s = new Dictionary<int, Section3>(section3Count);
            for (int i = 0; i < section3Count; i++)
                section3s[(int)br.Position] = new Section3(br);

            br.Position = section4Offset;
            var section4s = new Dictionary<int, Section4>(section4Count);
            for (int i = 0; i < section4Count; i++)
                section4s[(int)br.Position] = new Section4(br);

            br.Position = section5Offset;
            var section5s = new Dictionary<int, Section5>(section5Count);
            for (int i = 0; i < section5Count; i++)
                section5s[(int)br.Position] = new Section5(br);

            br.Position = section6Offset;
            var section6s = new Dictionary<int, Section6>(section6Count);
            for (int i = 0; i < section6Count; i++)
                section6s[(int)br.Position] = new Section6(br);

            br.Position = section7Offset;
            var section7s = new Dictionary<int, Section7>(section7Count);
            for (int i = 0; i < section7Count; i++)
                section7s[(int)br.Position] = new Section7(br);

            br.Position = section8Offset;
            var section8s = new Dictionary<int, Section8>(section8Count);
            for (int i = 0; i < section8Count; i++)
                section8s[(int)br.Position] = new Section8(br);

            br.Position = section9Offset;
            var section9s = new Dictionary<int, Section9>(section9Count);
            for (int i = 0; i < section9Count; i++)
                section9s[(int)br.Position] = new Section9(br);

            br.Position = section10Offset;
            var section10s = new Dictionary<int, Section10>(section10Count);
            for (int i = 0; i < section10Count; i++)
                section10s[(int)br.Position] = new Section10(br);

            br.Position = section11Offset;
            var section11s = new Dictionary<int, int>(section11Count);
            for (int i = 0; i < section11Count; i++)
                section11s[(int)br.Position] = br.ReadInt32();

            var section2List = new List<Section2>(section2s.Values);
            var section3List = new List<Section3>(section3s.Values);
            var section4List = new List<Section4>(section4s.Values);
            var section5List = new List<Section5>(section5s.Values);
            var section6List = new List<Section6>(section6s.Values);
            var section7List = new List<Section7>(section7s.Values);
            var section8List = new List<Section8>(section8s.Values);
            var section9List = new List<Section9>(section9s.Values);
            var section10List = new List<Section10>(section10s.Values);

            Section1Tree.Take(section2s);

            foreach (Section2 section2 in section2List)
                section2.Take(section3s);

            foreach (Section3 section3 in section3List)
                section3.Take(section11s);

            foreach (Section4 section4 in section4List)
                section4.Take(section4s, section5s, section6s);

            foreach (Section5 section5 in section5List)
                section5.Take(section6s);

            foreach (Section6 section6 in section6List)
                section6.Take(section7s, section10s, section11s);

            foreach (Section7 section7 in section7List)
                section7.Take(section8s, section11s);

            foreach (Section8 section8 in section8List)
                section8.Take(section9s, section11s);

            foreach (Section9 section9 in section9List)
                section9.Take(section11s);

            foreach (Section10 section10 in section10List)
                section10.Take(section11s);
            
            if (section2s.Count != 0)
                throw null;

            if (section3s.Count != 0)
                throw null;

            if (section4s.Count != 1)
                throw null;

            if (section5s.Count != 0)
                throw null;

            if (section6s.Count != 0)
                throw null;

            if (section7s.Count != 0)
                throw null;

            if (section8s.Count != 0)
                throw null;

            if (section9s.Count != 0)
                throw null;

            if (section10s.Count != 0)
                throw null;

            if (section11s.Count != 0)
                throw null;

            Section4Tree = section4s.Values.First();
        }

        internal override void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        public class Section1
        {
            public List<Section2> Section2s;

            private int section2Offset, section2Count;

            internal Section1(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                section2Count = br.ReadInt32();
                section2Offset = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, Section2> section2s)
            {
                Section2s = new List<Section2>(section2Count);
                for (int i = 0; i < section2Count; i++)
                {
                    int offset = section2Offset + i * 0x10;
                    Section2s.Add(section2s[offset]);
                    section2s.Remove(offset);
                }
            }
        }

        public class Section2
        {
            public List<Section3> Section3s;

            private int section3Offset, section3Count;

            internal Section2(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                section3Count = br.ReadInt32();
                section3Offset = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, Section3> section3s)
            {
                Section3s = new List<Section3>(section3Count);
                for (int i = 0; i < section3Count; i++)
                {
                    int offset = section3Offset + i * 0x60;
                    Section3s.Add(section3s[offset]);
                    section3s.Remove(offset);
                }
            }
        }

        public class Section3
        {
            public int Unk00, Unk08, Unk10, Unk18, Unk38, Unk40;

            public int Section11Data1, Section11Data2;

            private int section11Offset1, section11Offset2;

            internal Section3(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                br.AssertInt32(0);
                Unk08 = br.ReadInt32();
                br.AssertInt32(0);
                Unk10 = br.ReadInt32();
                br.AssertInt32(0);
                Unk18 = br.ReadInt32();
                br.AssertInt32(0);
                section11Offset1 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk38 = br.ReadInt32();
                br.AssertInt32(0);
                Unk40 = br.ReadInt32();
                br.AssertInt32(0);
                section11Offset2 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, int> section11s)
            {
                Section11Data1 = section11s[section11Offset1];
                section11s.Remove(section11Offset1);
                Section11Data2 = section11s[section11Offset2];
                section11s.Remove(section11Offset2);
            }
        }

        public class Section4
        {
            public short Unk00;

            public byte Unk02, Unk03;

            public List<Section4> Section4s;

            public List<Section5> Section5s;

            public List<Section6> Section6s;

            private int section4Offset, section4Count;
            private int section5Offset, section5Count;
            private int section6Offset, section6Count;

            internal Section4(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                Unk02 = br.ReadByte();
                Unk03 = br.ReadByte();
                br.AssertInt32(0);
                section5Count = br.ReadInt32();
                section6Count = br.ReadInt32();
                section4Count = br.ReadInt32();
                br.AssertInt32(0);
                section5Offset = br.ReadInt32();
                br.AssertInt32(0);
                section6Offset = br.ReadInt32();
                br.AssertInt32(0);
                section4Offset = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, Section4> section4s, Dictionary<int, Section5> section5s, Dictionary<int, Section6> section6s)
            {
                Section4s = new List<Section4>(section4Count);
                for (int i = 0; i < section4Count; i++)
                {
                    int offset = section4Offset + i * 0x30;
                    Section4s.Add(section4s[offset]);
                    section4s.Remove(offset);
                }

                Section5s = new List<Section5>(section5Count);
                for (int i = 0; i < section5Count; i++)
                {
                    int offset = section5Offset + i * 0x20;
                    Section5s.Add(section5s[offset]);
                    section5s.Remove(offset);
                }

                Section6s = new List<Section6>(section6Count);
                for (int i = 0; i < section6Count; i++)
                {
                    int offset = section6Offset + i * 0x40;
                    Section6s.Add(section6s[offset]);
                    section6s.Remove(offset);
                }
            }
        }

        public class Section5
        {
            public short Unk00;

            public byte Unk02, Unk03;

            public List<Section6> Section6s;

            private int section6Offset, section6Count;

            internal Section5(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                Unk02 = br.ReadByte();
                Unk03 = br.ReadByte();
                br.AssertInt32(0);
                br.AssertInt32(0);
                section6Count = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                section6Offset = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, Section6> section6s)
            {
                Section6s = new List<Section6>(section6Count);
                for (int i = 0; i < section6Count; i++)
                {
                    int offset = section6Offset + i * 0x40;
                    Section6s.Add(section6s[offset]);
                    section6s.Remove(offset);
                }
            }
        }

        public class Section6
        {
            public short Unk00;

            public byte Unk02, Unk03;

            public int Unk04;

            public List<Section7> Section7s1, Section7s2;

            public List<Section10> Section10s;

            public List<int> Section11s1, Section11s2;

            private int section7Offset, section7Count1, section7Count2;
            private int section10Offset, section10Count;
            private int section11Offset, section11Count1, section11Count2;

            internal Section6(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                Unk02 = br.ReadByte();
                Unk03 = br.ReadByte();
                Unk04 = br.ReadInt32();
                section11Count1 = br.ReadInt32();
                section10Count = br.ReadInt32();
                section7Count1 = br.ReadInt32();
                section11Count2 = br.ReadInt32();
                br.AssertInt32(0);
                section7Count2 = br.ReadInt32();
                section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                section10Offset = br.ReadInt32();
                br.AssertInt32(0);
                section7Offset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, Section7> section7s, Dictionary<int, Section10> section10s, Dictionary<int, int> section11s)
            {
                Section7s1 = new List<Section7>(section7Count1);
                for (int i = 0; i < section7Count1; i++)
                {
                    int offset = section7Offset + i * 0x28;
                    Section7s1.Add(section7s[offset]);
                    section7s.Remove(offset);
                }

                Section7s2 = new List<Section7>(section7Count2);
                for (int i = 0; i < section7Count2; i++)
                {
                    int offset = section7Offset + section7Count1 * 0x28 + i * 0x28;
                    Section7s2.Add(section7s[offset]);
                    section7s.Remove(offset);
                }

                Section10s = new List<Section10>(section10Count);
                for (int i = 0; i < section10Count; i++)
                {
                    int offset = section10Offset + i * 0x10;
                    Section10s.Add(section10s[offset]);
                    section10s.Remove(offset);
                }

                Section11s1 = new List<int>(section11Count1);
                for (int i = 0; i < section11Count1; i++)
                {
                    int offset = section11Offset + i * 4;
                    Section11s1.Add(section11s[offset]);
                    section11s.Remove(offset);
                }

                Section11s2 = new List<int>(section11Count2);
                for (int i = 0; i < section11Count2; i++)
                {
                    int offset = section11Offset + section11Count1 * 4 + i * 4;
                    Section11s2.Add(section11s[offset]);
                    section11s.Remove(offset);
                }
            }
        }

        public class Section7
        {
            public int Unk00, Unk04;

            public List<Section8> Section8s;

            public List<int> Section11s;

            private int section8Offset, section8Count;
            private int section11Offset, section11Count;

            internal Section7(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                section11Count = br.ReadInt32();
                br.AssertInt32(0);
                section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                section8Offset = br.ReadInt32();
                br.AssertInt32(0);
                section8Count = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, Section8> section8s, Dictionary<int, int> section11s)
            {
                Section8s = new List<Section8>(section8Count);
                for (int i = 0; i < section8Count; i++)
                {
                    int offset = section8Offset + i * 0x20;
                    Section8s.Add(section8s[offset]);
                    section8s.Remove(offset);
                }

                Section11s = new List<int>(section11Count);
                for (int i = 0; i < section11Count; i++)
                {
                    int offset = section11Offset + i * 4;
                    Section11s.Add(section11s[offset]);
                    section11s.Remove(offset);
                }
            }
        }

        public class Section8
        {
            public byte Unk00, Unk01, Unk02, Unk03;

            public int Unk04;

            public List<Section9> Section9s;

            public List<int> Section11s;

            private int section9Offset, section9Count;
            private int section11Offset, section11Count;

            internal Section8(BinaryReaderEx br)
            {
                Unk00 = br.ReadByte();
                Unk01 = br.ReadByte();
                Unk02 = br.ReadByte();
                Unk03 = br.ReadByte();
                Unk04 = br.ReadInt32();
                section11Count = br.ReadInt32();
                section9Count = br.ReadInt32();
                section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                section9Offset = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, Section9> section9s, Dictionary<int, int> section11s)
            {
                Section9s = new List<Section9>(section9Count);
                for (int i = 0; i < section9Count; i++)
                {
                    int offset = section9Offset + i * 0x18;
                    Section9s.Add(section9s[offset]);
                    section9s.Remove(offset);
                }

                Section11s = new List<int>(section11Count);
                for (int i = 0; i < section11Count; i++)
                {
                    int offset = section11Offset + i * 4;
                    Section11s.Add(section11s[offset]);
                    section11s.Remove(offset);
                }
            }
        }

        public class Section9
        {
            public int Unk00, Unk04;

            public List<int> Section11s;

            private int section11Offset, section11Count;

            internal Section9(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                section11Count = br.ReadInt32();
                br.AssertInt32(0);
                section11Offset = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, int> section11s)
            {
                Section11s = new List<int>(section11Count);
                for (int i = 0; i < section11Count; i++)
                {
                    int offset = section11Offset + i * 4;
                    Section11s.Add(section11s[offset]);
                    section11s.Remove(offset);
                }
            }
        }

        public class Section10
        {
            public List<int> Section11s;

            private int section11Offset, section11Count;

            internal Section10(BinaryReaderEx br)
            {
                section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                section11Count = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Take(Dictionary<int, int> section11s)
            {
                Section11s = new List<int>(section11Count);
                for (int i = 0; i < section11Count; i++)
                {
                    int offset = section11Offset + i * 4;
                    Section11s.Add(section11s[offset]);
                    section11s.Remove(offset);
                }
            }
        }
    }
}
