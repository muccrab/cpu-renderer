using CPU_Doom.Shaders;
using OpenTK.Mathematics;

namespace CPU_Doom.Interfaces
{
    /// <summary>
    /// Base interface for all shader types.
    /// </summary>
    public interface IShader {

        /// <summary>
        /// Executes the shader using the provided shader functions.
        /// </summary>
        /// <param name="func">A set of shader functions to be used during execution.</param>
        public void Execute(ShaderFunctions func);
    }

    /// <summary>
    /// Interface for vertex shaders, inheriting from IShader.
    /// </summary>
    public interface IVertexShader : IShader
    {
        /// <summary>
        /// The position of the vertex in 4D space.
        /// </summary>
        public Vector4 Position { get; set; }
    }

    /// <summary>
    /// Interface for fragment shaders, inheriting from IShader.
    /// </summary>
    public interface IFragmentShader : IShader  {}
}
