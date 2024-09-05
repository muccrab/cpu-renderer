using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CPU_Doom.Interfaces;
using CPU_Doom.Shaders;
using OpenTK.Mathematics;

namespace Demo.Shaders
{
    internal class TextureVertexShader : IVertexShader
    {
        public Vector4 Position { get; set; }

        [InputAttribute("in_position")]
        public Vector4 inPosition;
        [InputAttribute("in_texCoord")]
        public Vector2 inTexCoord;
        [OutputAttribute("f_texCoord")]
        public Vector2 outTexCoord;

        public void Execute(ShaderFunctions func)
        {
            outTexCoord = inTexCoord;
            inPosition.W = 1;
            Position = inPosition;
        }
    }
}
