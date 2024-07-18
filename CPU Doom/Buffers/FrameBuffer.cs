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

    public class FrameBuffer : SizedSetEnum<byte[]>
    {
        public override int Size => _size;
        public byte[] Data => _data;
        public FrameBuffer(int size, PIXELTYPE type)
        {
            _size = size;
            _typeLn = (int)type;
            _data = new byte[size * _typeLn];
        }

        public override byte[] this[int key]
        {
            get => _data[(key * _typeLn).._typeLn];
        }

        public override void Setter(int key, byte[] value)
        {
            int minLn = Math.Min(_typeLn, value.Length);
            for (int i = 0; i < minLn; ++i)
            {
                _data[(key * _typeLn) + i] = value[i];
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
        private int _size;

    }
}
