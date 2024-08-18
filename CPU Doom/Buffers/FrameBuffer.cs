using CPU_Doom.Types;
using OpenTK.Mathematics;


namespace CPU_Doom.Buffers
{
    public class FrameBuffer2d : SizedEnum<FrameBuffer>
    {
        public int TypeLength => _typeLn;
        public PIXELTYPE PixelType { get; private set; }
        public byte[] Data { get {
                byte[] ret = new byte[TypeLength * _width * _height];
                var data = (from buffer in _subBuffers select buffer.Data);
                int i = 0;
                foreach (var dataBytes in data)
                {
                    foreach (var b in dataBytes)
                    {
                        ret[i] = (byte)b;
                        i++;
                    }
                }
                return ret;
            } } //TODO: REDO FrameBuffer2d...This is not acceptable!!!!!
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

            int fullWidth = _width * _typeLn;

            if (data.Length < fullWidth * _height) data = data.Concat(new byte[fullWidth * height - data.Length]).ToArray();

            _subBuffers = (from i in Enumerable.Range(0, height) 
                           select new FrameBuffer(
                               data[(i * fullWidth)..((i+1) * fullWidth)],
                               width, type)
                           ).ToArray();
        }
        public FrameBuffer this[int key] => Get(key);
        public override int Size => _height;
        public int RowSize => _width;

        public override FrameBuffer Get(int key) => _subBuffers[key];

        public void Clear(Vector4 color)
        {
            foreach(var buffer in _subBuffers) buffer.Clear(color);
        }

        public void Clear(System.Drawing.Color color)
        {
            foreach (var buffer in _subBuffers) buffer.Clear(color);
        }

        public void Clear()
        {
            foreach (var buffer in _subBuffers) buffer.Clear();
        }
        private int _typeLn;
        private int _width, _height;
        private FrameBuffer[] _subBuffers;
    }

    public class FrameBuffer : SizedSetEnum<byte[]>
    {
        public override int Size => _size;
        public byte[] Data => _data;
        public int TypeLength => _typeLn;
        public PIXELTYPE PixelType { get; private set; }
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
        public override void Set(int key, byte[] value)
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
