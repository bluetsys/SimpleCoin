using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DriveAccessors
{
    public class BinaryDriveAccessor<T> : IDriveAccessor<T> where T : class
    {
        private Stream stream;
        private BinaryFormatter serializer;
        private bool disposed;
        private IIndexedStorage<long> addressStorage;

        public T this[int index]
        {
            get
            {
                long address = addressStorage[index];
                long currentPosition = stream.Position;

                stream.Position = address;

                T record = GetNextRecord();

                stream.Position = currentPosition;

                return record;
            }
        }

        public BinaryDriveAccessor(string path, IIndexedStorage<long> storage)
        {
            disposed = false;

            if (!File.Exists(path))
                throw new ArgumentException($"File \"{path}\" doesn't exist.");

            stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            addressStorage = storage;
            serializer = new BinaryFormatter();
        }

        public T GetNextRecord()
        {
            T nextRecord = null;

            try
            {
                nextRecord = (T)serializer.Deserialize(stream);
            }
            catch (SerializationException)
            {
                throw new InvalidDataException("Couldn't retrieve next record");
            }

            return nextRecord;
        }

        public void AddRecord(T instance)
        {
            long currentPosition = stream.Position;
            long lastRecordAddress = stream.Seek(0, SeekOrigin.End);

            serializer.Serialize(stream, instance);

            stream.Position = currentPosition;

            addressStorage.Add(lastRecordAddress);
        }

        public void Reset() => stream.Seek(0, SeekOrigin.Begin);

        public IEnumerator<T> GetEnumerator()
        {
            long enumPosition = 0;
            long currentPosition = stream.Position;

            while (true)
            {
                stream.Position = enumPosition;

                T nextRecord;

                try
                {
                    nextRecord = GetNextRecord();
                }
                catch (InvalidDataException)
                {
                    stream.Position = currentPosition;
                    break;
                }

                enumPosition = stream.Position;
                stream.Position = currentPosition;

                yield return nextRecord;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    stream.Close();

                    if (addressStorage is IDisposable storage)
                        storage.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}