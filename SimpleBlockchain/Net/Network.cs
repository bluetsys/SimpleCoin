using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;

namespace SimpleBlockchain.Net
{
    public class Network : IBroadcaster
    {
        public Blockchain Receiver { get; set; }

        public Network(Blockchain receiver)
        {
            Receiver = receiver;
        }

        public void BroadcastBlock(Block block) => Receiver.AcceptBlock(block);

        public void BroadcastTransaction(Transaction transaction) => Receiver.AcceptTransaction(transaction);
    }
}
