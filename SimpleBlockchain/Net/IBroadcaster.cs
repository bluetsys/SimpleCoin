using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;

namespace SimpleBlockchain.Net
{
    public interface IBroadcaster
    {
        void BroadcastBlock(Block block);
        void BroadcastTransaction(Transaction transaction);
        void Start();
        void Stop();
    }
}
