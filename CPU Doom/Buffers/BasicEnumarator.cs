using System.Collections;

namespace CPU_Doom.Buffers
{
    /// <summary>
    /// Interface for an enumerable that can return elements by key and has a size.
    /// </summary>
    /// <typeparam name="TRet">The type of elements in the enumerable.</typeparam>
    public interface ISizedEnum<TRet> : IEnumerable<TRet>
    {
        /// <summary>
        /// Gets the size of the enumerable.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Returns an element by its key.
        /// </summary>
        /// <param name="key">The key or index of the element to retrieve.</param>
        /// <returns>The element at the specified key.</returns>
        public TRet Get(int key);
    }

    /// <summary>
    /// Interface for an enumerable that can set elements by key.
    /// </summary>
    /// <typeparam name="TRet">The type of elements in the enumerable.</typeparam>
    public interface ISizedSetEnum<TRet> : IEnumerable<TRet>
    {
        /// <summary>
        /// Sets the value of an element at the specified key.
        /// </summary>
        /// <param name="key">The key or index of the element to set.</param>
        /// <param name="value">The value to set at the specified key.</param>
        public void Set(int key, TRet value);
    }

    /// <summary>
    /// Abstract class implementing a sized enumerable. Size is retrieved in constant time.
    /// </summary>
    /// <typeparam name="TRet">The type of elements in the enumerable.</typeparam> 
    public abstract class SizedEnum<TRet> : ISizedEnum<TRet>
    {
        /// <summary>
        /// Gets the size of the enumerable.
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// Retrieves an element by its key.
        /// </summary>
        /// <param name="key">The key or index of the element to retrieve.</param>
        /// <returns>The element at the specified key.</returns>
        public abstract TRet Get(int key);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator for the enumerable collection.</returns>
        public IEnumerator<TRet> GetEnumerator() => new BasicEnumarator<SizedEnum<TRet>, TRet>(this);
        IEnumerator IEnumerable.GetEnumerator() => new BasicEnumarator<SizedEnum<TRet>, TRet>(this);
    }

    /// <summary>
    /// Abstract class implementing a sized enumerable that can also set elements.
    /// </summary>
    /// <typeparam name="TRet">The type of elements in the enumerable.</typeparam>
    public abstract class SizedSetEnum<TRet> : SizedEnum<TRet>, ISizedSetEnum<TRet>
    {
        /// <summary>
        /// Sets the value of an element at the specified key.
        /// </summary>
        /// <param name="key">The key or index of the element to set.</param>
        /// <param name="value">The value to set at the specified key.</param>
        public abstract void Set(int key, TRet value);
    }

    /// <summary>
    /// Enumerator for enumerating through a sized enumerable.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumerable being enumerated.</typeparam>
    /// <typeparam name="TRet">The type of elements in the enumerable.</typeparam>
    public class BasicEnumarator<TEnum ,TRet> : IEnumerator<TRet> where TEnum : ISizedEnum<TRet> 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicEnumarator{TEnum, TRet}"/> class.
        /// </summary>
        /// <param name="enumerable">The enumerable to enumerate over.</param>
        public BasicEnumarator(TEnum enumerable)
        {
            _enum = enumerable;
        }

        /// <summary>
        /// Gets the current element in the enumerable.
        /// </summary>
        public TRet Current
        {
            get
            {
                if (_pos == -1) throw new InvalidOperationException("Enumerator is uninitialized");
                if (_pos >= _enum.Size) throw new InvalidOperationException("Enumerator has gone through the collection");
                return _enum.Get(_pos); 
            }
        }
        object? IEnumerator.Current => Current;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Advances the enumerator to the next element in the collection.
        /// </summary>
        /// <returns><c>true</c> if the enumerator successfully advanced to the next element; otherwise, <c>false</c>.</returns>
        public bool MoveNext()
        {
            _pos++;
            return _pos < _enum.Size;
        }

        /// <summary>
        /// Resets the enumerator to its initial position.
        /// </summary>
        public void Reset()
        {
            _pos = -1;
        }

        int _pos = -1; // Position of the current Enumerator in the enumerable
        TEnum _enum; // Enumerable of the Enumerator
    }
}
