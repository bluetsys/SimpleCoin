using System;
using SimpleBlockchain.Net;
using System.Net;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using System.Threading;

namespace ClientDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            P2PClient client = new P2PClient()
            {
                Signer = new ECDSASignatureProvider(),
                ByteConverter = new ByteConverter()
            };

            #region Connection.

            client.Connect("ws://" + Dns.GetHostName() + ":8900/simplecoin");

            Thread.Sleep(1000);

            Console.WriteLine(client.ClientState == ClientState.Connected ? "Connection successfully established" : "Connection was not established");

            #endregion
        }
    }
}
