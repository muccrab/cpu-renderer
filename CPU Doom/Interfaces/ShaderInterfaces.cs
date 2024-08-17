using CPU_Doom.Shaders;
using OpenTK.Mathematics;

namespace CPU_Doom.Interfaces
{
    public interface IShader { }

    public interface IVertexShader : IShader
    {
        public Vector4 Position { get; set; }
        public void Execute(ShaderFunctions func);
    }

    public interface IFragmentShader : IShader {
        public const bool DissableFloatConvertion = false;
        public void Execute(ShaderFunctions func);
    }
}
