namespace CPU_Doom.Buffers
{
    /// <summary>
    /// Vertex buffer that stores each vertex data in separate arrays (parallel arrays).
    /// </summary>
    public class ParallelVertexBuffer : SizedEnum<Vertex>
    {
        /// <summary>
        /// Gets the total number of separate vertices stored.
        /// </summary>
        public override int Size => _size; // All separate vertices stored

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelVertexBuffer"/> class with a stride, vertex data, and indices.
        /// </summary>
        /// <param name="stride">Stride structure representing the format of the vertex data.</param>
        /// <param name="data">The parallel vertex data arrays.</param>
        /// <param name="indices">Indices that map the vertices to their respective data positions.</param>
        public ParallelVertexBuffer(Stride stride, byte[][] data, int[,] indices)
        {
            _Stride = stride;
            _data = data;
            _indices = indices;
            _size = _indices.GetLength(0);
        }

        /// <summary>
        /// Gets the vertex at the specified index.
        /// </summary>
        /// <param name="key">The index of the vertex.</param>
        /// <returns>The vertex at the specified index.</returns>
        public ParallelVertex this[int key] => Get(key);

        /// <summary>
        /// Gets the vertex at the specified index.
        /// </summary>
        /// <param name="key">The index of the vertex.</param>
        /// <returns>A <see cref="ParallelVertex"/> instance.</returns>
        public override ParallelVertex Get(int key) => new ParallelVertex(key, this);
        
        private Stride _Stride { get; init; }
        byte[][] _data;
        int[,] _indices;
        int _size;

        /// <summary>
        /// Represents a single vertex that can retrieve data from a specific position within the buffer.
        /// </summary>
        public class ParallelVertex : Vertex
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParallelVertex"/> class.
            /// </summary>
            /// <param name="indexStart">The index of the vertex in the buffer.</param>
            /// <param name="buffer">The <see cref="ParallelVertexBuffer"/> containing the vertex data.</param>
            public ParallelVertex(int indexStart, ParallelVertexBuffer buffer)
            {
                _indexStart = indexStart;
                _buffer = buffer;
            }

            /// <summary>
            /// Gets the number of elements in the vertex.
            /// </summary>
            public override int Size => _buffer._Stride.StrideElements;

            /// <summary>
            /// Retrieves the data at a specific position in the vertex.
            /// </summary>
            /// <param name="key">The index within the vertex.</param>
            /// <returns>A byte array representing the vertex data at the specified index.</returns>
            public override byte[] Get(int key)
            {
                Stride stride = _buffer._Stride;
                int length = stride[key].entryLength * stride[key].typeLength;
                int offset = _buffer._indices[_indexStart, key] * length;
                return _buffer._data[key][offset..(offset + length)];
            }
            int _indexStart;
            ParallelVertexBuffer _buffer;
        }
    }
}
