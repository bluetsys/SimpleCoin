#define DEBUG

using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;
using WebSocketSharp;
using System.Net;
using SimpleBlockchain.Crypto.Signatures;
using Newtonsoft.Json;

namespace SimpleBlockchain.Net
{
    public class P2PClient
    {
        private WebSocket client;

        public ISignatureProvider Signer { get; set; }

        private void throwIfNotConnected()
        {
            if (client.ReadyState != WebSocketState.Open)
                throw new WebException("Socket is not connected");
        }

        public void Connect(string url)
        {
            client = new WebSocket(url);

            client.Connect();
        }

        public void Disconnect() => client?.Close();

        public void SendBlock(Block block)
        {
            throwIfNotConnected();

            string blockJson = JsonConvert.SerializeObject(block);
            string message = Commands.ClientAcceptBlockRequest + " " + blockJson;

            client.Send(message);
        }

        public void SendTransaction(Transaction transaction)
        {
            throwIfNotConnected();

            string transactionJson = JsonConvert.SerializeObject(transaction);
            string message = Commands.ClientAcceptTransactionRequest + " " + transactionJson;

            client.Send(message);
        }
    }
}
