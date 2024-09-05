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
    internal class MoveableVertexShader : IVertexShader
    {
        public Vector4 Position { get; set; }

        [InputAttribute("in_position")]
        public Vector3 inPosition;
        [InputAttribute("in_Normal")]
        public Vector3 inNormal;
        [InputAttribute("in_UV")]
        public Vector2 inUV;

        [UniformAttribute("u_time")]
        public static float uTime;

        [UniformAttribute("u_Model")]
        public static Matrix4 uModel;

        [UniformAttribute("u_View")]
        public static Matrix4 uView;

        [UniformAttribute("u_Projection")]
        public static Matrix4 uPerspective;

        [OutputAttribute("f_color")]
        public Vector4 outColor;

        [OutputAttribute("f_texCoord")]
        public Vector2 outTexCoord;




        private static float Rad(float angle) => angle * MathF.PI / 180;



        public void Execute(ShaderFunctions func)
        {
            inPosition.X = -inPosition.X;
            inPosition.Y = -inPosition.Y;
            Position = new Vector4(inPosition, 1) * uModel * uView * uPerspective;
            
            outTexCoord = inUV;
        }
    }
}
