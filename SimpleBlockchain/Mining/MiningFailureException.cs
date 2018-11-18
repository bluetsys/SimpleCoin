using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;

namespace SimpleBlockchain.Mining
{
    class MiningFailureException : Exception
    {
        public Block Block { get; }

        public MiningFailureException(string message, Block block)
            : base(message)
        {
            Block = block;
        }
    }
}
