using CPU_Doom.Buffers;
using CPU_Doom.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CPU_Doom_File_Loader
{
    public static class TextureLoader
    {
        private static Dictionary<string, TextureBuffer2d> _loadedTextureCache = new Dictionary<string, TextureBuffer2d>();
        public static TextureBuffer2d Load2dTextureFromFile(string path)
        {
            if (_loadedTextureCache.ContainsKey(path)) return _loadedTextureCache[path];
            var image = Image.Load<Rgba32>(path);
            byte[] imageArray = new byte[image.Width * image.Height * 4];
            var bytes = new Span<byte>(imageArray);
            image.CopyPixelDataTo(bytes);
            FrameBuffer2d buffer = new FrameBuffer2d(imageArray, image.Width, image.Height, PIXELTYPE.RGBA32);
            var texture = new TextureBuffer2d(buffer);
            _loadedTextureCache[path] = texture;
            return texture;
        }

        public static TextureBuffer2d Load2dTextureFromFile_New(string path) => Load2dTextureFromFile(path).Copy();

    }
}
