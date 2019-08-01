namespace SoulsFormats
{
    public static partial class FLVER
    {
        /// <summary>
        /// A vertex color with ARGB components, typically from 0 to 1.
        /// Used instead of System.Drawing.Color because some FLVERs use float colors with negative or >1 values.
        /// </summary>
        public struct VertexColor
        {
            /// <summary>
            /// Alpha component of the color.
            /// </summary>
            public float A;

            /// <summary>
            /// Red component of the color.
            /// </summary>
            public float R;

            /// <summary>
            /// Green component of the color.
            /// </summary>
            public float G;

            /// <summary>
            /// Blue component of the color.
            /// </summary>
            public float B;

            /// <summary>
            /// Creates a VertexColor with the given ARGB values.
            /// </summary>
            public VertexColor(float a, float r, float g, float b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            /// <summary>
            /// Creates a VertexColor with the given ARGB values divided by 255.
            /// </summary>
            public VertexColor(byte a, byte r, byte g, byte b)
            {
                A = a / 255f;
                R = r / 255f;
                G = g / 255f;
                B = b / 255f;
            }
        }
    }
}
