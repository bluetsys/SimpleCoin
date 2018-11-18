using System;
using System.Collections.Generic;
using System.Text;

namespace DriveAccessors
{
    public interface IDriveAccessor<T> : IEnumerable<T>, IDisposable where T : class
    {
        T this[int index] { get; }

        T GetNextRecord();
        void AddRecord(T instance);
        void Reset();
    }
}
