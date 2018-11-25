using System;
using System.Threading;
using System.Collections.Generic;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.Net;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.Mining;
using Newtonsoft.Json;
using SimpleBlockchain.Configs;
using SimpleBlockchain.Configs.Parameters;
using SimpleBlockchain.Storage;
using DriveAccessors;
using System.IO;
using System.Net;
using System.Linq;

namespace FirstPeer
{
    class Program
    {
        public const int Difficulty = 2;
        public const string MiningConfigPath = "miningConfig.json";
        public const string ManagerConfigPath = "managerConfig.json";
        public const string RepositoryConfigPath = "repositoryConfig.json";
        public const string WalletManagerConfigPath = "walletManagerConfig.json";
        public const string AddressBookPath = "addressBook.json";
        public const string NetworkConfigPath = "networkConfig.json";

        private static void removeDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
                return;

            dir.Delete(true);
        }

        static void Main(string[] args)
        {
            #region Configs.

            BlockchainStorageManagerParameters managerParameters = new BlockchainStorageManagerParameters() { BlocksInUnit = 2 };
            UnitRepositoryParameters repositoryParameters = new UnitRepositoryParameters() { DirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "blockchain") };
            WalletManagerParameters walletManagerParameters = new WalletManagerParameters() { WalletDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wallets") };

            NetworkParameters networkParametersFirst = new NetworkParameters()
            {
                AddressBookPath = Path.Combine(Directory.GetCurrentDirectory(), AddressBookPath),

                PeerHostName = Dns.GetHostName(),
                PeerPort = 8910,

                ClientTimeout = new TimeSpan(0, 0, 4),
                ServerTimeout = new TimeSpan(0, 0, 4)
            };

            BasicMiningFactoryParameters parameters = new BasicMiningFactoryParameters()
            {
                HashLeastUpFactor = 1.25,
                HashMostUpFactor = 1.75,

                HashLeastDownFactor = 0.25,
                HashMostDownFactor = 0.75,

                DifficultyLeastUpFactor = 1.25,
                DifficultyMostUpFactor = 1.75,

                DifficultyLeastDownFactor = 0.25,
                DifficultyMostDownFactor = 0.75,

                MinDifficulty = Difficulty
            };

            JsonSerializer jsonSerializer = new JsonSerializer() { Formatting = Formatting.Indented };

            using (Stream jsonFile = File.Open(MiningConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, parameters);

            using (Stream jsonFile = File.Open(ManagerConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, managerParameters);

            using (Stream jsonFile = File.Open(RepositoryConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, repositoryParameters);

            using (Stream jsonFile = File.Open(WalletManagerConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, walletManagerParameters);

            using (Stream jsonFile = File.Open(NetworkConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, networkParametersFirst);

            JsonBasicMiningFactoryConfig miningConfig = new JsonBasicMiningFactoryConfig(MiningConfigPath);
            JsonBlockchainStorageManagerConfig managerConfig = new JsonBlockchainStorageManagerConfig(ManagerConfigPath);
            JsonUnitRepositoryConfig repositoryConfig = new JsonUnitRepositoryConfig(RepositoryConfigPath);
            JsonWalletManagerConfig walletManagerConfig = new JsonWalletManagerConfig(WalletManagerConfigPath);
            JsonNetworkConfig networkConfig = new JsonNetworkConfig(NetworkConfigPath);

            #endregion

            IMiningFactory miningFactory = new BasicMiningFactory(miningConfig);
            IHashFactory hashFactory = miningFactory.GetMiningHashFactoryById(3);
            IHashFactory transactionHashFactory = new TransactionHashFactory();
            ISignatureFactory signatureFactory = new ECDSAFactory();
            IWalletManager walletManager = new WalletManager(walletManagerConfig, signatureFactory, transactionHashFactory);

            Wallet wallet = walletManager.GetWallets().First();
            UnitRepository repository = new UnitRepository(repositoryConfig);
            BlockchainStorageManager storageManager = new BlockchainStorageManager(managerConfig, repository);
            Blockchain blockchain = new Blockchain(wallet, walletManager, transactionHashFactory, signatureFactory, miningFactory, storageManager);

            wallet.Blockchain = blockchain;

            #region Network.

            AddressBook addressBook = new AddressBook(AddressBookPath);
            IBroadcaster network = new Network(blockchain, addressBook, networkConfig, wallet.Signer);

            blockchain.NetworkBroadcaster = network;

            network.Start();

            #endregion

            Console.WriteLine("Second");
            Console.WriteLine("0 - send transaction;\n1 - mine new block;\n2 - validate blockchain;\n3 - count your balance;\n4 - quit");

            string input;

            do
            {
                input = Console.ReadLine();

                switch (input)
                {
                    case "0":
                        wallet.SendTokens(8, addressBook.First().Key.PublicKey);

                        break;

                    case "1":
                        blockchain.ProcessPendingTransactions();

                        break;

                    case "2":
                        Console.WriteLine(blockchain.IsValid);

                        break;

                    case "3":
                        Console.WriteLine(blockchain.CountBalanceForWallet(wallet.PublicKey));

                        break;

                    case "4":
                        Console.WriteLine("Bye!");

                        break;

                    default:
                        Console.WriteLine("Wrong input!");

                        break;
                }
            }
            while (input != "4");

            network.Stop();
        }
    }
}
