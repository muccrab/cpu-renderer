using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.InteropServices;


namespace CPU_Doom.Types
{
    public enum PIXELTYPE
    {
        BYTE, SHORT, INT, FLOAT, DOUBLE,
        RGB24, RGBA32,
    }

    public static class PixelTypeConverter
    {
        public static int GetSize(PIXELTYPE pixelType)
        {
            switch (pixelType)
            {
                case PIXELTYPE.BYTE:   return sizeof(byte);
                case PIXELTYPE.SHORT:  return sizeof(short);
                case PIXELTYPE.INT:    return sizeof(int);
                case PIXELTYPE.FLOAT:  return sizeof(float);
                case PIXELTYPE.DOUBLE: return sizeof(double);
                case PIXELTYPE.RGB24:  return sizeof(byte) * 3;
                case PIXELTYPE.RGBA32: return sizeof(byte) * 4;
                default:               return 0;
            }
        }
        public static object ConvertToPixelType(byte[] data, PIXELTYPE pixelType)
        {
            switch (pixelType)
            {
                case PIXELTYPE.BYTE:   return data.ToByte();
                case PIXELTYPE.SHORT:  return data.ToShort();
                case PIXELTYPE.INT:    return data.ToInt();
                case PIXELTYPE.FLOAT:  return data.ToFloat();
                case PIXELTYPE.DOUBLE: return data.ToDouble();
                case PIXELTYPE.RGB24:  return data.ToRGB24_Float();
                case PIXELTYPE.RGBA32: return data.ToRGBA32_Float();
                default: return data;
            }
        }
        public static byte[] ConvertFromPixelType(object data, PIXELTYPE pixelType)
        {
            switch (pixelType)
            {
                case PIXELTYPE.BYTE:   if (data.GetType() == typeof(byte))    return ToByteArray((byte)data);          else break;
                case PIXELTYPE.SHORT:  if (data.GetType() == typeof(short))   return ToByteArray((short)data);         else break;
                case PIXELTYPE.INT:    if (data.GetType() == typeof(int))     return ToByteArray((int)data);           else break;
                case PIXELTYPE.FLOAT:  if (data.GetType() == typeof(float))   return ToByteArray((float)data);         else break;
                case PIXELTYPE.DOUBLE: if (data.GetType() == typeof(double))  return ToByteArray((double)data);        else break;
                case PIXELTYPE.RGB24:  if (data.GetType() == typeof(Vector3)) return ToByteArray_RGB24((Vector3)data); else break;
                case PIXELTYPE.RGBA32: if (data.GetType() == typeof(Vector4)) return ToByteArray_RGBA32((Vector4)data); else break;
            }
            return GetBytesFromStruct(data);
        }
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
            int floatLn = GetSize(PIXELTYPE.FLOAT);
            byte[] bytes = new byte[valueArray.Length * floatLn];
            for (int i = 0; i < valueArray.Length; i++)
            {
                byte[] valueByte = BitConverter.GetBytes(valueArray[i]);
                for (int j = 0; j < floatLn; j++) 
                    bytes[i * floatLn + j] = valueByte[j];
            }
            return bytes;
        }
        public static byte[] ToByteArray(this int[] valueArray)
        {
            int intLn = GetSize(PIXELTYPE.INT);
            byte[] bytes = new byte[valueArray.Length * intLn];
            for (int i = 0; i < valueArray.Length; i++)
            {
                byte[] valueByte = BitConverter.GetBytes(valueArray[i]);
                for (int j = 0; j < intLn; j++)
                    bytes[i * intLn + j] = valueByte[j];
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
        public static byte[] GetBytesFromStruct(object boxedStruct)
        {
            if (boxedStruct == null)
                throw new ArgumentNullException(nameof(boxedStruct));

            // Pin the object in memory
            GCHandle handle = GCHandle.Alloc(boxedStruct, GCHandleType.Pinned);
            try
            {
                IntPtr pointer = handle.AddrOfPinnedObject();
                int size = Marshal.SizeOf(boxedStruct.GetType());

                byte[] bytes = new byte[size];
                Marshal.Copy(pointer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                handle.Free();
            }
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
        public static T AssignByteArrayToValue<T>(this byte[] data) where T : struct
        {
            Type valueType = typeof(T);
            int valueSize = Marshal.SizeOf(valueType);
            byte[] buffer = new byte[valueSize];
            int lengthToCopy = Math.Min(data.Length, valueSize);
            Array.Copy(data, buffer, lengthToCopy);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            object? value = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), valueType);
            handle.Free();
            if (value == null) throw new Exception("Unable to Assign Byte Array to field");
            return (T)value;
        }
        public static void AssignByteArrayToField(this FieldInfo field, object obj, byte[] data, int fieldSize = -1)
        {
            Type fieldType = field.FieldType;
            if (fieldSize < 0) fieldSize = Marshal.SizeOf(fieldType);
            byte[] buffer = new byte[fieldSize];
            int lengthToCopy = Math.Min(data.Length, fieldSize);
            Array.Copy(data, buffer, lengthToCopy);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                object? fieldValue = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), fieldType);
                field.SetValue(obj, fieldValue);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
