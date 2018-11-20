using System;
using System.Collections.Generic;
using System.Text;
using SimpleBlockchain.BlockchainComponents;

namespace SimpleBlockchain.Net.EventArgs
{
    public class BlockAcceptEventArgs : System.EventArgs
    {
        public Block Block { get; }

        public BlockAcceptEventArgs(Block block)
        {
            Block = block;
        }
    }
}
