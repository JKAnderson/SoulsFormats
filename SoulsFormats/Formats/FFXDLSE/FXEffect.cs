using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class FXEffect : FXSerializable
        {
            internal override string ClassName => "FXSerializableEffect";

            internal override int Version => 5;

            [XmlAttribute]
            public int ID { get; set; }

            public List<int> Vector { get; set; }

            public List<ParamList> ParamLists { get; set; }

            public StateMap StateMap { get; set; }

            public ResourceSet ResourceSet { get; set; }

            public FXEffect()
            {
                Vector = new List<int>();
                ParamLists = new List<ParamList>();
                StateMap = new StateMap();
                ResourceSet = new ResourceSet();
            }

            internal FXEffect(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                br.AssertInt32(0);
                ID = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                int paramListCount = br.ReadInt32();
                br.AssertInt16(0);
                Vector = DLVector.Read(br, classNames);

                ParamLists = new List<ParamList>(paramListCount);
                for (int i = 0; i < paramListCount; i++)
                    ParamLists.Add(new ParamList(br, classNames));

                StateMap = new StateMap(br, classNames);
                ResourceSet = new ResourceSet(br, classNames);
                br.AssertByte(0);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                DLVector.AddClassNames(classNames);

                foreach (ParamList paramList in ParamLists)
                    paramList.AddClassNames(classNames);

                StateMap.AddClassNames(classNames);
                ResourceSet.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(ID);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(ParamLists.Count);
                bw.WriteInt16(0);
                DLVector.Write(bw, classNames, Vector);

                foreach (ParamList paramList in ParamLists)
                    paramList.Write(bw, classNames);

                StateMap.Write(bw, classNames);
                ResourceSet.Write(bw, classNames);
                bw.WriteByte(0);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
