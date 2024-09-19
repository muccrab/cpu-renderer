using CPU_Doom.Types;
using OpenTK.Mathematics;


namespace CPU_Doom.Buffers
{
    // 2D framebuffer for Rendering or Textures
    public class FrameBuffer2d : SizedEnum<FrameBuffer>
    {
        public int TypeLength => _typeLn; // Size of Framebuffer unit type
        public PIXELTYPE PixelType { get; private set; } // Famebuffer unit type
        public override int Size => _height; // Number of rows of the framebuffer
        public int RowSize => _width; // Number of columns of the framebuffer
        public FrameBuffer this[int key] => Get(key);
        public byte[] Data { get {
                var data = (from buffer in _subBuffers select buffer.Data);
                var ret = data.SelectMany(data => data).ToArray();
                return ret;
            } } // Returns all data in one array
        public FrameBuffer2d(int width, int height, PIXELTYPE type) 
        {
            _width = width;
            _height = height;
            _typeLn = PixelTypeConverter.GetSize(type);
            PixelType = type;
            _subBuffers = (from _ in Enumerable.Range(0, height) select new FrameBuffer(width, type) ).ToArray();
        }
        public FrameBuffer2d(byte[] data, int width, int height, PIXELTYPE type)
        {
            _width = width;
            _height = height;
            _typeLn = PixelTypeConverter.GetSize(type);
            PixelType = type;

            int fullWidth = _width * _typeLn; // width in number of bytes

            if (data.Length < fullWidth * _height) data = data.Concat(new byte[fullWidth * height - data.Length]).ToArray();

            _subBuffers = (from i in Enumerable.Range(0, height) 
                           select new FrameBuffer(
                               data[(i * fullWidth)..((i+1) * fullWidth)],
                               width, type)
                           ).ToArray();
        }

        public override FrameBuffer Get(int key) => _subBuffers[key];

        public void Clear(Vector4 color)
        {
            foreach(var buffer in _subBuffers) buffer.Clear(color);
        }

        public void Clear(System.Drawing.Color color)
        {
            foreach (var buffer in _subBuffers) buffer.Clear(color);
        }

        public void Clear(byte[] color)
        {
            foreach (var buffer in _subBuffers) buffer.Clear(color);
        }

        public void Clear()
        {
            foreach (var buffer in _subBuffers) buffer.Clear();
        }

        public FrameBuffer2d Copy()
        {
            return new FrameBuffer2d(Data ,_width, _height, PixelType);
        }

        private int _typeLn;
        private int _width, _height;
        private FrameBuffer[] _subBuffers;
    }

    // 1D Framebuffer for Rendering or Textures
    public class FrameBuffer : SizedSetEnum<byte[]>
    {
        public override int Size => _size; // Data size in relative length (Full byte size = TypeLength * Size)
        public byte[] Data => _data; 
        public int TypeLength => _typeLn; // Size of Framebuffer unit type
        public PIXELTYPE PixelType { get; private set; } // Famebuffer unit type
        public FrameBuffer(int size, PIXELTYPE type)
        {
            _size = size;
            _typeLn = PixelTypeConverter.GetSize(type);
            PixelType = type;
            _data = new byte[size * _typeLn];
        }
        public FrameBuffer(byte[] data, int size, PIXELTYPE type)
        {
            _size = size;
            _typeLn = PixelTypeConverter.GetSize(type);
            PixelType = type;

            int fullSize = _size * _typeLn;

            if (data.Length >= fullSize) _data = data[0..fullSize];
            else _data = data.Concat(new byte[fullSize - data.Length]).ToArray();

        }
        public byte[] this[int key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        public override byte[] Get(int key) => _data[(key * _typeLn)..(key * _typeLn + _typeLn)];
        public override void Set(int key, byte[] value) // Sets byte value at specific position, If larger, trims the value.
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
            for (int i = 0; i < Size; ++i) this[i] = clearColor;
        }
        public void Clear(System.Drawing.Color color)
        {
            byte[] clearColor = color.ToByteArray();
            for (int i = 0; i < Size; ++i) this[i] = clearColor;
        }
        public void Clear(byte[] color)
        {
            for (int i = 0; i < Size; ++i) this[i] = color;
        }

        public void Clear()
        {
            byte[] clearColor = new byte[_typeLn];
            for (int i = 0; i < Size; ++i) this[i] = clearColor;
        }

        public FrameBuffer Copy()
        {
            byte[] data = new byte[_typeLn * _size];
            Data.CopyTo(data, 0);
            return new FrameBuffer(data, _size, PixelType);
        }

        private int _typeLn;
        private byte[] _data;
        private int _size; 
    }
}
