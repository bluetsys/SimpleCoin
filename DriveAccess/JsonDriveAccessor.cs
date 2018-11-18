using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace DriveAccessors
{
    public class JsonDriveAccessor<T> : IDriveAccessor<T> where T : class
    {
        private Stream stream;
        private StreamWriter writer;
        private BinaryReader reader;
        private bool disposed;
        private IIndexedStorage<long> addressStorage;

        public JsonDriveAccessor(string path, IIndexedStorage<long> storage)
        {
            disposed = false;

            if (!File.Exists(path))
                throw new ArgumentException($"File \"{path}\" doesn't exist.");

            stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            addressStorage = storage;

            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            reader = new BinaryReader(stream);
        }

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

        public void AddRecord(T instance)
        {
            long currentPosition = stream.Position;
            long lastRecordAddress = stream.Seek(0, SeekOrigin.End);
            string jsonData = JsonConvert.SerializeObject(instance);

            writer.WriteLine(jsonData);

            stream.Position = currentPosition;

            addressStorage.Add(lastRecordAddress);
        }

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
                catch (EndOfStreamException)
                {
                    stream.Position = currentPosition;
                    break;
                }

                enumPosition = stream.Position;
                stream.Position = currentPosition;

                yield return nextRecord;
            }
        }

        public T GetNextRecord()
        {
            T nextRecord = null;

            try
            {
                List<char> chars = new List<char>();
                char newChar;

                do
                {
                    newChar = reader.ReadChar();
                    chars.Add(newChar);
                }
                while (newChar != '\n');

                string jsonData = new string(chars.ToArray());

                nextRecord = JsonConvert.DeserializeObject<T>(jsonData);
            }
            catch (EndOfStreamException)
            {
                throw new EndOfStreamException("End of stream reached");
            }
            catch (SerializationException)
            {
                throw new InvalidDataException("Couldn't retrieve next record");
            }

            return nextRecord;
        }

        public void Reset() => stream.Seek(0, SeekOrigin.Begin);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
