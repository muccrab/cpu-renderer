using CPU_Doom.Interfaces;
using OpenTK.Mathematics;

namespace Demo.Shaders
{
    internal class BasicFragmentShader : IFragmentShader<Vector4>
    {
        public const bool DissableFloatConvertion = true;
        public Vector4 Color => throw new NotImplementedException();

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
