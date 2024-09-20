using OpenTK.Mathematics;

namespace CPU_Doom.Shaders
{
    /// <summary>
    /// Provides functions that can be used by shaders to interact with textures.
    /// </summary>
    public class ShaderFunctions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderFunctions"/> class.
        /// </summary>
        /// <param name="program">The shader program that provides the textures.</param>
        public ShaderFunctions(ShaderProgram program) 
        {
            _program = program;
        }

        // <summary>
        /// Samples a 1D texture from the shader program at a specific position.
        /// </summary>
        /// <param name="sampler1d">The sampler ID for the 1D texture.</param>
        /// <param name="pos">The position on the 1D texture to sample.</param>
        /// <returns>The pixel data at the specified position, or an empty byte array if the texture is null.</returns>
        public byte[] Texture1d(int sampler1d, float pos) 
        {
            var texture = _program.GetTexture1d(sampler1d);
            if (texture == null) return new byte[] { };
            return texture.GetPixel(pos);
        }

        /// <summary>
        /// Samples a 2D texture from the shader program at specific x and y positions.
        /// </summary>
        /// <param name="sampler1d">The sampler ID for the 2D texture.</param>
        /// <param name="posX">The x position on the 2D texture to sample.</param>
        /// <param name="posY">The y position on the 2D texture to sample.</param>
        /// <returns>The pixel data at the specified position, or an empty byte array if the texture is null.</returns>
        public byte[] Texture2d(int sampler1d, float posX, float posY) 
        {
            var texture = _program.GetTexture2d(sampler1d);
            if (texture == null) return new byte[] { };
            return texture.GetPixel(posX, posY);
        }

        /// <summary>
        /// Samples a 2D texture from the shader program at a specified position.
        /// </summary>
        /// <param name="sampler1d">The sampler ID for the 2D texture.</param>
        /// <param name="pos">The position on the 2D texture to sample, provided as a <see cref="Vector2"/>.</param>
        /// <returns>The pixel data at the specified position, or an empty byte array if the texture is null.</returns>
        public byte[] Texture2d(int sampler1d, Vector2 pos) => Texture2d(sampler1d, pos.X, pos.Y);
        ShaderProgram _program;
    }
}
