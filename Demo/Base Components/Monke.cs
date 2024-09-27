using CPU_Doom.Buffers;
using CPU_Doom.Shaders;
using CPU_Doom_File_Loader;
using Demo.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Base_Components
{
    internal class Monke : ObjectComponent
    {
        public List<ObjectComponent> _shaderSetters = new List<ObjectComponent>();

        public override void Start()
        {
            var vaos = ObjectLoader.LoadVAOsFromObjFile("monke.obj");
            _vao = vaos[0];
            _shader = new ShaderProgram<MonkeVertexShader, BasicFragmentShader>();
        }

        public override void Update()
        {
            if (_shader is null || _vao is null) return;
            if (ParentObject == null) return;
            ParentObject.Transform.SetShader(_shader);
            ParentObject.LogicController?.MainCamera.SetShader(_shader);
            ParentObject.GameController?.EnqueueRender(new() { Program = _shader, VertexArray = _vao });
        }

        private VertexArrayObject? _vao;
        private ShaderProgram? _shader;
    }
}
