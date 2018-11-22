#define DEBUG

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
    public class P2PClient
    {
        private WebSocket client;

        public ClientState ClientState { get; private set; }

        public ISignatureProvider Signer { get; set; }
        public IByteConverter ByteConverter { get; set; }

        public P2PClient()
        {
            ClientState = ClientState.NotConnected;
        }

        private void onMessageHandler(object sender, MessageEventArgs eventArgs)
        {
            string message = eventArgs.Data;
            string[] words = message.Split(new char[] { ' ' });

            switch (words[0])
            {
                case Commands.ServerAuthRequest when ClientState == ClientState.InitializeConnection:
                    sendAuthResponse(ByteConverter.ConvertToByteArray(words[1]));
                    ClientState = ClientState.WaitsConnectionResponse;

                    break;

                case Commands.ServerAuthSuccessfulResponse when ClientState == ClientState.WaitsConnectionResponse:
                    ClientState = ClientState.Connected;

                    break;

                case Commands.ServerBusyResponse:
                    throw new WebException("Connection refused by endpoint; it is busy");

                case Commands.ServerProtocolViolationResponse:
                    throw new ProtocolViolationException($"Remote endpoint caused a protocol violation exception");

                default:
                    throw new ProtocolViolationException($"Command {words[0]} sent during {ClientState} state");
            }
        }

        private void sendAuthResponse(byte[] hash)
        {
            byte[] signature = Signer.SignHash(hash);

            string message = Commands.ClientAuthResponse + " " +
                             Commands.ClientPublicKeyResponse + " " + ByteConverter.ConvertToString(Signer.PublicKey) + " " +
                             Commands.ClientSignatureResponse + " " + ByteConverter.ConvertToString(signature);

#if (DEBUG)
            int length = message.Length;
            int publicKeyLength = ByteConverter.ConvertToString(Signer.PublicKey).Length;
            int signatureLength = ByteConverter.ConvertToString(signature).Length;
#endif

            client.Send(message);
        }

        public void Connect(string url)
        {
            client = new WebSocket(url);
            client.OnMessage += onMessageHandler;
            ClientState = ClientState.InitializeConnection;

            client.Connect();

            string message = Commands.ClientHello;

            client.Send(message);
        }

        public void Disconnect()
        {
            string message = Commands.ClientQuitRequest;

            client?.Send(message);
            client?.Close();

            ClientState = ClientState.NotConnected;
        }

        public void SendBlock(Block block)
        {
            if (ClientState != ClientState.Connected)
                return;

            string blockJson = JsonConvert.SerializeObject(block);
            string message = Commands.ClientAcceptBlockRequest + " " + blockJson;

            client.Send(message);
        }

        public void SendTransaction(Transaction transaction)
        {
            if (ClientState != ClientState.Connected)
                return;

            string transactionJson = JsonConvert.SerializeObject(transaction);
            string message = Commands.ClientAcceptTransactionRequest + " " + transactionJson;

            client.Send(message);
        }
    }
}
