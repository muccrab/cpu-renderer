using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CPU_Doom.Buffers
{
    
    public class VertexArrayObject
    {
        public ElementBuffer Indices { get; private init; }
        public VertexBuffer Vertices { get; private init; }
        public VertexArrayObject(ElementBuffer indices, VertexBuffer vertices)
        {
            Indices = indices;
            Vertices = vertices;
        }
    }
}
