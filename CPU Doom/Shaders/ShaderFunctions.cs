using OpenTK.Mathematics;

namespace CPU_Doom.Shaders
{
    public class ShaderFunctions
    {
        public ShaderFunctions(ShaderProgram program) 
        {
            _program = program;
        }
        public byte[] Texture1d(int sampler1d, float pos) 
        {
            var texture = _program.GetTexture1d(sampler1d);
            if (texture == null) return new byte[] { };
            return texture.GetPixel(pos);
        }
        public byte[] Texture2d(int sampler1d, float posX, float posY) 
        {
            var texture = _program.GetTexture2d(sampler1d);
            if (texture == null) return new byte[] { };
            return texture.GetPixel(posX, posY);
        }
        public byte[] Texture2d(int sampler1d, Vector2 pos) => Texture2d(sampler1d, pos.X, pos.Y);
        ShaderProgram _program;
    }
}
