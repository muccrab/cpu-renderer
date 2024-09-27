using CPU_Doom.Interfaces;
using CPU_Doom.Shaders;
using CPU_Doom.Types;
using OpenTK.Mathematics;

namespace Demo.Shaders
{
    internal class BasicFragmentShader : IFragmentShader
    {
        public const bool DissableFloatConvertion = true;

        [UniformAttribute("cameraPos")]
        public static Vector4d u_cameraPos;
        [UniformAttribute("lightPos")]
        public static Vector4d u_lightPos;

        [UniformAttribute("u_Time")]
        public static float uTime;


        [OutputAttribute("color")]
        public int o_color;

        [InputAttribute("f_color")]
        public Vector4 inColor = Vector4.One;

        public void Execute(ShaderFunctions func)
        {
            /*
            byte[] lByteColor = new byte[4] {
                (byte)(255 * ((inColor.X + uTime) > 1 ? (inColor.X + uTime) - (inColor.X + uTime) / 1 : (inColor.X + uTime))),
                (byte)(255 * ((inColor.Y + uTime) > 1 ? (inColor.Y + uTime) - (inColor.Y + uTime) / 1 : (inColor.Y + uTime))),
                (byte)(255 * ((inColor.Z + uTime) > 1 ? (inColor.Z + uTime) - (inColor.Z + uTime) / 1 : (inColor.Z + uTime))),
                255 };
            */

            byte[] lByteColor = new byte[4] {
                (byte)(255 * inColor.X),
                (byte)(255 * inColor.Y),
                (byte)(255 * inColor.Z),
                255 };



            o_color = lByteColor.ToInt();
            //o_color = (((255 * 255 + (int)(255 * inColor.Z)) * 255 + (int)(255 * inColor.Y)) * 255 + (int)inColor.X);
            //o_color.Y = ;
           // o_color.Z = ;
            //o_color.W = 255;
            //o_color = inColor;

        }

    }
}
