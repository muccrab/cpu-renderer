using CPU_Doom.Buffers;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using CPU_Doom.Types;
using OpenTK.Mathematics;
using System.Drawing;
using CPU_Doom.Shaders;



namespace CPU_Doom
{
    public class Window3D
    {
        
        

        public Window3D(int width, int height, string title, int renderBuffers = 2) 
        { 
            _window = CreateWindow(width, height, title);
            _buffers = new FrameBuffer2d[renderBuffers];
            for (int i = 0; i < renderBuffers; i++) _buffers[i] = new FrameBuffer2d(width, height, PIXELTYPE.RGBA32);
        }

        public void Update(Action<FrameContext> loop) 
        {
            while (_window.IsOpen)
            {
                _window.DispatchEvents();
                loop.Invoke(_frameContext);
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

        public void DrawFramebuffer()
        {
            Image image = new Image((uint)_width, (uint)_height, _buffers[_currentBuffer].Data); //Figure out if this fucks up for little endian colors......
            Texture texture = new Texture(image);
            Sprite sprite = new Sprite(texture, new IntRect(0, 0, _width, _height));
            _window.Draw(sprite);
            _window.Display();
        }

        public void Draw(ShaderProgram program)
        {
            if (_bindedArray == null) return;
            program.Draw(_buffers[_currentBuffer], _bindedArray);
        }

        public void BindVertexArray(VertexArrayObject? vao)
        {
            _bindedArray = vao;
        }
        public void UnbindVertexArray() => BindVertexArray(null);

   
        private RenderWindow CreateWindow(int width, int height, string title)
        {
            var window = new RenderWindow(new VideoMode((uint)width, (uint)height), "Doom-like Demo");
            _width = width; _height = height;
            window.Closed += (sender, e) => window.Close();
            return window;
        }



        private VertexArrayObject? _bindedArray;
        private RenderWindow _window;
        private FrameContext _frameContext = new FrameContext();
        private FrameBuffer2d[] _buffers;
        private int _currentBuffer = 0;
        private int _width, _height;

    }
}
