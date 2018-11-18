using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using System.Numerics;

namespace SimpleBlockchain.Storage
{
    public interface IBlockStorage : IEnumerable<Block>, IDisposable
    {
        Block Last { get; }

        (BigInteger unitNumber, BigInteger blockNumber) AddBlock(Block block);
        IEnumerable<Block> GetBlocksReverse();
    }
}
