using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.Crypto.Hash;

namespace SimpleBlockchain.Mining
{
    public interface IMiner
    {
        int HashAlgorithmId { get; }
        int Difficulty { get; }

        IHashFactory HashFactory { get; }

        void MineBlock(Block block);
    }
}
