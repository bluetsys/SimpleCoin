//#define CREATE_EXTRA_FILE

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

namespace SimpleBlockchain
{
    class Program
    {
        public const int Difficulty = 2;
        public const string MiningConfigPath = "miningConfig.json";
        public const string ManagerConfigPath = "managerConfig.json";
        public const string FirstRepositoryConfigPath = "repository0Config.json";
        public const string SecondRepositoryConfigPath = "repository1Config.json";
        public const string WalletManagerConfigPath = "walletManagerConfig.json";
        public const string FirstBookPath = "addressBook0.json";
        public const string SecondBookPath = "addressBook1.json";
        public const string FirstNetworkConfigPath = "networkConfig0.json";
        public const string SecondNetworkConfigPath = "networkConfig1.json";

        private static void removeDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
                return;

            dir.Delete(true);
        }

        static void Main(string[] args)
        {
            #region Configs creation.

            BlockchainStorageManagerParameters managerParameters = new BlockchainStorageManagerParameters() { BlocksInUnit = 2 };
            UnitRepositoryParameters firstRepositoryParameters = new UnitRepositoryParameters() { DirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "blockchain0") };
            UnitRepositoryParameters secondRepositoryParameters = new UnitRepositoryParameters() { DirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "blockchain1") };
            WalletManagerParameters walletManagerParameters = new WalletManagerParameters() { WalletDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wallets") };

            NetworkParameters networkParametersFirst = new NetworkParameters()
            {
                AddressBookPath = Path.Combine(Directory.GetCurrentDirectory(), FirstBookPath),

                PeerHostName = Dns.GetHostName(),
                PeerPort = 8900,

                ClientTimeout = new TimeSpan(0, 0, 4),
                ServerTimeout = new TimeSpan(0, 0, 4)
            };

            NetworkParameters networkParametersSecond = new NetworkParameters()
            {
                AddressBookPath = Path.Combine(Directory.GetCurrentDirectory(), SecondBookPath),

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

            using (Stream jsonFile = File.Open(FirstRepositoryConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, firstRepositoryParameters);

            using (Stream jsonFile = File.Open(SecondRepositoryConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, secondRepositoryParameters);

            using (Stream jsonFile = File.Open(WalletManagerConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, walletManagerParameters);

            using (Stream jsonFile = File.Open(FirstNetworkConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, networkParametersFirst);

            using (Stream jsonFile = File.Open(SecondNetworkConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, networkParametersSecond);

            JsonBasicMiningFactoryConfig miningConfig = new JsonBasicMiningFactoryConfig(MiningConfigPath);
            JsonBlockchainStorageManagerConfig managerConfig = new JsonBlockchainStorageManagerConfig(ManagerConfigPath);
            JsonUnitRepositoryConfig firstRepositoryConfig = new JsonUnitRepositoryConfig(FirstRepositoryConfigPath);
            JsonUnitRepositoryConfig secondRepositoryConfig = new JsonUnitRepositoryConfig(SecondRepositoryConfigPath);
            JsonWalletManagerConfig walletManagerConfig = new JsonWalletManagerConfig(WalletManagerConfigPath);
            JsonNetworkConfig firstNetworkConfig = new JsonNetworkConfig(FirstNetworkConfigPath);
            JsonNetworkConfig secondNetworkConfig = new JsonNetworkConfig(SecondNetworkConfigPath);

            #endregion

            #region Directory remove.

            removeDirectory(firstRepositoryConfig.DirectoryPath);
            removeDirectory(secondRepositoryConfig.DirectoryPath);
            removeDirectory(walletManagerConfig.WalletDirectoryPath);

            #endregion

            #region Directory creation.

            Directory.CreateDirectory(firstRepositoryParameters.DirectoryPath);
            Directory.CreateDirectory(secondRepositoryParameters.DirectoryPath);
            Directory.CreateDirectory(Path.Combine(firstRepositoryConfig.DirectoryPath, "addressers"));
            Directory.CreateDirectory(Path.Combine(firstRepositoryConfig.DirectoryPath, "units"));
            Directory.CreateDirectory(Path.Combine(secondRepositoryConfig.DirectoryPath, "addressers"));
            Directory.CreateDirectory(Path.Combine(secondRepositoryConfig.DirectoryPath, "units"));
            Directory.CreateDirectory(walletManagerConfig.WalletDirectoryPath);

            #endregion

            #region Initial units.

            // Addressers.
            string firstAddresser0Path = Path.Combine(firstRepositoryParameters.DirectoryPath, "addressers", "addresser0.addr");
            string secondAddresser0Path = Path.Combine(secondRepositoryParameters.DirectoryPath, "addressers", "addresser0.addr");

#if (CREATE_EXTRA_FILE)
            string firstAddresser1Path = Path.Combine(firstRepositoryParameters.DirectoryPath, "addressers", "addresser1.addr");
            string secondAddresser1Path = Path.Combine(secondRepositoryParameters.DirectoryPath, "addressers", "addresser1.addr");
#endif

            File.Create(firstAddresser0Path).Close();
            File.Create(secondAddresser0Path).Close();
            
#if (CREATE_EXTRA_FILE)
            File.Create(firstAddresser1Path).Close();
            File.Create(secondAddresser1Path).Close();
#endif

            // Units.
            string firstUnit0Path = Path.Combine(firstRepositoryParameters.DirectoryPath, "units", "unit0.unit");
            string secondUnit0Path = Path.Combine(secondRepositoryParameters.DirectoryPath, "units", "unit0.unit");

#if (CREATE_EXTRA_FILE)
            string firstUnit1Path = Path.Combine(firstRepositoryParameters.DirectoryPath, "units", "unit1.unit");
            string secondUnit1Path = Path.Combine(secondRepositoryParameters.DirectoryPath, "units", "unit1.unit");
#endif

            File.Create(firstUnit0Path).Close();
            File.Create(secondUnit0Path).Close();

#if (CREATE_EXTRA_FILE)
            File.Create(firstUnit1Path).Close();
            File.Create(secondUnit1Path).Close();
#endif

            BlockchainUnit firstUnit0 = new BlockchainUnit(firstUnit0Path, new AddressStorage(firstAddresser0Path));
            BlockchainUnit secondUnit0 = new BlockchainUnit(secondUnit0Path, new AddressStorage(secondAddresser0Path));

            #endregion

            IMiningFactory miningFactory = new BasicMiningFactory(miningConfig);
            IHashFactory hashFactory = miningFactory.GetMiningHashFactoryById(3);
            IHashFactory transactionHashFactory = new TransactionHashFactory();
            ISignatureFactory signatureFactory = new ECDSAFactory();
            IWalletManager walletManager = new WalletManager(walletManagerConfig, signatureFactory, transactionHashFactory);

            Wallet firstWallet = walletManager.AddNewWallet();

            #region Initial transactions.

            LinkedList<Transaction> firstInitialList = new LinkedList<Transaction>();
            LinkedList<Transaction> secondInitialList = new LinkedList<Transaction>();

            Transaction firstInitialTransaction = new Transaction(firstWallet.PublicKey, firstWallet.PublicKey, 32, transactionHashFactory);
            Transaction secondInitialTransaction = new Transaction(firstWallet.PublicKey, firstWallet.PublicKey, 19, transactionHashFactory);
            Transaction thirdInitialTransaction = new Transaction(firstWallet.PublicKey, firstWallet.PublicKey, 32, transactionHashFactory);
            Transaction fourthInitialTransaction = new Transaction(firstWallet.PublicKey, firstWallet.PublicKey, 19, transactionHashFactory);

            firstInitialTransaction.SignTransaction(firstWallet.Signer);
            secondInitialTransaction.SignTransaction(firstWallet.Signer);
            thirdInitialTransaction.SignTransaction(firstWallet.Signer);
            fourthInitialTransaction.SignTransaction(firstWallet.Signer);

            firstInitialList.AddLast(firstInitialTransaction);
            firstInitialList.AddLast(secondInitialTransaction);
            secondInitialList.AddLast(thirdInitialTransaction);
            secondInitialList.AddLast(fourthInitialTransaction);

            #endregion

            #region Initial blocks.
            
            IHashFactory miningHashFactory = miningFactory.GetMiningHashFactoryById(3);
            IMiner miner = new BasicMiner(3, Difficulty, miningHashFactory);

            Block firstInitialBlock = new Block(firstWallet.PublicKey, new byte[hashFactory.GetDigest().HashLength], firstInitialList, hashFactory);

            miner.MineBlock(firstInitialBlock);
            firstInitialBlock.SignBlock(firstWallet.Signer);

            Block secondInitialBlock = new Block(firstWallet.PublicKey, firstInitialBlock.Hash, secondInitialList, hashFactory);
            
            miner.MineBlock(secondInitialBlock);
            secondInitialBlock.SignBlock(firstWallet.Signer);

            firstUnit0.AddBlock(firstInitialBlock);
            firstUnit0.AddBlock(secondInitialBlock);
            secondUnit0.AddBlock(firstInitialBlock);
            secondUnit0.AddBlock(secondInitialBlock);

            firstUnit0.Dispose();
            secondUnit0.Dispose();

            #endregion

            UnitRepository firstRepository = new UnitRepository(firstRepositoryConfig);
            UnitRepository secondRepository = new UnitRepository(secondRepositoryConfig);

            BlockchainStorageManager firstManager = new BlockchainStorageManager(managerConfig, firstRepository);
            BlockchainStorageManager secondManager = new BlockchainStorageManager(managerConfig, secondRepository);

            Blockchain firstBlockchain = new Blockchain(firstWallet, walletManager, transactionHashFactory, signatureFactory, miningFactory, firstManager);
            Wallet secondWallet = walletManager.AddNewWallet();
            Blockchain secondBlockchain = new Blockchain(secondWallet, walletManager, transactionHashFactory, signatureFactory, miningFactory, secondManager);

            #region Address books creation.

            File.Create(FirstBookPath).Close();
            File.Create(SecondBookPath).Close();

            PeerAddress firstPeer = new PeerAddress(firstWallet.PublicKey);
            PeerAddress secondPeer = new PeerAddress(secondWallet.PublicKey);

            AddressBook firstPeerAddressBook = new AddressBook(FirstBookPath);
            AddressBook secondPeerAddressBook = new AddressBook(SecondBookPath);

            firstPeerAddressBook.Add(secondPeer, "ws://" + secondNetworkConfig.PeerHostName + $":{secondNetworkConfig.PeerPort}/simplecoin");
            secondPeerAddressBook.Add(firstPeer, "ws://" + firstNetworkConfig.PeerHostName + $":{firstNetworkConfig.PeerPort}/simplecoin");

            firstPeerAddressBook.SaveOnDrive();
            secondPeerAddressBook.SaveOnDrive();

            #endregion

            #region Network.

            IBroadcaster firstNetwork = new Network(firstBlockchain, firstPeerAddressBook, firstNetworkConfig, firstWallet.Signer);

            IBroadcaster secondNetwork = new Network(secondBlockchain, secondPeerAddressBook, secondNetworkConfig, secondWallet.Signer);

            firstBlockchain.NetworkBroadcaster = firstNetwork;
            secondBlockchain.NetworkBroadcaster = secondNetwork;

            firstNetwork.Start();
            secondNetwork.Start();

            #endregion

            firstWallet.Blockchain = firstBlockchain;
            secondWallet.Blockchain = secondBlockchain;

            firstWallet.SendTokens(16, secondWallet.PublicKey);
            firstWallet.SendTokens(8, secondWallet.PublicKey);

            secondBlockchain.ProcessPendingTransactions();

            secondWallet.SendTokens(8, firstWallet.PublicKey);
            firstBlockchain.ProcessPendingTransactions();

            Console.WriteLine("First blockchain:\n");

            foreach (Block block in firstBlockchain)
            {
                Console.WriteLine(JsonConvert.SerializeObject(block, Formatting.Indented));
                Console.WriteLine();
            }

            Console.WriteLine("--------------------------------------------------------------------------------------");

            Console.WriteLine("Second blockchain:\n");

            foreach (Block block in secondBlockchain)
            {
                Console.WriteLine(JsonConvert.SerializeObject(block, Formatting.Indented));
                Console.WriteLine();
            }

            Console.WriteLine("}\n");

            Console.WriteLine(firstBlockchain.IsValid ? "First blockchain is valid" : "First blockchain is invalid");
            Console.WriteLine(firstBlockchain.IsValid ? "Second blockchain is valid" : "Second blockchain is invalid");

            Console.WriteLine($"Cryptocurrency ownership for first wallet: {firstBlockchain.CountBalanceForWallet(firstWallet.PublicKey)}");
            Console.WriteLine($"Cryptocurrency ownership for second wallet: {firstBlockchain.CountBalanceForWallet(secondWallet.PublicKey)}");
        }
    }
}
