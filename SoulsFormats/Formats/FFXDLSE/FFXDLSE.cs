using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SoulsFormats
{
    /// <summary>
    /// An SFX configuration format used in DeS and DS2; only DS2 is supported. Extension: .ffx
    /// </summary>
    public partial class FFXDLSE : SoulsFile<FFXDLSE>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public FXEffect Effect { get; set; }

        public FFXDLSE()
        {
            Effect = new FXEffect();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "DLsE";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("DLsE");
            br.AssertByte(1);
            br.AssertByte(3);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertByte(0);
            br.AssertInt32(1);
            short classNameCount = br.ReadInt16();

            var classNames = new List<string>(classNameCount);
            for (int i = 0; i < classNameCount; i++)
            {
                int length = br.ReadInt32();
                classNames.Add(br.ReadASCII(length));
            }

            Effect = new FXEffect(br, classNames);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            var classNames = new List<string>();
            Effect.AddClassNames(classNames);

            bw.BigEndian = false;
            bw.WriteASCII("DLsE");
            bw.WriteByte(1);
            bw.WriteByte(3);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteByte(0);
            bw.WriteInt32(1);
            bw.WriteInt16((short)classNames.Count);

            foreach (string className in classNames)
            {
                bw.WriteInt32(className.Length);
                bw.WriteASCII(className);
            }

            Effect.Write(bw, classNames);
        }

        public class DLVector : List<int>
        {
            public DLVector() : base() { }

            public DLVector(int capacity) : base(capacity) { }

            public DLVector(IEnumerable<int> collection) : base(collection) { }

            internal DLVector(BinaryReaderEx br, List<string> classNames)
            {
                br.AssertInt16((short)(classNames.IndexOf("DLVector") + 1));
                int count = br.ReadInt32();
                AddRange(br.ReadInt32s(count));
            }

            internal void AddClassNames(List<string> classNames)
            {
                if (!classNames.Contains("DLVector"))
                    classNames.Add("DLVector");
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt16((short)(classNames.IndexOf("DLVector") + 1));
                bw.WriteInt32(Count);
                bw.WriteInt32s(this);
            }
        }

        public abstract class FXSerializable
        {
            internal abstract string ClassName { get; }

            internal abstract int Version { get; }

            public FXSerializable() { }

            internal FXSerializable(BinaryReaderEx br, List<string> classNames)
            {
                long start = br.Position;
                br.AssertInt16((short)(classNames.IndexOf(ClassName) + 1));
                br.AssertInt32(Version);
                int length = br.ReadInt32();
                Deserialize(br, classNames);
                if (br.Position != start + length)
                    throw new InvalidDataException("Failed to read all object data (or read too much of it).");
            }

            protected internal abstract void Deserialize(BinaryReaderEx br, List<string> classNames);

            internal virtual void AddClassNames(List<string> classNames)
            {
                if (!classNames.Contains(ClassName))
                    classNames.Add(ClassName);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                long start = bw.Position;
                bw.WriteInt16((short)(classNames.IndexOf(ClassName) + 1));
                bw.WriteInt32(Version);
                bw.ReserveInt32($"{start:X}Length");
                Serialize(bw, classNames);
                bw.FillInt32($"{start:X}Length", (int)(bw.Position - start));
            }

            protected internal abstract void Serialize(BinaryWriterEx bw, List<string> classNames);
        }

        public class ParamList : FXSerializable
        {
            internal override string ClassName => "FXSerializableParamList";

            internal override int Version => 2;

            [XmlAttribute]
            public int Unk04 { get; set; }

            public List<Param> Params { get; set; }

            public ParamList()
            {
                Params = new List<Param>();
            }

            internal ParamList(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                int paramCount = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Params = new List<Param>(paramCount);
                for (int i = 0; i < paramCount; i++)
                    Params.Add(Param.Read(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (Param param in Params)
                    param.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(Params.Count);
                bw.WriteInt32(Unk04);
                foreach (Param param in Params)
                    param.Write(bw, classNames);
            }
        }

        public class FXEffect : FXSerializable
        {
            internal override string ClassName => "FXSerializableEffect";

            internal override int Version => 5;

            [XmlAttribute]
            public int ID { get; set; }

            public DLVector Vector { get; set; }

            public List<ParamList> ParamLists { get; set; }

            public StateMap StateMap { get; set; }

            public ResourceSet ResourceSet { get; set; }

            public FXEffect()
            {
                Vector = new DLVector();
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
                Vector = new DLVector(br, classNames);

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
                Vector.AddClassNames(classNames);

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
                Vector.Write(bw, classNames);

                foreach (ParamList paramList in ParamLists)
                    paramList.Write(bw, classNames);

                StateMap.Write(bw, classNames);
                ResourceSet.Write(bw, classNames);
                bw.WriteByte(0);
            }
        }

        public class StateMap : FXSerializable
        {
            internal override string ClassName => "FXSerializableStateMap";

            internal override int Version => 1;

            public List<State> States { get; set; }

            public StateMap()
            {
                States = new List<State>();
            }

            internal StateMap(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                int stateCount = br.ReadInt32();
                States = new List<State>(stateCount);
                for (int i = 0; i < stateCount; i++)
                    States.Add(new State(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (State state in States)
                    state.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(States.Count);
                foreach (State state in States)
                    state.Write(bw, classNames);
            }
        }

        public class State : FXSerializable
        {
            internal override string ClassName => "FXSerializableState";

            internal override int Version => 1;

            public List<Action> Actions { get; set; }

            public List<Trigger> Triggers { get; set; }

            public State()
            {
                Actions = new List<Action>();
                Triggers = new List<Trigger>();
            }

            internal State(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                int actionCount = br.ReadInt32();
                int triggerCount = br.ReadInt32();
                Actions = new List<Action>(actionCount);
                for (int i = 0; i < actionCount; i++)
                    Actions.Add(new Action(br, classNames));
                Triggers = new List<Trigger>(triggerCount);
                for (int i = 0; i < triggerCount; i++)
                    Triggers.Add(new Trigger(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (Action action in Actions)
                    action.AddClassNames(classNames);
                foreach (Trigger trigger in Triggers)
                    trigger.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(Actions.Count);
                bw.WriteInt32(Triggers.Count);
                foreach (Action action in Actions)
                    action.Write(bw, classNames);
                foreach (Trigger trigger in Triggers)
                    trigger.Write(bw, classNames);
            }
        }

        public class Action : FXSerializable
        {
            internal override string ClassName => "FXSerializableAction";

            internal override int Version => 1;

            [XmlAttribute]
            public int ID { get; set; }

            public ParamList ParamList { get; set; }

            public Action()
            {
                ParamList = new ParamList();
            }

            internal Action(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                ID = br.ReadInt32();
                ParamList = new ParamList(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                ParamList.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(ID);
                ParamList.Write(bw, classNames);
            }
        }

        public class Trigger : FXSerializable
        {
            internal override string ClassName => "FXSerializableTrigger";

            internal override int Version => 1;

            [XmlAttribute]
            public int StateIndex { get; set; }

            public Evaluatable Evaluator { get; set; }

            public Trigger() { }

            internal Trigger(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                StateIndex = br.ReadInt32();
                Evaluator = Evaluatable.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Evaluator.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(StateIndex);
                Evaluator.Write(bw, classNames);
            }
        }

        public class ResourceSet : FXSerializable
        {
            internal override string ClassName => "FXResourceSet";

            internal override int Version => 1;

            public DLVector Vector1 { get; set; }

            public DLVector Vector2 { get; set; }

            public DLVector Vector3 { get; set; }

            public DLVector Vector4 { get; set; }

            public DLVector Vector5 { get; set; }

            public ResourceSet()
            {
                Vector1 = new DLVector();
                Vector2 = new DLVector();
                Vector3 = new DLVector();
                Vector4 = new DLVector();
                Vector5 = new DLVector();
            }

            internal ResourceSet(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Vector1 = new DLVector(br, classNames);
                Vector2 = new DLVector(br, classNames);
                Vector3 = new DLVector(br, classNames);
                Vector4 = new DLVector(br, classNames);
                Vector5 = new DLVector(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Vector1.AddClassNames(classNames);
                Vector2.AddClassNames(classNames);
                Vector3.AddClassNames(classNames);
                Vector4.AddClassNames(classNames);
                Vector5.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                Vector1.Write(bw, classNames);
                Vector2.Write(bw, classNames);
                Vector3.Write(bw, classNames);
                Vector4.Write(bw, classNames);
                Vector5.Write(bw, classNames);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
