using System;
using System.Collections.Generic;
using System.Linq;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.Configs;

namespace SimpleBlockchain.Mining
{
    public class BasicMiningFactory : IMiningFactory
    {
        private Lazy<BasicRewarder> rewarder;
        private IBasicMiningFactoryConfig config;

        public BasicMiningFactory(IBasicMiningFactoryConfig config)
        {
            rewarder = new Lazy<BasicRewarder>(() => new BasicRewarder());
            this.config = config;
        }

        private double amountAverage(IEnumerable<Transaction> transactions) => transactions.Average(transaction => transaction.Amount);

        private double amountFactor(IEnumerable<Transaction> currentTransactions, IEnumerable<Transaction> previousTransactions)
        {
            double currentAverage = amountAverage(currentTransactions);
            double previousAverage = amountAverage(previousTransactions);
            double factor = currentAverage / previousAverage;

            return factor;
        }

        private int adjustDifficultyByFactor(double factor, int difficulty, int maxDifficulty)
        {
            if (factor <= config.DifficultyMostDownFactor)
            {
                difficulty--;

                if (factor <= config.DifficultyLeastDownFactor)
                    difficulty--;
            }

            if (factor >= config.DifficultyLeastUpFactor)
            {
                difficulty++;

                if (factor >= config.DifficultyMostUpFactor)
                    difficulty++;
            }

            if (difficulty < config.MinDifficulty)
                difficulty = config.MinDifficulty;

            if (difficulty > maxDifficulty)
                difficulty = maxDifficulty;

            return difficulty;
        }

        private int adjustAlgorithmIdByFactor(double factor, int algorithmId, int maxId)
        {
            if (factor <= config.HashMostDownFactor)
            {
                algorithmId--;

                if (factor <= config.HashLeastDownFactor)
                    algorithmId--;
            }

            if (factor >= config.HashLeastUpFactor)
            {
                algorithmId++;

                if (factor >= config.HashMostUpFactor)
                    algorithmId++;
            }

            if (algorithmId < 0)
                algorithmId = 0;

            if (algorithmId > maxId)
                algorithmId = maxId;

            return algorithmId;
        }

        public IMiner GetMiner(IEnumerable<Transaction> transactions, Block previousBlock)
        {
            double factor = amountFactor(transactions, previousBlock.Transactions);
            int algorithmId = previousBlock.HashAlgorithmId;
            int difficulty = previousBlock.Difficulty;
            
            algorithmId = adjustAlgorithmIdByFactor(factor, algorithmId, maxId: 3);

            IHashFactory hashFactory = GetMiningHashFactoryById(algorithmId);
            int maxDifficulty = hashFactory.GetDigest().HashLength / 2;

            difficulty = adjustDifficultyByFactor(factor, difficulty, maxDifficulty);

            return new BasicMiner(algorithmId, difficulty, hashFactory);
        }

        public IHashFactory GetMiningHashFactoryForTransactions(IEnumerable<Transaction> transactions, Block previousBlock)
        {
            double factor = amountFactor(transactions, previousBlock.Transactions);
            int algorithmId = adjustAlgorithmIdByFactor(factor, previousBlock.HashAlgorithmId, maxId: 3);

            return GetMiningHashFactoryById(algorithmId);
        }

        public IHashFactory GetMiningHashFactoryById(int id)
        {
            switch (id)
            {
                case 0:
                    return new KeccakFactory(hashLengthInBits: 224, nonceLength: 16);

                case 1:
                    return new KeccakFactory(hashLengthInBits: 256, nonceLength: 24);

                case 2:
                    return new KeccakFactory(hashLengthInBits: 384, nonceLength: 48);

                case 3:
                    return new KeccakFactory(hashLengthInBits: 512, nonceLength: 64);

                default:
                    return null;
            }
        }

        public IRewarder GetRewarder() => rewarder.Value;
    }
}
