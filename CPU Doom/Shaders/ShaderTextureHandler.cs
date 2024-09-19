using CPU_Doom.Buffers;

namespace CPU_Doom.Shaders
{
    // Helps shader program with Texture work
    internal class ShaderTextureHandler
    {
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
        public TextureBuffer1d? GetTexture1d(int texturePos)
        {
            if (texturePos < _textures.Count) return _textures[texturePos];
            else return null;
        }
        public TextureBuffer2d? GetTexture2d(int texturePos)
        {
            if (texturePos < _textures2d.Count) return _textures2d[texturePos];
            else return null;
        }

        List<TextureBuffer1d> _textures = new List<TextureBuffer1d>();
        List<TextureBuffer2d> _textures2d = new List<TextureBuffer2d>();
    }
}
