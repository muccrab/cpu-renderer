namespace CPU_Doom.Buffers
{
    public class VertexArrayObject
    {
        public ElementBuffer Indices { get; private init; }
        public SizedEnum<Vertex> Vertices { get; private init; }
        public VertexArrayObject(ElementBuffer indices, SizedEnum<Vertex> vertices)
        {
            Indices = indices;
            Vertices = vertices;
        }
    }
}
