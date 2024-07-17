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
                window.ClearCurrentBuffer(System.Drawing.Color.Green);
                //_window.SetClearColor(System.Drawing.Color.Red);
                window.BindVertexArray(_vao);
                window.Draw();
                window.UnbindVertexArray();

                window.DrawFramebuffer();
                window.SwitchBuffers();
                Console.WriteLine("-");
            });
        }


        static void Start(Window3D window)
        {
            byte[] vertices = new float[]
            {
                -.5f, -.5f, 1, 0, 0,
                   0,  .5f, 0, 1, 0,
                 .5f, -.5f, 0, 0, 1,
            }.ToByteArray();


            ElementBuffer indeces =
            {
                0, 1, 2
            };


            Stride stride = new Stride();
            stride.AddEntry(PIXELTYPE.FLOAT, 2); // Vertex Position
            stride.AddEntry(PIXELTYPE.FLOAT, 3); // Vertex Color

            VertexBuffer vbo = new VertexBuffer(stride, vertices);
            _vao = new VertexArrayObject(indeces, vbo);

            ShaderProgram program = new ShaderProgram(typeof(BasicVertexShader), typeof(BasicFragmentShader));
            Console.WriteLine("Shaders Linked");
        }


        private static VertexArrayObject _vao;
    }
}
