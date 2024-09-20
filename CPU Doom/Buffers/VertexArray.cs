namespace CPU_Doom.Buffers
{
    /// <summary>
    /// Vertex Array Object that connects VertexBuffer and ElementBuffer
    /// </summary>
    public class VertexArrayObject
    {
        /// <summary>
        /// Gets Element Buffer.
        /// </summary>
        public ElementBuffer Indices { get; private init; }

        /// <summary>
        /// Gets Vertex Buffer.
        /// </summary>
        public SizedEnum<Vertex> Vertices { get; private init; }

        /// <summary>
        /// Initializes a new 2D texture buffer using a frame buffer.
        /// </summary>
        /// <param name="indices">Element Buffer</param>
        /// <param name="vertices">Vertex Buffer</param>
        public VertexArrayObject(ElementBuffer indices, SizedEnum<Vertex> vertices)
        {
            Indices = indices;
            Vertices = vertices;
        }
    }
}
