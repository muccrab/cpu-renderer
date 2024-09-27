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

        [UniformAttribute("u_Time")]
        public static float uTime;

        [UniformAttribute("u_Model")]
        public static Matrix4 uModel;

        [UniformAttribute("u_View")]
        public static Matrix4 uView;

        [UniformAttribute("u_Projection")]
        public static Matrix4 uPerspective;

        [OutputAttribute("f_color")]
        public Vector4 outColor;




        private static float Rad(float angle) => angle * MathF.PI / 180;



        public void Execute(ShaderFunctions func)
        {
            inPosition.W = 1;
            //inPosition.Z = 1;
            //Matrix4 rotation = Matrix4.CreateRotationX(uTime) * Matrix4.CreateRotationY(uTime / 2);
            Matrix4 rotation = Matrix4.Identity;
            Position = inPosition * rotation * uModel * uView * uPerspective;
            outColor = inColor; //+ new Vector4(uTime > 1 ? uTime - (int)uTime : uTime);
        }
    }
}
