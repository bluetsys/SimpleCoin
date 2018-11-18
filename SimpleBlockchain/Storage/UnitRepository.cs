using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DriveAccessors;
using SimpleBlockchain.Configs;
using SimpleBlockchain.BlockchainComponents;
using System.Collections;
using System.Numerics;

namespace SimpleBlockchain.Storage
{
    public class UnitRepository : IUnitRepository, IEnumerable<IBlockchainUnit>
    {
        private bool disposed;
        
        private IUnitRepositoryConfig config;

        public BigInteger Count { get; private set; }
        public BigInteger GlobalCount { get; private set; }

        public IBlockchainUnit Last => loadUnitById(Count - 1);

        public UnitRepository(IUnitRepositoryConfig config)
        {
            disposed = false;

            this.config = config;

            Count = getCount();
            GlobalCount = getGlobalCount();
        }

        private BigInteger getCount() => Directory.EnumerateFiles(Path.Combine(config.DirectoryPath, "units")).Count();

        private BigInteger getGlobalCount()
        {
            string lastFile = Directory.EnumerateFiles(Path.Combine(config.DirectoryPath, "units")).Last();

            lastFile = Path.GetFileNameWithoutExtension(lastFile);

            string lastFileNumber = lastFile.Remove(0, 4);

            return BigInteger.Parse(lastFileNumber) + 1;
        }

        private IBlockchainUnit loadUnit(string unitPath, string addresserPath)
        {
            IIndexedStorage<long> addressStorage = new AddressStorage(addresserPath);
            BlockchainUnit unit = new BlockchainUnit(unitPath, addressStorage);

            return unit;
        }

        private IBlockchainUnit loadUnitById(BigInteger id)
        {
            string unitPath = Path.Combine(config.DirectoryPath, "units", $"unit{id}.unit");
            string addresserPath = Path.Combine(config.DirectoryPath, "addressers", $"addresser{id}.addr");

            return loadUnit(unitPath, addresserPath);
        }

        public IBlockchainUnit CreateNewUnit()
        {
            string addresserPath = Path.Combine(config.DirectoryPath, "addressers", $"addresser{GlobalCount}.addr");
            string unitPath = Path.Combine(config.DirectoryPath, "units", $"unit{GlobalCount}.unit");

            File.Create(addresserPath).Close();
            File.Create(unitPath).Close();

            IBlockchainUnit unit = loadUnit(unitPath, addresserPath);

            Count++;
            GlobalCount++;

            return unit;
        }

        public IEnumerable<IBlockchainUnit> GetUnits()
        {
            for (BigInteger i = GlobalCount - Count; i < GlobalCount; i++)
            {
                IBlockchainUnit unit = loadUnitById(i);

                yield return unit;
            }
        }

        public IEnumerable<IBlockchainUnit> GetUnitsWithoutLast()
        {
            for (BigInteger i = GlobalCount - Count; i < GlobalCount - 1; i++)
            {
                IBlockchainUnit unit = loadUnitById(i);

                yield return unit;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IEnumerator<IBlockchainUnit> GetEnumerator() => GetUnits().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<IBlockchainUnit> GetUnitsReverse()
        {
            for (BigInteger i = GlobalCount - 1; i >= GlobalCount - Count; i--)
            {
                IBlockchainUnit unit = loadUnitById(i);

                yield return unit;
            }
        }

        public IEnumerable<IBlockchainUnit> GetUnitsReverseWithoutLast()
        {
            for (BigInteger i = GlobalCount - 2; i >= GlobalCount - Count; i--)
            {
                IBlockchainUnit unit = loadUnitById(i);

                yield return unit;
            }
        }
    }
}
