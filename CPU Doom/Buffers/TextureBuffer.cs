using CPU_Doom.Types;

namespace CPU_Doom.Buffers
{
    public enum WrapMode
    {
        CLAMP, REPEAT, REVERSE
    }

    public enum FilterMode
    {
        LINEAR, NONE
    }

    // Fuction class that is used for texture operations
    internal static class TextureBufferFunc
    {
        public static float ApplyWrap(WrapMode wrap, float value)
        {
            switch (wrap)
            {
                case WrapMode.REPEAT: return ApplyRepeat(value);
                case WrapMode.REVERSE: return ApplyReverse(value);
                default: return ApplyClamp(value);
            }
        }
        private static float ApplyRepeat(float value)
        {
            return value - MathF.Floor(value);
        }

        private static float ApplyReverse(float origValue)
        {
            float value = ApplyRepeat(origValue);
            return (MathF.Floor(origValue) % 2 == 0) ? value : 1-value;
        }
        private static float ApplyClamp(float value)
        {
            if (value > 1) value = 1;
            else if (value < 0) value = 0;
            return value;
        }
        public static bool TryApplyLinearFiler(dynamic from, dynamic to, float value, out dynamic result)
        {
            try
            {
                result = from * (1 - value) + to * (value);
                return true;
            }
            catch {
                result = 0;    
                return false; 
            }
        }
    }

    // 1D Texture Buffer (Wrapper class for FrameBuffer)
    public class TextureBuffer1d : SizedEnum<byte[]>
    {
        public override int Size => _buffer.Size;
        public TextureBuffer1d(FrameBuffer buffer) 
        {
            _buffer = buffer;
        }
        public TextureBuffer1d(byte[] data, int size, Types.PIXELTYPE type) : this(new FrameBuffer(data, size, type)) { }
        public TextureBuffer1d SetWrapMode(WrapMode wrap)
        {
            _wrap = wrap;
            return this;
        }
        public TextureBuffer1d SetFiltering(FilterMode filter)
        {
            _filter = filter;
            return this;
        }
        public byte[] this[int key] => Get(key);
        public byte[] GetPixel(float key) // Gets Pixel Wrapped and Filtered
        {
            key = TextureBufferFunc.ApplyWrap(_wrap, key);
            key *= _buffer.Size - 1;
            return GetFiltered(key);
        }
        public override byte[] Get(int key) => _buffer.Get(key);

        public TextureBuffer1d Copy() => new TextureBuffer1d(_buffer.Copy()).SetWrapMode(_wrap).SetFiltering(_filter);

        private byte[] GetFiltered(float value)
        {
            switch(_filter) 
            {
                case FilterMode.LINEAR: return ApplyLinearFiltering(value);
                default: return Get((int)value);
            }
        }
        private byte[] ApplyLinearFiltering(float value)
        {
            if ((int)value == _buffer.Size - 1) return Get((int)value);

            byte[] dataFrom = Get((int)value);
            byte[] dataTo = Get((int)value + 1);

            dynamic from = PixelTypeConverter.ConvertToPixelType(dataFrom, _buffer.PixelType);
            dynamic to   = PixelTypeConverter.ConvertToPixelType(dataTo, _buffer.PixelType);

            if (TextureBufferFunc.TryApplyLinearFiler(from, to, value - (int)value, out dynamic result))
            {
                byte[] dataResult = PixelTypeConverter.ConvertFromPixelType(result, _buffer.PixelType);
                return dataResult;
            }
            
            _filter = FilterMode.NONE;
            return Get((int)value);
        }
        FrameBuffer _buffer;
        WrapMode _wrap;
        FilterMode _filter;
    }

    // 2D Texture Buffer
    public class TextureBuffer2d : SizedEnum<FrameBuffer>
    {
        public override int Size => _buffer.Size;
        public TextureBuffer2d(FrameBuffer2d buffer)
        {
            _buffer = buffer;
        }
        public TextureBuffer2d(byte[] data, int width, int height, Types.PIXELTYPE type) : this(new FrameBuffer2d(data, width, height, type)) { }
        public TextureBuffer2d SetWrapModeHorizontal(WrapMode wrap)
        {
            _wrapHorizontal = wrap;
            return this;
        }
        public TextureBuffer2d SetWrapModeVertical(WrapMode wrap)
        {
            _wrapVertical = wrap;
            return this;
        }
        public TextureBuffer2d SetFiltering(FilterMode filter)
        {
            _filter = filter;
            return this;
        }
        public FrameBuffer this[int key] => Get(key);

        public TextureBuffer2d Copy() => new TextureBuffer2d(_buffer.Copy()).SetWrapModeHorizontal(_wrapHorizontal)
                                                                            .SetWrapModeVertical(_wrapVertical)
                                                                            .SetFiltering(_filter);

        public byte[] GetPixel(float keyX, float keyY) // Get Pixel Wrapped and Filtered
        {
            keyX = TextureBufferFunc.ApplyWrap(_wrapHorizontal,keyX);
            keyY = TextureBufferFunc.ApplyWrap(_wrapVertical, keyY);
            keyY *= _buffer.Size - 1;
            keyX *= _buffer.RowSize - 1;
            return GetFiltered(keyX, keyY);
        }
        private byte[] GetFiltered(float keyX, float keyY)
        {
            switch (_filter)
            {
                case FilterMode.LINEAR: return ApplyBilinearFiltering(keyX, keyY);
                default: return _buffer[(int)keyY][(int)keyX];
            }
        }
        private byte[] ApplyBilinearFiltering(float keyX, float keyY)
        {
            int flooredKeyX = (int)keyX;
            int flooredKeyY = (int)keyY;

            // If keys are on the top corner return without filterring
            if (flooredKeyX == _buffer.RowSize - 1 && flooredKeyY == _buffer.Size - 1) 
                return _buffer[(int)keyY][(int)keyX];
            
            // Lineary filter if the key is on the last row or column
            if (flooredKeyX == _buffer.RowSize - 1)
            {
                byte[] dataFrom = _buffer[flooredKeyY][flooredKeyX];
                byte[] dataTo = _buffer[flooredKeyY + 1][flooredKeyX];

                dynamic from = PixelTypeConverter.ConvertToPixelType(dataFrom, _buffer.PixelType);
                dynamic to = PixelTypeConverter.ConvertToPixelType(dataTo, _buffer.PixelType);

                if (TextureBufferFunc.TryApplyLinearFiler(from, to, keyY - flooredKeyY, out dynamic resultY))
                {
                    byte[] dataResult = PixelTypeConverter.ConvertFromPixelType(resultY, _buffer.PixelType);
                    return dataResult;
                }
                else BilinearFail(flooredKeyX, flooredKeyY);
            }
            else if (flooredKeyY == _buffer.Size - 1)
            {
                byte[] dataFrom = _buffer[flooredKeyY][flooredKeyX];
                byte[] dataTo = _buffer[flooredKeyY][flooredKeyX + 1];

                dynamic from = PixelTypeConverter.ConvertToPixelType(dataFrom, _buffer.PixelType);
                dynamic to = PixelTypeConverter.ConvertToPixelType(dataTo, _buffer.PixelType);

                if (TextureBufferFunc.TryApplyLinearFiler(from, to, keyX - flooredKeyX, out dynamic resultX))
                {
                    byte[] dataResult = PixelTypeConverter.ConvertFromPixelType(resultX, _buffer.PixelType);
                    return dataResult;
                }
                else BilinearFail(flooredKeyX, flooredKeyY);
            }

            // Else try apply Billinear Filtering
            byte[] libLeftData   = _buffer[flooredKeyY][flooredKeyX];             // Down-Left of a Texture
            byte[] libRightData  = _buffer[flooredKeyY][flooredKeyX + 1];         // Down-Right of a Texture
            byte[] authLeftData  = _buffer[flooredKeyY + 1][flooredKeyX];         // Up-Left of a Texture
            byte[] authRightData = _buffer[flooredKeyY + 1][flooredKeyX + 1];     // Up-Right of a Texture

            dynamic libLeft    = PixelTypeConverter.ConvertToPixelType(libLeftData, _buffer.PixelType);
            dynamic libRight   = PixelTypeConverter.ConvertToPixelType(libRightData, _buffer.PixelType);
            dynamic authLeft   = PixelTypeConverter.ConvertToPixelType(authLeftData, _buffer.PixelType);
            dynamic authRight  = PixelTypeConverter.ConvertToPixelType(authRightData, _buffer.PixelType);

            bool success = true;
            success &= TextureBufferFunc.TryApplyLinearFiler(libLeft, authLeft, keyY - flooredKeyY, out dynamic resultLeft);
            success &= TextureBufferFunc.TryApplyLinearFiler(libRight, authRight, keyY - flooredKeyY, out dynamic resultRight);
            success &= TextureBufferFunc.TryApplyLinearFiler(resultLeft, resultRight, keyX - flooredKeyX, out dynamic result);
            if (!success) BilinearFail(flooredKeyX, flooredKeyY);
            return PixelTypeConverter.ConvertFromPixelType(result, _buffer.PixelType);
        }
        private byte[] BilinearFail(int keyX, int keyY)
        {
            WindowStatic.Logger.LogWarn("Texture couldn't have been billineary filtered. Switching to no filtering");
            _filter = FilterMode.NONE;
            return _buffer[keyY][keyX];
        }
        public override FrameBuffer Get(int key) => _buffer.Get(key);

        FrameBuffer2d _buffer;
        WrapMode _wrapHorizontal, _wrapVertical;
        FilterMode _filter;
    }
}
