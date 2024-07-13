using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using CPU_Doom.Types;
using System.Drawing;
using System.Collections;
using System.Reflection;
using OpenTK.Mathematics;

namespace CPU_Doom.Buffers
{



    public class FrameBuffer : IEnumerable<byte[]>
    {
        public int Size { get; private set; }
        public byte[] Data => _data;
        public FrameBuffer(int size, PIXELTYPE type)
        {
            Size = size;
            _typeLn = (int)type;
            _data = new byte[size * _typeLn];
        }

        public byte[] this[int key]
        {
            get => _data[(key * _typeLn).._typeLn];
            set {
                int minLn = Math.Min(_typeLn, value.Length);
                for (int i = 0; i < minLn; ++i)
                {
                    _data[(key * _typeLn) + i] = value[i];
                }
            }
        }

        public void Clear(Vector4 color)
        {
            byte[] clearColor = color.ToByteArray_RGBA32();
            Parallel.For(0, Size, i => 
            {
                this[i] = clearColor;
            });
        }

        public void Clear(System.Drawing.Color color)
        {
            byte[] clearColor = color.ToByteArray();
            Parallel.For(0, Size, i =>
            {
                this[i] = clearColor;
            });
        }

        public void Clear()
        {
            byte[] clearColor = new byte[_typeLn];
            Parallel.For(0, Size, i =>
            {
                this[i] = clearColor;
            });
        }


        private int _typeLn;
        private byte[] _data;


        public IEnumerator<byte[]> GetEnumerator() => new FrameBuffer_Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
       

        class FrameBuffer_Enumerator : IEnumerator<byte[]>
        {
            public FrameBuffer_Enumerator(FrameBuffer buffer)
            {
                _buffer = buffer;
            }

            public byte[] Current { get {
                    if (_pos == -1) throw new InvalidOperationException("Enumerator is uninitialized");
                    if (_pos >= _buffer.Size) throw new InvalidOperationException("Enumerator has gone through the collection");
                    return _buffer[_pos];
                } }

            object IEnumerator.Current => Current;

            public void Dispose() {}

            public bool MoveNext()
            {
                _pos++;
                return _pos < _buffer.Size;
            }

            public void Reset()
            {
                _pos = -1;
            }

            private FrameBuffer _buffer;
            private int _pos = -1;
        }


    }
}
