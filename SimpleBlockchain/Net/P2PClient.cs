using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;
using WebSocketSharp;
using System.Net;
using System.IO;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.Crypto.Hash;
using Newtonsoft.Json;

namespace SimpleBlockchain.Net
{
    class P2PClient
    {
        private ClientState state;
        private WebSocket client;

        public ISignatureProvider Signer { get; set; }
        public IByteConverter ByteConverter { get; set; }

        public P2PClient(ISignatureProvider signer, IByteConverter converter)
        {
            state = ClientState.NotConnected;

            Signer = signer;
            ByteConverter = converter;
        }

        private void onMessageHandler(object sender, MessageEventArgs eventArgs)
        {
            string message = eventArgs.Data;
            string[] words = message.Split(new char[] { ' ' });

            switch (words[0])
            {
                case Commands.ServerAuthRequest when state == ClientState.InitializeConnection:
                    sendAuthResponse(ByteConverter.ConvertToByteArray(words[1]));
                    state = ClientState.WaitsConnectionResponse;
                    break;

                case Commands.ServerAuthSuccessfulResponse when state == ClientState.WaitsConnectionResponse:
                    state = ClientState.Connected;
                    break;

                default:
                    throw new ProtocolViolationException($"Command {words[0]} sent during {state} state");
            }
        }

        private void sendAuthResponse(byte[] hash)
        {
            byte[] signature = Signer.SignHash(hash);

            string message = Commands.ClientAuthResponse + " " +
                             Commands.ClientPublicKeyResponse + " " + ByteConverter.ConvertToString(Signer.PublicKey) + " " +
                             Commands.ClientSignatureResponse + " " + ByteConverter.ConvertToString(signature);

            client.Send(message);
        }

        public void Connect(string url)
        {
            client = new WebSocket(url);
            client.OnMessage += onMessageHandler;
            state = ClientState.InitializeConnection;

            string message = Commands.ClientHello;

            client.Send(Encoding.Unicode.GetBytes(message));
        }

        public void Disconnect()
        {
            client?.Send(Commands.ClientQuitRequest);
            client?.Close();

            state = ClientState.NotConnected;
        }

        public void SendBlock(Block block)
        {
            if (state != ClientState.Connected)
                return;

            string blockJson = JsonConvert.SerializeObject(block);
            string message = Commands.ClientAcceptBlockRequest + " " + blockJson;

            client.Send(message);
        }

        public void SendTransaction(Transaction transaction)
        {
            if (state != ClientState.Connected)
                return;

            string transactionJson = JsonConvert.SerializeObject(transaction);
            string message = Commands.ClientAcceptTransactionRequest + " " + transactionJson;

            client.Send(message);
        }
    }
}
