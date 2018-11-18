using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DriveAccessors;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.Crypto.Hash;
using MerkleTree;
using SHA3;
using System.Collections;
using System.Numerics;

namespace SimpleBlockchain.Storage
{
    public partial class BlockchainUnit : IBlockchainUnit
    {
        private bool disposed;

        private JsonDriveAccessor<Block> blockStorage;
        private MerkleTreeBuilder merkleRootComputer;
        private SHA3Managed digest;

        public Block Last => blockStorage.Last();

        public BlockchainUnit(string path, IIndexedStorage<long> addressStorage)
        {
            disposed = false;

            blockStorage = new JsonDriveAccessor<Block>(path, addressStorage);
            merkleRootComputer = new MerkleTreeBuilder();
            digest = new SHA3Managed(512);
        }

        public (BigInteger unitNumber, BigInteger blockNumber) AddBlock(Block block)
        {
            blockStorage.AddRecord(block);

            return (0, this.Count() - 1);
        }

        // TODO: Compute hash with outer merkle root method.
        public byte[] GetUnitHash()
        {
            IEnumerable<byte[]> blockHashes = blockStorage.Select(block => block.Hash);
            byte[] unitHash = merkleRootComputer.GetMerkleRoot(blockHashes, digest);

            return unitHash;
        }

        public IEnumerator<Block> GetEnumerator() => blockStorage.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    blockStorage.Dispose();
                }

                disposed = true;
            }
        }
        
        public void Dispose() => Dispose(true);

        public IEnumerable<Block> GetBlocksReverse() => this.Reverse();
    }
}
