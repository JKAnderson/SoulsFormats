using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Point light sources in a map, used in BB, DS3, and Sekiro.
    /// </summary>
    public class BTL : SoulsFile<BTL>
    {
        /// <summary>
        /// Indicates the version, probably.
        /// </summary>
        public int Version { get; set; }

        public bool LongOffsets { get; set; }

        /// <summary>
        /// Light sources in this BTL.
        /// </summary>
        public List<Light> Lights { get; set; }

        /// <summary>
        /// Creates a BTL with Sekiro's version and no lights.
        /// </summary>
        public BTL()
        {
            Version = 16;
            LongOffsets = true;
            Lights = new List<Light>();
        }

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertInt32(2);
            Version = br.AssertInt32(1, 2, 5, 6, 16);
            int lightCount = br.ReadInt32();
            int namesLength = br.ReadInt32();
            br.AssertInt32(0);
            LongOffsets = br.AssertInt32(0xC0, 0xC8, 0xE8) != 0xC0; // Light size
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            long namesStart = br.Position;
            br.Skip(namesLength);
            Lights = new List<Light>(lightCount);
            for (int i = 0; i < lightCount; i++)
                Lights.Add(new Light(br, namesStart, Version, LongOffsets));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteInt32(2);
            bw.WriteInt32(Version);
            bw.WriteInt32(Lights.Count);
            bw.ReserveInt32("NamesLength");
            bw.WriteInt32(0);
            bw.WriteInt32(Version == 16 ? 0xE8 : (LongOffsets ? 0xC8 : 0xC0));
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            long namesStart = bw.Position;
            var nameOffsets = new List<long>(Lights.Count);
            foreach (Light entry in Lights)
            {
                long nameOffset = bw.Position - namesStart;
                nameOffsets.Add(nameOffset);
                bw.WriteUTF16(entry.Name, true);
                if (nameOffset % 0x10 != 0)
                    bw.WriteNull((int)(0x10 - (nameOffset % 0x10)), false);
            }

            bw.FillInt32("NamesLength", (int)(bw.Position - namesStart));
            for (int i = 0; i < Lights.Count; i++)
                Lights[i].Write(bw, nameOffsets[i], Version, LongOffsets);
        }

        /// <summary>
        /// An omnidirectional and/or spot light source.
        /// </summary>
        public class Light
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
            /// Name of this light.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk1C { get; set; }

            /// <summary>
            /// Color of the light on diffuse surfaces.
            /// </summary>
            public Color DiffuseColor { get; set; }

            /// <summary>
            /// Intensity of diffuse lighting.
            /// </summary>
            public float DiffusePower { get; set; }

            /// <summary>
            /// Color of the light on reflective surfaces.
            /// </summary>
            public Color SpecularColor { get; set; }

            /// <summary>
            /// Whether the light casts shadows.
            /// </summary>
            public bool CastShadows { get; set; }

            /// <summary>
            /// Intensity of specular lighting.
            /// </summary>
            public float SpecularPower { get; set; }

            /// <summary>
            /// Tightness of the spot light beam.
            /// </summary>
            public float ConeAngle { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk30 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk34 { get; set; }

            /// <summary>
            /// Center of the light.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of a spot light.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk50 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk54 { get; set; }

            /// <summary>
            /// Distance the light shines.
            /// </summary>
            public float Radius { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk5C { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] Unk64 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk68 { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] Unk6C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk70 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk74 { get; set; }

            /// <summary>
            /// Opacity of cast shadows.
            /// </summary>
            public float ShadowOpacity { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk7C { get; set; }
            
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk80 { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] Unk84 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk88 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk90 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk98 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk9C { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] UnkA0 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float UnkA4 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float UnkAC { get; set; }

            /// <summary>
            /// Stretches the spot light beam.
            /// </summary>
            public float Width { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float UnkBC { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] UnkC0 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float UnkC4 { get; set; }

            /// <summary>
            /// Unknown; only in Sekiro.
            /// </summary>
            public float UnkC8 { get; set; }

            /// <summary>
            /// Unknown; only in Sekiro.
            /// </summary>
            public float UnkCC { get; set; }

            /// <summary>
            /// Unknown; only in Sekiro.
            /// </summary>
            public float UnkD0 { get; set; }

            /// <summary>
            /// Unknown; only in Sekiro.
            /// </summary>
            public float UnkD4 { get; set; }

            /// <summary>
            /// Unknown; only in Sekiro.
            /// </summary>
            public float UnkD8 { get; set; }

            /// <summary>
            /// Unknown; only in Sekiro.
            /// </summary>
            public int UnkDC { get; set; }

            /// <summary>
            /// Unknown; only in Sekiro.
            /// </summary>
            public float UnkE0 { get; set; }

            /// <summary>
            /// Creates a Light with default values.
            /// </summary>
            public Light()
            {
                Name = "";
                DiffuseColor = Color.White;
                SpecularColor = Color.White;
                Unk64 = new byte[4];
                Unk6C = new byte[4];
                Unk84 = new byte[4];
                UnkA0 = new byte[4];
                UnkC0 = new byte[4];
            }

            internal Light(BinaryReaderEx br, long namesStart, int version, bool longOffsets)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                long nameOffset;
                if (longOffsets)
                    nameOffset = br.ReadInt64();
                else
                    nameOffset = br.ReadInt32();
                Name = br.GetUTF16(namesStart + nameOffset);

                Unk18 = br.ReadInt32();
                Unk1C = br.ReadBoolean();
                byte r = br.ReadByte();
                byte g = br.ReadByte();
                byte b = br.ReadByte();
                DiffuseColor = Color.FromArgb(255, r, g, b);
                DiffusePower = br.ReadSingle();
                r = br.ReadByte();
                g = br.ReadByte();
                b = br.ReadByte();
                CastShadows = br.ReadBoolean();
                SpecularColor = Color.FromArgb(255, r, g, b);
                SpecularPower = br.ReadSingle();
                ConeAngle = br.ReadSingle();
                Unk30 = br.ReadSingle();
                Unk34 = br.ReadSingle();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk50 = br.ReadInt32();
                Unk54 = br.ReadSingle();
                Radius = br.ReadSingle();
                Unk5C = br.ReadInt32();
                br.AssertInt32(0);
                Unk64 = br.ReadBytes(4);
                Unk68 = br.ReadSingle();
                Unk6C = br.ReadBytes(4);
                Unk70 = br.ReadSingle();
                Unk74 = br.ReadSingle();
                ShadowOpacity = br.ReadSingle();
                Unk7C = br.ReadSingle();
                Unk80 = br.ReadInt32();
                Unk84 = br.ReadBytes(4);
                Unk88 = br.ReadSingle();
                br.AssertInt32(0);
                Unk90 = br.ReadSingle();
                br.AssertInt32(0);
                Unk98 = br.ReadSingle();
                Unk9C = br.ReadSingle();
                UnkA0 = br.ReadBytes(4);
                UnkA4 = br.ReadSingle();
                br.AssertInt32(0);
                UnkAC = br.ReadSingle();

                if (longOffsets)
                    br.AssertInt64(0);
                else
                    br.AssertInt32(0);

                Width = br.ReadSingle();
                UnkBC = br.ReadSingle();
                UnkC0 = br.ReadBytes(4);
                UnkC4 = br.ReadSingle();

                if (version >= 16)
                {
                    UnkC8 = br.ReadSingle();
                    UnkCC = br.ReadSingle();
                    UnkD0 = br.ReadSingle();
                    UnkD4 = br.ReadSingle();
                    UnkD8 = br.ReadSingle();
                    UnkDC = br.ReadInt32();
                    UnkE0 = br.ReadSingle();
                    br.AssertInt32(0);
                }
            }

            internal void Write(BinaryWriterEx bw, long nameOffset, int version, bool longOffsets)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                if (longOffsets)
                    bw.WriteInt64(nameOffset);
                else
                    bw.WriteInt32((int)nameOffset);

                bw.WriteInt32(Unk18);
                bw.WriteBoolean(Unk1C);
                bw.WriteByte(DiffuseColor.R);
                bw.WriteByte(DiffuseColor.G);
                bw.WriteByte(DiffuseColor.B);
                bw.WriteSingle(DiffusePower);
                bw.WriteByte(SpecularColor.R);
                bw.WriteByte(SpecularColor.G);
                bw.WriteByte(SpecularColor.B);
                bw.WriteBoolean(CastShadows);
                bw.WriteSingle(SpecularPower);
                bw.WriteSingle(ConeAngle);
                bw.WriteSingle(Unk30);
                bw.WriteSingle(Unk34);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk50);
                bw.WriteSingle(Unk54);
                bw.WriteSingle(Radius);
                bw.WriteInt32(Unk5C);
                bw.WriteInt32(0);
                bw.WriteBytes(Unk64);
                bw.WriteSingle(Unk68);
                bw.WriteBytes(Unk6C);
                bw.WriteSingle(Unk70);
                bw.WriteSingle(Unk74);
                bw.WriteSingle(ShadowOpacity);
                bw.WriteSingle(Unk7C);
                bw.WriteInt32(Unk80);
                bw.WriteBytes(Unk84);
                bw.WriteSingle(Unk88);
                bw.WriteInt32(0);
                bw.WriteSingle(Unk90);
                bw.WriteInt32(0);
                bw.WriteSingle(Unk98);
                bw.WriteSingle(Unk9C);
                bw.WriteBytes(UnkA0);
                bw.WriteSingle(UnkA4);
                bw.WriteInt32(0);
                bw.WriteSingle(UnkAC);

                if (longOffsets)
                    bw.WriteInt64(0);
                else
                    bw.WriteInt32(0);

                bw.WriteSingle(Width);
                bw.WriteSingle(UnkBC);
                bw.WriteBytes(UnkC0);
                bw.WriteSingle(UnkC4);

                if (version >= 16)
                {
                    bw.WriteSingle(UnkC8);
                    bw.WriteSingle(UnkCC);
                    bw.WriteSingle(UnkD0);
                    bw.WriteSingle(UnkD4);
                    bw.WriteSingle(UnkD8);
                    bw.WriteInt32(UnkDC);
                    bw.WriteSingle(UnkE0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Returns the name of the light.
            /// </summary>
            public override string ToString()
            {
                return Name;
            }
        }
    }
}
