global using ElementBuffer = int[];
using CPU_Doom;
using CPU_Doom.Buffers;
using CPU_Doom.Shaders;
using CPU_Doom.Interfaces;
using CPU_Doom.Types;

using Demo.Shaders;
using OpenTK.Mathematics;
using System.Reflection;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            BasicFragmentShader fragmentShader = new BasicFragmentShader();
            Console.WriteLine(fragmentShader.GetType().GetField("DissableFloatConvertion", BindingFlags.Static | BindingFlags.Public).GetValue(null).ToString());
            IFragmentShader<Vector4> inter = fragmentShader;
            Console.WriteLine(inter.GetType().GetField("DissableFloatConvertion", BindingFlags.Static | BindingFlags.Public).GetValue(null).ToString());
            */

            Window3D window = new Window3D(320, 200, "Doom on CPU");
            Start(window);
            window.Update((context) =>
            {
                float time = (float)context.Time.Time;
                window.ClearCurrentBuffer(System.Drawing.Color.Green);
                window.ClearDepthBuffer();
                //_window.SetClearColor(System.Drawing.Color.Red);
                window.BindVertexArray(_vao);
                _shader.SetUniform("u_time", time);
                window.Draw(_shader);
                window.UnbindVertexArray();

                window.DrawFramebuffer();
                window.SwitchBuffers();
                //Console.WriteLine(time);
            });
        }


        static void Start(Window3D window)
        {
            
            byte[] vertices = new float[]
            {
                -0.5f, -0.5f, -0.5f, 1, 1, 1, //0
                +0.5f, -0.5f, -0.5f, 0, 1, 1, //1
                -0.5f, -0.5f, +0.5f, 1, 0, 1, //2
                +0.5f, -0.5f, +0.5f, 1, 1, 0, //3
                -0.5f, +0.5f, -0.5f, 1, 0, 0, //4
                +0.5f, +0.5f, -0.5f, 0, 1, 0, //5
                -0.5f, +0.5f, +0.5f, 0, 0, 1, //6
                +0.5f, +0.5f, +0.5f, 0, 0, 0, //7
            }.ToByteArray();


            ElementBuffer indeces =
            {
                0, 1, 2, 1, 2, 3, //BOTTOM
                0, 1, 4, 1, 4, 5, //FRONT
                0, 2, 4, 2, 4, 6, //LEFT
                1, 3, 5, 3, 5, 7, //RIGHT
                2, 3, 6, 3, 6, 7, //BACK
                4, 5, 6, 5, 6, 7, //TOP
            };
            
            /*
            byte[] vertices = new float[]
            {
                -0.5f, -0.5f, 0f, 1, 0, 0, //0
                +0.5f, -0.5f, 0f, 0, 1, 0, //1
                -0.5f, +0.5f, 0f, 0, 0, 1, //2
                +0.5f, +0.5f, 0f, 1, 1, 1, //3

                -0.25f, -0.25f, 1f, 1, 1, 1, //4
                +0.25f, -0.75f, -1f, 1, 1, 1, //5
                -0.25f, +0.25f, 1f, 0, 0, 0, //6
                +0.25f, +0.75f, -1f, 0, 0, 0, //7


            }.ToByteArray();


            ElementBuffer indeces =
            {
                0, 1, 2, 1, 2, 3,
                4, 5, 6, 5, 6, 7,
            };
            */
            /*
            byte[] vertices = new float[]
            {
                -0.5f, -0.5f, 0f, 1, 0, 0, //0
                +0.0f, +0.5f, 0f, 0, 1, 0, //1
                +0.5f, -0.5f, 0f, 0, 0, 1, //2
                
                -0.25f, -0.25f,  1f,  1, 1, 1, //3
                +0.00f, +0.75f,  -1f,  0, 0, 0, //4
                +0.25f, -0.25f,  1f,  1, 1, 1, //5



            }.ToByteArray();


            ElementBuffer indeces =
            {
                0, 1, 2, 3, 4, 5
            };
            */

            Stride stride = new Stride();
            stride.AddEntry(PIXELTYPE.FLOAT, 3); // Vertex Position
            stride.AddEntry(PIXELTYPE.FLOAT, 3); // Vertex Color

            VertexBuffer vbo = new VertexBuffer(stride, vertices);
            _vao = new VertexArrayObject(indeces, vbo);

            _shader = new ShaderProgram<BasicVertexShader, BasicFragmentShader>();
            Console.WriteLine("Shaders Linked");
            window.DepthBufferingEnabled(true);
        }

        private static ShaderProgram _shader;
        private static VertexArrayObject _vao;
    }
}
