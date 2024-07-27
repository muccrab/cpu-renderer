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

        [UniformAttribute("u_time")]
        public static float uTime;

        [OutputAttribute("f_color")]
        public Vector4 outColor;




        private static float Rad(float angle) => angle * MathF.PI / 180;



        public void Execute()
        {
            inPosition.W = 1;
            //inPosition.Z = 1;
            Matrix4 rotation = Matrix4.CreateRotationX(uTime) * Matrix4.CreateRotationY(uTime / 2);
            Position = inPosition * rotation + new Vector4(0,0,1,0);
            outColor = inColor; //+ new Vector4(uTime > 1 ? uTime - (int)uTime : uTime);
        }
    }
}
