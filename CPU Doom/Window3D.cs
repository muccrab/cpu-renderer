using CPU_Doom.Buffers;
using SFML.Graphics;
using SFML.Window;
using CPU_Doom.Types;
using OpenTK.Mathematics;
using CPU_Doom.Shaders;
using Logger;

namespace CPU_Doom
{
    // Static Properties For CPU Renderer
    internal static class WindowStatic
    {
        public static AsyncLogger Logger { get; private set; }

        static WindowStatic()
        {
            Logger = new AsyncLogger();
        }
    }

    /// <summary>
    /// Provides a 3D window for rendering with CPU-based graphics.
    /// </summary>
    public class Window3D
    {
        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        public Action<KeyboardInput> KeyPress;

        /// <summary>
        /// Occurs when a key is released.
        /// </summary>
        public Action<KeyboardInput> KeyReleased;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window3D"/> class with the specified dimensions and title.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="title">The title of the window.</param>
        /// <param name="renderBuffers">The number of render buffers to use (default is 2).</param>
        public Window3D(int width, int height, string title, int renderBuffers = 2) 
        {
            KeyPress += (x) => { }; //Compiller was angry at me that I kept these without declaration.
            KeyReleased += (x) => { };

            _window = CreateWindow(width, height, title);
            _buffers = new FrameBuffer2d[renderBuffers];
            for (int i = 0; i < renderBuffers; i++) _buffers[i] = new FrameBuffer2d(width, height, PIXELTYPE.RGBA32);
            _depthBuffer = new FrameBuffer2d(_width, _height, PIXELTYPE.FLOAT);
        }


        public void Dispose()
        {
            WindowStatic.Logger.Stop();
        }

        /// <summary>
        /// Runs the specified action in a loop until the window is closed.
        /// </summary>
        /// <param name="loop">The action to run on each frame.</param>
        public void Update(Action<IUserFrameContext> loop)  
        {
            FrameContext frameContext = new FrameContext(new WidnowTime());
            while (_window.IsOpen)
            {
                frameContext.NextFrame();
                _window.DispatchEvents();
                loop.Invoke(frameContext);
            }
            Dispose();
        }


        /// <summary>
        /// Clears the current render buffer with the specified color.
        /// </summary>
        /// <param name="clearColor">The color to use for clearing the buffer.</param>
        public void ClearCurrentBuffer(Vector4 clearColor)
        {
            _buffers[_currentBuffer].Clear(clearColor);
        }

        /// <summary>
        /// Clears the current render buffer with the specified color.
        /// </summary>
        /// <param name="clearColor">The color to use for clearing the buffer.</param>
        public void ClearCurrentBuffer(System.Drawing.Color clearColor)
        {
            _buffers[_currentBuffer].Clear(clearColor);
        }

        /// <summary>
        /// Clears the current render buffer with the specified color.
        /// </summary>
        /// <param name="clearColor">The color to use for clearing the buffer as a byte array.</param>
        public void ClearCurrentBuffer(byte[] clearColor)
        {
            _buffers[_currentBuffer].Clear(clearColor);
        }

        /// <summary>
        /// Clears the current render buffer with the default clear color.
        /// </summary>
        public void ClearCurrentBuffer()
        {
            _buffers[_currentBuffer].Clear();
        }

        /// <summary>
        /// Switches between the render buffers.
        /// </summary>
        public void SwitchBuffers() 
        {
            _currentBuffer += 1;
            _currentBuffer %= _buffers.Length;
        }

        /// <summary>
        /// Enables or disables depth buffering.
        /// </summary>
        /// <param name="enabled">Whether depth buffering should be enabled.</param>
        public void DepthBufferingEnabled(bool enabled) => _depthBufferEnabled = enabled;

        /// <summary>
        /// Clears the depth buffer.
        /// </summary>
        public void ClearDepthBuffer() => _depthBuffer.Clear();

        /// <summary>
        /// Draws the current framebuffer to the window.
        /// </summary>
        public void DrawFramebuffer()
        {
            Image image = new Image((uint)_width, (uint)_height, _buffers[_currentBuffer].Data); 
            Texture texture = new Texture(image);
            Sprite sprite = new Sprite(texture, new IntRect(0, 0, _width, _height));
            _window.Draw(sprite);
            _window.Display();
        }

        /// <summary>
        /// Draws the currently bound vertex array to the current framebuffer using the specified shader program.
        /// </summary>
        /// <param name="program">The shader program to use for rendering.</param>
        public void Draw(ShaderProgram program)
        {
            if (_bindedArray == null) return;
            program.Draw(_buffers[_currentBuffer], _bindedArray, _depthBufferEnabled ? _depthBuffer : null);
        }

        /// <summary>
        /// Binds a vertex array object to the renderer.
        /// </summary>
        /// <param name="vao">The vertex array object to bind.</param>
        public void BindVertexArray(VertexArrayObject? vao)
        {
            _bindedArray = vao;
        }

        /// <summary>
        /// Unbinds the current vertex array object.
        /// </summary>
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
