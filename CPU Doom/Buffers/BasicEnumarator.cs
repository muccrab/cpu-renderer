using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom.Buffers
{
    public interface ISizedEnum<TRet> : IEnumerable<TRet>
    {
        int Size { get; }
        public TRet this[int key] { get; }
    }

    public abstract class SizedEnum<TRet> : ISizedEnum<TRet>
    {
        public abstract TRet this[int key] { get; }
        public abstract int Size { get; }
        public IEnumerator<TRet> GetEnumerator() => new BasicEnumarator<SizedEnum<TRet>, TRet>(this);
        IEnumerator IEnumerable.GetEnumerator() => new BasicEnumarator<SizedEnum<TRet>, TRet>(this);
    }



    public abstract class SizedSetEnum<TRet> : SizedEnum<TRet>
    {
        public TRet this[long key] { set {
                if (key > int.MaxValue) throw new ArgumentOutOfRangeException("The index was out of bounds");
                Setter((int)key, value);
            } }

        public abstract void Setter(int key, TRet value);
    }

    public class BasicEnumarator<TEnum ,TRet> : IEnumerator<TRet> where TEnum : ISizedEnum<TRet> 
    {
        public BasicEnumarator(TEnum enumerable)
        {
            _enum = enumerable;
        }

        public TRet Current
        {
            get
            {
                if (_pos == -1) throw new InvalidOperationException("Enumerator is uninitialized");
                if (_pos >= _enum.Size) throw new InvalidOperationException("Enumerator has gone through the collection");
                return _enum[_pos];
            }
        }

        object? IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            _pos++;
            return _pos < _enum.Size;
        }

        public void Reset()
        {
            _pos = -1;
        }

        int _pos = -1;
        TEnum _enum;
    }


}
