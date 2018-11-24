using System;
using SimpleBlockchain.Net;
using System.Net;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.Mining;
using System.Threading;

namespace ClientDebug
{
    class Program
    {
        private static Random random = new Random(DateTime.Now.Millisecond);

        private static byte randomByte() => (byte)random.Next(0, 255);

        static void Main(string[] args)
        {
            P2PClient client = new P2PClient()
            {
                Signer = new ECDSASignatureProvider(),
                ByteConverter = new ByteConverter()
            };

            #region Connection.

            client.Connect("ws://" + Dns.GetHostName() + ":8900/simplecoin");

            Thread.Sleep(4000);

            Console.WriteLine(client.ClientState == ClientState.Connected ? "Connection successfully established" : "Connection was not established");

            #endregion

            #region Block sending.
            
            Block block = new Block(
                                    new byte[] { randomByte(), randomByte(), randomByte() },
                                    new byte[] { randomByte(), randomByte(), randomByte() },
                                    new Transaction[]
                                    {
                                        new Transaction
                                        (
                                            new byte[] { randomByte(), randomByte(), randomByte() },
                                            new byte[] { randomByte(), randomByte(), randomByte() },
                                            random.Next(1, 100),
                                            new TransactionHashFactory()
                                        ),
                                        new Transaction
                                        (
                                            new byte[] { randomByte(), randomByte(), randomByte() },
                                            new byte[] { randomByte(), randomByte(), randomByte() },
                                            random.Next(1, 100),
                                            new TransactionHashFactory()
                                        )
                                    },
                                    new KeccakFactory(512, 64)
                                    );

            client.SendBlock(block);

            Thread.Sleep(1000);

            #endregion

            #region Transaction sending.

            Transaction transaction = new Transaction
                (
                    new byte[] { randomByte(), randomByte(), randomByte() },
                    new byte[] { randomByte(), randomByte(), randomByte() },
                    random.Next(1, 100),
                    new TransactionHashFactory()
                );

            client.SendTransaction(transaction);

            Thread.Sleep(1000);

            #endregion

            client.Disconnect();

            client.Connect("ws://" + Dns.GetHostName() + ":8900/simplecoin");

            Thread.Sleep(1000);

            client.SendTransaction(transaction);
        }
    }
}
