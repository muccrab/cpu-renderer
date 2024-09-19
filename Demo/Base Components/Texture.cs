using CPU_Doom.Buffers;
using CPU_Doom.Shaders;
using CPU_Doom_File_Loader;
using OpenTK.Mathematics;
using SFML.Graphics;
using SharpGL.SceneGraph.Assets;
namespace Demo.Base_Components
{
    internal class Texture : ObjectComponent
    {
        [ParserInput("ObjectLocation")]
        string _textureLoc = "obamna.png";

        TextureBuffer2d? _texture;
        int _texturePos = -1;
        public override void Start()
        {
            var meshComponent = ParentObject?.GetComponent<Mesh>();
            if (meshComponent == null) return;
            meshComponent.AddShaderSetter(this);

            _texture = TextureLoader.Load2dTextureFromFile(_textureLoc)
               .SetWrapModeHorizontal(WrapMode.REVERSE).SetWrapModeVertical(WrapMode.REVERSE).SetFiltering(FilterMode.LINEAR);
        }

        protected override void OnSetShader(ShaderProgram shader)
        {
            if (_texture == null) return;
            if (_texturePos < 0) _texturePos = shader.SetTexture2d(_texture);
            shader.SetUniform("u_texture", _texturePos);
        }
    }
}
