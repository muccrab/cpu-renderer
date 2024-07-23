using CPU_Doom.Interfaces;
using CPU_Doom.Shaders;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Demo.Shaders
{
    internal class BasicVertexShader : IVertexShader
    {
        public Vector4 Position { get; set; }

        [UniformAttribute("cameraPos")]
        public static Vector4d uniCameraPos;
        [UniformAttribute("MVP")]
        public static Vector4d uniMVP;


        [InputAttribute("in_position")]
        public Vector4 inPosition;
        [InputAttribute("in_color")]
        public Vector4 inColor;



        [OutputAttribute("f_color")]
        public Vector4 outColor;

        public void Execute()
        {
            inPosition.W = 1;
            Position = inPosition;
            outColor = inColor;
        }
    }
}
