using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CPU_Doom.Interfaces;
using CPU_Doom.Shaders;
using CPU_Doom.Types;
using OpenTK.Mathematics;

namespace Demo.Shaders
{
    internal class TextureFragmentShader : IFragmentShader
    {

        [InputAttribute("f_texCoord")]
        public Vector2 inTexCoord;

        [OutputAttribute("out_color")]
        public int color;

        [UniformAttribute("u_texture")]
        public static int textureSampler;

        public void Execute(ShaderFunctions func)
        {
            color = func.Texture2d(textureSampler, inTexCoord).ToInt();  
        }
    }
}
