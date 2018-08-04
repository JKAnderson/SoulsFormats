using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// An interface configuration file used in DS1, DSR, DeS, and NB. Very poorly supported at the moment.
    /// </summary>
    public class DRB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Dictionary<int, string> strings;
        public List<TEXIEntry> textures;
        public Dictionary<int, SHAPEntry> shapEntries;
        public Dictionary<int, CTRLEntry> ctrlEntries;
        public Dictionary<int, ANIKEntry> anikEntries;
        public Dictionary<int, ANIOEntry> anioEntries;
        public Dictionary<int, ANIMEntry> animEntries;
        public Dictionary<int, SCDKEntry> scdkEntries;
        public Dictionary<int, SCDOEntry> scdoEntries;
        public Dictionary<int, SCDLEntry> scdlEntries;
        public Dictionary<int, DLGOEntry> dlgoEntries;
        public Dictionary<int, DLGEntry> dlgEntries;

        public static DRB Read(byte[] bytes, bool dsr)
        {
            return new DRB(bytes, dsr);
        }

        private DRB(byte[] bytes, bool dsr)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            br.AssertASCII("DRB\0");
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            strings = ReadSTR(br);
            textures = ReadTEXI(br, strings);
            int shprOffset = ReadSHPR(br);
            int ctprOffset = ReadCTPR(br);
            int anipOffset = ReadANIP(br);
            int intpOffset = ReadINTP(br);
            int scdpOffset = ReadSCDP(br);
            shapEntries = ReadSHAP(br, dsr, strings, shprOffset);
            ctrlEntries = ReadCTRL(br, strings, ctprOffset);
            anikEntries = ReadANIK(br, strings, intpOffset, anipOffset);
            anioEntries = ReadANIO(br, anikEntries);
            animEntries = ReadANIM(br, strings, anioEntries);
            scdkEntries = ReadSCDK(br, strings, scdpOffset);
            scdoEntries = ReadSCDO(br, strings, scdkEntries);
            scdlEntries = ReadSCDL(br, strings, scdoEntries);
            dlgoEntries = ReadDLGO(br, strings, shapEntries, ctrlEntries);
            dlgEntries = ReadDLG(br, strings, shapEntries, ctrlEntries, dlgoEntries);

            br.AssertASCII("END\0");
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
        }

        private static void ReadSectionHeader(BinaryReaderEx br, string name, out int entrySize, out int entryCount)
        {
            br.AssertASCII(name);
            entrySize = br.ReadInt32();
            entryCount = br.ReadInt32();
            br.AssertInt32(0);
        }

        private static void ReadSectionHeaderSingle(BinaryReaderEx br, string name, out int entrySize)
        {
            br.AssertASCII(name);
            entrySize = br.ReadInt32();
            br.AssertInt32(1);
            br.AssertInt32(0);
        }

        private static Dictionary<int, string> ReadSTR(BinaryReaderEx br)
        {
            ReadSectionHeader(br, "STR\0", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, string> strings = new Dictionary<int, string>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                strings[offset] = br.ReadUTF16();
            }

            br.Pad(0x10);
            return strings;
        }

        private static List<TEXIEntry> ReadTEXI(BinaryReaderEx br, Dictionary<int, string> strings)
        {
            ReadSectionHeader(br, "TEXI", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            List<TEXIEntry> textures = new List<TEXIEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                textures.Add(new TEXIEntry(br, strings));
            }

            br.Pad(0x10);
            return textures;
        }

        public class TEXIEntry
        {
            public string Name, Path;

            public TEXIEntry(BinaryReaderEx br, Dictionary<int, string> strings)
            {
                int nameOffset = br.ReadInt32();
                int pathOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = strings[nameOffset];
                Path = strings[pathOffset];
            }
        }

        private static int ReadSHPR(BinaryReaderEx br)
        {
            ReadSectionHeaderSingle(br, "SHPR", out int entrySize);
            int position = (int)br.Position;
            br.Skip(entrySize);
            br.Pad(0x10);
            return position;
        }

        private static int ReadCTPR(BinaryReaderEx br)
        {
            ReadSectionHeaderSingle(br, "CTPR", out int entrySize);
            int position = (int)br.Position;
            br.Skip(entrySize);
            br.Pad(0x10);
            return position;
        }

        private static int ReadANIP(BinaryReaderEx br)
        {
            ReadSectionHeaderSingle(br, "ANIP", out int entrySize);
            int position = (int)br.Position;
            br.Skip(entrySize);
            br.Pad(0x10);
            return position;
        }

        private static int ReadINTP(BinaryReaderEx br)
        {
            ReadSectionHeaderSingle(br, "INTP", out int entrySize);
            int position = (int)br.Position;
            br.Skip(entrySize);
            br.Pad(0x10);
            return position;
        }

        private static int ReadSCDP(BinaryReaderEx br)
        {
            ReadSectionHeaderSingle(br, "SCDP", out int entrySize);
            int position = (int)br.Position;
            br.Skip(entrySize);
            br.Pad(0x10);
            return position;
        }

        private static Dictionary<int, SHAPEntry> ReadSHAP(BinaryReaderEx br, bool dsr, Dictionary<int, string> strings, int shprOffset)
        {
            ReadSectionHeader(br, "SHAP", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, SHAPEntry> shapEntries = new Dictionary<int, SHAPEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                int typeOffset = br.ReadInt32();
                int shprEntryOffset = br.ReadInt32();
                int restorePosition = (int)br.Position;

                SHAPEntry entry;
                string type = strings[typeOffset];
                br.Position = shprOffset + shprEntryOffset;

                if (type == "Null")
                    entry = new ShapeNull(br);
                else if (type == "Dialog")
                    entry = new ShapeDialog(br);
                else if (type == "ScrollText")
                    entry = new ShapeScrollText(br);
                else if (type == "Text")
                    entry = new ShapeText(br);
                else if (type == "Sprite")
                    entry = new ShapeSprite(br, dsr);
                else if (type == "MonoRect")
                    entry = new ShapeMonoRect(br);
                else if (type == "GouraudRect")
                    entry = new ShapeGouraudRect(br);
                else if (type == "MonoFrame")
                    entry = new ShapeMonoFrame(br);
                else if (type == "GouraudSprite")
                    entry = new ShapeGouraudSprite(br);
                else if (type == "Mask")
                    entry = new ShapeMask(br);
                else
                    throw null;

                entry.Offset = shprOffset + shprEntryOffset;
                shapEntries[offset] = entry;
                br.Position = restorePosition;
            }

            br.Pad(0x10);
            return shapEntries;
        }

        public abstract class SHAPEntry
        {
            public string Type;
            public int Offset;
        }

        #region Shapes
        public class ShapeNull : SHAPEntry
        {
            public short LeftEdge, TopEdge, RightEdge, BottomEdge;

            public ShapeNull(BinaryReaderEx br)
            {
                Type = "Null";
                LeftEdge = br.ReadInt16(); // +00
                TopEdge = br.ReadInt16(); // +02
                RightEdge = br.ReadInt16(); // +04
                BottomEdge = br.ReadInt16(); // +06
            }
        }

        public class ShapeDialog : SHAPEntry
        {
            public byte[] Raw;
            public short Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Unk7, Unk8, Unk9;

            public ShapeDialog(BinaryReaderEx br)
            {
                Type = "Dialog";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16(); // +00
                Unk2 = br.ReadInt16(); // +02
                Unk3 = br.ReadInt16(); // +04
                Unk4 = br.ReadInt16(); // +06

                Unk5 = br.ReadInt16(); // +08 + 00
                Unk6 = br.ReadInt16(); // +08 + 02
                br.ReadInt16(); // +08 + 04
                br.ReadInt16(); // +08 + 06
                br.ReadByte(); // +08 + 12
                br.ReadByte(); // +08 + 13
                br.ReadByte(); // +08 + 1C
                br.ReadInt16(); // +08 + 20
                br.ReadInt16(); // +08 + 22
                br.ReadInt32(); // +08 + 24*/
            }
        }

        public class ShapeScrollText : SHAPEntry
        {
            public byte[] Raw;
            public short Unk1, Unk2, Unk3, Unk4;
            public int Unk5, Unk6, Unk7, Unk8, Unk9, Unk10, Unk11, Unk12, Unk13, Unk14, Unk15;
            public short Unk16;

            public ShapeScrollText(BinaryReaderEx br)
            {
                Type = "ScrollText";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16();
                Unk2 = br.ReadInt16();
                Unk3 = br.ReadInt16();
                Unk4 = br.ReadInt16();
                Unk5 = br.ReadInt32();
                Unk6 = br.ReadInt32();
                Unk7 = br.ReadInt32();
                Unk8 = br.ReadInt32();
                Unk9 = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk11 = br.ReadInt32();
                Unk12 = br.ReadInt32();
                Unk13 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk15 = br.ReadInt32();
                Unk16 = br.ReadInt16();*/
            }
        }

        public class ShapeText : SHAPEntry
        {
            public byte[] Raw;
            /*public short Unk1, Unk2, Unk3, Unk4;
            public int Unk5, Unk6, Unk7;
            public short Unk8, Unk9;
            public int Unk10, Unk11, Unk12;
            public short Unk13, Unk14, Unk15;*/

            public ShapeText(BinaryReaderEx br)
            {
                Type = "Text";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16();
                Unk2 = br.ReadInt16();
                Unk3 = br.ReadInt16();
                Unk4 = br.ReadInt16();
                Unk5 = br.ReadInt32();
                Unk6 = br.ReadInt32();
                Unk7 = br.ReadInt32();
                Unk8 = br.ReadInt16();
                Unk9 = br.ReadInt16();
                Unk10 = br.ReadInt32();
                Unk11 = br.ReadInt32();
                Unk12 = br.ReadInt32();
                Unk13 = br.ReadInt16();

                if (Unk12 != 0)
                {
                    Unk14 = br.ReadInt16();
                    Unk15 = br.ReadInt16();
                }*/
            }
        }

        public class ShapeSprite : SHAPEntry
        {
            public short Unk1, Unk2, Unk3, Unk4;
            public short Dsr1, Dsr2, Dsr3, Dsr4;
            public short LeftEdge, TopEdge, RightEdge, BottomEdge, TexiIndex;
            public byte OrientationFlags;
            public byte Unk11;
            public int Unk12;
            public byte Unk13, Unk14, Unk15, Unk16;

            public ShapeSprite(BinaryReaderEx br, bool dsr)
            {
                Type = "Sprite";
                Unk1 = br.ReadInt16(); // +00 ? Unused?
                Unk2 = br.ReadInt16(); // +02
                Unk3 = br.ReadInt16(); // +04
                Unk4 = br.ReadInt16(); // +06
                if (dsr)
                {
                    Dsr1 = br.ReadInt16(); // +08
                    Dsr2 = br.ReadInt16(); // +0A
                    Dsr3 = br.ReadInt16(); // +0C
                    Dsr4 = br.ReadInt16(); // +0E ? Unused?
                }
                LeftEdge = br.ReadInt16(); // +10 LeftEdge TopEdge
                TopEdge = br.ReadInt16();
                RightEdge = br.ReadInt16(); // +14 RightEdge BottomEdge
                BottomEdge = br.ReadInt16();
                TexiIndex = br.ReadInt16(); // +18 TexiIndex
                OrientationFlags = br.ReadByte();  // +1A OrientationFlags
                Unk11 = br.ReadByte();  // +1B
                Unk12 = br.ReadInt32(); // +1C
                Unk13 = br.ReadByte();  // +20
                Unk14 = br.ReadByte();  // +21
                Unk15 = br.ReadByte();  // +22
                Unk16 = br.ReadByte();  // +23
            }
        }

        public class ShapeMonoRect : SHAPEntry
        {
            public byte[] Raw;
            /*public short Unk1, Unk2, Unk3, Unk4;
            public int Unk5, Unk6;
            public short Unk7, Unk8;*/

            public ShapeMonoRect(BinaryReaderEx br)
            {
                Type = "MonoRect";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16();
                Unk2 = br.ReadInt16();
                Unk3 = br.ReadInt16();
                Unk4 = br.ReadInt16();
                Unk5 = br.ReadInt32();
                Unk6 = br.ReadInt32();
                Unk7 = br.ReadInt16();
                Unk8 = br.ReadInt16();*/
            }
        }

        public class ShapeGouraudRect : SHAPEntry
        {
            public byte[] Raw;
            /*public short Unk1, Unk2, Unk3, Unk4;
            public int Unk5, Unk6, Unk7, Unk8, Unk9;*/

            public ShapeGouraudRect(BinaryReaderEx br)
            {
                Type = "GouraudRect";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16();
                Unk2 = br.ReadInt16();
                Unk3 = br.ReadInt16();
                Unk4 = br.ReadInt16();
                Unk5 = br.ReadInt32();
                Unk6 = br.ReadInt32();
                Unk7 = br.ReadInt32();
                Unk8 = br.ReadInt32();
                Unk9 = br.ReadInt32();*/
            }
        }

        public class ShapeMonoFrame : SHAPEntry
        {
            public byte[] Raw;
            /*public short Unk1, Unk2, Unk3, Unk4, Unk5, Unk6;
            public int Unk7;
            public byte Unk8, Unk9, Unk10, Unk11;*/

            public ShapeMonoFrame(BinaryReaderEx br)
            {
                Type = "MonoFrame";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16();
                Unk2 = br.ReadInt16();
                Unk3 = br.ReadInt16();
                Unk4 = br.ReadInt16();
                Unk5 = br.ReadInt16();
                Unk6 = br.ReadInt16();
                Unk7 = br.ReadInt32();
                Unk8 = br.ReadByte();
                Unk9 = br.ReadByte();
                Unk10 = br.ReadByte();
                Unk11 = br.ReadByte();*/
            }
        }

        public class ShapeGouraudSprite : SHAPEntry
        {
            public byte[] Raw;
            /*public short Unk1, Unk2, Unk3, Unk4;
            public int Unk5, Unk6;
            public short Unk7, Unk8;
            public int Unk9;
            public short Unk10, Unk11, Unk12, Unk13;
            public int Unk14;*/

            public ShapeGouraudSprite(BinaryReaderEx br)
            {
                Type = "GouraudSprite";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16();
                Unk2 = br.ReadInt16();
                Unk3 = br.ReadInt16();
                Unk4 = br.ReadInt16();
                Unk5 = br.ReadInt32();
                Unk6 = br.ReadInt32();
                Unk7 = br.ReadInt16();
                Unk8 = br.ReadInt16();
                Unk9 = br.ReadInt32();
                Unk10 = br.ReadInt16();
                Unk11 = br.ReadInt16();
                Unk12 = br.ReadInt16();
                Unk13 = br.ReadInt16();
                Unk14 = br.ReadInt32();*/
            }
        }

        public class ShapeMask : SHAPEntry
        {
            public byte[] Raw;
            /*public short Unk1, Unk2, Unk3, Unk4;
            public byte Unk5, Unk6;
            public short Unk7;
            public int Unk8, Unk9;
            public short Unk10;
            public byte Unk11;*/

            public ShapeMask(BinaryReaderEx br)
            {
                Type = "Mask";
                Raw = br.ReadBytes(64);
                /*Unk1 = br.ReadInt16();
                Unk2 = br.ReadInt16();
                Unk3 = br.ReadInt16();
                Unk4 = br.ReadInt16();
                Unk5 = br.ReadByte();
                Unk6 = br.ReadByte();
                Unk7 = br.ReadInt16();
                Unk8 = br.ReadInt32();
                Unk9 = br.ReadInt32();
                Unk10 = br.ReadInt16();
                Unk11 = br.ReadByte();*/
            }
        }
        #endregion

        private static Dictionary<int, CTRLEntry> ReadCTRL(BinaryReaderEx br, Dictionary<int, string> strings, int ctprOffset)
        {
            ReadSectionHeader(br, "CTRL", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, CTRLEntry> ctrlEntries = new Dictionary<int, CTRLEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                int typeOffset = br.ReadInt32();
                int ctprEntryOffset = br.ReadInt32();
                int restorePosition = (int)br.Position;

                CTRLEntry entry;
                string type = strings[typeOffset];
                br.Position = ctprOffset + ctprEntryOffset;

                if (type == "Static")
                    entry = new ControlStatic(br);
                else if (type == "DmeCtrlScrollText")
                    entry = new ControlDmeCtrlScrollText(br);
                else if (type == "FrpgMenuDlgObjContentsHelpItem")
                    entry = new ControlFrpgMenuDlgObjContentsHelpItem(br);
                else
                    throw null;

                ctrlEntries[offset] = entry;
                br.Position = restorePosition;
            }

            br.Pad(0x10);
            return ctrlEntries;
        }

        public abstract class CTRLEntry
        {
            public string Type;
        }

        #region Controls
        public class ControlStatic : CTRLEntry
        {
            public ControlStatic(BinaryReaderEx br)
            {
                Type = "Static";
                br.AssertInt32(0);
            }
        }

        public class ControlDmeCtrlScrollText : CTRLEntry
        {
            public ControlDmeCtrlScrollText(BinaryReaderEx br)
            {
                Type = "DmeCtrlScrollText";
                br.AssertInt32(0);
            }
        }

        public class ControlFrpgMenuDlgObjContentsHelpItem : CTRLEntry
        {
            public int Unk1, Unk2, Unk3, Unk4, Unk5, Unk6;
            public byte Unk7, Unk8;
            public short Unk9;

            public ControlFrpgMenuDlgObjContentsHelpItem(BinaryReaderEx br)
            {
                Type = "FrpgMenuDlgObjContentsHelpItem";
                Unk1 = br.ReadInt32();
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadInt32();
                Unk4 = br.ReadInt32();
                Unk5 = br.ReadInt32();
                Unk6 = br.ReadInt32();
                Unk7 = br.ReadByte();
                Unk8 = br.ReadByte();
                Unk9 = br.ReadInt16();
            }
        }
        #endregion

        private static Dictionary<int, ANIKEntry> ReadANIK(BinaryReaderEx br, Dictionary<int, string> strings, int intpOffset, int anipOffset)
        {
            ReadSectionHeader(br, "ANIK", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, ANIKEntry> anikEntries = new Dictionary<int, ANIKEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                anikEntries[offset] = new ANIKEntry(br, strings, intpOffset, anipOffset);
            }

            br.Pad(0x10);
            return anikEntries;
        }

        public class ANIKEntry
        {
            public string Name;
            public int Unk2, Unk3;
            public int INTPOffset, ANIPOffset;

            public ANIKEntry(BinaryReaderEx br, Dictionary<int, string> strings, int intpOffset, int anipOffset)
            {
                int nameOffset = br.ReadInt32();
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadInt32();
                INTPOffset = br.ReadInt32();
                ANIPOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = strings[nameOffset];
            }
        }

        private static Dictionary<int, ANIOEntry> ReadANIO(BinaryReaderEx br, Dictionary<int, ANIKEntry> anikEntries)
        {
            ReadSectionHeader(br, "ANIO", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, ANIOEntry> anioEntries = new Dictionary<int, ANIOEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                anioEntries[offset] = new ANIOEntry(br, anikEntries);
            }

            br.Pad(0x10);
            return anioEntries;
        }

        public class ANIOEntry
        {
            public int Unk1, Unk2;
            public ANIKEntry ANIK;
            public int Unk4;

            public ANIOEntry(BinaryReaderEx br, Dictionary<int, ANIKEntry> anikEntries)
            {
                Unk1 = br.ReadInt32();
                Unk2 = br.ReadInt32();
                int anikOffset = br.ReadInt32();
                Unk4 = br.ReadInt32();

                // Last ANIOEntry may have offset pointing out of the ANIK section
                // FromSoft coding.jpg?
                if (anikEntries.ContainsKey(anikOffset))
                    ANIK = anikEntries[anikOffset];
                else
                    ANIK = null;
            }
        }

        private static Dictionary<int, ANIMEntry> ReadANIM(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, ANIOEntry> anioEntries)
        {
            ReadSectionHeader(br, "ANIM", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, ANIMEntry> animEntries = new Dictionary<int, ANIMEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                animEntries[offset] = new ANIMEntry(br, strings, anioEntries);
            }

            br.Pad(0x10);
            return animEntries;
        }

        public class ANIMEntry
        {
            public string Name;
            public ANIOEntry ANIO;
            public int Unk4;

            public ANIMEntry(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, ANIOEntry> anioEntries)
            {
                int nameOffset = br.ReadInt32();
                br.AssertInt32(4);
                int anioOffset = br.ReadInt32();
                Unk4 = br.ReadInt32();
                br.AssertInt32(4);
                br.AssertInt32(4);
                br.AssertInt32(4);
                br.AssertInt32(1);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = strings[nameOffset];
                ANIO = anioEntries[anioOffset];
            }
        }

        private static Dictionary<int, SCDKEntry> ReadSCDK(BinaryReaderEx br, Dictionary<int, string> strings, int scdpOffset)
        {
            ReadSectionHeader(br, "SCDK", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, SCDKEntry> scdkEntries = new Dictionary<int, SCDKEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                scdkEntries[offset] = new SCDKEntry(br, strings, scdpOffset);
            }

            br.Pad(0x10);
            return scdkEntries;
        }

        public class SCDKEntry
        {
            public string Name;
            public int Unk2, Unk4;
            public int SCDP;
            public int Unk6;

            public SCDKEntry(BinaryReaderEx br, Dictionary<int, string> strings, int scdpOffset)
            {
                int nameOffset = br.ReadInt32();
                Unk2 = br.ReadInt32();
                br.AssertInt32(1);
                Unk4 = br.ReadInt32();
                SCDP = br.ReadInt32();
                Unk6 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = strings[nameOffset];
            }
        }

        private static Dictionary<int, SCDOEntry> ReadSCDO(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, SCDKEntry> scdkEntries)
        {
            ReadSectionHeader(br, "SCDO", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, SCDOEntry> scdoEntries = new Dictionary<int, SCDOEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                scdoEntries[offset] = new SCDOEntry(br, strings, scdkEntries);
            }

            br.Pad(0x10);
            return scdoEntries;
        }

        public class SCDOEntry
        {
            public string Name;
            public int Unk2;
            public SCDKEntry SCDK;
            public int Unk4;

            public SCDOEntry(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, SCDKEntry> scdkEntries)
            {
                int nameOffset = br.ReadInt32();
                Unk2 = br.ReadInt32();
                int scdkOffset = br.ReadInt32();
                Unk4 = br.ReadInt32();

                Name = strings[nameOffset];
                SCDK = scdkEntries[scdkOffset];
            }
        }

        private static Dictionary<int, SCDLEntry> ReadSCDL(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, SCDOEntry> scdoEntries)
        {
            ReadSectionHeader(br, "SCDL", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, SCDLEntry> scdlEntries = new Dictionary<int, SCDLEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                scdlEntries[offset] = new SCDLEntry(br, strings, scdoEntries);
            }

            br.Pad(0x10);
            return scdlEntries;
        }

        public class SCDLEntry
        {
            public string Name;
            public int Unk2;
            public SCDOEntry SCDO;
            public int Unk4;

            public SCDLEntry(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, SCDOEntry> scdoEntries)
            {
                int nameOffset = br.ReadInt32();
                Unk2 = br.ReadInt32();
                int scdoOffset = br.ReadInt32();
                Unk4 = br.ReadInt32();

                Name = strings[nameOffset];
                SCDO = scdoEntries[scdoOffset];
            }
        }

        private static Dictionary<int, DLGOEntry> ReadDLGO(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, SHAPEntry> shapEntries, Dictionary<int, CTRLEntry> ctrlEntries)
        {
            ReadSectionHeader(br, "DLGO", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, DLGOEntry> dlgoEntries = new Dictionary<int, DLGOEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                dlgoEntries[offset] = new DLGOEntry(br, strings, shapEntries, ctrlEntries);
            }

            br.Pad(0x10);
            return dlgoEntries;
        }

        public class DLGOEntry
        {
            public string Name;
            public SHAPEntry Shape;
            public CTRLEntry Control;

            public DLGOEntry(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, SHAPEntry> shapEntries, Dictionary<int, CTRLEntry> ctrlEntries)
            {
                int nameOffset = br.ReadInt32();
                int shapOffset = br.ReadInt32();
                int ctrlOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = strings[nameOffset];
                Shape = shapEntries[shapOffset];
                Control = ctrlEntries[ctrlOffset];
            }
        }

        private static Dictionary<int, DLGEntry> ReadDLG(BinaryReaderEx br, Dictionary<int, string> strings,
            Dictionary<int, SHAPEntry> shapEntries, Dictionary<int, CTRLEntry> ctrlEntries, Dictionary<int, DLGOEntry> dlgoEntries)
        {
            ReadSectionHeader(br, "DLG\0", out int entrySize, out int entryCount);

            int startPosition = (int)br.Position;
            Dictionary<int, DLGEntry> dlgEntries = new Dictionary<int, DLGEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - startPosition;
                dlgEntries[offset] = new DLGEntry(br, strings, shapEntries, ctrlEntries, dlgoEntries);
            }

            br.Pad(0x10);
            return dlgEntries;
        }

        public class DLGEntry
        {
            public string Name;
            public SHAPEntry Shape;
            public CTRLEntry Control;
            public int Unk9;
            public DLGOEntry DLGO;
            public short LeftEdge, TopEdge, RightEdge, BottomEdge;
            public short Unk14;

            public DLGEntry(BinaryReaderEx br, Dictionary<int, string> strings,
                Dictionary<int, SHAPEntry> shapEntries, Dictionary<int, CTRLEntry> ctrlEntries, Dictionary<int, DLGOEntry> dlgoEntries)
            {
                int nameOffset = br.ReadInt32();
                int shapOffset = br.ReadInt32();
                int ctrlOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk9 = br.ReadInt32();
                int dlgoOffset = br.ReadInt32();
                LeftEdge = br.ReadInt16();
                TopEdge = br.ReadInt16();
                RightEdge = br.ReadInt16();
                BottomEdge = br.ReadInt16();
                Unk14 = br.ReadInt16();
                br.AssertInt16(-1);
                br.AssertInt32(-1);
                br.AssertInt16(-1);
                br.AssertInt16(0);
                br.AssertInt32(0);

                Name = strings[nameOffset];
                Shape = shapEntries[shapOffset];
                Control = ctrlEntries[ctrlOffset];
                DLGO = dlgoEntries[dlgoOffset];
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
