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

        [OutputAttribute("color")]
        public int o_color;

        [InputAttribute("f_color")]
        public Vector4 inColor;

        public void Execute()
        {
            o_color = new byte[4] { 
                (byte)(255 * inColor.X),
                (byte)(255 * inColor.Y),
                (byte)(255 * inColor.Z), 
                255 }.ToInt();
            //o_color = (((255 * 255 + (int)(255 * inColor.Z)) * 255 + (int)(255 * inColor.Y)) * 255 + (int)inColor.X);
            //o_color.Y = ;
           // o_color.Z = ;
            //o_color.W = 255;
            //o_color = inColor;
        }

    }
}
