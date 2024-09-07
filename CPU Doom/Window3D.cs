using CPU_Doom.Buffers;
using SFML.Graphics;
using SFML.Window;
using CPU_Doom.Types;
using OpenTK.Mathematics;
using CPU_Doom.Shaders;
using Logger;

namespace CPU_Doom
{
    internal static class WindowStatic
    {
        public static AsyncLogger Logger { get; private set; }

        static WindowStatic()
        {
            Logger = new AsyncLogger();
        }
    }


    public class Window3D
    {

        public Action<KeyboardInput> KeyPress;
        public Action<KeyboardInput> KeyReleased;

        public Window3D(int width, int height, string title, int renderBuffers = 2) 
        { 
            _window = CreateWindow(width, height, title);
            _buffers = new FrameBuffer2d[renderBuffers];
            for (int i = 0; i < renderBuffers; i++) _buffers[i] = new FrameBuffer2d(width, height, PIXELTYPE.RGBA32);
            _depthBuffer = new FrameBuffer2d(_width, _height, PIXELTYPE.FLOAT);
        }
        public void Update(Action<IUserFrameContext> loop) 
        {
            FrameContext frameContext = new FrameContext(new WidnowTime());
            while (_window.IsOpen)
            {
                frameContext.NextFrame();
                _window.DispatchEvents();
                loop.Invoke(frameContext);
            }
        }
        public void ClearCurrentBuffer(Vector4 clearColor)
        {
            _buffers[_currentBuffer].Clear(clearColor);
        }
        public void ClearCurrentBuffer(System.Drawing.Color clearColor)
        {
            _buffers[_currentBuffer].Clear(clearColor);
        }
        public void ClearCurrentBuffer()
        {
            _buffers[_currentBuffer].Clear();
        }
        public void SwitchBuffers() 
        {
            _currentBuffer += 1;
            _currentBuffer %= _buffers.Length;
        }
        public void DepthBufferingEnabled(bool enabled) => _depthBufferEnabled = enabled;
        public void ClearDepthBuffer() => _depthBuffer.Clear();
        public void DrawFramebuffer()
        {
            Image image = new Image((uint)_width, (uint)_height, _buffers[_currentBuffer].Data); 
            Texture texture = new Texture(image);
            Sprite sprite = new Sprite(texture, new IntRect(0, 0, _width, _height));
            _window.Draw(sprite);
            _window.Display();
        }
        public void Draw(ShaderProgram program)
        {
            if (_bindedArray == null) return;
            program.Draw(_buffers[_currentBuffer], _bindedArray, _depthBufferEnabled ? _depthBuffer : null);
        }
        public void BindVertexArray(VertexArrayObject? vao)
        {
            _bindedArray = vao;
        }
        public void UnbindVertexArray() => BindVertexArray(null);

        private void OnKeyPress(object? sender, KeyEventArgs e) => KeyPress(new KeyboardInput(sender, e));
        private void OnKeyRelease(object? sender, KeyEventArgs e) => KeyReleased(new KeyboardInput(sender, e));

        private RenderWindow CreateWindow(int width, int height, string title)
        {
            var window = new RenderWindow(new VideoMode((uint)width, (uint)height), title);
            _width = width; _height = height;
            window.Closed += (sender, e) => window.Close();
            window.KeyPressed += OnKeyPress;
            window.KeyReleased += OnKeyRelease;
            return window;
        }
        private VertexArrayObject? _bindedArray;
        private RenderWindow _window;
        private FrameBuffer2d[] _buffers;
        private FrameBuffer2d _depthBuffer;
        private bool _depthBufferEnabled = false;
        private int _currentBuffer = 0;
        private int _width, _height;
    }
}
