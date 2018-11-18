using System.Collections.Generic;

namespace DriveAccessors
{
    public interface IIndexedStorage<T> : IEnumerable<T>
    {
        T this[int index] { get; }

        void Add(T item);
    }
}