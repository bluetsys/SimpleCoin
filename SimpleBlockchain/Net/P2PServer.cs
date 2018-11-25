#define DEBUG

using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Security.Cryptography;
using SHA3;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.Configs.Parameters;
using Newtonsoft.Json;
using SimpleBlockchain.Net.EventArgs;
using System.IO;
using System.Threading.Tasks;

namespace SimpleBlockchain.Net
{
    public class P2PServer : WebSocketBehavior
    {
        private WebSocketServer server;

        public string HostName { get; set; }
        public int Port { get; set; }

        public event EventHandler<BlockAcceptEventArgs> OnBlockAccepted;
        public event EventHandler<TransactionAcceptEventArgs> OnTransactionAccepted;

        public P2PServer(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        public void Start()
        {
            server = new WebSocketServer("ws://" + HostName + $":{Port}");
            
            server.AddWebSocketService("/simplecoin", () =>
            {
                P2PServer p2pServer = new P2PServer(HostName, Port);

                p2pServer.OnBlockAccepted += OnBlockAccepted;
                p2pServer.OnTransactionAccepted += OnTransactionAccepted;

                return p2pServer;
            });

            server.Start();
        }

        public void Stop() => server.Stop();

        protected override void OnMessage(MessageEventArgs e)
        {
            string message = e.Data;
            string[] words = message.Split(new char[] { ' ' });
#if (DEBUG)
            int length = message.Length;
#endif
            switch (words[0])
            {
                case Commands.ClientAcceptBlockRequest:
                    string blockJson = words[1];
                    Block block = JsonConvert.DeserializeObject<Block>(blockJson);
                    BlockAcceptEventArgs blockEventArgs = new BlockAcceptEventArgs(block);
#if (DEBUG)
                    Console.WriteLine($"Accepted block:\n{blockJson}");
#endif
                    OnBlockAccepted?.Invoke(this, blockEventArgs);

                    break;

                case Commands.ClientAcceptTransactionRequest:
                    string transactionJson = words[1];
                    Transaction transaction = JsonConvert.DeserializeObject<Transaction>(transactionJson);
                    TransactionAcceptEventArgs transactionEventArgs = new TransactionAcceptEventArgs(transaction);
#if (DEBUG)
                    Console.WriteLine($"Accepted transaction:\n{transactionJson}");
#endif
                    OnTransactionAccepted?.Invoke(this, transactionEventArgs);

                    break;
            }
        }
    }
}
