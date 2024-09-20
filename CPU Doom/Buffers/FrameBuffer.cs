using CPU_Doom.Types;
using OpenTK.Mathematics;


namespace CPU_Doom.Buffers
{
    /// <summary>
    /// 2D framebuffer for rendering or textures.
    /// </summary>
    public class FrameBuffer2d : SizedEnum<FrameBuffer>
    {
        /// <summary>
        /// Gets the size of a framebuffer unit type in bytes.
        /// </summary>
        public int TypeLength => _typeLn; // Size of Framebuffer unit type

        /// <summary>
        /// Gets the type of the framebuffer pixel.
        /// </summary>
        public PIXELTYPE PixelType { get; private set; } // Famebuffer unit type

        /// <summary>
        /// Gets the number of rows in the framebuffer.
        /// </summary>
        public override int Size => _height; // Number of rows of the framebuffer

        /// <summary>
        /// Gets the number of columns in the framebuffer.
        /// </summary>
        public int RowSize => _width; // Number of columns of the framebuffer

        /// <summary>
        /// Gets the framebuffer at the specified key (row index).
        /// </summary>
        /// <param name="key">The row index.</param>
        /// <returns>The framebuffer row.</returns>
        public FrameBuffer this[int key] => Get(key);

        /// <summary>
        /// Gets all framebuffer data as a single byte array.
        /// </summary>
        public byte[] Data { get {
                var data = (from buffer in _subBuffers select buffer.Data);
                var ret = data.SelectMany(data => data).ToArray();
                return ret;
            } } // Returns all data in one array

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameBuffer2d"/> class with a specific width, height, and pixel type.
        /// </summary>
        /// <param name="width">The width (number of columns) of the framebuffer.</param>
        /// <param name="height">The height (number of rows) of the framebuffer.</param>
        /// <param name="type">The pixel type for each framebuffer unit.</param>
        public FrameBuffer2d(int width, int height, PIXELTYPE type) 
        {
            _width = width;
            _height = height;
            _typeLn = PixelTypeConverter.GetSize(type);
            PixelType = type;
            _subBuffers = (from _ in Enumerable.Range(0, height) select new FrameBuffer(width, type) ).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameBuffer2d"/> class with raw data, width, height, and pixel type.
        /// </summary>
        /// <param name="data">Raw framebuffer data.</param>
        /// <param name="width">The width of the framebuffer.</param>
        /// <param name="height">The height of the framebuffer.</param>
        /// <param name="type">The pixel type.</param>
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

        /// <summary>
        /// Gets the framebuffer row at the specified index.
        /// </summary>
        /// <param name="key">The row index.</param>
        /// <returns>The framebuffer at the specified row.</returns>
        public override FrameBuffer Get(int key) => _subBuffers[key];

        /// <summary>
        /// Clears the framebuffer with a specific color (Vector4).
        /// </summary>
        /// <param name="color">The color to clear the framebuffer with.</param>
        public void Clear(Vector4 color)
        {
            foreach(var buffer in _subBuffers) buffer.Clear(color);
        }

        /// <summary>
        /// Clears the framebuffer with a specific color (System.Drawing.Color).
        /// </summary>
        /// <param name="color">The color to clear the framebuffer with.</param>
        public void Clear(System.Drawing.Color color)
        {
            foreach (var buffer in _subBuffers) buffer.Clear(color);
        }

        /// <summary>
        /// Clears the framebuffer with a specific byte array representing a color.
        /// </summary>
        /// <param name="color">The color to clear the framebuffer with.</param>
        public void Clear(byte[] color)
        {
            foreach (var buffer in _subBuffers) buffer.Clear(color);
        }

        /// <summary>
        /// Clears the framebuffer.
        /// </summary>
        public void Clear()
        {
            foreach (var buffer in _subBuffers) buffer.Clear();
        }

        /// <summary>
        /// Creates a copy of the framebuffer.
        /// </summary>
        /// <returns>A copy of the framebuffer.</returns>
        public FrameBuffer2d Copy()
        {
            return new FrameBuffer2d(Data ,_width, _height, PixelType);
        }

        private int _typeLn;
        private int _width, _height;
        private FrameBuffer[] _subBuffers;
    }

    /// <summary>
    /// 1D framebuffer for rendering or textures.
    /// </summary>
    public class FrameBuffer : SizedSetEnum<byte[]>
    {
        /// <summary>
        /// Gets the size of the framebuffer in terms of element count.
        /// </summary>
        public override int Size => _size; // Data size in relative length (Full byte size = TypeLength * Size)

        /// <summary>
        /// Gets the raw data of the framebuffer.
        /// </summary>
        public byte[] Data => _data;

        /// <summary>
        /// Gets the size of a framebuffer unit type in bytes.
        /// </summary>
        public int TypeLength => _typeLn; // Size of Framebuffer unit type

        /// <summary>
        /// Gets the pixel type of the framebuffer.
        /// </summary>
        public PIXELTYPE PixelType { get; private set; } // Famebuffer unit type

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameBuffer"/> class.
        /// </summary>
        /// <param name="size">The number of framebuffer elements.</param>
        /// <param name="type">The pixel type for each element.</param>
        public FrameBuffer(int size, PIXELTYPE type)
        {
            _size = size;
            _typeLn = PixelTypeConverter.GetSize(type);
            PixelType = type;
            _data = new byte[size * _typeLn];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameBuffer"/> class with raw data.
        /// </summary>
        /// <param name="data">Raw framebuffer data.</param>
        /// <param name="size">The number of framebuffer elements.</param>
        /// <param name="type">The pixel type.</param>
        public FrameBuffer(byte[] data, int size, PIXELTYPE type)
        {
            _size = size;
            _typeLn = PixelTypeConverter.GetSize(type);
            PixelType = type;

            int fullSize = _size * _typeLn;

            if (data.Length >= fullSize) _data = data[0..fullSize];
            else _data = data.Concat(new byte[fullSize - data.Length]).ToArray();

        }

        /// <summary>
        /// Gets or sets the data at the specified index.
        /// </summary>
        /// <param name="key">The index of the data.</param>
        /// <returns>The data at the specified index.</returns>
        public byte[] this[int key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        /// <summary>
        /// Gets the data at the specified index.
        /// </summary>
        /// <param name="key">The index of the data.</param>
        /// <returns>The data at the specified index.</returns>
        public override byte[] Get(int key) => _data[(key * _typeLn)..(key * _typeLn + _typeLn)];

        /// <summary>
        /// Sets the data at the specified index.
        /// </summary>
        /// <param name="key">The index to set the data.</param>
        /// <param name="value">The value to set at the specified index.</param>
        public override void Set(int key, byte[] value) // Sets byte value at specific position, If larger, trims the value.
        {
            int minLn = Math.Min(_typeLn, value.Length);
            for (int i = 0; i < minLn; ++i)
            {
                _data[(key * _typeLn) + i] = value[i];
            }
        }

        /// <summary>
        /// Clears the framebuffer with a specific color (Vector4).
        /// </summary>
        /// <param name="color">The color to clear the framebuffer with.</param>
        public void Clear(Vector4 color)
        {
            byte[] clearColor = color.ToByteArray_RGBA32();
            for (int i = 0; i < Size; ++i) this[i] = clearColor;
        }

        /// <summary>
        /// Clears the framebuffer with a specific color (System.Drawing.Color).
        /// </summary>
        /// <param name="color">The color to clear the framebuffer with.</param>
        public void Clear(System.Drawing.Color color)
        {
            byte[] clearColor = color.ToByteArray();
            for (int i = 0; i < Size; ++i) this[i] = clearColor;
        }

        /// <summary>
        /// Clears the framebuffer with a specific byte array representing a color.
        /// </summary>
        /// <param name="color">The color to clear the framebuffer with.</param>
        public void Clear(byte[] color)
        {
            for (int i = 0; i < Size; ++i) this[i] = color;
        }

        /// <summary>
        /// Clears the framebuffer.
        /// </summary>
        public void Clear()
        {
            byte[] clearColor = new byte[_typeLn];
            for (int i = 0; i < Size; ++i) this[i] = clearColor;
        }

        /// <summary>
        /// Creates a copy of the framebuffer.
        /// </summary>
        /// <returns>A copy of the framebuffer.</returns>
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
