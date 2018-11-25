using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.Configs;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.Crypto.Hash;
using System.Threading;

namespace SimpleBlockchain.Net
{
    public class Network : IBroadcaster
    {
        private INetworkConfig config;

        private P2PClient client;
        private P2PServer server;

        public IAddressBook AddressBook { get; set; }
        public Blockchain Blockchain { get; set; }

        public Network(Blockchain blockchain, IAddressBook addressBook, INetworkConfig config, ISignatureProvider signer)
        {
            this.config = config;

            AddressBook = addressBook;
            Blockchain = blockchain;

            client = new P2PClient() { Signer = signer };
            server = new P2PServer
                (
                config.PeerHostName,
                config.PeerPort
                );

            server.OnBlockAccepted += (sender, eventArgs) => Blockchain?.AcceptBlock(eventArgs.Block);
            server.OnTransactionAccepted += (sender, eventArgs) => Blockchain?.AcceptTransaction(eventArgs.Transaction);
        }

        public void Start() => server.Start();

        public void Stop() => server.Stop();

        public void BroadcastBlock(Block block)
        {
            foreach (var item in AddressBook)
            {
                client.Connect(item.Value);
                Thread.Sleep(config.ClientTimeout);
                client.SendBlock(block);
                Thread.Sleep(config.ClientTimeout);
                client.Disconnect();
            }
        }

        public void BroadcastTransaction(Transaction transaction)
        {
            foreach (var item in AddressBook)
            {
                client.Connect(item.Value);
                Thread.Sleep(config.ClientTimeout);
                client.SendTransaction(transaction);
                Thread.Sleep(config.ClientTimeout);
                client.Disconnect();
            }
        }
    }
}
