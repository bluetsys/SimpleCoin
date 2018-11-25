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
        public const string SecondPeerRepositoryPath = @"C:\Users\zergon321\Documents\Programming\C#\SimpleCoin\SecondPeer\bin\Debug\netcoreapp2.1\blockchain";

        private static void removeDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
                return;

            dir.Delete(true);
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);

                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);

                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
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

                HashLength = 512,
                RandomNumberLength = 64,

                PeerHostName = Dns.GetHostName(),
                PeerPort = 8900,

                ClientTimeout = new TimeSpan(0, 0, 20),
                ServerTimeout = new TimeSpan(0, 0, 20)
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

            #region Directory remove.

            removeDirectory(repositoryConfig.DirectoryPath);
            //removeDirectory(walletManagerConfig.WalletDirectoryPath);

            #endregion

            #region Directory creation.

            Directory.CreateDirectory(repositoryParameters.DirectoryPath);
            Directory.CreateDirectory(Path.Combine(repositoryConfig.DirectoryPath, "addressers"));
            Directory.CreateDirectory(Path.Combine(repositoryConfig.DirectoryPath, "units"));
            //Directory.CreateDirectory(walletManagerConfig.WalletDirectoryPath);

            #endregion

            #region Initial units.

            // Addressers.
            string addresser0Path = Path.Combine(repositoryParameters.DirectoryPath, "addressers", "addresser0.addr");

            File.Create(addresser0Path).Close();

            // Units.
            string unit0Path = Path.Combine(repositoryParameters.DirectoryPath, "units", "unit0.unit");

            File.Create(unit0Path).Close();

            BlockchainUnit unit0 = new BlockchainUnit(unit0Path, new AddressStorage(addresser0Path));

            #endregion

            IMiningFactory miningFactory = new BasicMiningFactory(miningConfig);
            IHashFactory hashFactory = miningFactory.GetMiningHashFactoryById(3);
            IHashFactory transactionHashFactory = new TransactionHashFactory();
            ISignatureFactory signatureFactory = new ECDSAFactory();
            IWalletManager walletManager = new WalletManager(walletManagerConfig, signatureFactory, transactionHashFactory);

            Wallet wallet = walletManager.GetWallets().First();

            #region Initial transactions.

            LinkedList<Transaction> firstInitialList = new LinkedList<Transaction>();
            LinkedList<Transaction> secondInitialList = new LinkedList<Transaction>();

            Transaction firstInitialTransaction = new Transaction(wallet.PublicKey, wallet.PublicKey, 32, transactionHashFactory);
            Transaction secondInitialTransaction = new Transaction(wallet.PublicKey, wallet.PublicKey, 19, transactionHashFactory);
            Transaction thirdInitialTransaction = new Transaction(wallet.PublicKey, wallet.PublicKey, 32, transactionHashFactory);
            Transaction fourthInitialTransaction = new Transaction(wallet.PublicKey, wallet.PublicKey, 19, transactionHashFactory);

            firstInitialTransaction.SignTransaction(wallet.Signer);
            secondInitialTransaction.SignTransaction(wallet.Signer);
            thirdInitialTransaction.SignTransaction(wallet.Signer);
            fourthInitialTransaction.SignTransaction(wallet.Signer);

            firstInitialList.AddLast(firstInitialTransaction);
            firstInitialList.AddLast(secondInitialTransaction);
            secondInitialList.AddLast(thirdInitialTransaction);
            secondInitialList.AddLast(fourthInitialTransaction);

            #endregion

            #region Initial blocks.

            IHashFactory miningHashFactory = miningFactory.GetMiningHashFactoryById(3);
            IMiner miner = new BasicMiner(3, Difficulty, miningHashFactory);

            Block firstInitialBlock = new Block(wallet.PublicKey, new byte[hashFactory.GetDigest().HashLength], firstInitialList, hashFactory);

            miner.MineBlock(firstInitialBlock);
            firstInitialBlock.SignBlock(wallet.Signer);

            Block secondInitialBlock = new Block(wallet.PublicKey, firstInitialBlock.Hash, secondInitialList, hashFactory);

            miner.MineBlock(secondInitialBlock);
            secondInitialBlock.SignBlock(wallet.Signer);

            unit0.AddBlock(firstInitialBlock);
            unit0.AddBlock(secondInitialBlock);

            unit0.Dispose();

            #endregion

            Copy(repositoryConfig.DirectoryPath, SecondPeerRepositoryPath);

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

            Console.WriteLine("First");
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
