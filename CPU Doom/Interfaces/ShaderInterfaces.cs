using OpenTK.Mathematics;

namespace CPU_Doom.Interfaces
{
    public interface IVertexShader
    {
        public Vector4 Position { get; }
        public void Execute();
    }

    public interface IFragmentShader<T> where T : struct
    {
        T Color { get; }
        public void Execute();
    }
}
