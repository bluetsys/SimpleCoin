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

namespace FirstPeer
{
    class Program
    {
        public const int Difficulty = 2;
        public const string MiningConfigPath = "miningConfig.json";
        public const string ManagerConfigPath = "managerConfig.json";
        public const string FirstRepositoryConfigPath = "repositoryConfig.json";
        public const string WalletManagerConfigPath = "walletManagerConfig.json";
        public const string FirstBookPath = "addressBook.json";
        public const string FirstNetworkConfigPath = "networkConfig.json";

        private static void removeDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
                return;

            dir.Delete(true);
        }

        static void Main(string[] args)
        {
            BlockchainStorageManagerParameters managerParameters = new BlockchainStorageManagerParameters() { BlocksInUnit = 2 };
            UnitRepositoryParameters firstRepositoryParameters = new UnitRepositoryParameters() { DirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "blockchain") };
            WalletManagerParameters walletManagerParameters = new WalletManagerParameters() { WalletDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wallets") };

            NetworkParameters networkParametersFirst = new NetworkParameters()
            {
                AddressBookPath = Path.Combine(Directory.GetCurrentDirectory(), FirstBookPath),

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

            using (Stream jsonFile = File.Open(FirstRepositoryConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, firstRepositoryParameters);

            using (Stream jsonFile = File.Open(WalletManagerConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, walletManagerParameters);

            using (Stream jsonFile = File.Open(FirstNetworkConfigPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                jsonSerializer.Serialize(jsonWriter, networkParametersFirst);

            JsonBasicMiningFactoryConfig miningConfig = new JsonBasicMiningFactoryConfig(MiningConfigPath);
            JsonBlockchainStorageManagerConfig managerConfig = new JsonBlockchainStorageManagerConfig(ManagerConfigPath);
            JsonUnitRepositoryConfig firstRepositoryConfig = new JsonUnitRepositoryConfig(FirstRepositoryConfigPath);
            JsonWalletManagerConfig walletManagerConfig = new JsonWalletManagerConfig(WalletManagerConfigPath);
            JsonNetworkConfig firstNetworkConfig = new JsonNetworkConfig(FirstNetworkConfigPath);

            #region Directory remove.

            removeDirectory(firstRepositoryConfig.DirectoryPath);
            //removeDirectory(walletManagerConfig.WalletDirectoryPath);

            #endregion

            #region Directory creation.

            Directory.CreateDirectory(firstRepositoryParameters.DirectoryPath);
            Directory.CreateDirectory(Path.Combine(firstRepositoryConfig.DirectoryPath, "addressers"));
            Directory.CreateDirectory(Path.Combine(firstRepositoryConfig.DirectoryPath, "units"));
            Directory.CreateDirectory(walletManagerConfig.WalletDirectoryPath);

            #endregion

            #region Initial units.

            // Addressers.
            string firstAddresser0Path = Path.Combine(firstRepositoryParameters.DirectoryPath, "addressers", "addresser0.addr");

            File.Create(firstAddresser0Path).Close();

            // Units.
            string firstUnit0Path = Path.Combine(firstRepositoryParameters.DirectoryPath, "units", "unit0.unit");

            File.Create(firstUnit0Path).Close();

            BlockchainUnit firstUnit0 = new BlockchainUnit(firstUnit0Path, new AddressStorage(firstAddresser0Path));

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

            firstUnit0.Dispose();

            #endregion

            UnitRepository firstRepository = new UnitRepository(firstRepositoryConfig);
            BlockchainStorageManager firstManager = new BlockchainStorageManager(managerConfig, firstRepository);
            Blockchain firstBlockchain = new Blockchain(firstWallet, walletManager, transactionHashFactory, signatureFactory, miningFactory, firstManager);

            #region Address books creation.

            File.Create(FirstBookPath).Close();

            PeerAddress other = new PeerAddress(firstWallet.PublicKey);
            AddressBook firstPeerAddressBook = new AddressBook(FirstBookPath);

            //firstPeerAddressBook.Add(secondPeer, "ws://" + secondNetworkConfig.PeerHostName + $":{secondNetworkConfig.PeerPort}/simplecoin");

            firstPeerAddressBook.SaveOnDrive();

            #endregion
        }
    }
}
