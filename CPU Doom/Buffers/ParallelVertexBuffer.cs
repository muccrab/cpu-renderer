using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom.Buffers
{
    public class ParallelVertexBuffer : SizedEnum<Vertex>
    {
        public override int Size => _size;
        public ParallelVertexBuffer(Stride stride, byte[][] data, int[,] indices)
        {
            _Stride = stride;
            _data = data;
            _indices = indices;
            _size = _indices.GetLength(0);
        }
        public ParallelVertex this[int key] => Get(key);
        public override ParallelVertex Get(int key) => new ParallelVertex(key, this);
        private Stride _Stride { get; init; }
        byte[][] _data;
        int[,] _indices;
        int _size;

        public class ParallelVertex : Vertex
        {
            public ParallelVertex(int indexStart, ParallelVertexBuffer buffer)
            {
                _indexStart = indexStart;
                _buffer = buffer;
            }
            public override int Size => _buffer._Stride.StrideElements;

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
