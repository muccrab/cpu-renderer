using CPU_Doom.Buffers;
using CPU_Doom.Shaders;
using CPU_Doom_File_Loader;
using Demo.Shaders;

namespace Demo.Base_Components
{
    internal class Mesh : ObjectComponent
    {
        [ParserInput("ObjectLocation")]
        string _objectLoc = "pyramid.obj";

        public List<ObjectComponent> _shaderSetters = new List<ObjectComponent>();

        public override void Start()
        {
            var vaos = ObjectLoader.LoadVAOsFromObjFile(_objectLoc);
            _vao = vaos[0];
            _shader = new ShaderProgram<MoveableVertexShader, TextureFragmentShader>();
        }

        public override void Update()
        {
            if (_shader is null || _vao is null) return;
            if (ParentObject == null) return;
            ParentObject.Transform.SetShader(_shader);
            ParentObject.LogicController?.MainCamera.SetShader(_shader);
            foreach (var shaderSetter in _shaderSetters) shaderSetter.SetShader(_shader);
            ParentObject.GameController?.EnqueueRender(new() { Program = _shader, VertexArray = _vao });
        }

        public void AddShaderSetter(ObjectComponent component) => _shaderSetters.Add(component);

        private VertexArrayObject? _vao;
        private ShaderProgram? _shader;
        private int _texPos;
    }
}
