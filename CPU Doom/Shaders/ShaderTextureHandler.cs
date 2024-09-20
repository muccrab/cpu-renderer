using CPU_Doom.Buffers;

namespace CPU_Doom.Shaders
{
    /// <summary>
    /// Helps the shader program manage textures.
    /// </summary>
    internal class ShaderTextureHandler
    {
        /// <summary>
        /// Sets a 1D texture to the specified position or appends it to the list.
        /// </summary>
        /// <param name="texture">The 1D texture to set.</param>
        /// <param name="texturePos">The position at which to set the texture. If -1, the texture will be appended to the list.</param>
        /// <returns>The position at which the texture is set.</returns>
        public int SetTexture1d(TextureBuffer1d texture, int texturePos = -1)
        {
            if (texturePos < _textures.Count && texturePos >= 0)
            {
                _textures[texturePos] = texture;
                return texturePos;
            }

            _textures.Add(texture);
            return _textures.Count - 1;
        }

        /// <summary>
        /// Sets a 2D texture to the specified position or appends it to the list.
        /// </summary>
        /// <param name="texture">The 2D texture to set.</param>
        /// <param name="texturePos">The position at which to set the texture. If -1, the texture will be appended to the list.</param>
        /// <returns>The position at which the texture is set.</returns>
        public int SetTexture2d(TextureBuffer2d texture, int texturePos = -1)
        {
            if (texturePos < _textures.Count && texturePos >= 0)
            {
                _textures2d[texturePos] = texture;
                return texturePos;
            }

            _textures2d.Add(texture);
            return _textures2d.Count - 1;
        }

        /// <summary>
        /// Retrieves a 1D texture from the specified position.
        /// </summary>
        /// <param name="texturePos">The position of the texture to retrieve.</param>
        /// <returns>The 1D texture at the specified position, or null if the position is out of range.</returns>
        public TextureBuffer1d? GetTexture1d(int texturePos)
        {
            if (texturePos < _textures.Count) return _textures[texturePos];
            else return null;
        }

        /// <summary>
        /// Retrieves a 2D texture from the specified position.
        /// </summary>
        /// <param name="texturePos">The position of the texture to retrieve.</param>
        /// <returns>The 2D texture at the specified position, or null if the position is out of range.</returns>
        public TextureBuffer2d? GetTexture2d(int texturePos)
        {
            if (texturePos < _textures2d.Count) return _textures2d[texturePos];
            else return null;
        }

        List<TextureBuffer1d> _textures = new List<TextureBuffer1d>();
        List<TextureBuffer2d> _textures2d = new List<TextureBuffer2d>();
    }
}
