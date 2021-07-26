using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// Possible types of a UI element.
        /// </summary>
        public enum ShapeType
        {
            /// <summary>
            /// References another element group attached to this element.
            /// </summary>
            Dialog,

            /// <summary>
            /// A rectangle with color interpolated between each corner.
            /// </summary>
            GouraudRect,

            /// <summary>
            /// Presumably a sprite with interpolated coloring.
            /// </summary>
            GouraudSprite,

            /// <summary>
            /// Unknown.
            /// </summary>
            Mask,

            /// <summary>
            /// Unknown.
            /// </summary>
            MonoFrame,

            /// <summary>
            /// A rectangle with a solid color.
            /// </summary>
            MonoRect,

            /// <summary>
            /// An invisible element used to mark a position.
            /// </summary>
            Null,

            /// <summary>
            /// A scrolling text field.
            /// </summary>
            ScrollText,

            /// <summary>
            /// Displays a region of a texture.
            /// </summary>
            Sprite,

            /// <summary>
            /// A fixed text field.
            /// </summary>
            Text
        }

        /// <summary>
        /// Describes the appearance of a UI element.
        /// </summary>
        public abstract class Shape
        {
            /// <summary>
            /// The type of this element.
            /// </summary>
            public abstract ShapeType Type { get; }

            /// <summary>
            /// Left bound of this element, relative to 1280x720.
            /// </summary>
            public short LeftEdge { get; set; }

            /// <summary>
            /// Top bound of this element, relative to 1280x720.
            /// </summary>
            public short TopEdge { get; set; }

            /// <summary>
            /// Right bound of this element, relative to 1280x720.
            /// </summary>
            public short RightEdge { get; set; }

            /// <summary>
            /// Bottom bound of this element, relative to 1280x720.
            /// </summary>
            public short BottomEdge { get; set; }

            /// <summary>
            /// For DSR, the X coordinate which the element scales relative to.
            /// </summary>
            public short ScalingOriginX { get; set; }

            /// <summary>
            /// For DSR, the Y coordinate which the element scales relative to.
            /// </summary>
            public short ScalingOriginY { get; set; }

            /// <summary>
            /// For DSR, the behavior of scaling for this element.
            /// </summary>
            public int ScalingType { get; set; }

            internal Shape()
            {
                ScalingOriginX = -1;
                ScalingOriginY = -1;
            }

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
                    ScalingOriginX = br.ReadInt16();
                    ScalingOriginY = br.ReadInt16();
                    ScalingType = br.ReadInt32();
                }
                else
                {
                    ScalingOriginX = -1;
                    ScalingOriginY = -1;
                    ScalingType = 0;
                }
            }

            internal void WriteData(BinaryWriterEx bw, bool dsr, Dictionary<string, int> stringOffsets)
            {
                bw.WriteInt16(LeftEdge);
                bw.WriteInt16(TopEdge);
                bw.WriteInt16(RightEdge);
                bw.WriteInt16(BottomEdge);

                if (dsr && Type != ShapeType.Null)
                {
                    bw.WriteInt16(ScalingOriginX);
                    bw.WriteInt16(ScalingOriginY);
                    bw.WriteInt32(ScalingType);
                }

                WriteSpecific(bw, stringOffsets);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets);

            internal void WriteHeader(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> shprOffsets)
            {
                bw.WriteInt32(stringOffsets[Type.ToString()]);
                bw.WriteInt32(shprOffsets.Dequeue());
            }

            /// <summary>
            /// Returns the type and bounds of this Shape.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} ({LeftEdge}, {TopEdge}) ({RightEdge}, {BottomEdge})";
            }

            /// <summary>
            /// References another element group attached to this element.
            /// </summary>
            public class Dialog : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.Dialog;

                /// <summary>
                /// Dlg referenced by this element; must be found in the DRB's Dlg list.
                /// </summary>
                public Dlg Dlg { get; set; }
                internal short DlgIndex;

                /// <summary>
                /// Unknown; always 0 or 1.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk03 { get; set; }

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
                /// Creates a Dialog with default values.
                /// </summary>
                public Dialog()
                {
                    DlgIndex = -1;
                    Unk03 = 1;
                }

                internal Dialog(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    DlgIndex = br.ReadInt16();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt16(DlgIndex);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            /// <summary>
            /// A rectangle with color interpolated between each corner.
            /// </summary>
            public class GouraudRect : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.GouraudRect;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// The color of the top left corner of the rectangle.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color of the top right corner of the rectangle.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color of the bottom right corner of the rectangle.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color of the bottom left corner of the rectangle.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudRect with default values.
                /// </summary>
                public GouraudRect() : base()
                {
                    Unk00 = 1;
                }

                internal GouraudRect(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    TopLeftColor = br.ReadABGR();
                    TopRightColor = br.ReadABGR();
                    BottomRightColor = br.ReadABGR();
                    BottomLeftColor = br.ReadABGR();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteABGR(TopLeftColor);
                    bw.WriteABGR(TopRightColor);
                    bw.WriteABGR(BottomRightColor);
                    bw.WriteABGR(BottomLeftColor);
                }
            }

            /// <summary>
            /// Presumably a sprite with interpolated coloring.
            /// </summary>
            public class GouraudSprite : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.GouraudSprite;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown; always -1.
                /// </summary>
                public short Unk08 { get; set; }

                /// <summary>
                /// Unknown; always 256.
                /// </summary>
                public short Unk0A { get; set; }

                /// <summary>
                /// The color of the top left corner of the sprite.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color of the top right corner of the sprite.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color of the bottom right corner of the sprite.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color of the bottom left corner of the sprite.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudSprite with default values.
                /// </summary>
                public GouraudSprite()
                {
                    Unk08 = -1;
                    Unk0A = 256;
                }

                internal GouraudSprite(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt16();
                    Unk0A = br.ReadInt16();
                    TopLeftColor = br.ReadABGR();
                    TopRightColor = br.ReadABGR();
                    BottomRightColor = br.ReadABGR();
                    BottomLeftColor = br.ReadABGR();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt16(Unk08);
                    bw.WriteInt16(Unk0A);
                    bw.WriteABGR(TopLeftColor);
                    bw.WriteABGR(TopRightColor);
                    bw.WriteABGR(BottomRightColor);
                    bw.WriteABGR(BottomLeftColor);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Mask : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.Mask;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk0C { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk0D { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk0E { get; set; }

                /// <summary>
                /// Creates a Mask with default values.
                /// </summary>
                public Mask() : base()
                {
                    Unk04 = 1;
                }

                internal Mask(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadByte();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteByte(Unk0E);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MonoFrame : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.MonoFrame;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk00 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk01 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown; always 1-3.
                /// </summary>
                public byte Unk03 { get; set; }

                /// <summary>
                /// Unknown; always 0-7.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown; possibly a color.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Creates a MonoFrame with default values.
                /// </summary>
                public MonoFrame() : base()
                {
                    Unk00 = 1;
                    Unk03 = 1;
                }

                internal MonoFrame(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                }
            }

            /// <summary>
            /// A rectangle with a solid color.
            /// </summary>
            public class MonoRect : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.MonoRect;

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Chooses a color from a palette of 1-80, or 0 to use a custom color.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// When PaletteColor is 0, specifies the color of the rectangle.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a MonoRect with default values.
                /// </summary>
                public MonoRect() : base()
                {
                    Unk00 = 1;
                }

                internal MonoRect(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    Unk00 = br.ReadInt32();
                    PaletteColor = br.ReadInt32();
                    CustomColor = br.ReadABGR();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(PaletteColor);
                    bw.WriteABGR(CustomColor);
                }
            }

            /// <summary>
            /// An invisible element used to mark a position.
            /// </summary>
            public class Null : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.Null;

                /// <summary>
                /// Creates a Null with default values.
                /// </summary>
                public Null() : base() { }

                internal Null(BinaryReaderEx br, bool dsr) : base(br, dsr) { }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) { }
            }

            /// <summary>
            /// A scrolling text field.
            /// </summary>
            public class ScrollText : TextBase
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.ScrollText;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk20 { get; set; }

                /// <summary>
                /// Unknown; always 15 or 100.
                /// </summary>
                public int Unk24 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk28 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public short Unk2C { get; set; }

                /// <summary>
                /// Creates a ScrollText with default values.
                /// </summary>
                public ScrollText() : base()
                {
                    Unk24 = 15;
                }

                internal ScrollText(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings) : base(br, dsr, strings)
                {
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    base.WriteSpecific(bw, stringOffsets);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt16(Unk2C);
                }
            }

            /// <summary>
            /// Indicates the display properties of a Sprite.
            /// </summary>
            [Flags]
            public enum SpriteFlags : ushort
            {
                /// <summary>
                /// No modification.
                /// </summary>
                None = 0,

                /// <summary>
                /// Rotate texture 90 degrees clockwise.
                /// </summary>
                RotateCW = 0x10,

                /// <summary>
                /// Rotate texture 180 degrees.
                /// </summary>
                Rotate180 = 0x20,

                /// <summary>
                /// Flip texture vertically.
                /// </summary>
                FlipVertical = 0x40,

                /// <summary>
                /// Flip texture horizontally.
                /// </summary>
                FlipHorizontal = 0x80,

                /// <summary>
                /// Allow alpha transparency.
                /// </summary>
                Alpha = 0x100,

                /// <summary>
                /// Overlay the texture on the underlying visual.
                /// </summary>
                Overlay = 0x200,
            }

            /// <summary>
            /// Displays a region of a texture.
            /// </summary>
            public class Sprite : Shape
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.Sprite;

                /// <summary>
                /// Left bound of the texture region displayed by this element.
                /// </summary>
                public short TexLeftEdge { get; set; }

                /// <summary>
                /// Top bound of the texture region displayed by this element.
                /// </summary>
                public short TexTopEdge { get; set; }

                /// <summary>
                /// Right bound of the texture region displayed by this element.
                /// </summary>
                public short TexRightEdge { get; set; }

                /// <summary>
                /// Bottom bound of the texture region displayed by this element.
                /// </summary>
                public short TexBottomEdge { get; set; }

                /// <summary>
                /// The texture to display, indexing textures in menu.tpf.
                /// </summary>
                public short TextureIndex { get; set; }

                /// <summary>
                /// Flags modifying how the texture is displayed.
                /// </summary>
                public SpriteFlags Flags { get; set; }

                /// <summary>
                /// Tints the sprite from a palette of 1-80, or 0 to use custom color below.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Tints the sprite a certain color.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a Sprite with default values.
                /// </summary>
                public Sprite() : base()
                {
                    Flags = SpriteFlags.Alpha;
                    CustomColor = Color.White;
                }

                internal Sprite(BinaryReaderEx br, bool dsr) : base(br, dsr)
                {
                    TexLeftEdge = br.ReadInt16();
                    TexTopEdge = br.ReadInt16();
                    TexRightEdge = br.ReadInt16();
                    TexBottomEdge = br.ReadInt16();
                    TextureIndex = br.ReadInt16();
                    Flags = (SpriteFlags)br.ReadUInt16();
                    PaletteColor = br.ReadInt32();
                    CustomColor = br.ReadABGR();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt16(TexLeftEdge);
                    bw.WriteInt16(TexTopEdge);
                    bw.WriteInt16(TexRightEdge);
                    bw.WriteInt16(TexBottomEdge);
                    bw.WriteInt16(TextureIndex);
                    bw.WriteUInt16((ushort)Flags);
                    bw.WriteInt32(PaletteColor);
                    bw.WriteABGR(CustomColor);
                }
            }

            /// <summary>
            /// Indicates the positioning of text within its element.
            /// </summary>
            [Flags]
            public enum AlignFlags : byte
            {
                /// <summary>
                /// Anchor to top left.
                /// </summary>
                TopLeft = 0,

                /// <summary>
                /// Anchor to right side.
                /// </summary>
                Right = 1,

                /// <summary>
                /// Center horizontally.
                /// </summary>
                CenterHorizontal = 2,

                /// <summary>
                /// Anchor to bottom side.
                /// </summary>
                Bottom = 4,

                /// <summary>
                /// Center vertically.
                /// </summary>
                CenterVertical = 8
            }

            /// <summary>
            /// Indicates the source of a text element's text.
            /// </summary>
            public enum TxtType : byte
            {
                /// <summary>
                /// Text is a literal value stored in the DRB.
                /// </summary>
                Literal = 0,

                /// <summary>
                /// Text is a static FMG ID.
                /// </summary>
                FMG = 1,

                /// <summary>
                /// Text is assigned at runtime.
                /// </summary>
                Dynamic = 2
            }

            /// <summary>
            /// A fixed text field.
            /// </summary>
            public class Text : TextBase
            {
                /// <summary>
                /// The type of this element.
                /// </summary>
                public override ShapeType Type => ShapeType.Text;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public short Unk20 { get; set; }

                /// <summary>
                /// Creates a Text with default values.
                /// </summary>
                public Text() : base() { }

                internal Text(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings) : base(br, dsr, strings)
                {
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    base.WriteSpecific(bw, stringOffsets);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt16(Unk20);
                }
            }

            /// <summary>
            /// Either a fixed text or scrolling text element.
            /// </summary>
            public abstract class TextBase : Shape
            {
                /// <summary>
                /// Unknown; always 0-2.
                /// </summary>
                public byte Unk00 { get; set; }

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk01 { get; set; }

                /// <summary>
                /// Distance between each line of text.
                /// </summary>
                public short LineSpacing { get; set; }

                /// <summary>
                /// Chooses a color from a palette of 1-80, or 0 to use a custom color.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// When PaletteColor is 0, specifies the color of the text.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// From 0-11, different sizes of the menu font. 12 is the subtitle font.
                /// </summary>
                public short FontSize { get; set; }

                /// <summary>
                /// The horizontal and vertical alignment of the text.
                /// </summary>
                public AlignFlags Alignment { get; set; }

                /// <summary>
                /// Whether the element uses a text literal, a static FMG ID, or is assigned at runtime.
                /// </summary>
                public TxtType TextType { get; set; }

                /// <summary>
                /// Unknown; always 0x1C.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// The maximum characters to display.
                /// </summary>
                public int CharLength { get; set; }

                /// <summary>
                /// If TextType is Literal, the text to display, otherwise null.
                /// </summary>
                public string TextLiteral { get; set; }

                /// <summary>
                /// If TextType is FMG, the FMG ID to display, otherwise -1.
                /// </summary>
                public int TextID { get; set; }

                internal TextBase() : base()
                {
                    Unk01 = 1;
                    TextType = TxtType.FMG;
                    Unk10 = 0x1C;
                    TextID = -1;
                }

                internal TextBase(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings) : base(br, dsr)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    LineSpacing = br.ReadInt16();
                    PaletteColor = br.ReadInt32();
                    CustomColor = br.ReadABGR();
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
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteInt16(LineSpacing);
                    bw.WriteInt32(PaletteColor);
                    bw.WriteABGR(CustomColor);
                    bw.WriteInt16(FontSize);
                    bw.WriteByte((byte)Alignment);
                    bw.WriteByte((byte)TextType);
                    bw.WriteInt32(Unk10);

                    if (TextType == TxtType.Literal)
                    {
                        bw.WriteInt32(stringOffsets[TextLiteral]);
                    }
                    else if (TextType == TxtType.FMG)
                    {
                        bw.WriteInt32(CharLength);
                        bw.WriteInt32(TextID);
                    }
                    else if (TextType == TxtType.Dynamic)
                    {
                        bw.WriteInt32(CharLength);
                    }
                }
            }
        }
    }
}
