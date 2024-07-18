using CPU_Doom.Types;
using SFML.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom.Buffers
{
    
    public struct StrideEntry
    {
        public int startOffset, typeLength, entryLength;
        public StrideEntry(PIXELTYPE type, int length, int offset)
        {
            startOffset = offset;
            typeLength = (int)type;
            entryLength = length;
        }
    }

    public class Stride
    {
        public int StrideLength { get; private set; } = 0;
        public int StrideElements { get; private set; } = 0;
        public void AddEntry(PIXELTYPE type, int length)
        {
            _stride.Add(new(type, length, StrideLength));
            StrideLength += (int)type;
            StrideElements++;
        }
        public StrideEntry this[int key] => _stride[key];
        private List<StrideEntry> _stride = new List<StrideEntry>();
    }

    public class VertexBuffer : SizedEnum<VertexBuffer.Vertex>
    {
        public override int Size => _size;
        public VertexBuffer(Stride stride, byte[] data, int size) 
        {
            _Stride = stride;
            _data = data;
            _size = size;
        }

        public override Vertex this[int key] => new Vertex(key * _Stride.StrideLength, this);

        private Stride _Stride { get; init; }

        byte[] _data;
        int _size;


        public class Vertex : SizedEnum<byte[]>
        {
            public Vertex(int indexStart, VertexBuffer buffer)
            {
                _indexStart = indexStart;
                _buffer = buffer;
            }
            int _indexStart;
            VertexBuffer _buffer;

            public override int Size => _buffer._Stride.StrideElements;
            public override byte[] this[int key]
            {
                get
                {
                    Stride stride = _buffer._Stride;
                    var stridePair = stride[key];
                    int offset = stridePair.startOffset;
                    int typeLenght = stridePair.typeLength;
                    int length = stridePair.entryLength;

                    return _buffer._data[offset..(typeLenght * length)];
                }
            }
        }


        
    }
}
