using System;
using SimpleBlockchain.Net;
using System.Net;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using Newtonsoft.Json;

namespace ServerDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            P2PServer server = new P2PServer()
            {
                RandomNumberLength = 64,
                HashLength = 512,

                HostName = Dns.GetHostName(),
                Port = 8900,

                Converter = new ByteConverter(),
                Verifier = new ECDSASignatureVerifier()
            };

            server.OnBlockAccepted += (sender, eventArgs) => Console.WriteLine($"Accepted block:\n{JsonConvert.SerializeObject(eventArgs.Block, Formatting.Indented)}");
            server.OnTransactionAccepted += (sender, eventArgs) => Console.WriteLine($"Accepted transaction: {JsonConvert.SerializeObject(eventArgs.Transaction, Formatting.Indented)}");

            server.Start();

            Console.ReadKey();

            server.Stop();
        }
    }
}
