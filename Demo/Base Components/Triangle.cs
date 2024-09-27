using CPU_Doom.Buffers;
using CPU_Doom.Shaders;
using CPU_Doom.Types;
using CPU_Doom_File_Loader;
using Demo.Shaders;

namespace Demo.Base_Components
{
    internal class Triangle : ObjectComponent
    {

        public override void Start()
        {
            byte[] vertices = new float[3 * (2 + 3)]
            {
                -0.5f,  0.5f, 1, 0, 0,
                 0.0f, -0.5f, 0, 1, 0,
                 0.5f,  0.5f, 0, 0, 1
            }.ToByteArray();

            Stride stride = new Stride();
            stride.AddEntry(PIXELTYPE.FLOAT, 2); // Position
            stride.AddEntry(PIXELTYPE.FLOAT, 3); // Color

            int[] indices = new int[3] { 0, 1, 2 };

            _vao = new VertexArrayObject(indices, new VertexBuffer(stride, vertices));
            _shader = new ShaderProgram<BasicVertexShader, BasicFragmentShader>();
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
