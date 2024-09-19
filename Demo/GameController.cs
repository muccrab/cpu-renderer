using CPU_Doom;
using CPU_Doom.Buffers;
using CPU_Doom.Shaders;
using Demo.Base_Components;

namespace Demo
{
    // Main Controller of the Game Engine
    internal class GameController
    {
        public const int WIDTH = 700;
        public const int HEIGHT = 360;
        const string TITLE = "Game";

        public KeyboardManager KeyboardManager => _keyboardManager;

        public GameController()
        {
            _window = new Window3D(WIDTH, HEIGHT, TITLE);
            _logic = new LogicController(this);
            _keyboardManager = new KeyboardManager(_window);

            var scene = new GameObject();
            scene.AddComponent(new Mesh());
            scene.AddComponent(new Texture());

            var cameraObj = new GameObject();
            cameraObj.SetParent(scene);
            var camera = new Camera();
            cameraObj.AddComponent(camera);
            cameraObj.Transform.Position = new OpenTK.Mathematics.Vector3(0.5f, 0.5f, -10);
            cameraObj.AddComponent(new KeyboardMC());
            cameraObj.AddComponent(new CharacterController());

            _logic.LoadScene("Pyramid", scene);
            _window.DepthBufferingEnabled(true);
            _window.Update(Update);
        }

        private void Update(IUserFrameContext context)
        {
            _logic.Update();
            _window.ClearCurrentBuffer(System.Drawing.Color.Green);
            _window.ClearDepthBuffer();
            while (_renderQueue.Count > 0) 
            {
                var property = _renderQueue.Dequeue();
                _window.BindVertexArray(property.VertexArray);
                _window.Draw(property.Program);
            }
            _window.DrawFramebuffer();
            _window.SwitchBuffers();
        }

        public void EnqueueRender(RenderProperty property, int priority = int.MaxValue)
        {
            _renderQueue.Enqueue(property, priority);
        }


        private Window3D _window;
        private LogicController _logic;
        private KeyboardManager _keyboardManager;

        private PriorityQueue<RenderProperty, int> _renderQueue = new PriorityQueue<RenderProperty, int>();

    }

    internal struct RenderProperty
    {
        public VertexArrayObject VertexArray;
        public ShaderProgram Program;
    }



}
