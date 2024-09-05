using CPU_Doom.Buffers;
using CPU_Doom.Shaders;
using CPU_Doom_File_Loader;
using Demo.Shaders;
using SharpGL.SceneGraph.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    internal class Mesh : ObjectComponent
    {
        public override void Start()
        {
            var vaos = ObjectLoader.LoadVAOsFromObjFile("pyramid.obj");
            _vao = vaos[0];
            _shader = new ShaderProgram<MoveableVertexShader, TextureFragmentShader>();
            var texture = TextureLoader.Load2dTextureFromFile("shrek.jpg")
                .SetWrapModeHorizontal(WrapMode.REVERSE).SetWrapModeVertical(WrapMode.REVERSE).SetFiltering(FilterMode.LINEAR);
            _texPos = _shader.SetTexture2d(texture, 0);
        }

        public override void Update()
        {
            if (_shader is null || _vao is null) return;
            if (ParentObject == null) return;
            _shader.SetUniform("u_texture", _texPos);
            ParentObject.Transform.SetShader(_shader);
            ParentObject.LogicController?.MainCamera.SetShader(_shader);
            ParentObject.GameController?.EnqueueRender(new() { Program = _shader, VertexArray = _vao });
        }

        private VertexArrayObject? _vao;
        private ShaderProgram? _shader;
        private int _texPos;

    }
}
