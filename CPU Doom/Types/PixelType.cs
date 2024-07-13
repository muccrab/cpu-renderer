using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom.Types
{
    public enum PIXELTYPE
    {
        BYTE = sizeof(byte), SHORT = sizeof(short), INT = sizeof(int), FLOAT = sizeof(float), DOUBLE = sizeof(double),
        RGB24 = 3 * BYTE, RGBA32 = 4 * BYTE,
    }

    public static class PixelTypeConverter
    {
        public static byte ToByte(this byte[] bytes) => bytes[0];
        public static short ToShort(this byte[] bytes) => BitConverter.ToInt16(bytes, 0);
        public static int ToInt(this byte[] bytes) => BitConverter.ToInt32(bytes, 0);
        public static float ToFloat(this byte[] bytes) => BitConverter.ToSingle(bytes, 0);
        public static double ToDouble(this byte[] bytes) => BitConverter.ToDouble(bytes, 0);
        public static Vector4 ToRGBA32_Float(this byte[] bytes)
        {
            if (bytes.Length < 4) bytes.ExpandBy(4 - bytes.Length);
            return new Vector4(bytes[0] / 255f,
                               bytes[1] / 255f,
                               bytes[2] / 255f,
                               bytes[3] / 255f);
        }
        public static Vector3 ToRGB24_Float(this byte[] bytes)
        {
            if (bytes.Length < 3) bytes.ExpandBy(3 - bytes.Length);
            return new Vector3(bytes[0] / 255f,
                               bytes[1] / 255f,
                               bytes[2] / 255f);
        }


        public static byte[] ToByteArray(this byte value) => new byte[] { value };
        public static byte[] ToByteArray(this short value) => BitConverter.GetBytes(value);
        public static byte[] ToByteArray(this int value) => BitConverter.GetBytes(value);
        public static byte[] ToByteArray(this float value) => BitConverter.GetBytes(value);
        public static byte[] ToByteArray(this double value) => BitConverter.GetBytes(value);

        public static byte[] ToByteArray(this float[] valueArray) {
            int floatLn = (int)PIXELTYPE.FLOAT;
            byte[] bytes = new byte[valueArray.Length * floatLn];
            for (int i = 0; i < valueArray.Length; i++)
            {
                byte[] valueByte = BitConverter.GetBytes(valueArray[i]);
                for (int j = 0; j < floatLn; j++) 
                    bytes[i * floatLn + j] = valueByte[j];
            }
            return bytes;
        }



        public static byte[] ToByteArray(this System.Drawing.Color color) => new byte[] { color.R, color.G, color.B, color.A };
        public static Vector4 ToVectorForm(this System.Drawing.Color color) => new Vector4( color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f );
        public static byte[] ToByteArray_RGBA32(this Vector4 value)
        {
            value = Vector4.Clamp(value, Vector4.Zero, Vector4.One);
            value *= 255;
            return new byte[]
            {
                (byte)value.X, (byte)value.Y, (byte)value.Z, (byte)value.W
            };
        }

        public static byte[] ToByteArray_RGB24(this Vector3 value)
        {
            value = Vector3.Clamp(value, Vector3.Zero, Vector3.One);
            value *= 255;
            return new byte[]
            {
                (byte)value.X, (byte)value.Y, (byte)value.Z
            };
        }

        public static byte[] ExpandBy(this byte[] bytes, int value)
        {
            if (value == 0) return bytes;
            byte[] ret = new byte[bytes.Length + value];
            for ( int i = 0; i < bytes.Length; i++ )
            {
                ret[i] = bytes[i];
            }
            return ret;
        }


    }



}
