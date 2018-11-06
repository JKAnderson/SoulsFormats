using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Point light sources in a map, used in DS3 and BB.
    /// </summary>
    public class BTL : SoulsFile<BTL>
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk04;
        
        /// <summary>
        /// Light sources in this BTL.
        /// </summary>
        public List<Light> Lights;

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertInt32(2);
            Unk04 = br.AssertInt32(1, 2, 5, 6);
            int entryCount = br.ReadInt32();
            int nameSize = br.ReadInt32();
            br.AssertInt32(0);
            // Entry size
            br.AssertInt32(0xC8);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            long nameStart = br.Position;
            br.Position = nameStart + nameSize;
            Lights = new List<Light>(entryCount);
            for (int i = 0; i < entryCount; i++)
                Lights.Add(new Light(br, nameStart));
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteInt32(2);
            bw.WriteInt32(Unk04);
            bw.WriteInt32(Lights.Count);
            bw.ReserveInt32("NameSize");
            bw.WriteInt32(0);
            bw.WriteInt32(0xC8);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            long nameStart = bw.Position;
            var nameOffsets = new List<int>(Lights.Count);
            foreach (Light entry in Lights)
            {
                int nameOffset = (int)(bw.Position - nameStart);
                nameOffsets.Add(nameOffset);
                bw.WriteUTF16(entry.Name, true);
                if (nameOffset % 0x10 != 0)
                {
                    for (int i = 0; i < 0x10 - (nameOffset % 0x10); i++)
                        bw.WriteByte(0);
                }
            }

            bw.FillInt32("NameSize", (int)(bw.Position - nameStart));
            for (int i = 0; i < Lights.Count; i++)
                Lights[i].Write(bw, nameOffsets[i]);
        }

        /// <summary>
        /// An omnidirectional and/or spot light source.
        /// </summary>
        public class Light
        {
            /// <summary>
            /// Name of this light.
            /// </summary>
            public string Name;
            
            /// <summary>
            /// Center of the light.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Rotation of a spot light.
            /// </summary>
            public Vector3 Rotation;

            /// <summary>
            /// Color of the light on diffuse surfaces.
            /// </summary>
            public Color DiffuseColor;

            /// <summary>
            /// Intensity of diffuse lighting.
            /// </summary>
            public float DiffuseBrightness;

            /// <summary>
            /// Color of the light on reflective surfaces.
            /// </summary>
            public Color SpecularColor;

            /// <summary>
            /// Intensity of specular lighting.
            /// </summary>
            public float SpecularBrightness;

            /// <summary>
            /// Distance the light shines.
            /// </summary>
            public float Radius;

            /// <summary>
            /// Tightness of the spot light beam.
            /// </summary>
            public float ConeAngle;

            /// <summary>
            /// Stretches the spot light beam.
            /// </summary>
            public float Width;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk1C, Unk27;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk00, Unk04, Unk08, Unk18, Unk0C, Unk50, Unk5C, Unk64, UnkA0;

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk30, Unk34, Unk54, Unk68, Unk6C, Unk70, Unk74, Unk78, Unk7C, Unk98, Unk9C, UnkA4, UnkC4;

            internal Light(BinaryReaderEx br, long nameStart)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                int nameOffset = br.ReadInt32();
                Name = br.GetUTF16(nameStart + nameOffset);

                br.AssertInt32(0);
                Unk18 = br.ReadInt32();

                Unk1C = br.ReadBoolean();
                byte r = br.ReadByte();
                byte g = br.ReadByte();
                byte b = br.ReadByte();
                DiffuseColor = Color.FromArgb(255, r, g, b);
                DiffuseBrightness = br.ReadSingle();

                r = br.ReadByte();
                g = br.ReadByte();
                b = br.ReadByte();
                Unk27 = br.ReadBoolean();
                SpecularColor = Color.FromArgb(255, r, g, b);
                SpecularBrightness = br.ReadSingle();

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
                Unk64 = br.ReadInt32();
                Unk68 = br.ReadSingle();
                Unk6C = br.ReadSingle();
                Unk70 = br.ReadSingle();
                Unk74 = br.ReadSingle();
                Unk78 = br.ReadSingle();
                Unk7C = br.ReadSingle();
                br.AssertInt32(-1);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk98 = br.ReadSingle();
                Unk9C = br.ReadSingle();
                UnkA0 = br.ReadInt32();
                UnkA4 = br.ReadSingle();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Width = br.ReadSingle();
                br.AssertInt32(0);
                br.AssertInt32(0);
                UnkC4 = br.ReadSingle();
            }

            internal void Write(BinaryWriterEx bw, int nameOffset)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(nameOffset);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk18);

                bw.WriteBoolean(Unk1C);
                bw.WriteByte(DiffuseColor.R);
                bw.WriteByte(DiffuseColor.G);
                bw.WriteByte(DiffuseColor.B);
                bw.WriteSingle(DiffuseBrightness);

                bw.WriteByte(SpecularColor.R);
                bw.WriteByte(SpecularColor.G);
                bw.WriteByte(SpecularColor.B);
                bw.WriteBoolean(Unk27);
                bw.WriteSingle(SpecularBrightness);

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
                bw.WriteInt32(Unk64);
                bw.WriteSingle(Unk68);
                bw.WriteSingle(Unk6C);
                bw.WriteSingle(Unk70);
                bw.WriteSingle(Unk74);
                bw.WriteSingle(Unk78);
                bw.WriteSingle(Unk7C);
                bw.WriteInt32(-1);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteSingle(Unk98);
                bw.WriteSingle(Unk9C);
                bw.WriteInt32(UnkA0);
                bw.WriteSingle(Unk74);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteSingle(Width);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteSingle(UnkC4);
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
