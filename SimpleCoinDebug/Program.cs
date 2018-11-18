//#define CREATE_EXTRA_FILE

using System;
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

            JsonBasicMiningFactoryConfig miningConfig = new JsonBasicMiningFactoryConfig(MiningConfigPath);
            JsonBlockchainStorageManagerConfig managerConfig = new JsonBlockchainStorageManagerConfig(ManagerConfigPath);
            JsonUnitRepositoryConfig firstRepositoryConfig = new JsonUnitRepositoryConfig(FirstRepositoryConfigPath);
            JsonUnitRepositoryConfig secondRepositoryConfig = new JsonUnitRepositoryConfig(SecondRepositoryConfigPath);
            JsonWalletManagerConfig walletManagerConfig = new JsonWalletManagerConfig(WalletManagerConfigPath);

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
            IMiner miner = new BasicMiner(Difficulty, miningHashFactory);

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

            #region Network.

            IBroadcaster firstToSecond = new Network(secondBlockchain);
            IBroadcaster secondToFirst = new Network(firstBlockchain);

            firstBlockchain.NetworkBroadcaster = firstToSecond;
            secondBlockchain.NetworkBroadcaster = secondToFirst;

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
