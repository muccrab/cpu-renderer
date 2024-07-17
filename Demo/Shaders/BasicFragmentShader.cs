using CPU_Doom.Interfaces;
using CPU_Doom.Shaders;
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
        public Vector4 o_color;

        [InputAttribute("normal")]
        public Vector3 i_normal;
        [InputAttribute("intensity")]
        public float i_intensity;

        public void Execute()
        {
        if (1 == 1) return;
        }

    }
}
