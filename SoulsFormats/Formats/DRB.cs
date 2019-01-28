using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    /// <summary>
    /// A UI configuration format used in DS1.
    /// </summary>
    public class DRB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool DSR;

        public List<Texture> Textures;

        public byte[] AnipBytes;

        public byte[] IntpBytes;

        public List<Anim> Anims;

        public List<Scdl> Scdls;

        public List<Dlg> Dlgs;

        private const int ANIK_SIZE = 0x20;
        private const int ANIO_SIZE = 0x10;
        private const int SCDK_SIZE = 0x20;
        private const int SCDO_SIZE = 0x10;
        private const int DLGO_SIZE = 0x20;

        private static bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "DRB\0";
        }

        private void Read(BinaryReaderEx br, bool dsr)
        {
            br.BigEndian = false;
            DSR = dsr;

            ReadNullBlock(br, "DRB\0");
            Dictionary<int, string> strings = ReadSTR(br);
            Textures = ReadTEXI(br, strings);
            long shprStart = ReadBlobBlock(br, "SHPR");
            long ctprStart = ReadBlobBlock(br, "CTPR");
            AnipBytes = ReadBlobBytes(br, "ANIP");
            IntpBytes = ReadBlobBytes(br, "INTP");
            long scdpStart = ReadBlobBlock(br, "SCDP");
            Dictionary<int, Shape> shapes = ReadSHAP(br, dsr, strings, shprStart);
            Dictionary<int, Control> controls = ReadCTRL(br, strings, ctprStart);
            Dictionary<int, Anik> aniks = ReadANIK(br, strings);
            Dictionary<int, Anio> anios = ReadANIO(br, aniks);
            Anims = ReadANIM(br, strings, anios);
            Dictionary<int, Scdk> scdks = ReadSCDK(br, strings, scdpStart);
            Dictionary<int, Scdo> scdos = ReadSCDO(br, strings, scdks);
            Scdls = ReadSCDL(br, strings, scdos);
            Dictionary<int, Dlgo> dlgos = ReadDLGO(br, strings, shapes, controls);
            Dlgs = ReadDLG(br, strings, shapes, controls, dlgos);
        }

        private void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException();
        }

        private Dictionary<int, string> ReadSTR(BinaryReaderEx br)
        {
            long start = ReadBlockHeader(br, "STR\0", out int count);
            var strings = new Dictionary<int, string>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                strings[offset] = br.ReadUTF16();
            }
            br.Pad(0x10);
            return strings;
        }

        private List<Texture> ReadTEXI(BinaryReaderEx br, Dictionary<int, string> strings)
        {
            ReadBlockHeader(br, "TEXI", out int count);
            var textures = new List<Texture>(count);
            for (int i = 0; i < count; i++)
            {
                textures.Add(new Texture(br, strings));
            }
            br.Pad(0x10);
            return textures;
        }

        public class Texture
        {
            public string Name;

            public string Path;

            internal Texture(BinaryReaderEx br, Dictionary<int, string> strings)
            {
                int nameOffset = br.ReadInt32();
                int pathOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = strings[nameOffset];
                Path = strings[pathOffset];
            }

            public override string ToString()
            {
                return $"{Name} - {Path}";
            }
        }

        private Dictionary<int, Shape> ReadSHAP(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings, long shprStart)
        {
            long start = ReadBlockHeader(br, "SHAP", out int count);
            var shapes = new Dictionary<int, Shape>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                shapes[offset] = Shape.Read(br, dsr, strings, shprStart);
            }
            br.Pad(0x10);
            return shapes;
        }

        public enum ShapeType
        {
            Dialog,
            GouraudRect,
            GouraudSprite,
            Mask,
            MonoFrame,
            MonoRect,
            Null,
            ScrollText,
            Sprite,
            Text
        }

        public abstract class Shape
        {
            public abstract ShapeType Type { get; }

            public short LeftEdge, TopEdge, RightEdge, BottomEdge;

            public short Dsr1, Dsr2;

            public int Dsr3;

            internal static Shape Read(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings, long shprStart)
            {
                int typeOffset = br.ReadInt32();
                int shprOffset = br.ReadInt32();
                string type = strings[typeOffset];

                Shape result;
                br.StepIn(shprStart + shprOffset);
                {
                    if (type == "Dialog")
                        result = new Dialog(br, dsr);
                    else if (type == "GouraudRect")
                        result = new GouraudRect(br, dsr);
                    else if (type == "GouraudSprite")
                        result = new GouraudSprite(br, dsr);
                    else if (type == "Mask")
                        result = new Mask(br, dsr);
                    else if (type == "MonoFrame")
                        result = new MonoFrame(br, dsr);
                    else if (type == "MonoRect")
                        result = new MonoRect(br, dsr);
                    else if (type == "Null")
                        result = new Null(br, dsr);
                    else if (type == "ScrollText")
                        result = new ScrollText(br, dsr, strings);
                    else if (type == "Sprite")
                        result = new Sprite(br, dsr);
                    else if (type == "Text")
                        result = new Text(br, dsr, strings);
                    else
                        throw new InvalidDataException($"Unknown shape type: {type}");
                }
                br.StepOut();
                return result;
            }

            internal Shape(BinaryReaderEx br, bool dsr)
            {
                LeftEdge = br.ReadInt16();
                TopEdge = br.ReadInt16();
                RightEdge = br.ReadInt16();
                BottomEdge = br.ReadInt16();

                if (dsr && Type != ShapeType.Null)
                {
                    Dsr1 = br.ReadInt16();
                    Dsr2 = br.ReadInt16();
                    Dsr3 = br.ReadInt32();
                }
                else
                {
                    Dsr1 = -1;
                    Dsr2 = -1;
                    Dsr3 = 0;
                }
            }

            public override string ToString()
            {
                return $"{Type} ({LeftEdge}, {TopEdge}) ({RightEdge}, {BottomEdge})";
            }

            public class Dialog : Shape
            {
                public override ShapeType Type => ShapeType.Dialog;

                public short DlgIndex;

                public Dlg Dlg;

                /// <summary>
                /// Unknown; always 0 or 1.
                /// </summary>
                public byte Unk02;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk03;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C;

                internal Dialog(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    DlgIndex = br.ReadInt16();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }
            }

            public class GouraudRect : Shape
            {
                public override ShapeType Type => ShapeType.GouraudRect;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public int Unk00;

                public Color TopLeftColor, TopRightColor, BottomRightColor, BottomLeftColor;

                internal GouraudRect(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    TopLeftColor = ReadABGR(br);
                    TopRightColor = ReadABGR(br);
                    BottomRightColor = ReadABGR(br);
                    BottomLeftColor = ReadABGR(br);
                }
            }

            public class GouraudSprite : Shape
            {
                public override ShapeType Type => ShapeType.GouraudSprite;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk00;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk04;

                /// <summary>
                /// Unknown; always -1.
                /// </summary>
                public short Unk08;

                /// <summary>
                /// Unknown; always 256.
                /// </summary>
                public short Unk0A;

                public Color TopLeftColor, TopRightColor, BottomRightColor, BottomLeftColor;

                internal GouraudSprite(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt16();
                    Unk0A = br.ReadInt16();
                    TopLeftColor = ReadABGR(br);
                    TopRightColor = ReadABGR(br);
                    BottomRightColor = ReadABGR(br);
                    BottomLeftColor = ReadABGR(br);
                }
            }

            public class Mask : Shape
            {
                public override ShapeType Type => ShapeType.Mask;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public int Unk04;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk08;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk0C, Unk0D, Unk0E;

                internal Mask(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadByte();
                }
            }

            public class MonoFrame : Shape
            {
                public override ShapeType Type => ShapeType.MonoFrame;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk00;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk01;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk02;

                /// <summary>
                /// Unknown; always 1-3.
                /// </summary>
                public byte Unk03;

                /// <summary>
                /// Unknown; always 0-7.
                /// </summary>
                public int Unk04;

                /// <summary>
                /// Unknown; possibly a color.
                /// </summary>
                public int Unk08;

                internal MonoFrame(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                }
            }

            public class MonoRect : Shape
            {
                public override ShapeType Type => ShapeType.MonoRect;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public int Unk00;

                public int PaletteColor;

                public Color CustomColor;

                internal MonoRect(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadABGR(br);
                }
            }

            public class Null : Shape
            {
                public override ShapeType Type => ShapeType.Null;

                internal Null(BinaryReaderEx br, bool dsr) : base(br, dsr) { }
            }

            public class ScrollText : Shape
            {
                public override ShapeType Type => ShapeType.ScrollText;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk00;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk01;

                public short LineSpacing;

                public int PaletteColor;

                public Color CustomColor;

                public short FontSize;

                public AlignFlags Alignment;

                public TxtType TextType;

                /// <summary>
                /// Unknown; always 0x1C.
                /// </summary>
                public int Unk10;

                public int CharLength;

                public string TextLiteral;

                public int TextID;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk1C;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk20;

                /// <summary>
                /// Unknown; always 15 or 100.
                /// </summary>
                public int Unk24;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk28;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public short Unk2C;

                internal ScrollText(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings) : base(br, dsr)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    LineSpacing = br.ReadInt16();
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadABGR(br);
                    FontSize = br.ReadInt16();
                    Alignment = (AlignFlags)br.ReadByte();
                    TextType = br.ReadEnum8<TxtType>();
                    Unk10 = br.ReadInt32();

                    // ScrollText never uses Literal in vanilla
                    if (TextType == TxtType.Literal)
                    {
                        int textOffset = br.ReadInt32();
                        TextLiteral = strings[textOffset];
                        CharLength = -1;
                        TextID = -1;
                    }
                    else if (TextType == TxtType.FMG)
                    {
                        CharLength = br.ReadInt32();
                        TextID = br.ReadInt32();
                        TextLiteral = null;
                    }
                    else if (TextType == TxtType.Dynamic)
                    {
                        CharLength = br.ReadInt32();
                        TextLiteral = null;
                        TextID = -1;
                    }

                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt16();
                }
            }

            [Flags]
            public enum SpriteFlags : ushort
            {
                None = 0,
                RotateCW = 0x10,
                Rotate180 = 0x20,
                FlipVertical = 0x40,
                FlipHorizontal = 0x80,
                Alpha = 0x100,
                Overlay = 0x200,
            }

            public class Sprite : Shape
            {
                public override ShapeType Type => ShapeType.Sprite;

                public short TexLeftEdge, TexTopEdge, TexRightEdge, TexBottomEdge;

                public short TextureIndex;

                public SpriteFlags Flags;

                /// <summary>
                /// Unknown; often 0. Palette color?
                /// </summary>
                public int Unk0C;

                public Color Color;

                internal Sprite(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    TexLeftEdge = br.ReadInt16();
                    TexTopEdge = br.ReadInt16();
                    TexRightEdge = br.ReadInt16();
                    TexBottomEdge = br.ReadInt16();
                    TextureIndex = br.ReadInt16();
                    Flags = (SpriteFlags)br.ReadUInt16();
                    Unk0C = br.ReadInt32();
                    Color = ReadABGR(br);
                }
            }

            [Flags]
            public enum AlignFlags : byte
            {
                TopLeft = 0,
                Right = 1,
                CenterHorizontal = 2,
                Bottom = 4,
                CenterVertical = 8
            }

            public enum TxtType : byte
            {
                Literal = 0,
                FMG = 1,
                Dynamic = 2
            }

            public class Text : Shape
            {
                public override ShapeType Type => ShapeType.Text;

                /// <summary>
                /// Unknown; always 0-2.
                /// </summary>
                public byte Unk00;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk01;

                public short LineSpacing;

                public int PaletteColor;

                public Color CustomColor;

                public short FontSize;

                public AlignFlags Alignment;

                public TxtType TextType;

                /// <summary>
                /// Unknown; always 0x1C.
                /// </summary>
                public int Unk10;

                public int CharLength;

                public string TextLiteral;

                public int TextID;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk1C;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public short Unk20;

                internal Text(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings) : base(br, dsr)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    LineSpacing = br.ReadInt16();
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadABGR(br);
                    FontSize = br.ReadInt16();
                    Alignment = (AlignFlags)br.ReadByte();
                    TextType = br.ReadEnum8<TxtType>();
                    Unk10 = br.ReadInt32();

                    if (TextType == TxtType.Literal)
                    {
                        int textOffset = br.ReadInt32();
                        TextLiteral = strings[textOffset];
                        CharLength = -1;
                        TextID = -1;
                    }
                    else if (TextType == TxtType.FMG)
                    {
                        CharLength = br.ReadInt32();
                        TextID = br.ReadInt32();
                        TextLiteral = null;
                    }
                    else if (TextType == TxtType.Dynamic)
                    {
                        CharLength = br.ReadInt32();
                        TextLiteral = null;
                        TextID = -1;
                    }

                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt16();
                }
            }
        }

        private Dictionary<int, Control> ReadCTRL(BinaryReaderEx br, Dictionary<int, string> strings, long ctprStart)
        {
            long start = ReadBlockHeader(br, "CTRL", out int count);
            var controls = new Dictionary<int, Control>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                controls[offset] = Control.Read(br, strings, ctprStart);
            }
            br.Pad(0x10);
            return controls;
        }

        public enum ControlType
        {
            DmeCtrlScrollText,
            FrpgMenuDlgObjContentsHelpItem,
            Static
        }

        public abstract class Control
        {
            public abstract ControlType Type { get; }

            internal static Control Read(BinaryReaderEx br, Dictionary<int, string> strings, long ctprStart)
            {
                int typeOffset = br.ReadInt32();
                int ctprOffset = br.ReadInt32();
                string type = strings[typeOffset];

                Control result;
                br.StepIn(ctprStart + ctprOffset);
                {
                    if (type == "DmeCtrlScrollText")
                        result = new ScrollTextDummy(br);
                    else if (type == "FrpgMenuDlgObjContentsHelpItem")
                        result = new HelpItem(br);
                    else if (type == "Static")
                        result = new Static(br);
                    else
                        throw new InvalidDataException($"Unknown control type: {type}");
                }
                br.StepOut();
                return result;
            }

            public override string ToString()
            {
                return $"{Type}";
            }

            public class ScrollTextDummy : Control
            {
                public override ControlType Type => ControlType.DmeCtrlScrollText;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk00;

                internal ScrollTextDummy(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }
            }

            public class HelpItem : Control
            {
                public override ControlType Type => ControlType.FrpgMenuDlgObjContentsHelpItem;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14;

                public int TextID;

                internal HelpItem(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    TextID = br.ReadInt32();
                }
            }

            public class Static : Control
            {
                public override ControlType Type => ControlType.Static;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk00;

                internal Static(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }
            }
        }

        private Dictionary<int, Anik> ReadANIK(BinaryReaderEx br, Dictionary<int, string> strings)
        {
            long start = ReadBlockHeader(br, "ANIK", out int count);
            var aniks = new Dictionary<int, Anik>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                aniks[offset] = new Anik(br, strings);
            }
            br.Pad(0x10);
            return aniks;
        }

        public class Anik
        {
            public string Name;

            public int Unk04;

            /// <summary>
            /// Unknown; always 0-1.
            /// </summary>
            public byte Unk08;

            /// <summary>
            /// Unknown; always 1-2.
            /// </summary>
            public byte Unk09;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public short Unk0A;

            public int IntpOffset;

            public int AnipOffset;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk14;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk18;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk1C;

            internal Anik(BinaryReaderEx br, Dictionary<int, string> strings)
            {
                int nameOffset = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadByte();
                Unk09 = br.ReadByte();
                Unk0A = br.ReadInt16();
                IntpOffset = br.ReadInt32();
                AnipOffset = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Name = strings[nameOffset];
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private Dictionary<int, Anio> ReadANIO(BinaryReaderEx br, Dictionary<int, Anik> aniks)
        {
            long start = ReadBlockHeader(br, "ANIO", out int count);
            var anios = new Dictionary<int, Anio>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                anios[offset] = new Anio(br, aniks);
            }
            br.Pad(0x10);
            return anios;
        }

        public class Anio
        {
            public int Unk00;

            public List<Anik> Aniks;

            public int Unk0C;

            internal Anio(BinaryReaderEx br, Dictionary<int, Anik> aniks)
            {
                Unk00 = br.ReadInt32();
                int anikCount = br.ReadInt32();
                int anikOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Aniks = new List<Anik>(anikCount);
                for (int i = 0; i < anikCount; i++)
                {
                    int offset = anikOffset + ANIK_SIZE * i;
                    Aniks.Add(aniks[offset]);
                    aniks.Remove(offset);
                }
            }
        }

        private List<Anim> ReadANIM(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Anio> anios)
        {
            ReadBlockHeader(br, "ANIM", out int count);
            var anims = new List<Anim>(count);
            for (int i = 0; i < count; i++)
            {
                anims.Add(new Anim(br, strings, anios));
            }
            br.Pad(0x10);
            return anims;
        }

        public class Anim
        {
            public string Name;

            public List<Anio> Anios;

            public int Unk0C;

            /// <summary>
            /// Unknown; always 4.
            /// </summary>
            public int Unk10;

            /// <summary>
            /// Unknown; always 4.
            /// </summary>
            public int Unk14;

            /// <summary>
            /// Unknown; always 4.
            /// </summary>
            public int Unk18;

            /// <summary>
            /// Unknown; always 1.
            /// </summary>
            public int Unk1C;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk20;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk24;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk28;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk2C;

            internal Anim(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Anio> anios)
            {
                int nameOffset = br.ReadInt32();
                int anioCount = br.ReadInt32();
                int anioOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Name = strings[nameOffset];
                Anios = new List<Anio>(anioCount);
                for (int i = 0; i < anioCount; i++)
                {
                    int offset = anioOffset + ANIO_SIZE * i;
                    Anios.Add(anios[offset]);
                    anios.Remove(offset);
                }
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private Dictionary<int, Scdk> ReadSCDK(BinaryReaderEx br, Dictionary<int, string> strings, long scdpStart)
        {
            long start = ReadBlockHeader(br, "SCDK", out int count);
            var scdks = new Dictionary<int, Scdk>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                scdks[offset] = new Scdk(br, strings, scdpStart);
            }
            br.Pad(0x10);
            return scdks;
        }

        public class Scdk
        {
            public string Name;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk04;

            /// <summary>
            /// Unknown; always 1.
            /// </summary>
            public int Unk08;

            /// <summary>
            /// Unknown; always 0-1.
            /// </summary>
            public int Unk0C;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk18;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk1C;

            public int AnimIndex;

            /// <summary>
            /// Unknown; always 0-1.
            /// </summary>
            public int Scdp04;

            internal Scdk(BinaryReaderEx br, Dictionary<int, string> strings, long scdpStart)
            {
                int nameOffset = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                int scdpOffset = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Name = strings[nameOffset];
                br.StepIn(scdpStart + scdpOffset);
                {
                    AnimIndex = br.ReadInt32();
                    Scdp04 = br.ReadInt32();
                }
                br.StepOut();
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private Dictionary<int, Scdo> ReadSCDO(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Scdk> scdks)
        {
            long start = ReadBlockHeader(br, "SCDO", out int count);
            var scdos = new Dictionary<int, Scdo>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                scdos[offset] = new Scdo(br, strings, scdks);
            }
            br.Pad(0x10);
            return scdos;
        }

        public class Scdo
        {
            public string Name;

            public List<Scdk> Scdks;

            public int Unk0C;

            internal Scdo(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Scdk> scdks)
            {
                int nameOffset = br.ReadInt32();
                int scdkCount = br.ReadInt32();
                int scdkOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Name = strings[nameOffset];
                Scdks = new List<Scdk>(scdkCount);
                for (int i = 0; i < scdkCount; i++)
                {
                    int offset = scdkOffset + SCDK_SIZE * i;
                    Scdks.Add(scdks[offset]);
                    scdks.Remove(offset);
                }
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private List<Scdl> ReadSCDL(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Scdo> scdos)
        {
            ReadBlockHeader(br, "SCDL", out int count);
            var scdls = new List<Scdl>(count);
            for (int i = 0; i < count; i++)
            {
                scdls.Add(new Scdl(br, strings, scdos));
            }
            br.Pad(0x10);
            return scdls;
        }

        public class Scdl
        {
            public string Name;

            public List<Scdo> Scdos;

            public int Unk0C;

            internal Scdl(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Scdo> scdos)
            {
                int nameOffset = br.ReadInt32();
                int scdoCount = br.ReadInt32();
                int scdoOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Name = strings[nameOffset];
                Scdos = new List<Scdo>(scdoCount);
                for (int i = 0; i < scdoCount; i++)
                {
                    int offset = scdoOffset + SCDO_SIZE * i;
                    Scdos.Add(scdos[offset]);
                    scdos.Remove(offset);
                }
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private Dictionary<int, Dlgo> ReadDLGO(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls)
        {
            long start = ReadBlockHeader(br, "DLGO", out int count);
            var dlgos = new Dictionary<int, Dlgo>(count);
            for (int i = 0; i < count; i++)
            {
                int offset = (int)(br.Position - start);
                dlgos[offset] = new Dlgo(br, strings, shapes, controls);
            }
            br.Pad(0x10);
            return dlgos;
        }

        public class Dlgo
        {
            public string Name;

            public Shape Shape;

            public Control Control;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk0C;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk10;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk14;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk18;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk1C;

            internal Dlgo(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls)
            {
                int nameOffset = br.ReadInt32();
                int shapOffset = br.ReadInt32();
                int ctrlOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Name = strings[nameOffset];
                Shape = shapes[shapOffset];
                shapes.Remove(shapOffset);
                Control = controls[ctrlOffset];
                controls.Remove(ctrlOffset);
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private List<Dlg> ReadDLG(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls, Dictionary<int, Dlgo> dlgos)
        {
            ReadBlockHeader(br, "DLG\0", out int count);
            var dlgs = new List<Dlg>(count);
            for (int i = 0; i < count; i++)
            {
                dlgs.Add(new Dlg(br, strings, shapes, controls, dlgos));
            }
            br.Pad(0x10);
            return dlgs;
        }

        public class Dlg : Dlgo
        {
            public List<Dlgo> Dlgos;

            public short LeftEdge, TopEdge, RightEdge, BottomEdge;

            public short[] Unk30 { get; private set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public short Unk3A;

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk3C;

            internal Dlg(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls, Dictionary<int, Dlgo> dlgos) : base(br, strings, shapes, controls)
            {
                int dlgoCount = br.ReadInt32();
                int dlgoOffset = br.ReadInt32();
                LeftEdge = br.ReadInt16();
                TopEdge = br.ReadInt16();
                RightEdge = br.ReadInt16();
                BottomEdge = br.ReadInt16();
                Unk30 = br.ReadInt16s(5);
                Unk3A = br.ReadInt16();
                Unk3C = br.ReadInt32();

                Dlgos = new List<Dlgo>(dlgoCount);
                for (int i = 0; i < dlgoCount; i++)
                {
                    int offset = dlgoOffset + DLGO_SIZE * i;
                    Dlgos.Add(dlgos[offset]);
                    dlgos.Remove(offset);
                }
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        private static void ReadNullBlock(BinaryReaderEx br, string name)
        {
            br.AssertASCII(name);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
        }

        private static long ReadBlobBlock(BinaryReaderEx br, string name)
        {
            br.AssertASCII(name);
            int size = br.ReadInt32();
            br.AssertInt32(1);
            br.AssertInt32(0);

            long start = br.Position;
            br.Skip(size);
            br.Pad(0x10);
            return start;
        }

        private static byte[] ReadBlobBytes(BinaryReaderEx br, string name)
        {
            br.AssertASCII(name);
            int size = br.ReadInt32();
            br.AssertInt32(1);
            br.AssertInt32(0);

            byte[] bytes = br.ReadBytes(size);
            br.Pad(0x10);
            return bytes;
        }

        private static long ReadBlockHeader(BinaryReaderEx br, string name, out int count)
        {
            br.AssertASCII(name);
            int size = br.ReadInt32();
            count = br.ReadInt32();
            br.AssertInt32(0);
            return br.Position;
        }

        private static Color ReadABGR(BinaryReaderEx br)
        {
            byte[] abgr = br.ReadBytes(4);
            return Color.FromArgb(abgr[0], abgr[3], abgr[2], abgr[1]);
        }
        
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #region SoulsFile boilerplate
        /// <summary>
        /// The type of DCX compression to be used when writing.
        /// </summary>
        public DCX.Type Compression = DCX.Type.None;

        /// <summary>
        /// Returns true if the bytes appear to be a DRB.
        /// </summary>
        public static bool Is(byte[] bytes)
        {
            if (bytes.Length == 0)
                return false;

            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return Is(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a DRB.
        /// </summary>
        public static bool Is(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length == 0)
                    return false;

                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                return Is(SFUtil.GetDecompressedBR(br, out _));
            }
        }

        /// <summary>
        /// Loads a DRB from a byte array, automatically decompressing it if necessary.
        /// </summary>
        public static DRB Read(byte[] bytes, bool dsr)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            DRB drb = new DRB();
            br = SFUtil.GetDecompressedBR(br, out drb.Compression);
            drb.Read(br, dsr);
            return drb;
        }

        /// <summary>
        /// Loads a DRB from the specified path, automatically decompressing it if necessary.
        /// </summary>
        public static DRB Read(string path, bool dsr)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(false, stream);
                DRB drb = new DRB();
                br = SFUtil.GetDecompressedBR(br, out drb.Compression);
                drb.Read(br, dsr);
                return drb;
            }
        }

        /// <summary>
        /// Writes file data to a BinaryWriterEx, compressing it afterwards if specified.
        /// </summary>
        private void Write(BinaryWriterEx bw, DCX.Type compression)
        {
            if (compression == DCX.Type.None)
            {
                Write(bw);
            }
            else
            {
                BinaryWriterEx bwUncompressed = new BinaryWriterEx(false);
                Write(bwUncompressed);
                byte[] uncompressed = bwUncompressed.FinishBytes();
                DCX.Compress(uncompressed, bw, compression);
            }
        }

        /// <summary>
        /// Writes the file to an array of bytes, automatically compressing it if necessary.
        /// </summary>
        public byte[] Write()
        {
            return Write(Compression);
        }

        /// <summary>
        /// Writes the file to an array of bytes, compressing it as specified.
        /// </summary>
        public byte[] Write(DCX.Type compression)
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw, compression);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Writes the file to the specified path, automatically compressing it if necessary.
        /// </summary>
        public void Write(string path)
        {
            Write(path, Compression);
        }

        /// <summary>
        /// Writes the file to the specified path, compressing it as specified.
        /// </summary>
        public void Write(string path, DCX.Type compression)
        {
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, stream);
                Write(bw, compression);
                bw.Finish();
            }
        }
        #endregion
    }
}
