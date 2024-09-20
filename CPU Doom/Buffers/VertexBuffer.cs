using CPU_Doom.Types;

namespace CPU_Doom.Buffers
{
    /// <summary>
    /// Abstract base class for vertex types, inheriting from SizedEnum.
    /// </summary>
    public abstract class Vertex : SizedEnum<byte[]>
    {
        /// <summary>
        /// Accesses a vertex by index.
        /// </summary>
        /// <param name="key">Index of the vertex.</param>
        /// <returns>The byte data of the vertex at the specified index.</returns>
        public byte[] this[int key] => Get(key);
    }

    /// <summary>
    /// Structure to represent a single stride entry, including offsets and lengths.
    /// </summary>
    public struct StrideEntry
    {
        public int startOffset, typeLength, entryLength;

        /// <summary>
        /// Creates a new StrideEntry.
        /// </summary>
        /// <param name="type">The type of pixel (defines its size).</param>
        /// <param name="length">The length of the stride entry.</param>
        /// <param name="offset">The starting offset of the stride entry.</param>
        public StrideEntry(PIXELTYPE type, int length, int offset)
        {
            startOffset = offset;
            typeLength = PixelTypeConverter.GetSize(type);
            entryLength = length;
        }
    }

    /// <summary>
    /// Describes how vertex data is stored within a buffer (stride).
    /// </summary>
    public class Stride
    {
        /// <summary>
        /// Total length of the stride.
        /// </summary>
        public int StrideLength { get; private set; } = 0;

        /// <summary>
        /// Number of stride elements.
        /// </summary>
        public int StrideElements { get; private set; } = 0;

        /// <summary>
        /// Adds a new stride entry to the stride structure.
        /// </summary>
        /// <param name="type">The type of pixel (defines its size).</param>
        /// <param name="length">The length of the stride entry.</param>
        public void AddEntry(PIXELTYPE type, int length)
        {
            _stride.Add(new(type, length, StrideLength));
            StrideLength += PixelTypeConverter.GetSize(type) * length;
            StrideElements++;
        }

        /// <summary>
        /// Retrieves a stride entry by index.
        /// </summary>
        /// <param name="key">The index of the stride entry.</param>
        /// <returns>The stride entry at the specified index.</returns>
        public StrideEntry this[int key] => _stride[key];
        private List<StrideEntry> _stride = new List<StrideEntry>();
    }

    /// <summary>
    /// A vertex buffer that stores all vertex data in a single array.
    /// </summary>
    public class VertexBuffer : SizedEnum<Vertex>
    {
        /// <summary>
        /// Gets the size of the vertex buffer.
        /// </summary>
        public override int Size => _size;

        /// <summary>
        /// Initializes a new vertex buffer with the given stride and data.
        /// </summary>
        /// <param name="stride">The stride that defines how data is organized.</param>
        /// <param name="data">The vertex data stored in the buffer.</param>
        public VertexBuffer(Stride stride, byte[] data) 
        {
            _Stride = stride;
            _data = data;
            _size = data.Length / stride.StrideLength;
        }

        /// <summary>
        /// Retrieves a vertex by index.
        /// </summary>
        /// <param name="key">The index of the vertex.</param>
        /// <returns>The SimpleVertex corresponding to the index.</returns>
        public SimpleVertex this[int key] => Get(key);

        /// <summary>
        /// Retrieves a vertex by index.
        /// </summary>
        /// <param name="key">The index of the vertex.</param>
        /// <returns>The SimpleVertex corresponding to the index.</returns>
        public override SimpleVertex Get(int key) => new SimpleVertex(key * _Stride.StrideLength, this);
        private Stride _Stride { get; init; }
        byte[] _data;
        int _size;

        /// <summary>
        /// Represents a single vertex in the vertex buffer.
        /// </summary>
        public class SimpleVertex : Vertex
        {
            /// <summary>
            /// Initializes a SimpleVertex with a starting index and buffer reference.
            /// </summary>
            /// <param name="indexStart">Starting index in the data array.</param>
            /// <param name="buffer">The buffer this vertex belongs to.</param>
            public SimpleVertex(int indexStart, VertexBuffer buffer)
            {
                _indexStart = indexStart;
                _buffer = buffer;
            }

            /// <summary>
            /// Gets the number of elements in this vertex.
            /// </summary>
            public override int Size => _buffer._Stride.StrideElements;

            /// <summary>
            /// Retrieves the byte data for a given element in the vertex.
            /// </summary>
            /// <param name="key">Index of the vertex element.</param>
            /// <returns>The byte data for the vertex element.</returns>
            public override byte[] Get(int key) 
            {
                Stride stride = _buffer._Stride;
                var stridePair = stride[key];
                int offset = _indexStart + stridePair.startOffset;
                int typeLenght = stridePair.typeLength;
                int length = stridePair.entryLength;

                return _buffer._data[offset..(offset + typeLenght * length)];
            }
            int _indexStart;
            VertexBuffer _buffer;
        }
    }
}
