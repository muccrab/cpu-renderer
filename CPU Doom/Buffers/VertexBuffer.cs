using CPU_Doom.Types;

namespace CPU_Doom.Buffers
{  
    public abstract class Vertex : SizedEnum<byte[]>
    {
        public byte[] this[int key] => Get(key);
    }

    public struct StrideEntry
    {
        public int startOffset, typeLength, entryLength;
        public StrideEntry(PIXELTYPE type, int length, int offset)
        {
            startOffset = offset;
            typeLength = PixelTypeConverter.GetSize(type);
            entryLength = length;
        }
    }

    // Stride object that helps users tell vertexbuffer how data is stored for one vertex
    public class Stride
    {
        public int StrideLength { get; private set; } = 0;
        public int StrideElements { get; private set; } = 0;
        public void AddEntry(PIXELTYPE type, int length)
        {
            _stride.Add(new(type, length, StrideLength));
            StrideLength += PixelTypeConverter.GetSize(type) * length;
            StrideElements++;
        }
        public StrideEntry this[int key] => _stride[key];
        private List<StrideEntry> _stride = new List<StrideEntry>();
    }

    // Basic Vertex buffer that stores all vertex information on one array
    public class VertexBuffer : SizedEnum<Vertex>
    {
        public override int Size => _size;
        public VertexBuffer(Stride stride, byte[] data) 
        {
            _Stride = stride;
            _data = data;
            _size = data.Length / stride.StrideLength;
        }
        public SimpleVertex this[int key] => Get(key);
        public override SimpleVertex Get(int key) => new SimpleVertex(key * _Stride.StrideLength, this);
        private Stride _Stride { get; init; }
        byte[] _data;
        int _size;

        public class SimpleVertex : Vertex
        {
            public SimpleVertex(int indexStart, VertexBuffer buffer)
            {
                _indexStart = indexStart;
                _buffer = buffer;
            }
            public override int Size => _buffer._Stride.StrideElements;

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
