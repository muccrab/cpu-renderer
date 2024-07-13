using OpenTK.Mathematics;

namespace CPU_Doom.Interfaces
{
    public interface IVertexShader
    {
        public Vector4 Position { get; }
        public void Execute();
    }

    public interface IFragmentShader {
        public const bool DissableFloatConvertion = false;
        public void Execute(); 
    }

    public interface IFragmentShader<T> : IFragmentShader where T : struct
    {
        public T Color { get; }
    }
}
