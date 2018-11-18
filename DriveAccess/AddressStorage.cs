using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;

namespace DriveAccessors
{
    public class AddressStorage : IIndexedStorage<long>, IDisposable
    {
        private Stream stream;
        private BinaryFormatter serializer;
        private int? lastIndex;
        private bool disposed;

        public long this[int index]
        {
            get
            {
                if (lastIndex == null)
                    throw new InvalidOperationException("There are no stored elements");

                if (index > lastIndex)
                    throw new IndexOutOfRangeException($"There are no elements stored by index {index}");

                if (index < 0)
                    throw new IndexOutOfRangeException($"Index value must be non-negative");

                long currentPosition = stream.Position;
                long result;

                stream.Seek(0, SeekOrigin.Begin);

                do
                {
                    result = (long)serializer.Deserialize(stream);
                    index--;
                }
                while (index >= 0);

                stream.Position = currentPosition;

                return result;
            }
        }

        public AddressStorage(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"File \"{path}\" doesn't exist.");

            stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            serializer = new BinaryFormatter();
            disposed = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    stream.Close();
                }

                disposed = true;
            }
        }

        public void Add(long item)
        {
            serializer.Serialize(stream, item);

            lastIndex = lastIndex == null ? 0 : lastIndex + 1;
        }

        public IEnumerator<long> GetEnumerator()
        {
            long enumPosition = 0;
            long currentPosition = stream.Position;

            while (true)
            {
                stream.Position = enumPosition;

                long nextAddress;

                try
                {
                    nextAddress = (long)serializer.Deserialize(stream);
                }
                catch (SerializationException)
                {
                    stream.Position = currentPosition;
                    break;
                }

                enumPosition = stream.Position;
                stream.Position = currentPosition;

                yield return nextAddress;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() => Dispose(true);
    }
}