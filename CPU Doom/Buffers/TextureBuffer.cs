using CPU_Doom.Types;

namespace CPU_Doom.Buffers
{
    /// <summary>
    /// Enum for defining wrap mode behaviors (CLAMP, REPEAT, REVERSE).
    /// </summary>
    public enum WrapMode
    {
        CLAMP, REPEAT, REVERSE
    }

    /// <summary>
    /// Enum for defining filter mode behaviors (LINEAR, NONE).
    /// </summary>
    public enum FilterMode
    {
        LINEAR, NONE
    }

    /// <summary>
    /// Helper functions for texture operations like wrapping and filtering.
    /// </summary>
    internal static class TextureBufferFunc
    {
        /// <summary>
        /// Applies the selected wrap mode (CLAMP, REPEAT, or REVERSE) to a given value.
        /// </summary>
        /// <param name="wrap">Wrap mode to apply.</param>
        /// <param name="value">Value to wrap.</param>
        /// <returns>Wrapped value.</returns>
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
        /// <summary>
        /// Tries to apply linear filtering between two values.
        /// </summary>
        /// <param name="from">Start value.</param>
        /// <param name="to">End value.</param>
        /// <param name="value">Interpolation factor.</param>
        /// <param name="result">The result of the interpolation.</param>
        /// <returns>True if filtering succeeded, false otherwise.</returns>
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

    /// <summary>
    /// 1D texture buffer for handling texture data with optional wrapping and filtering.
    /// </summary>
    public class TextureBuffer1d : SizedEnum<byte[]>
    {
        /// <summary>
        /// Gets the size of the texture buffer.
        /// </summary>
        public override int Size => _buffer.Size;

        /// <summary>
        /// Initializes a new 1D texture buffer using a frame buffer.
        /// </summary>
        /// <param name="buffer">The frame buffer to use.</param>
        public TextureBuffer1d(FrameBuffer buffer) 
        {
            _buffer = buffer;
        }

        /// <summary>
        /// Initializes a new 2D texture buffer using a frame buffer.
        /// </summary>
        /// <param name="data">The pixel data of the texture</param>
        /// <param name="size">The size of the texture</param>
        /// <param name="type">The pixel type</param>
        public TextureBuffer1d(byte[] data, int size, Types.PIXELTYPE type) : this(new FrameBuffer(data, size, type)) { }

        /// <summary>
        /// Sets the wrap mode for the texture buffer.
        /// </summary>
        /// <param name="wrap">The wrap mode to use.</param>
        /// <returns>The texture buffer with updated wrap mode.</returns>
        public TextureBuffer1d SetWrapMode(WrapMode wrap)
        {
            _wrap = wrap;
            return this;
        }

        /// <summary>
        /// Sets the filtering mode for the texture buffer.
        /// </summary>
        /// <param name="filter">The filter mode to use.</param>
        /// <returns>The texture buffer with updated filter mode.</returns>
        public TextureBuffer1d SetFiltering(FilterMode filter)
        {
            _filter = filter;
            return this;
        }

        /// <summary>
        /// Gets a pixel value based on key.
        /// </summary>
        /// <param name="key">The key to fetch the pixel.</param>
        /// <returns>The raw pixel data.</returns>
        public byte[] this[int key] => Get(key);

        /// <summary>
        /// Gets a wrapped and filtered pixel value based on key.
        /// </summary>
        /// <param name="key">The key to fetch the pixel.</param>
        /// <returns>The filtered pixel data.</returns>
        public byte[] GetPixel(float key) // Gets Pixel Wrapped and Filtered
        {
            key = TextureBufferFunc.ApplyWrap(_wrap, key);
            key *= _buffer.Size - 1;
            return GetFiltered(key);
        }

        /// <summary>
        /// Gets a pixel value based on key.
        /// </summary>
        /// <param name="key">The key to fetch the pixel.</param>
        /// <returns>The raw pixel data.</returns>
        public override byte[] Get(int key) => _buffer.Get(key);

        /// <summary>
        /// Copies the current texture buffer, preserving its settings.
        /// </summary>
        /// <returns>A copy of the current texture buffer.</returns>
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

    /// <summary>
    /// 2D texture buffer for handling texture data with optional wrapping and filtering.
    /// </summary>
    public class TextureBuffer2d : SizedEnum<FrameBuffer>
    {
        /// <summary>
        /// Gets the size of the 2D texture buffer.
        /// </summary>
        public override int Size => _buffer.Size;

        /// <summary>
        /// Initializes a new 2D texture buffer using a frame buffer.
        /// </summary>
        /// <param name="buffer">The 2D frame buffer to use.</param>
        public TextureBuffer2d(FrameBuffer2d buffer)
        {
            _buffer = buffer;
        }

        /// <summary>
        /// Initializes a new 2D texture buffer using a frame buffer.
        /// </summary>
        /// <param name="data">The pixel data of the texture</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height of the texture</param>
        /// <param name="type">The pixel type</param>
        public TextureBuffer2d(byte[] data, int width, int height, Types.PIXELTYPE type) : this(new FrameBuffer2d(data, width, height, type)) { }

        /// <summary>
        /// Sets the horizontal wrap mode for the texture buffer.
        /// </summary>
        /// <param name="wrap">The horizontal wrap mode to use.</param>
        /// <returns>The texture buffer with updated wrap mode.</returns>
        public TextureBuffer2d SetWrapModeHorizontal(WrapMode wrap)
        {
            _wrapHorizontal = wrap;
            return this;
        }

        /// <summary>
        /// Sets the vertical wrap mode for the texture buffer.
        /// </summary>
        /// <param name="wrap">The vertical wrap mode to use.</param>
        /// <returns>The texture buffer with updated wrap mode.</returns>
        public TextureBuffer2d SetWrapModeVertical(WrapMode wrap)
        {
            _wrapVertical = wrap;
            return this;
        }

        /// <summary>
        /// Sets the filtering mode for the texture buffer.
        /// </summary>
        /// <param name="filter">The filter mode to use.</param>
        /// <returns>The texture buffer with updated filter mode.</returns>
        public TextureBuffer2d SetFiltering(FilterMode filter)
        {
            _filter = filter;
            return this;
        }

        /// <summary>
        /// Gets a Framebuffer with row data of the specified key.
        /// </summary>
        /// <param name="key">The key to fetch the Framebufer.</param>
        /// <returns>The Framebuffer with row data</returns>
        public FrameBuffer this[int key] => Get(key);

        /// <summary>
        /// Gets a Framebuffer with row data of the specified key.
        /// </summary>
        /// <param name="key">The key to fetch the Framebufer.</param>
        /// <returns>The Framebuffer with row data</returns>
        public override FrameBuffer Get(int key) => _buffer.Get(key);

        /// <summary>
        /// Copies the current 2D texture buffer, preserving its settings.
        /// </summary>
        /// <returns>A copy of the current 2D texture buffer.</returns>
        public TextureBuffer2d Copy() => new TextureBuffer2d(_buffer.Copy()).SetWrapModeHorizontal(_wrapHorizontal)
                                                                            .SetWrapModeVertical(_wrapVertical)
                                                                            .SetFiltering(_filter);

        /// <summary>
        /// Gets a wrapped and filtered pixel value based on X and Y coordinates.
        /// </summary>
        /// <param name="keyX">The X coordinate.</param>
        /// <param name="keyY">The Y coordinate.</param>
        /// <returns>The filtered pixel data.</returns>
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


        FrameBuffer2d _buffer;
        WrapMode _wrapHorizontal, _wrapVertical;
        FilterMode _filter;
    }
}
