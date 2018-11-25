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

        public bool IsReady { get; private set; }

        public ISignatureProvider Signer { get; set; }

        public P2PClient()
        {
            IsReady = false;
        }

        private void throwIfNotConnected()
        {
            if (client.ReadyState != WebSocketState.Open)
                throw new WebException("Socket is not connected");
        }

        private void messageHandler(object sender, MessageEventArgs eventArgs)
        {
            string message = eventArgs.Data;

            switch (message)
            {
                case Commands.ServerOkResponse when IsReady == false:
                    IsReady = true;

                    break;

                case Commands.ServerFailureResponse when IsReady == false:
                    IsReady = true;

                    throw new WebException("Couldn't successfully send data to the server");

                default:
                    throw new ProtocolViolationException($"Message {message} sent when client is {(IsReady ? "ready" : "not ready")}");
            }
        }

        public void Connect(string url)
        {
            client = new WebSocket(url);

            client.Connect();
            IsReady = true;
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
