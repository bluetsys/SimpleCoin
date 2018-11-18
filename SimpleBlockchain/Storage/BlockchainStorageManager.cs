using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.Configs;
using System.IO;
using System.Numerics;

namespace SimpleBlockchain.Storage
{
    public class BlockchainStorageManager : IBlockStorage
    {
        private bool disposed;
        private IBlockchainUnit lastUnit;

        private IBlockchainStorageManagerConfig config;
        private IUnitRepository unitRepository;
        
        public Block Last => lastUnit.Last;

        public BlockchainStorageManager(IBlockchainStorageManagerConfig config, IUnitRepository unitRepository)
        {
            disposed = false;

            this.config = config;
            this.unitRepository = unitRepository;

            lastUnit = unitRepository.Last;
        }

        public (BigInteger unitNumber, BigInteger blockNumber) AddBlock(Block block)
        {
            if (lastUnit.Count() >= config.BlocksInUnit)
            {
                IBlockchainUnit newUnit = unitRepository.CreateNewUnit();

                newUnit.AddBlock(block);

                lastUnit.Dispose();
                lastUnit = newUnit;
            }
            else
                lastUnit.AddBlock(block);

            return (unitRepository.Count - 1, lastUnit.Count() - 1);
        }

        public IEnumerator<Block> GetEnumerator()
        {
            IEnumerable<Block> blocks;

            foreach (IBlockStorage unit in unitRepository.GetUnitsWithoutLast())
            {
                blocks = unit.ToArray();
                unit.Dispose();

                foreach (Block block in blocks)
                    yield return block;
            }

            foreach (Block block in lastUnit)
                yield return block;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    unitRepository.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose() => Dispose(true);

        public IEnumerable<Block> GetBlocksReverse()
        {
            foreach (Block block in lastUnit.GetBlocksReverse())
                yield return block;

            IEnumerable<Block> blocks;

            foreach (IBlockStorage unit in unitRepository.GetUnitsReverseWithoutLast())
            {
                blocks = unit.ToArray();
                unit.Dispose();

                foreach (Block block in blocks.Reverse())
                    yield return block;
            }
        }
    }
}
