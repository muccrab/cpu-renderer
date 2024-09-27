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
    internal class MonkeVertexShader : IVertexShader
    {
        public Vector4 Position { get; set; }

        [InputAttribute("in_position")]
        public Vector3 inPosition;
        [InputAttribute("in_Normal")]
        public Vector3 inNormal;
        [InputAttribute("in_UV")]
        public Vector2 inUV;

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

        [OutputAttribute("f_texCoord")]
        public Vector2 outTexCoord;

        public void Execute(ShaderFunctions func)
        {
            Matrix4 rotation = Matrix4.CreateRotationX(uTime) * Matrix4.CreateRotationY(uTime / 2);
            Position =  (new Vector4(inPosition, 1.0f) * rotation) * uModel * uView * uPerspective;
            Vector4 normal = new Vector4(inNormal, 1.0f) * rotation;
            outColor = Vector4.One * Math.Abs(Vector3.Dot(normal.Xyz.Normalized(), 
                (new Vector3(1,-1,0) - Position.Xyz).Normalized()
                ));
            outTexCoord = inUV;
        }
    }
}
